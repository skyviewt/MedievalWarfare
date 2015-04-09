using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class UnitManager : MonoBehaviour {

	public GameObject meadowPrefab;
	public GameObject curEffect;
	public GameObject attackEffect1;
	public GameObject attackEffect2;
	public VillageManager villageManager;
	public TileManager tileManager;
	public InGameGUI gameGUI;
	public readonly int TEN = 10;

	// Use this for initialization

	void Start () {
		villageManager = GameObject.Find ("VillageManager").GetComponent<VillageManager>();
		tileManager = GameObject.Find ("TileManager").GetComponent<TileManager> ();
		gameGUI = GameObject.Find ("attachingGUI").GetComponent<InGameGUI>();
	}

	[RPC]
	void moveUnitNet(NetworkViewID unitID, NetworkViewID tileID){
		Unit unitToMove = NetworkView.Find (unitID).gameObject.GetComponent<Unit>();
		Tile dest = NetworkView.Find (tileID).gameObject.GetComponent<Tile>();
		moveUnit (unitToMove, dest);
	}

	// needs networking
	public void moveUnit(Unit unit, Tile dest)
	{
		//print ("----in move unit----");
		Village destVillage = dest.getVillage ();
		Village srcVillage = unit.getVillage ();
		
		Unit destUnit = dest.getOccupyingUnit ();
		UnitType srcUnitType = unit.getUnitType();

		bool unitPermitted = canUnitMove (unit, dest);
		
		//if the move is allowed to move onto the tile
		if (unitPermitted == true) 	
		{
			Tile originalLocation = unit.getLocation ();
			// moving within your region
			if (srcVillage == destVillage)
			{
				performMove(unit,dest);
				originalLocation.setOccupyingUnit(null);
			}
			else if (srcVillage != destVillage)
			{
				// taking over neutral tiles
				if (destVillage == null)
				{
					srcVillage.addTile(dest);
					performMove(unit,dest);
					villageManager.MergeAlliedRegions(dest);
					unit.setAction(UnitActionType.CapturingNeutral);
					originalLocation.setOccupyingUnit(null);
				}

				// TODO taking over enemy tiles and networking it
				else if (srcUnitType == UnitType.PEASANT)
				{ 
					gameGUI.displayError (@"A peasant is too weak to invade!");
					return;
				}
				else
				{
					// quit if tile is guarded by unit or tower
					bool guarded = tileManager.checkNeighboursForGuards(dest, unit);
					if (guarded){
						gameGUI.displayError (@"That area is being protected");
						return;
					}

					// unit on unit combat!!
					// if there is any enemy unit
					if (destUnit!=null){
						if(srcUnitType>destUnit.getUnitType()){
							//unit.animation.CrossFade("attack");
							// kill enemy unit, remove it from tile, remove it from village
							//perform move gets called after.
							destVillage.removeUnit(destUnit); //removes U from V's army AND sets U's v to null
							dest.setOccupyingUnit(unit);
							Destroy (destUnit.gameObject);
							//adding an attack effect
							curEffect = Instantiate(attackEffect1, new Vector3(dest.point.x, 0.2f, dest.point.y), attackEffect1.transform.rotation) as GameObject;
							//unit.animation.CrossFadeQueued("idle");

						} else {
							gameGUI.displayError (@"The enemy is too strong! I dont want to die!");
							return;
						}
					}
					// if the tile contains the enemy village
						// pillage, then move the hovel
					if (destVillage.getLocatedAt()==dest){
						if (srcUnitType > UnitType.INFANTRY){
							// plunder village will handle stealing resources
							villageManager.plunderVillage (srcVillage, destVillage, dest);
							// it also calls respawn hovel and creating a meadow
						} else {
							gameGUI.displayError (@"This unit is too weak to plunder villages");
							return;
						}
					}
					// destroy towers
					if (dest.getStructure()!=null && srcUnitType>UnitType.INFANTRY){
						dest.setStructure(false);
						dest.replace (null);
					}

					villageManager.takeoverTile(srcVillage,dest); //also splits region
					villageManager.MergeAlliedRegions(dest);
					performMove(unit,dest);
					unit.setAction(UnitActionType.CapturingEnemy);
					originalLocation.setOccupyingUnit(null);

				} 
			}
		}

	}

	private void movePrefab(Unit u, Vector3 vector)
	{
		u.transform.localPosition = vector;
	}

	//It's presumed that the unit is already ontop of the tile.
	//Thus the tile is either grass or meadow.
	public void cultivateMeadow (Unit u)
	{
		if (u.getUnitType() != UnitType.PEASANT) {
			gameGUI.displayError (@"Only your peasants are willing to cultivate meadows.");
		} 
		else 
		{
			Tile uLocation = u.getLocation();
			LandType tileType = uLocation.getLandType();
			if(tileType == LandType.Meadow)
			{
				gameGUI.displayError (@"There is already a lovely meadow here.");
			}
			else if(tileType == LandType.Grass)
			{
				//TODO delay for turn manager
				uLocation.setLandType(LandType.Meadow);
				uLocation.prefab = Instantiate (meadowPrefab, new Vector3 (uLocation.point.x, 0, uLocation.point.y), meadowPrefab.transform.rotation) as GameObject;

				u.setAction(UnitActionType.StartCultivating);
			}
		}

	}

	private void performMove(Unit unit, Tile dest)
	{
		dest.setOccupyingUnit(unit);
		unit.setLocation(dest);
		Village srcVillage = unit.getVillage ();
		UnitType srcUnitType = unit.getUnitType();
		LandType destLandType = dest.getLandType ();

		if (srcUnitType == UnitType.KNIGHT || srcUnitType == UnitType.SOLDIER || srcUnitType == UnitType.CANNON) 
		{
			bool destHasRoad = dest.checkRoad ();
			if (destLandType == LandType.Meadow && destHasRoad == false) 
			{
				// detroying the meadow by knight
				dest.setLandType (LandType.Grass);
				Destroy (dest.prefab);
			}
			unit.setAction (UnitActionType.Moved);
			if (srcUnitType == UnitType.CANNON){
				unit.setAction (UnitActionType.CannonMoved);
			}
		} 
		else
		{
			if (destLandType == LandType.Trees)
			{
				//print ("entered cutting trees");
				unit.setAction(UnitActionType.ChoppingTree);
				//unit.animation.CrossFade("attack");
				Destroy (dest.prefab);
				dest.prefab = null;

				//unit.animation.CrossFadeQueued("idle");
				srcVillage.addWood(1);
				dest.setLandType(LandType.Grass);
			}
			else if (destLandType == LandType.Tombstone)
			{
				unit.setAction(UnitActionType.ClearingTombstone);
				dest.setLandType(LandType.Grass);
			}
		}
		unit.transform.localPosition = new Vector3 (dest.point.x, 0.15f,dest.point.y);
	}

	private bool canUnitMove(Unit u, Tile t)
	{
		// castle check
		foreach (Tile n in t.getNeighbours()) {
			try{
				Village v = n.getVillage ();
				VillageType vt = v.getMyType();
				Player them = v.controlledBy;
				Player you = u.getVillage().controlledBy;
				if (them!=you && vt==VillageType.Castle){
					gameGUI.displayError (@"I cant even get near to their castle!");
					return false;
				}
			} catch {
				continue;
			}
		}
		// friendly checks
		if (t.getVillage ()==null || t.getVillage ().controlledBy == u.getVillage ().controlledBy) {
			if((t.getLandType () == LandType.Trees || t.getLandType () == LandType.Tombstone) && (u.getUnitType() == UnitType.KNIGHT || u.getUnitType() == UnitType.CANNON)){
				gameGUI.displayError (@"Knights are too fancy to do manual labour. ¯\(°_o)/¯");
				return false;
			} else if (t.getStructure ()!=null){
				gameGUI.displayError (@"The tower doesn't want you to stand ontop of it. ¯\(°_o)/¯");
				return false;
			} else if (t.getOccupyingUnit () != null) {
				gameGUI.displayError (@"There is a unit already standing there!!! ¯\(°_o)/¯");
				return false;
			} else {
				return true;
			}
		// enemy checks
		} else if (t.getVillage ().controlledBy != u.getVillage ().controlledBy){
			if (u.getUnitType()==UnitType.PEASANT){
				gameGUI.displayError (@"Peasants cant attack! ¯\(°_o)/¯");
				return false;
			} else if (u.getUnitType()==UnitType.CANNON){
				gameGUI.displayError (@"Cannons cant move into enemy territory");
				return false;
			} else if((t.getLandType () == LandType.Trees || t.getLandType () == LandType.Tombstone) && u.getUnitType() == UnitType.KNIGHT){
				gameGUI.displayError (@"Knights are too fancy to do manual labour. ¯\(°_o)/¯");
				return false;
			} else if (t.getStructure ()!=null && u.getUnitType()!= UnitType.KNIGHT){
				gameGUI.displayError (@"Only a knight can take a tower. ¯\(°_o)/¯");
				return false;
			} else if (t.getOccupyingUnit()!=null && u.getUnitType()<=t.getOccupyingUnit().getUnitType()){
				if (t.getOccupyingUnit().getUnitType()==UnitType.CANNON && u.getUnitType()<=UnitType.SOLDIER){
					gameGUI.displayError (@"You need a knight to take out their cannon");
					return false;
				} 
				gameGUI.displayError (@"Your unit cant fight theirs. ¯\(°_o)/¯");
				return false;
			} else {
				return true;
			}
		} 
		//default
		return false;
	}

	public void upgradeUnit(Unit u, UnitType newLevel)
	{
		Village unitVillage = u.getVillage();
		VillageType unitVillageLevel = unitVillage.getMyType();
		UnitType unitType = u.getUnitType();
		UnitActionType unitAction = u.getAction();
		int goldAvailable = unitVillage.getGold();
		int goldRequired = (newLevel - unitType) * TEN;
		if (unitType == UnitType.KNIGHT) {
			gameGUI.displayError (@"The Knight is already your strongest warrior! ¯\(°_o)/¯");
		}
		else if((goldAvailable >= goldRequired)&&(newLevel > unitType)&&(unitAction == UnitActionType.ReadyForOrders || unitAction == UnitActionType.Moved))
		{
			if(newLevel == UnitType.SOLDIER && unitVillageLevel < VillageType.Town)
			{
				gameGUI.displayError (@"Please upgrade your village to a Town first. ¯\(°_o)/¯");
			}
			else if(newLevel == UnitType.KNIGHT && unitVillageLevel < VillageType.Fort)
			{
				gameGUI.displayError (@"Please upgrade your village to a Fort first. ¯\(°_o)/¯");
			}
			else
			{
				unitVillage.setGold (goldAvailable - goldRequired);
				u.upgrade(newLevel);
			}
		}
	}

	[RPC]
	void upgradeUnitNet(NetworkViewID unitID, int newlvl){
		Unit u = NetworkView.Find (unitID).gameObject.GetComponent<Unit>();
		upgradeUnit (u, (UnitType)newlvl);
	}

}

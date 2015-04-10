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
	
	
	public void moveUnit(Unit unit, Tile dest)
	{
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
				originalLocation.gameObject.networkView.RPC ("removeOccupyingUnitNet",RPCMode.AllBuffered);
			}
			else if (srcVillage != destVillage)
			{
				// taking over neutral tiles
				if (destVillage == null)
				{
					//srcVillage.addTile(dest);
					srcVillage.gameObject.networkView.RPC ("addTileNet",RPCMode.AllBuffered,dest.gameObject.networkView.viewID);
					performMove(unit,dest);
					villageManager.MergeAlliedRegions(dest);
					unit.gameObject.networkView.RPC("setActionNet",RPCMode.AllBuffered,(int)UnitActionType.CapturingNeutral);
					originalLocation.gameObject.networkView.RPC ("removeOccupyingUnitNet",RPCMode.AllBuffered);
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
							destVillage.gameObject.networkView.RPC ("removeUnitNet",RPCMode.AllBuffered,destUnit.gameObject.networkView.viewID);
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
						dest.setStructure(null);
						dest.replace (null);
					}
					
					villageManager.takeoverTile(srcVillage,dest); //also splits region
					villageManager.MergeAlliedRegions(dest);
					performMove(unit,dest);
					unit.gameObject.networkView.RPC("setActionNet",RPCMode.AllBuffered,(int)UnitActionType.CapturingEnemy);
					originalLocation.gameObject.networkView.RPC ("removeOccupyingUnitNet",RPCMode.AllBuffered);

				} 
			}
		}
	}
	
	private void performMove(Unit unit, Tile dest){
		dest.setOccupyingUnit(unit);
		unit.setLocation(dest);
		Village srcVillage = unit.getVillage ();
		UnitType srcUnitType = unit.getUnitType();
		LandType destLandType = dest.getLandType ();

		if (destLandType == LandType.Meadow) 
		{
			if (srcUnitType==UnitType.CANNON||srcUnitType==UnitType.SOLDIER||srcUnitType==UnitType.KNIGHT)
			{
				if (dest.hasRoad)
				{
					unit.gameObject.networkView.RPC("setActionNet",RPCMode.AllBuffered,(int)UnitActionType.Moved);
				} 
				else 
				{
					gameGUI.displayError (@"You have trampled the crops!");
					dest.gameObject.networkView.RPC ("setLandTypeNet",RPCMode.AllBuffered,(int)LandType.Grass);
					dest.gameObject.networkView.RPC ("setLandTypeNet",RPCMode.AllBuffered,(int)LandType.Grass);
					dest.gameObject.networkView.RPC ("destroyPrefab",RPCMode.AllBuffered);
				}
				if (srcUnitType == UnitType.CANNON)
				{
					unit.gameObject.networkView.RPC("setActionNet",RPCMode.AllBuffered,(int)UnitActionType.CannonMoved);

				}
			}
		} 
		else if (destLandType == LandType.Trees) 
		{
			dest.gameObject.networkView.RPC ("setLandTypeNet",RPCMode.AllBuffered,(int)LandType.Grass);
			unit.gameObject.networkView.RPC("setActionNet",RPCMode.AllBuffered,(int)UnitActionType.ChoppingTree);
			srcVillage.gameObject.networkView.RPC ("addWoodNet",RPCMode.AllBuffered,1);
			dest.gameObject.networkView.RPC ("destroyPrefab",RPCMode.AllBuffered);
		} 
		else if (destLandType == LandType.Tombstone) 
		{
			dest.gameObject.networkView.RPC ("setLandTypeNet",RPCMode.AllBuffered,(int)LandType.Grass);
			unit.gameObject.networkView.RPC("setActionNet",RPCMode.AllBuffered,(int)UnitActionType.ClearingTombstone);
			dest.gameObject.networkView.RPC ("destroyPrefab",RPCMode.AllBuffered);
		}
		movePrefab (unit,new Vector3 (dest.point.x, 0.15f,dest.point.y));
	}

	[RPC]
	void movePrefabNet(NetworkViewID unitID, Vector3 vector)
	{
	}

	private void movePrefab(Unit u, Vector3 vector)
	{
		u.transform.localPosition = vector;
	}
	
	private bool canUnitMove(Unit u, Tile t)
	{
		// castle check
		foreach (Tile n in t.getNeighbours()) {
			try{
				Village v = n.getVillage ();
				VillageType vt = v.getMyType();
				Player them = v.getPlayer ();
				Player you = u.getVillage().getPlayer ();
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
	[RPC]
	void destroyUnitNet(NetworkViewID unitID)
	{
		Unit u = NetworkView.Find (unitID).gameObject.GetComponent<Unit>();
		Destroy(u);
	}
	[RPC]
	public void cultivateMeadowNet (NetworkViewID unitID)
	{
		Unit unitToCultivate = NetworkView.Find (unitID).gameObject.GetComponent<Unit>();
		cultivateMeadow (unitToCultivate);
	}
	public void cultivateMeadow(Unit u)
	{
		u.setAction (UnitActionType.StartedCultivating);
				u.gameObject.networkView.RPC ("setActionNet", RPCMode.AllBuffered, (int)UnitActionType.StartedCultivating);
	}

	public void fireCannon(Unit cannon, Tile t){
		Player you = cannon.getVillage ().controlledBy;
		Player them;
		if (t.getVillage () != null) { // stupid null checks to prevent errors
			them = t.getVillage ().controlledBy;
		} else { // check if neutral
			gameGUI.displayError (@"No need to fire on neutral land");
			return;
		}
		if (you == them) { // dont fire on yourself
			gameGUI.displayError (@"Dont shoot yourself!!");
			return;
		} else { // finally, give em hell!
			Structure s = t.getStructure();
			Unit u = t.getOccupyingUnit();
			Village v = t.getVillage ();
			Tile l = v.getLocatedAt();
			if (s!=null){
				t.replace (null);
				t.setStructure(null);
			}
			if (u!=null){
				v.removeUnit(u);
				t.setOccupyingUnit(null);
				t.replace (null);
				t.setLandType(LandType.Tombstone);
				GameObject tombPrefab = villageManager.tombPrefab;
				t.prefab = Instantiate (tombPrefab, new Vector3 (t.point.x, 0.4f, t.point.y), tombPrefab.transform.rotation) as GameObject;
				Destroy (u.gameObject);
			}
			if (t==l){
				v.takeDamage ();
			}
		
		}

	}
}
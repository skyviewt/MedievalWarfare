using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class UnitManager : MonoBehaviour {
	
	public GameObject meadowPrefab;
	public GameObject tombPrefab;
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
				//TODO STOPPED HERE
			}
			else if (srcVillage != destVillage)
			{
				// taking over neutral tiles
				if (destVillage == null)
				{
					//srcVillage.addTile(dest);
					srcVillage.gameObject.networkView.RPC ("addTileNet",RPCMode.AllBuffered,dest.gameObject.networkView.viewID);
					dest.gameObject.networkView.RPC ("setVillageNet",RPCMode.AllBuffered,srcVillage.gameObject.networkView.viewID);
					int color = srcVillage.getPlayer().getColor();
					dest.gameObject.networkView.RPC ("setAndColor",RPCMode.AllBuffered,color);

					performMove(unit,dest);
					villageManager.MergeAlliedRegions(dest);
					unit.gameObject.networkView.RPC("setActionNet",RPCMode.AllBuffered,(int)UnitActionType.CapturingNeutral);
					originalLocation.gameObject.networkView.RPC ("removeOccupyingUnitNet",RPCMode.AllBuffered);
				}
				

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
							//destVillage.removeUnit(destUnit); //removes U from V's army AND sets U's v to null
							destVillage.gameObject.networkView.RPC ("removeUnitNet",RPCMode.AllBuffered,destUnit.gameObject.networkView.viewID);
							//dest.setOccupyingUnit(unit);
							dest.gameObject.networkView.RPC ("setOccupyingUnitNet",RPCMode.AllBuffered,unit.gameObject.networkView.viewID);
							//Destroy (destUnit.gameObject);
							gameObject.networkView.RPC ("destroyUnitNet",RPCMode.AllBuffered,unit.gameObject.networkView.viewID);
							//adding an attack effect
							//curEffect = Instantiate(attackEffect1, new Vector3(dest.point.x, 0.2f, dest.point.y), attackEffect1.transform.rotation) as GameObject;
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
					if (dest.checkTower() == true && srcUnitType>UnitType.INFANTRY){
						//dest.setStructure(null);
						dest.gameObject.networkView.RPC ("setStructureNet",RPCMode.AllBuffered,false);
						//dest.replace (null);
						dest.gameObject.networkView.RPC ("destroyPrefab",RPCMode.AllBuffered);
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
	
	private void performMove(Unit unit, Tile dest)
	{
		dest.gameObject.networkView.RPC ("setOccupyingUnitNet", RPCMode.AllBuffered, unit.gameObject.networkView.viewID);
		unit.gameObject.networkView.RPC ("setLocationNet", RPCMode.AllBuffered, dest.gameObject.networkView.viewID);
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
		//movePrefab (unit,new Vector3 (dest.point.x, 0.15f,dest.point.y));
		gameObject.networkView.RPC ("moveUnitPrefabNet", RPCMode.AllBuffered, unit.networkView.viewID, new Vector3 (dest.point.x, 0.15f, dest.point.y));
	}

	[RPC]
	void moveUnitPrefabNet(NetworkViewID unitID, Vector3 vector)
	{
		Unit u = NetworkView.Find (unitID).gameObject.GetComponent<Unit> ();
		u.transform.localPosition = vector;
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
			} else if (t.checkTower ()){
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
			} else if (t.checkTower () == true && (u.getUnitType()!= UnitType.KNIGHT || u.getUnitType () != UnitType.SOLDIER)){
				gameGUI.displayError (@"Only a soldiers and knights can destroy an enemy tower. ¯\(°_o)/¯");
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
	public void initializeUnit(Village v,GameObject unitPrefab, UnitType type)
	{
		Tile tileAt = v.getLocatedAt ();
		GameObject newUnit = Network.Instantiate(unitPrefab, new Vector3(tileAt.point.x, 0.15f, tileAt.point.y), tileAt.transform.rotation, 0) as GameObject;
		Unit u = newUnit.GetComponent<Unit>();

		Tile toplace = null;
		foreach (Tile a in tileAt.getNeighbours()) 
		{
			if(a.prefab == null && a.getOccupyingUnit() == null && a.getColor() == tileAt.getColor())
			{
				toplace = a;
			}
		}
		if(toplace == null)
		{
			toplace = tileAt;
		}
		gameObject.networkView.RPC ("moveUnitPrefabNet",RPCMode.AllBuffered,u.networkView.viewID,new Vector3(toplace.point.x, 0.15f, toplace.point.y));
//		locatedAt = toplace;
		u.networkView.RPC ("setLocationNet", RPCMode.AllBuffered, u.getLocation ().networkView.viewID);
//		myType = unitType;
		u.networkView.RPC ("setUnitTypeNet", RPCMode.AllBuffered, (int)type);
		u.networkView.RPC ("switchUnitPrefabNet", RPCMode.AllBuffered, (int)type);
//		myVillage = v;
		u.networkView.RPC ("setVillageNet", RPCMode.AllBuffered, v.networkView.viewID);
//		myAction = UnitActionType.ReadyForOrders;
		u.networkView.RPC ("setActionNet", RPCMode.AllBuffered, (int)UnitActionType.ReadyForOrders);
		u.getLocation ().networkView.RPC ("setOccupyingUnitNet", RPCMode.AllBuffered, u.networkView.viewID);
		int unitCost = 10 *((int)type+1);
		v.gameObject.networkView.RPC("addGoldNet", RPCMode.AllBuffered, -unitCost);
		v.gameObject.networkView.RPC("addUnitNet", RPCMode.AllBuffered, newUnit.networkView.viewID);
	}

	public void initializeCannon(Village v, GameObject cannonPrefab)
	{
		Tile tileAt = v.getLocatedAt ();
		GameObject newCannon = Network.Instantiate(cannonPrefab, new Vector3(tileAt.point.x, 0.15f, tileAt.point.y), tileAt.transform.rotation, 0) as GameObject;
		Unit u = newCannon.GetComponent<Unit>();

		Tile toplace = null;
		foreach (Tile a in tileAt.getNeighbours()) 
		{
			if(a.prefab == null && a.getOccupyingUnit() == null && a.getColor() == tileAt.getColor())
			{
				toplace = a;
			}
		}
		if(toplace == null)
		{
			toplace = tileAt;
		}
		gameObject.networkView.RPC ("moveUnitPrefabNet",RPCMode.AllBuffered,u.networkView.viewID,new Vector3(toplace.point.x, 0.15f, toplace.point.y));
		u.networkView.RPC ("setLocationNet", RPCMode.AllBuffered, u.getLocation ().networkView.viewID);
		u.networkView.RPC ("setUnitTypeNet", RPCMode.AllBuffered, (int)UnitType.CANNON);
		u.networkView.RPC ("setVillageNet", RPCMode.AllBuffered, v.networkView.viewID);
		u.networkView.RPC ("setActionNet", RPCMode.AllBuffered, (int)UnitActionType.ReadyForOrders);
		u.getLocation ().networkView.RPC ("setOccupyingUnitNet", RPCMode.AllBuffered, u.networkView.viewID);
		v.gameObject.networkView.RPC("addGoldNet", RPCMode.AllBuffered, -35);
		v.gameObject.networkView.RPC("addWoodNet", RPCMode.AllBuffered, -12);
		v.gameObject.networkView.RPC("addUnitNet", RPCMode.AllBuffered, newCannon.networkView.viewID);
	}
	[RPC]
	void upgradeUnitNet(NetworkViewID unitID, int newlvl){
		Unit u = NetworkView.Find (unitID).gameObject.GetComponent<Unit>();
		upgradeUnit (u, (UnitType)newlvl);
	}
	[RPC]
	void destroyUnitNet(NetworkViewID unitID)
	{
		GameObject u = NetworkView.Find (unitID).gameObject;
		Destroy(u);
	}

	public void cultivateMeadow(Unit u)
	{
		u.gameObject.networkView.RPC ("setActionNet", RPCMode.AllBuffered, (int)UnitActionType.StartedCultivating);
	}
	public void buildRoad(Unit u)
	{
		u.gameObject.networkView.RPC ("setActionNet", RPCMode.AllBuffered, (int)UnitActionType.BuildingRoad);
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
			bool hasTower = t.checkTower ();
			Unit u = t.getOccupyingUnit();
			Village v = t.getVillage ();
			Tile l = v.getLocatedAt();
			if (hasTower == true){
//				t.replace (null);
				t.networkView.RPC ("destroyPrefab",RPCMode.AllBuffered);
				t.networkView.RPC ("setStructureNet",RPCMode.AllBuffered,false);
			}
			if (u!=null){
//				v.removeUnit(u);
				v.networkView.RPC ("removeUnitNet",RPCMode.AllBuffered,u.networkView.viewID);
//				t.setOccupyingUnit(null);
				t.networkView.RPC ("removeOccupyingUnitNet",RPCMode.AllBuffered);
//				t.replace (null);
//				t.networkView.RPC ("destroyPrefab",RPCMode.AllBuffered);
//				t.setLandType(LandType.Tombstone);
				t.gameObject.networkView.RPC ("setLandTypeNet",RPCMode.AllBuffered,(int)LandType.Tombstone);
				GameObject tomb = Network.Instantiate (tombPrefab, new Vector3 (t.point.x, 0.4f, t.point.y), tombPrefab.transform.rotation,0) as GameObject;
				t.networkView.RPC ("replaceTilePrefabNet",RPCMode.AllBuffered,tomb.networkView.viewID);
//				Destroy (u.gameObject);
				gameObject.networkView.RPC ("destroyUnitNet",RPCMode.AllBuffered,u.networkView.viewID);
			}
			if (t==l){
				villageManager.takeCannonDamage(v);
			}
		
		}

	}
}
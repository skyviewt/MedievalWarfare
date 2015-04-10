using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class VillageManager : MonoBehaviour {

	public bool isInGame = false;
	public readonly int ZERO = 0;
	public readonly int ONE = 1;
	public readonly int THREE = 3;
	public readonly int EIGHT = 8;
	public readonly int TEN = 10;
	public readonly int TWENTY = 20;
	public readonly int THIRTY = 30;
	public readonly int FOURTY = 40;
	public readonly int NEUTRAL = 2;
	private InGameGUI gameGUI;
	// Use this for initialization
	public GameObject meadowPrefab;
	public GameObject roadPrefab;
	public GameObject treePrefab;
	public GameObject hovelPrefab;
	public GameObject tombPrefab;
	public GameObject towerPrefab;

	private UnitManager unitManager;
	
	void Update () {
		if( isInGame && gameGUI == null )
		{
			Debug.Log ("finding attachingGUI");
			gameGUI = GameObject.Find ("attachingGUI").GetComponent<InGameGUI>();
			unitManager = GameObject.Find ("UnitManager").GetComponent<UnitManager> ();
			Debug.Log (gameGUI);
		}
	}
	
	public void upgradeVillage(Village v)
	{
		int vWood = v.getWood ();
		VillageType vType = v.getMyType ();
		VillageActionType vAction = v.getAction ();
		if (vType == VillageType.Fort)
		{
			gameGUI.displayError(@"The only structure stronger than a Fort is a Castle. ¯\(°_o)/¯\n Press Build Castle to go to the next level!");
		}
		else if ((vType != VillageType.Fort) && (vWood >= 8) && (vAction == VillageActionType.ReadyForOrders)) 
		{
			//v.setAction (VillageActionType.StartedUpgrading);
			v.networkView.RPC ("setVillageActionNet",RPCMode.AllBuffered,(int)VillageActionType.StartedUpgrading);
			//v.addWood(-8);
			v.networkView.RPC("addWoodNet",RPCMode.AllBuffered,-8);
		} 
	}	

	public void MergeAlliedRegions(Tile newTile)
	{
		Village myVillage = newTile.getVillage ();
		List<Tile> neighbours = newTile.getNeighbours();
		//int mySize = myVillage.getRegionSize ();
		Player myPlayer = myVillage.getPlayer ();
		List<Village> villagesToMerge = new List<Village>();
		villagesToMerge.Add (myVillage);
		Village biggestVillage = myVillage;
		//VillageType biggestType = biggestVillage.getMyType ();

		foreach (Tile neighbour in neighbours) 
		{
			Village neighbourVillage = neighbour.getVillage ();
			if( neighbourVillage != null )
			{
				Player neighbourPlayer = neighbourVillage.getPlayer ();
				if((myPlayer == neighbourPlayer) && !(villagesToMerge.Contains(neighbourVillage)))
				{
					villagesToMerge.Add(neighbourVillage);
					VillageType neighbourType = neighbourVillage.getMyType();
					int neighbourSize = neighbourVillage.getRegionSize();
					if (neighbourType>biggestVillage.getMyType())
					{
						biggestVillage = neighbourVillage;
					} 
					else if (neighbourType==biggestVillage.getMyType()&&neighbourSize>biggestVillage.getRegionSize())
					{
						biggestVillage = neighbourVillage;
					}
				}
			}
		}

		foreach (Village village in villagesToMerge) {
			if (village != biggestVillage) {
//				biggestVillage.addGold (village.getGold ());
				biggestVillage.gameObject.networkView.RPC ("addGoldNet",RPCMode.AllBuffered,village.getGold ());
//				biggestVillage.addWood (village.getWood ());
				biggestVillage.gameObject.networkView.RPC ("addWoodNet",RPCMode.AllBuffered,village.getGold ());
				biggestVillage.addRegion(village.getControlledRegion ());
				//foreach (Unit u in village.getControlledUnits ()){
				//	biggestVillage.addUnit (u);
				//}
				// remove prefab
				Tile villageLocation = village.getLocatedAt();
				//Destroy (villageLocation.prefab);
				villageLocation.gameObject.networkView.RPC ("destroyPrefab",RPCMode.AllBuffered);
				//villageLocation.setLandType (LandType.Meadow);
				villageLocation.gameObject.networkView.RPC("setLandTypeNet",RPCMode.AllBuffered,(int)LandType.Meadow);
				GameObject meadow = Network.Instantiate (meadowPrefab, new Vector3 (villageLocation.point.x, 0.1f, villageLocation.point.y), meadowPrefab.transform.rotation,0) as GameObject;
				villageLocation.gameObject.networkView.RPC("replaceTilePrefabNet",RPCMode.AllBuffered,meadow.networkView.viewID);
				//villageLocation.prefab = Instantiate (meadowPrefab, new Vector3 (villageLocation.point.x, 0.1f, villageLocation.point.y), meadowPrefab.transform.rotation) as GameObject;
				//myPlayer.myVillages.Remove (village);
				myPlayer.gameObject.networkView.RPC("removeVillageNet",RPCMode.AllBuffered,village.gameObject.networkView.viewID,myPlayer.getColor ());
				//Destroy (village.gameObject.networkView.viewID);
				gameObject.networkView.RPC ("destroyVillageNet",RPCMode.AllBuffered,village.gameObject.networkView.viewID);
			}
		}
	}
	[RPC]
	void destroyVillageNet(NetworkViewID villageObjectID)
	{
		GameObject vilObject = NetworkView.Find (villageObjectID).gameObject;
		Destroy (vilObject);
	}
	[RPC]
	void DontDestroyVillageManager(NetworkViewID mID)
	{
		DontDestroyOnLoad(NetworkView.Find (mID).gameObject);
	}
	public void plunderVillage (Village pluderingVillage, Village plunderedVillage, Tile dest)
	{
		//determine amount to steal
		int wood = plunderedVillage.getWood ();
		int gold = plunderedVillage.getGold ();

		// remove from enemy village
		plunderedVillage.gameObject.networkView.RPC ("addGoldNet",RPCMode.AllBuffered,-gold);
		plunderedVillage.gameObject.networkView.RPC ("addWoodNet",RPCMode.AllBuffered,-wood);

		// add to yours
		pluderingVillage.gameObject.networkView.RPC ("addGoldNet",RPCMode.AllBuffered,gold);
		pluderingVillage.gameObject.networkView.RPC ("addWoodNet",RPCMode.AllBuffered,wood);

		//Destroy (dest.prefab); // destroy the village, create a meadow
		GameObject meadow = Network.Instantiate(meadowPrefab, new Vector3 (dest.point.x, 0, dest.point.y), meadowPrefab.transform.rotation,0) as GameObject;
		dest.gameObject.networkView.RPC ("replaceTilePrefabNet",RPCMode.AllBuffered,meadowPrefab.networkView.viewID);
		// respawn enemy hovel happens during the split
	}

	/*
	 * Function adds village to invader. If the dest had a village prefab on it, then we take all resources
	 * Already plundered!
	 */ 
	public void takeoverTile(Village invader, Tile dest)
	{
		Village invadedVillage = dest.getVillage ();
//		dest.setVillage (invader);
		dest.gameObject.networkView.RPC ("setVillageNet", RPCMode.AllBuffered, invader.gameObject.networkView.viewID);
//		invader.addTile(dest);
		invader.gameObject.networkView.RPC ("addTileNet", RPCMode.AllBuffered, dest.gameObject.networkView.viewID);
//		invadedVillage.removeTile(dest);
		invadedVillage.gameObject.networkView.RPC ("removeTileNet", RPCMode.AllBuffered, dest.gameObject.networkView.viewID);
		splitRegion(dest, invadedVillage);
	}

	public Tile getTileForRespawn(List<Tile> region){
		System.Random rand = new System.Random();
		List<Tile> validTiles = new List<Tile>();
		int randomTileIndex;
		foreach (Tile t in region) {
			if (t.checkTower () == false){
				validTiles.Add (t);
			}
		}
		if (validTiles.Count <= 0) {
			randomTileIndex = rand.Next (0, region.Count);
			return region[randomTileIndex];
		} else {
			randomTileIndex = rand.Next (0, validTiles.Count);
			return validTiles[randomTileIndex];
		}
	}
	
	private void splitRegion(Tile splitTile, Village villageToSplit)
	{	
		List<List<Tile>> lstRegions = new List<List<Tile>>(); 
		int oldWood = villageToSplit.getWood ();
		int oldGold = villageToSplit.getGold ();
		Tile oldLocation = villageToSplit.getLocatedAt ();
		Player p = villageToSplit.getPlayer();

		// prep for BFS
		foreach (Tile t in villageToSplit.getControlledRegion()) {
			t.setVisited(false);
		}
		// build connected regions
		foreach (Tile t in splitTile.getNeighbours()) {
			if (t.getVillage()==villageToSplit && !t.getVisited()){
				List<Tile> newRegion = new List<Tile>();
				t.setVisited(true);
				newRegion.Add (t);
				splitBFS (t,villageToSplit,newRegion);
				if (newRegion.Count<3){
					Neutralize (newRegion);
				} else{
					lstRegions.Add (newRegion);
				}
			}
		}
		print ("after the bfs");
		/*// working test methods color each new region a different color
		Color[] lstColors = {Color.black, Color.cyan, Color.yellow};
		int i = 0;
		foreach (List<Tile> region in lstRegions){
			Color RandomColor = lstColors[i];
			i++;
			foreach (Tile t in region){
				t.gameObject.renderer.material.color = RandomColor;
			}
		}*/

		if (lstRegions.Count <= 0) {
			//oldLocation.replace (null);

//			oldLocation.setLandType (LandType.Meadow);
			oldLocation.gameObject.networkView.RPC ("setLandTypeNet",RPCMode.AllBuffered,(int)LandType.Meadow);
//			oldLocation.prefab = Instantiate (meadowPrefab, new Vector3 (oldLocation.point.x, 0, oldLocation.point.y), meadowPrefab.transform.rotation) as GameObject;
			GameObject meadow = Network.Instantiate (meadowPrefab, new Vector3 (oldLocation.point.x, 0, oldLocation.point.y), meadowPrefab.transform.rotation,0) as GameObject;
			oldLocation.networkView.RPC ("replaceTilePrefabNet",RPCMode.AllBuffered,meadow.networkView.viewID);

			villageToSplit.retireAllUnits();
			// remove village from player if not already done so
//			p.myVillages.Remove (villageToSplit);
			p.networkView.RPC ("removeVillageNet",RPCMode.AllBuffered,villageToSplit.networkView.viewID);
//			Destroy (villageToSplit.gameObject);
			gameObject.networkView.RPC ("destroyVillageNet",RPCMode.AllBuffered,villageToSplit.networkView.viewID);
//			print ("Village destroyed completely");
			return; //stop here if no region is big enough
		}

		int splitWood = oldWood/lstRegions.Count;
		int splitGold = oldGold/lstRegions.Count;

		//create new villages
		foreach(List<Tile> region in lstRegions)
		{
			print ("creating new village");
			Vector3 hovelLocation;
			Tile tileLocation;

			if (region.Contains (oldLocation)){
				tileLocation = oldLocation;
				hovelLocation = new Vector3(tileLocation.point.x, 0.1f, tileLocation.point.y);		
			} else {
				tileLocation = getTileForRespawn(region);
				hovelLocation = new Vector3(tileLocation.point.x, 0.1f, tileLocation.point.y);
			}

			GameObject newTown = Network.Instantiate(hovelPrefab, hovelLocation, hovelPrefab.transform.rotation, 0) as GameObject;
			Village v = newTown.GetComponent<Village>();
			//tileLocation.replace (newTown);
			tileLocation.networkView.RPC ("replaceTilePrefabNet",RPCMode.AllBuffered,newTown.networkView.viewID);
			v.addRegion (region); //adds T<>V and any U<>V

//			v.setLocation (tileLocation);
			v.gameObject.networkView.RPC ("setLocationNet",RPCMode.AllBuffered,tileLocation.networkView.viewID);
//			p.addVillage(v);
			p.gameObject.networkView.RPC ("addVillageNet",RPCMode.AllBuffered,v.networkView.viewID,p.getColor ());
//			v.setControlledBy(p);
			GameManager GM = GameObject.Find ("preserveGM").GetComponent<GameManager>();
			int playerIndex = GM.findPlayerIndex(p);
			v.gameObject.networkView.RPC ("setControlledByNet",RPCMode.AllBuffered,playerIndex);

			if (region.Contains (oldLocation)){
				VillageType vType = villageToSplit.getMyType();
				//v.setMyType(vType);
				v.gameObject.networkView.RPC ("setVillageTypeNet",RPCMode.AllBuffered,(int)vType);
				if (vType == VillageType.Hovel) 
				{
//					newTown.transform.FindChild("Hovel").gameObject.SetActive (true);
//					newTown.transform.FindChild("Town").gameObject.SetActive (false);
//					newTown.transform.FindChild("Fort").gameObject.SetActive (false);
//					newTown.transform.FindChild("Castle").gameObject.SetActive (false);
					newTown.gameObject.networkView.RPC ("switchPrefabNet",RPCMode.AllBuffered,(int)vType);
				}
				else if (vType == VillageType.Town) 
				{
//					newTown.transform.FindChild("Hovel").gameObject.SetActive (false);
//					newTown.transform.FindChild("Town").gameObject.SetActive (true);
//					newTown.transform.FindChild("Fort").gameObject.SetActive (false);
//					newTown.transform.FindChild("Castle").gameObject.SetActive (false);
					newTown.gameObject.networkView.RPC ("switchPrefabNet",RPCMode.AllBuffered,(int)vType);

				}
				else if (vType == VillageType.Fort) 
				{					
//					newTown.transform.FindChild("Hovel").gameObject.SetActive (false);
//					newTown.transform.FindChild("Town").gameObject.SetActive (false);
//					newTown.transform.FindChild("Fort").gameObject.SetActive (true);
//					newTown.transform.FindChild("Castle").gameObject.SetActive (false);
					newTown.gameObject.networkView.RPC ("switchPrefabNet",RPCMode.AllBuffered,(int)vType);

				}
				else if (vType == VillageType.Castle) 
				{					
//					newTown.transform.FindChild("Hovel").gameObject.SetActive (false);
//					newTown.transform.FindChild("Town").gameObject.SetActive (false);
//					newTown.transform.FindChild("Fort").gameObject.SetActive (false);
//					newTown.transform.FindChild("Castle").gameObject.SetActive (true);
					newTown.gameObject.networkView.RPC ("switchPrefabNet",RPCMode.AllBuffered,(int)vType);
				}
			}

			//v.addGold(splitGold);
			v.gameObject.networkView.RPC ("addGoldNet",RPCMode.AllBuffered,splitGold);
//			v.addWood(splitWood);
			v.gameObject.networkView.RPC ("addWoodNet",RPCMode.AllBuffered,splitWood);

		}
//		villageToSplit.gameObject.transform.Translate (0, 1, 0);
		villageToSplit.networkView.RPC ("transformVillageNet", RPCMode.AllBuffered);
//		Destroy (villageToSplit.gameObject);
		gameObject.networkView.RPC ("destroyVillageNet", RPCMode.AllBuffered, villageToSplit.networkView.viewID);
	}

	// de-color, kill units, destroy structures, etc
	// WORKING
	private void Neutralize (List<Tile> region){
		Village v = region[0].getVillage();
		foreach (Tile t in region) {
			t.gameObject.networkView.RPC("setAndColor", RPCMode.AllBuffered, 0);
//			v.removeTile(t);
			v.gameObject.networkView.RPC ("removeTileNet",RPCMode.AllBuffered,t.gameObject.networkView.viewID);
//			t.setVillage (null);
			t.gameObject.networkView.RPC("neutralizeVillageNet",RPCMode.AllBuffered);
			Unit u = t.getOccupyingUnit ();
			if (u!=null){
//				v.removeUnit(u); // also u.setVillage(null)
				v.gameObject.networkView.RPC ("removeUnitNet",RPCMode.AllBuffered,u.networkView.viewID);
//				Destroy (u.gameObject);
				unitManager.gameObject.networkView.RPC ("destroyUnitNet",RPCMode.AllBuffered,u.networkView.viewID);
//				GameObject tomb = Instantiate (tombPrefab, new Vector3 (t.point.x, 0.4f, t.point.y), tombPrefab.transform.rotation) as GameObject;
				GameObject tomb = Network.Instantiate(tombPrefab, new Vector3 (t.point.x, 0.4f, t.point.y), tombPrefab.transform.rotation,0) as GameObject;
//				t.setLandType(LandType.Tombstone);
				t.gameObject.networkView.RPC ("setLandTypeNet",RPCMode.AllBuffered,(int)LandType.Tombstone);
//				t.replace (tomb);
				t.networkView.RPC ("replaceTilePrefabNet",RPCMode.AllBuffered,tomb.networkView.viewID);
			}
			else if(t.checkTower())
			{
				GameObject tower = Network.Instantiate(towerPrefab, new Vector3 (t.point.x, 0.4f, t.point.y), towerPrefab.transform.rotation,0) as GameObject;
				t.networkView.RPC ("replaceTilePrefabNet",RPCMode.AllBuffered,tower.networkView.viewID);
				t.networkView.RPC ("setStructureNet",RPCMode.AllBuffered,false);
			}
		}
	}

	// takes in a list, and builds it up with connected tiles
	private void splitBFS (Tile tiletoSearch, Village villageToSplit, List<Tile> tilesToReturn){
		foreach (Tile n in tiletoSearch.getNeighbours()){
			if (n.getVillage()==villageToSplit && !n.getVisited()){
				n.setVisited( true );
				tilesToReturn.Add(n);
				splitBFS (n, villageToSplit, tilesToReturn);
			}
		}
	}

	public void hirePeasant(Village v,GameObject unitPrefab)
	{
//		Tile tileAt = v.getLocatedAt ();
		int villageGold = v.getGold ();
		if (villageGold >= 10)
		{
			unitManager.initializeUnit(v,unitPrefab,UnitType.PEASANT);
		} 
		else 
		{
			gameGUI.displayError (@"Wow you're broke, can't even afford a peasant? ¯\(°_o)/¯");
		}

	}

	public void hireInfantry(Village v,GameObject unitPrefab)
	{
//		Tile tileAt = v.getLocatedAt ();
		int villageGold = v.getGold ();
		if (villageGold >= 20) {
//			//Unit p = Unit.CreateComponent (UnitType.INFANTRY, tileAt, v, unitPrefab);
//			GameObject newInfantry = Network.Instantiate(unitPrefab, new Vector3(tileAt.point.x, 0.15f, tileAt.point.y), tileAt.transform.rotation, 0) as GameObject;
//			newInfantry.networkView.RPC("initUnitNet", RPCMode.AllBuffered, (int)UnitType.INFANTRY, tileAt.gameObject.networkView.viewID, v.gameObject.networkView.viewID);
			unitManager.initializeUnit(v,unitPrefab,UnitType.INFANTRY);

			//p.gameObject.transform.FindChild("Peasant").gameObject.SetActive (false);
			//p.gameObject.transform.FindChild("Infantry").gameObject.SetActive (true);
			//p.gameObject.transform.FindChild("Soldier").gameObject.SetActive (false);
			//p.gameObject.transform.FindChild("Knight").gameObject.SetActive (false);
//			newInfantry.networkView.RPC ("setActiveNet", RPCMode.AllBuffered, "Infantry");
//
//			//v.setGold (villageGold - TWENTY);
//			v.gameObject.networkView.RPC("addGoldNet", RPCMode.AllBuffered, -20);
//
//			//v.addUnit (p);
//			v.gameObject.networkView.RPC("addUnitNet", RPCMode.AllBuffered, newInfantry.networkView.viewID);
		} else {
			gameGUI.displayError (@"You do not have enough gold to train infantry. ¯\(°_o)/¯");
		}
	}

	public void hireSoldier(Village v, GameObject unitPrefab)
	{
//		Tile tileAt = v.getLocatedAt ();
		int villageGold = v.getGold ();
		if (villageGold >= 30) {
			if(v.getMyType() >= VillageType.Town)
			{
				unitManager.initializeUnit(v,unitPrefab,UnitType.SOLDIER);

//				//Unit p = Unit.CreateComponent (UnitType.SOLDIER, tileAt, v, unitPrefab);
//				GameObject newSoldier = Network.Instantiate(unitPrefab, new Vector3(tileAt.point.x, 0.15f, tileAt.point.y), tileAt.transform.rotation, 0) as GameObject;
//				newSoldier.networkView.RPC("initUnitNet", RPCMode.AllBuffered, (int)UnitType.SOLDIER, tileAt.gameObject.networkView.viewID, v.gameObject.networkView.viewID);
//
//				//p.gameObject.transform.FindChild("Peasant").gameObject.SetActive (false);
//				//p.gameObject.transform.FindChild("Infantry").gameObject.SetActive (false);
//				//p.gameObject.transform.FindChild("Soldier").gameObject.SetActive (true);
//				//p.gameObject.transform.FindChild("Knight").gameObject.SetActive (false);
//
//				newSoldier.networkView.RPC ("setActiveNet", RPCMode.AllBuffered, "Soldier");
//				//v.setGold (villageGold - THIRTY);
//				v.gameObject.networkView.RPC("addGoldNet", RPCMode.AllBuffered, -30);
//				//v.addUnit (p);
//				v.gameObject.networkView.RPC("addUnitNet", RPCMode.AllBuffered, newSoldier.networkView.viewID);
			}
			else
			{
				gameGUI.displayError(@"Please upgrade your village to a Town first. ¯\(°_o)/¯");
			}
		} else {
			gameGUI.displayError(@"You can't afford a soldier. ¯\(°_o)/¯");
		}
	}
	public void hireKnight(Village v, GameObject unitPrefab)
	{
//		Tile tileAt = v.getLocatedAt ();
		int villageGold = v.getGold ();
		if (villageGold >= 40) {
			if(v.getMyType() == VillageType.Fort)
			{
				unitManager.initializeUnit(v,unitPrefab,UnitType.KNIGHT);

//				//Unit p = Unit.CreateComponent (UnitType.KNIGHT, tileAt, v, unitPrefab);
//				GameObject newKnight = Network.Instantiate(unitPrefab, new Vector3(tileAt.point.x, 0.15f, tileAt.point.y), tileAt.transform.rotation, 0) as GameObject;
//				newKnight.networkView.RPC("initUnitNet", RPCMode.AllBuffered, (int)UnitType.KNIGHT, tileAt.gameObject.networkView.viewID, v.gameObject.networkView.viewID);
//
//				//p.gameObject.transform.FindChild("Peasant").gameObject.SetActive (false);
//				//p.gameObject.transform.FindChild("Infantry").gameObject.SetActive (false);
//				//p.gameObject.transform.FindChild("Soldier").gameObject.SetActive (false);
//				//p.gameObject.transform.FindChild("Knight").gameObject.SetActive (true);
//				newKnight.networkView.RPC ("setActiveNet", RPCMode.AllBuffered, "Knight");
//
//				//v.setGold (villageGold - FOURTY);
//				v.gameObject.networkView.RPC("addGoldNet", RPCMode.AllBuffered, -40);
//
//				//v.addUnit (p);
//				v.gameObject.networkView.RPC("addUnitNet", RPCMode.AllBuffered, newKnight.networkView.viewID);
			}
			else
			{
				gameGUI.displayError (@"Please upgrade your village to a Fort first. ¯\(°_o)/¯");
			}
		} else {
			gameGUI.displayError(@"You don't have enough gold for a knight. ¯\(°_o)/¯");
		}

	}

	public void hireCannon(Village v, GameObject cannonPrefab)
	{
//		Tile tileAt = v.getLocatedAt ();
		int vGold = v.getGold ();
		int vWood = v.getWood ();
		if (vGold >= 35 && vWood>=12) {
			if(v.getMyType() >= VillageType.Fort)
			{
				unitManager.initializeCannon(v,cannonPrefab);
				// initialize the cannon
//				cannon.networkView.RPC("initUnitNet", RPCMode.AllBuffered, (int)UnitType.CANNON, tileAt.gameObject.networkView.viewID, v.gameObject.networkView.viewID);
//				v.gameObject.networkView.RPC("addGoldNet", RPCMode.AllBuffered, -35);
//				v.gameObject.networkView.RPC("addWoodNet", RPCMode.AllBuffered, -12);
//				v.gameObject.networkView.RPC("addUnitNet", RPCMode.AllBuffered, cannon.networkView.viewID);
			}
			else
			{
				gameGUI.displayError(@"Please upgrade your village to a Fort first. ¯\(°_o)/¯");
			}
		} else {
			gameGUI.displayError(@"Cannons cost 35 gold and 12 wood");
		}
	}

	public void buildTower(Village v, Tile t)
	{
		GameObject tower = Network.Instantiate(towerPrefab, new Vector3(t.point.x, 0.1f, t.point.y), Quaternion.identity, 0) as GameObject;
		Structure s = tower.GetComponent<Structure> ();
		s.gameObject.networkView.RPC("initTower",RPCMode.AllBuffered,v.networkView.viewID,t.networkView.viewID );
		t.gameObject.networkView.RPC ("replaceTilePrefabNet", RPCMode.AllBuffered, tower.networkView.viewID);
		t.gameObject.networkView.RPC ("setStructureNet", RPCMode.AllBuffered, true);
		v.gameObject.networkView.RPC ("addWoodNet",RPCMode.AllBuffered,-5);
	}

	public void tombstonePhase (Village v)
	{
		List<Tile> controlledRegion = v.getControlledRegion ();
		foreach (Tile tile in controlledRegion) {
			LandType currentTileType = tile.getLandType ();
			if (currentTileType == LandType.Tombstone && !tile.hasRoad) 
			{
				tile.gameObject.networkView.RPC("setLandTypeNet",RPCMode.AllBuffered, (int)LandType.Trees);
				GameObject tree = Network.Instantiate (treePrefab, new Vector3 (tile.point.x, 0, tile.point.y), treePrefab.transform.rotation, 0) as GameObject;
				tile.gameObject.networkView.RPC("replaceTilePrefabNet",RPCMode.AllBuffered, tree.networkView.viewID);
			}
		}
	}

	public void updateUnitActions (Village v)
	{
		List<Unit> units = v.getControlledUnits();
		foreach (Unit u in units) 
		{
			UnitActionType currentUnitAction = u.getAction();
			if(currentUnitAction == UnitActionType.StartedCultivating)
			{
				u.gameObject.networkView.RPC("setActionNet",RPCMode.AllBuffered,(int)UnitActionType.FinishedCultivating);
			}
			else if (currentUnitAction == UnitActionType.FinishedCultivating)
			{
				Tile tile = u.getLocation();
				tile.networkView.RPC("setLandTypeNet", RPCMode.AllBuffered, (int) LandType.Meadow);
				GameObject meadow = Network.Instantiate(meadowPrefab, new Vector3 (tile.point.x, 0, tile.point.y), meadowPrefab.transform.rotation,0) as GameObject;
				tile.networkView.RPC ("replaceTilePrefabNet", RPCMode.AllBuffered, meadow.networkView.viewID);
				u.gameObject.networkView.RPC("setActionNet",RPCMode.AllBuffered,(int)UnitActionType.ReadyForOrders);
			}
			else if(currentUnitAction == UnitActionType.BuildingRoad)
			{
				Tile tile = u.getLocation();
				tile.networkView.RPC ("setRoadNet", RPCMode.AllBuffered, true);
				u.gameObject.networkView.RPC("setActionNet",RPCMode.AllBuffered,(int)UnitActionType.ReadyForOrders);
			}
			else if(currentUnitAction == UnitActionType.UpgradingCombining)
			{
				u.gameObject.networkView.RPC("setActionNet",RPCMode.AllBuffered,(int)UnitActionType.ReadyForOrders);
			}
			else
			{
				u.gameObject.networkView.RPC("setActionNet",RPCMode.AllBuffered,(int)UnitActionType.ReadyForOrders);
			}

		}
	}

	public void updateVillageActions (Village v)
	{
		VillageActionType action = v.getAction ();
		if (action == VillageActionType.StartedUpgrading) 
		{
			v.gameObject.networkView.RPC ("setVillageActionNet",RPCMode.AllBuffered,(int)VillageActionType.FinishedUpgrading);
		} 
		else if (action == VillageActionType.FinishedUpgrading) 
		{
			v.gameObject.networkView.RPC ("setVillageActionNet",RPCMode.AllBuffered,(int)VillageActionType.ReadyForOrders);
			VillageType vType = v.getMyType();
			if (vType == VillageType.Hovel) 
			{
				v.gameObject.networkView.RPC ("switchPrefabNet",RPCMode.AllBuffered,(int)VillageType.Town);
				v.gameObject.networkView.RPC ("setVillageTypeNet",RPCMode.AllBuffered,(int)VillageType.Town);
				v.gameObject.networkView.RPC ("setHealthNet",RPCMode.AllBuffered,2);
			}
			else if (vType == VillageType.Town) 
			{
				v.gameObject.networkView.RPC ("switchPrefabNet",RPCMode.AllBuffered,(int)VillageType.Fort);
				v.gameObject.networkView.RPC ("setVillageTypeNet",RPCMode.AllBuffered,(int)VillageType.Fort);
				v.gameObject.networkView.RPC ("setHealthNet",RPCMode.AllBuffered,5);
			}
			else if (vType == VillageType.Fort) 
			{
				v.gameObject.networkView.RPC ("switchPrefabNet",RPCMode.AllBuffered,(int)VillageType.Castle);
				v.gameObject.networkView.RPC ("setVillageTypeNet",RPCMode.AllBuffered,(int)VillageType.Castle);
				v.gameObject.networkView.RPC ("setHealthNet",RPCMode.AllBuffered,10);
				v.gameObject.networkView.RPC ("setWageNet",RPCMode.AllBuffered,80);
			}
		}
	}

	public void incomePhase(Village v)
	{
		Debug.Log ("in IncomePhase");
		List<Tile> controlledRegion = v.getControlledRegion ();
		foreach (Tile tile in controlledRegion) 
		{
			LandType currentTileType = tile.getLandType();
			if (currentTileType == LandType.Grass) 
			{
				v.gameObject.networkView.RPC ("addGoldNet",RPCMode.AllBuffered,1);
				
			}
			if (currentTileType == LandType.Meadow) 
			{
				
				v.gameObject.networkView.RPC ("addGoldNet",RPCMode.AllBuffered,2);
			}
		}
	}

	public void paymentPhase (Village v)
	{
		int totalWages = v.getTotalWages ();
		int villageGold = v.getGold ();
		if (villageGold >= totalWages) { //means have enough money to pay units
			villageGold = villageGold - totalWages;
			v.gameObject.networkView.RPC ("setGoldNet",RPCMode.AllBuffered,villageGold);
		} 
		else {
			v.retireAllUnits();
		}
	}


	public void updateVillage(Village v)
	{
		Debug.Log ("in update village net");
		tombstonePhase(v);
		updateUnitActions(v);
		updateVillageActions(v);
		incomePhase(v);
		paymentPhase(v);
	}

	public void buildCastle(Village v)
	{
		int vWood = v.getWood ();
		VillageType vType = v.getMyType ();
		VillageActionType vAction = v.getAction ();
		if (vType != VillageType.Fort) {
			gameGUI.displayError (@"Upgrade to a Fort before building a Castle");
		} else if (vWood < 12) {
			gameGUI.displayError (@"Castles require more lumber (12)");
		} else if (vAction != VillageActionType.ReadyForOrders) {
			gameGUI.displayError (@"You cant queue build orders :/");
		} else {
			v.setAction(VillageActionType.StartedUpgrading);
			v.addWood (-12);
		}
	}	


	public void takeCannonDamage (Village v)
	{
		v.networkView.RPC ("setHealthNet", RPCMode.AllBuffered, v.getHealth () - 1);
		if (v.getHealth () <= 0) {
			v.networkView.RPC ("setGoldNet",RPCMode.AllBuffered,0);
			v.networkView.RPC ("setWoodNet",RPCMode.AllBuffered,0);
			v.networkView.RPC ("switchPrefabNet",RPCMode.AllBuffered,(int)VillageType.Hovel);
			Tile respawnLocation = getTileForRespawn(v.getControlledRegion());
			respawnLocation.networkView.RPC ("replaceTilePrefabNet",RPCMode.AllBuffered,v.gameObject);
			networkView.RPC ("moveVillagePrefabNet",RPCMode.AllBuffered,v.networkView.viewID, new Vector3(respawnLocation.point.x, 0.1f, respawnLocation.point.y));
			v.networkView.RPC ("setLocatedAtNet",RPCMode.AllBuffered,respawnLocation.networkView.viewID);
		}
	}

	[RPC]
	void moveVillagePrefabNet(NetworkViewID villageID, Vector3 vector)
	{
		Village v = NetworkView.Find (villageID).gameObject.GetComponent<Village> ();
		v.transform.localPosition = vector;
	}
			              
}

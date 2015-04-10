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
			gameGUI.displayError(@"The only structure stronger than a Fort is a Castle ¯\(°_o)/¯");
		}
		else if ((vType != VillageType.Fort) && (vWood >= 8) && (vAction == VillageActionType.ReadyForOrders)) 
		{
			v.setAction (VillageActionType.StartedUpgrading);
			v.addWood(-8);
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
				biggestVillage.addGold (village.getGold ());
				biggestVillage.addWood (village.getWood ());
				biggestVillage.addRegion(village.getControlledRegion ());
				//foreach (Unit u in village.getControlledUnits ()){
				//	biggestVillage.addUnit (u);
				//}
				// remove prefab
				Tile villageLocation = village.getLocatedAt();
				Destroy (villageLocation.prefab);
				villageLocation.setLandType (LandType.Meadow);
				villageLocation.prefab = Instantiate (meadowPrefab, new Vector3 (villageLocation.point.x, 0.1f, villageLocation.point.y), meadowPrefab.transform.rotation) as GameObject;

				myPlayer.myVillages.Remove (village);
				Destroy (village.gameObject);

			}
		}
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
		plunderedVillage.addGold(-gold);
		plunderedVillage.addWood(-wood);
		// add to yours
		pluderingVillage.addWood(wood);
		pluderingVillage.addGold(gold);

		//Destroy (dest.prefab); // destroy the village, create a meadow
		dest.prefab = Instantiate (meadowPrefab, new Vector3 (dest.point.x, 0, dest.point.y), meadowPrefab.transform.rotation) as GameObject;
		dest.replace (meadowPrefab);

		// respawn enemy hovel happens during the split
	}

	/*
	 * Function adds village to invader. If the dest had a village prefab on it, then we take all resources
	 * Already plundered!
	 */ 
	public void takeoverTile(Village invader, Tile dest)
	{
		Village invadedVillage = dest.getVillage ();
		dest.setVillage (invader);
		invader.addTile(dest);
		invadedVillage.removeTile(dest);
		splitRegion(dest, invadedVillage);
	}

	public Tile getTileForRespawn(List<Tile> region){
		System.Random rand = new System.Random();
		List<Tile> validTiles = new List<Tile>();
		int randomTileIndex;
		foreach (Tile t in region) {
			if (t.getStructure()!=null){
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


	//TODO needs networking component
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
			Destroy (oldLocation.prefab);
			oldLocation.replace (null);
			oldLocation.setLandType (LandType.Meadow);
			oldLocation.prefab = Instantiate (meadowPrefab, new Vector3 (oldLocation.point.x, 0, oldLocation.point.y), meadowPrefab.transform.rotation) as GameObject;

			villageToSplit.retireAllUnits();
			// remove village from player if not already done so
			p.myVillages.Remove (villageToSplit);
			Destroy (villageToSplit.gameObject);
			print ("Village destroyed completely");
			return; //stop here if no region is big enough
		}

		int splitWood = oldWood/lstRegions.Count;
		int splitGold = oldGold/lstRegions.Count;

		//create new villages
		foreach(List<Tile> region in lstRegions){

			print ("creating new village");
			Vector3 hovelLocation;
			Tile tileLocation;

			if (region.Contains (oldLocation)){
				tileLocation = oldLocation;
				hovelLocation = new Vector3(tileLocation.point.x, 0.1f, tileLocation.point.y);		
			} else {
				tileLocation = getTileForRespawn(region);
				tileLocation.replace (null);
				hovelLocation = new Vector3(tileLocation.point.x, 0.1f, tileLocation.point.y);
			}

			GameObject newTown = Network.Instantiate(hovelPrefab, hovelLocation, hovelPrefab.transform.rotation, 0) as GameObject;
			Village v = newTown.GetComponent<Village>();
			v.addRegion (region); //adds T<>V and any U<>V
			v.setLocation (tileLocation);
			p.addVillage(v);
			v.setControlledBy(p);

			if (region.Contains (oldLocation)){
				VillageType vType = villageToSplit.getMyType();
				v.setMyType(vType);
				if (vType == VillageType.Hovel) 
				{
					newTown.transform.FindChild("Hovel").gameObject.SetActive (true);
					newTown.transform.FindChild("Town").gameObject.SetActive (false);
					newTown.transform.FindChild("Fort").gameObject.SetActive (false);
					newTown.transform.FindChild("Castle").gameObject.SetActive (false);
				}
				else if (vType == VillageType.Town) 
				{
					newTown.transform.FindChild("Hovel").gameObject.SetActive (false);
					newTown.transform.FindChild("Town").gameObject.SetActive (true);
					newTown.transform.FindChild("Fort").gameObject.SetActive (false);
					newTown.transform.FindChild("Castle").gameObject.SetActive (false);
				}
				else if (vType == VillageType.Fort) 
				{					
					newTown.transform.FindChild("Hovel").gameObject.SetActive (false);
					newTown.transform.FindChild("Town").gameObject.SetActive (false);
					newTown.transform.FindChild("Fort").gameObject.SetActive (true);
					newTown.transform.FindChild("Castle").gameObject.SetActive (false);
				}
				else if (vType == VillageType.Castle) 
				{					
					newTown.transform.FindChild("Hovel").gameObject.SetActive (false);
					newTown.transform.FindChild("Town").gameObject.SetActive (false);
					newTown.transform.FindChild("Fort").gameObject.SetActive (false);
					newTown.transform.FindChild("Castle").gameObject.SetActive (true);
				}
			}

			v.addGold(splitGold);
			v.addWood(splitWood);

		}
		villageToSplit.gameObject.transform.Translate (0, 1, 0);
		//villageToSplit.gameObject.renderer.material.color = Color.magenta;
		Destroy (villageToSplit.gameObject);

	}

	// de-color, kill units, destroy structures, etc
	// WORKING
	private void Neutralize (List<Tile> region){
		Village v = region[0].getVillage();
		foreach (Tile t in region) {
			t.gameObject.networkView.RPC("setAndColor", RPCMode.AllBuffered, 2); // TODO hard coded neutral color...
			v.removeTile(t);
			t.setVillage (null);
			Unit u = t.getOccupyingUnit ();
			if (u!=null){
				v.removeUnit(u); // also u.setVillage(null)
				Destroy (u.gameObject);
				GameObject tomb = Instantiate (tombPrefab, new Vector3 (t.point.x, 0.4f, t.point.y), tombPrefab.transform.rotation) as GameObject;
				t.setLandType(LandType.Tombstone);
			}
			t.setStructure(null); // helper method needs to be finished
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

	//TODO needs networking component
	public void removeUnitFromVillage(Village v,Unit u)
	{
		v.removeUnit(u);
	}

	//TODO needs networking component
	public void removeTileFromVillage(Village v, Tile t)
	{
		v.removeTile (t);
		t.setVillage (null);
	}

	public void hirePeasant(Village v,GameObject unitPrefab)
	{
		Tile tileAt = v.getLocatedAt ();
		int villageGold = v.getGold ();
		if (villageGold >= 10) 
		{
			//Unit p = Unit.CreateComponent (UnitType.PEASANT, tileAt, v, unitPrefab);
			GameObject newPeasant = Network.Instantiate(unitPrefab, new Vector3(tileAt.point.x, 0.15f, tileAt.point.y), tileAt.transform.rotation, 0) as GameObject;
			newPeasant.networkView.RPC("initUnitNet", RPCMode.AllBuffered, (int)UnitType.PEASANT, tileAt.gameObject.networkView.viewID, v.gameObject.networkView.viewID);

			//p.gameObject.transform.FindChild("Peasant").gameObject.SetActive (true);
			//p.gameObject.transform.FindChild("Infantry").gameObject.SetActive (false);
			//p.gameObject.transform.FindChild("Soldier").gameObject.SetActive (false);
			//p.gameObject.transform.FindChild("Knight").gameObject.SetActive (false);

			//new method that sets the mesh active:
			newPeasant.networkView.RPC("setActiveNet", RPCMode.All, "Peasant");
			//v.setGold (villageGold - TEN);
			v.gameObject.networkView.RPC("addGoldNet", RPCMode.AllBuffered, -10);
			//v.addUnit (p);
			v.gameObject.networkView.RPC("addUnitNet", RPCMode.AllBuffered, newPeasant.networkView.viewID);
		} else {
			gameGUI.displayError (@"Wow you're broke, can't even afford a peasant? ¯\(°_o)/¯");
		}

	}

	public void hireInfantry(Village v,GameObject unitPrefab)
	{
		Tile tileAt = v.getLocatedAt ();
		int villageGold = v.getGold ();
		if (villageGold >= 20) {
			//Unit p = Unit.CreateComponent (UnitType.INFANTRY, tileAt, v, unitPrefab);
			GameObject newInfantry = Network.Instantiate(unitPrefab, new Vector3(tileAt.point.x, 0.15f, tileAt.point.y), tileAt.transform.rotation, 0) as GameObject;
			newInfantry.networkView.RPC("initUnitNet", RPCMode.AllBuffered, (int)UnitType.INFANTRY, tileAt.gameObject.networkView.viewID, v.gameObject.networkView.viewID);

			//p.gameObject.transform.FindChild("Peasant").gameObject.SetActive (false);
			//p.gameObject.transform.FindChild("Infantry").gameObject.SetActive (true);
			//p.gameObject.transform.FindChild("Soldier").gameObject.SetActive (false);
			//p.gameObject.transform.FindChild("Knight").gameObject.SetActive (false);
			newInfantry.networkView.RPC ("setActiveNet", RPCMode.AllBuffered, "Infantry");

			//v.setGold (villageGold - TWENTY);
			v.gameObject.networkView.RPC("addGoldNet", RPCMode.AllBuffered, -20);

			//v.addUnit (p);
			v.gameObject.networkView.RPC("addUnitNet", RPCMode.AllBuffered, newInfantry.networkView.viewID);
		} else {
			gameGUI.displayError (@"You do not have enough gold to train infantry. ¯\(°_o)/¯");
		}
	}

	public void hireSoldier(Village v, GameObject unitPrefab)
	{
		Tile tileAt = v.getLocatedAt ();
		int villageGold = v.getGold ();
		if (villageGold >= 30) {
			if(v.getMyType() >= VillageType.Town)
			{
				//Unit p = Unit.CreateComponent (UnitType.SOLDIER, tileAt, v, unitPrefab);
				GameObject newSoldier = Network.Instantiate(unitPrefab, new Vector3(tileAt.point.x, 0.15f, tileAt.point.y), tileAt.transform.rotation, 0) as GameObject;
				newSoldier.networkView.RPC("initUnitNet", RPCMode.AllBuffered, (int)UnitType.SOLDIER, tileAt.gameObject.networkView.viewID, v.gameObject.networkView.viewID);

				//p.gameObject.transform.FindChild("Peasant").gameObject.SetActive (false);
				//p.gameObject.transform.FindChild("Infantry").gameObject.SetActive (false);
				//p.gameObject.transform.FindChild("Soldier").gameObject.SetActive (true);
				//p.gameObject.transform.FindChild("Knight").gameObject.SetActive (false);

				newSoldier.networkView.RPC ("setActiveNet", RPCMode.AllBuffered, "Soldier");
				//v.setGold (villageGold - THIRTY);
				v.gameObject.networkView.RPC("addGoldNet", RPCMode.AllBuffered, -30);
				//v.addUnit (p);
				v.gameObject.networkView.RPC("addUnitNet", RPCMode.AllBuffered, newSoldier.networkView.viewID);
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
		Tile tileAt = v.getLocatedAt ();
		int villageGold = v.getGold ();
		if (villageGold >= 40) {
			if(v.getMyType() == VillageType.Fort)
			{
				//Unit p = Unit.CreateComponent (UnitType.KNIGHT, tileAt, v, unitPrefab);
				GameObject newKnight = Network.Instantiate(unitPrefab, new Vector3(tileAt.point.x, 0.15f, tileAt.point.y), tileAt.transform.rotation, 0) as GameObject;
				newKnight.networkView.RPC("initUnitNet", RPCMode.AllBuffered, (int)UnitType.KNIGHT, tileAt.gameObject.networkView.viewID, v.gameObject.networkView.viewID);

				//p.gameObject.transform.FindChild("Peasant").gameObject.SetActive (false);
				//p.gameObject.transform.FindChild("Infantry").gameObject.SetActive (false);
				//p.gameObject.transform.FindChild("Soldier").gameObject.SetActive (false);
				//p.gameObject.transform.FindChild("Knight").gameObject.SetActive (true);
				newKnight.networkView.RPC ("setActiveNet", RPCMode.AllBuffered, "Knight");

				//v.setGold (villageGold - FOURTY);
				v.gameObject.networkView.RPC("addGoldNet", RPCMode.AllBuffered, -40);

				//v.addUnit (p);
				v.gameObject.networkView.RPC("addUnitNet", RPCMode.AllBuffered, newKnight.networkView.viewID);
			}
			else
			{
				gameGUI.displayError (@"Please upgrade your village to a Fort first. ¯\(°_o)/¯");
			}
		} else {
			gameGUI.displayError(@"You don't have enough gold for a knight. ¯\(°_o)/¯");
		}

	}

	//TODO networking good?
	public void buildTower(NetworkViewID village, NetworkViewID tile)
	{
		Village v = NetworkView.Find (village).gameObject.GetComponent<Village>();
		Tile t = NetworkView.Find (tile).gameObject.GetComponent<Tile>();
		GameObject tower = Network.Instantiate(towerPrefab, new Vector3(t.point.x, 0.1f, t.point.y), Quaternion.identity, 0) as GameObject;
		tower.gameObject.networkView.RPC("initTower",RPCMode.AllBuffered,v.networkView.viewID,t.networkView.viewID );
		Structure s = tower.GetComponent<Structure> ();
		t.gameObject.networkView.RPC ("replaceTilePrefabNet", RPCMode.AllBuffered, tower.networkView.viewID);
		t.gameObject.networkView.RPC ("setStructureNet", RPCMode.AllBuffered, s);
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

	public void buildCannon(Village v, GameObject cannonPrefab)
	{
		Tile tileAt = v.getLocatedAt ();
		int vGold = v.getGold ();
		int vWood = v.getWood ();
		if (vGold >= 35 && vWood>=12) {
			if(v.getMyType() >= VillageType.Fort)
			{
				//TODO create cannon prefab, separate from normal units, since it cant upgrade
				GameObject cannon = Network.Instantiate(cannonPrefab, new Vector3(tileAt.point.x, 0.15f, tileAt.point.y), tileAt.transform.rotation, 0) as GameObject;
				// initialize the cannon
				cannon.networkView.RPC("initUnitNet", RPCMode.AllBuffered, (int)UnitType.CANNON, tileAt.gameObject.networkView.viewID, v.gameObject.networkView.viewID);
				v.gameObject.networkView.RPC("addGoldNet", RPCMode.AllBuffered, -35);
				v.gameObject.networkView.RPC("addWoodNet", RPCMode.AllBuffered, -12);
				v.gameObject.networkView.RPC("addUnitNet", RPCMode.AllBuffered, cannon.networkView.viewID);
			}
			else
			{
				gameGUI.displayError(@"Please upgrade your village to a Fort first. ¯\(°_o)/¯");
			}
		} else {
			gameGUI.displayError(@"Cannons cost 35 gold and 12 wood");
		}
	}
}

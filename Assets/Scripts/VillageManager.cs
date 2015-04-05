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
	public GameObject hovelPrefab;
	public GameObject tombPrefab;

	void Update () {
		if( isInGame && gameGUI == null )
		{
			Debug.Log ("finding attachingGUI");
			gameGUI = GameObject.Find ("attachingGUI").GetComponent<InGameGUI>();
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
			gameGUI.displayError(@"The Fort is your strongest village! ¯\(°_o)/¯");
		}
		else if ((vType != VillageType.Fort) && (vWood >= 8) && (vAction == VillageActionType.ReadyForOrders)) 
		{
			v.upgrade ();
		} 
	}	

	[RPC]
	void upgradeVillageNet(NetworkViewID villageID){
		Village v = NetworkView.Find (villageID).gameObject.GetComponent<Village>();
		upgradeVillage (v);
	}
	
	public void MergeAlliedRegions(Tile newTile)
	{
		Village myVillage = newTile.getVillage ();
		List<Tile> neighbours = newTile.getNeighbours();
		int mySize = myVillage.getRegionSize ();
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
				villageLocation.prefab = Instantiate (meadowPrefab, new Vector3 (villageLocation.point.x, 0.2f, villageLocation.point.y), meadowPrefab.transform.rotation) as GameObject;
			}
		}
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

		dest.replace (meadowPrefab); // if a village is invaded, it is supposed to turn into a meadow
		// respawn enemy hovel
		while(plunderedVillage.getLocatedAt() == dest)
		{
			respawnHovel(plunderedVillage);
		}
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

	// network component ?
		// shouldnt need to be networked, this is just a helper function
	private List<Tile> getValidTilesForRespawn(List<Tile> region)
	{
		List<Tile> validTiles = new List<Tile> ();
		foreach (Tile t in region) 
		{
			if(t.getStructure() == null)
			{
				validTiles.Add(t);
			}
		}
		return validTiles;
	}

	//TODO needs networking component
	private void respawnHovel(Village v)
	{
		print ("made it to respawnhovel");
		List<Tile> validTiles = getValidTilesForRespawn (v.getControlledRegion ());
		System.Random rand = new System.Random();
		int randomTileIndex;
		Tile respawnLocation;
		if(validTiles.Count == 0) // all tiles occupied by structures, then "repurpose" one of them
		{
			randomTileIndex = rand.Next (0, v.getRegionSize());
			respawnLocation = validTiles[randomTileIndex];
			respawnLocation.replace (hovelPrefab); // TODO needs to use RPC replace
			// replace destroys the current prefab and sets the new one
			v.setLocation(respawnLocation);
		}
		else
		{
			randomTileIndex = rand.Next (0, validTiles.Count);
			respawnLocation = validTiles[randomTileIndex];
			respawnLocation.replace (hovelPrefab); // TODO needs to use RPC replace
			v.setLocation(respawnLocation);
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
		// working test methods color each new region a different color
		Color[] lstColors = {Color.black, Color.gray, Color.green, Color.magenta};
		int i = 0;
		foreach (List<Tile> region in lstRegions){
			Color RandomColor = lstColors[i];
			i++;
			foreach (Tile t in region){
				t.gameObject.renderer.material.color = RandomColor;
			}
		}

		if (lstRegions.Count <= 0) {
			Destroy (oldLocation.prefab);
			oldLocation.setLandType (LandType.Meadow);
			oldLocation.prefab = Instantiate (meadowPrefab, new Vector3 (oldLocation.point.x, 0.2f, oldLocation.point.y), meadowPrefab.transform.rotation) as GameObject;

			villageToSplit.retireAllUnits();
			// remove village from player if not already done so
			p.myVillages.Remove (villageToSplit);
			print ("Village destroyed completely");
			return; //stop here if no region is big enough
		}

		/*
		//remove old region, cuz its easier to scrap and rebuild....
		//p.myVillages.Remove (villageToSplit);
		foreach (Tile t in villageToSplit.getControlledRegion()) {
			Village v = t.getVillage();
			t.setColor (2); // hardcoded neutral
			t.colorTile (); // replace with setAndColor RPC
			villageToSplit.removeTile(t);
			t.setVillage (null);
			oldLocation.replace(null);
		}

		if (lstRegions.Count == 0) {
			oldLocation.replace (meadowPrefab);
			villageToSplit.retireAllUnits();
			return; //stop here if no region is big enough
		}

		int splitWood = oldWood/lstRegions.Count;
		int splitGold = oldGold/lstRegions.Count;

		// create new villages
		foreach(List<Tile> region in lstRegions){
			print ("creating new village");
			Vector3 hovelLocation = new Vector3(region[0].point.x, 0, region[0].point.y);
			GameObject hovel = Network.Instantiate(hovelPrefab, hovelLocation, hovelPrefab.transform.rotation, 0) as GameObject;
			Village v = hovel.GetComponent<Village>();
			v.addRegion (region); //adds T<>V and any U<>V
			v.setLocation (region[0]);
			p.addVillage(v);

			if (region.Contains (oldLocation)){
				v.getLocatedAt().replace (null);
				v.setLocation(oldLocation);
				v.getLocatedAt().replace (hovelPrefab);
			}
		}*/



	}

	// de-color, kill units, destroy structures, etc
	// WORKING
	private void Neutralize (List<Tile> region){
		Village v = region[0].getVillage();
		foreach (Tile t in region) {
			t.gameObject.networkView.RPC("setAndColor", RPCMode.AllBuffered, 2);	
			v.removeTile(t);
			t.setVillage (null);
			Unit u = t.getOccupyingUnit ();
			if (u!=null){
				v.removeUnit(u); // also u.setVillage(null)
				Destroy (u.gameObject);
				GameObject tomb = Instantiate (tombPrefab, new Vector3 (t.point.x, 1f, t.point.y), tombPrefab.transform.rotation) as GameObject;
				t.setLandType(LandType.Tombstone);
			}
			t.setStructure(false); // method needs to be finished
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
}

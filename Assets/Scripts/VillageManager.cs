using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class VillageManager : MonoBehaviour {

	private InGameGUI gameGUI;
	// Use this for initialization
	public GameObject meadowPrefab;
	public GameObject hovelPrefab;

	void Start () {
		gameGUI = GameObject.Find ("attachingGUI").GetComponent<InGameGUI>();
	}
	
	public void upgradeVillage(Village v)
	{
		int vWood = v.wood;
		VillageType vType = v.myType;
		VillageActionType vAction = v.myAction;
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
		Village myVillage = newTile.myVillage;
		List<Tile> neighbours = newTile.neighbours;
		int mySize = myVillage.controlledRegion.Count();
		Player myPlayer = myVillage.controlledBy;
		List<Village> villagesToMerge = new List<Village>();
		villagesToMerge.Add (myVillage);
		Village biggestVillage = myVillage;
		//VillageType biggestType = biggestVillage.getMyType ();

		foreach (Tile neighbour in neighbours) 
		{
			Village neighbourVillage = neighbour.myVillage;
			if( neighbourVillage != null )
			{
				Player neighbourPlayer = neighbourVillage.controlledBy;
				if((myPlayer == neighbourPlayer) && !(villagesToMerge.Contains(neighbourVillage)))
				{
					villagesToMerge.Add(neighbourVillage);
					VillageType neighbourType = neighbourVillage.myType;
					int neighbourSize = neighbourVillage.controlledRegion.Count();
					if (neighbourType>biggestVillage.myType)
					{
						biggestVillage = neighbourVillage;
					} 
					else if (neighbourType==biggestVillage.myType&&neighbourSize>biggestVillage.controlledRegion.Count())
					{
						biggestVillage = neighbourVillage;
					}
				}
			}
		}

		foreach (Village village in villagesToMerge) {
			if (village != biggestVillage) {
				biggestVillage.gold += village.gold;
				biggestVillage.wood += village.wood;
				biggestVillage.addRegion(village.controlledRegion);
				//foreach (Unit u in village.getControlledUnits ()){
				//	biggestVillage.addUnit (u);
				//}
				// remove prefab
				Tile villageLocation = village.locatedAt;
				Destroy (villageLocation.prefab);
				villageLocation.myType = LandType.Meadow;
				villageLocation.prefab = Instantiate (meadowPrefab, new Vector3 (villageLocation.point.x, 0.2f, villageLocation.point.y), meadowPrefab.transform.rotation) as GameObject;
			}
		}
	}

	public void plunderVillage (Village pluderingVillage, Village plunderedVillage, Tile dest)
	{
		pluderingVillage.wood += plunderedVillage.wood;
		pluderingVillage.gold += plunderedVillage.gold;
		plunderedVillage.wood = 0;
		plunderedVillage.gold = 0;
		dest.replace (meadowPrefab); // if a village is invaded, it is supposed to turn into a meadow
		//TODO this needs to occur AFTER split region finishes
		// tho i think split respawns the hovel anyways
		while(plunderedVillage.locatedAt == dest)
		{
			respawnHovel(plunderedVillage);
		}
	}

	/*
	 * Function adds village to invader. If the dest had a village prefab on it, then we take all resources
	 */ 
	public void takeoverTile(Village invader, Tile dest)
	{
		Village invadedVillage = dest.myVillage;
		invader.addTile(dest);
		invadedVillage.controlledRegion.Remove(dest);
		splitRegion(dest, invadedVillage); //the big sheblamo
	}
	//TODO network component ?
	private List<Tile> getValidTilesForRespawn(List<Tile> region)
	{
		List<Tile> validTiles = new List<Tile> ();
		foreach (Tile t in region) 
		{
			if(t.occupyingStructure == null)
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
		List<Tile> validTiles = getValidTilesForRespawn (v.controlledRegion);
		System.Random rand = new System.Random();
		int randomTileIndex;
		Tile respawnLocation;
		if(validTiles.Count == 0)
		{
			randomTileIndex = rand.Next (0, v.controlledRegion.Count());
			respawnLocation = validTiles[randomTileIndex];
			respawnLocation.replace (hovelPrefab); // TODO needs to use RPC replace
			v.locatedAt = respawnLocation;
			// do we need to set tile's occupying structure? or does village not count?
		}
		else
		{
			randomTileIndex = rand.Next (0, validTiles.Count);
			respawnLocation = validTiles[randomTileIndex];
			respawnLocation.replace (hovelPrefab); // TODO needs to use RPC replace
			v.locatedAt=respawnLocation;
		}
	}

	//TODO needs networking component
	private void splitRegion(Tile splitTile, Village villageToSplit)
	{	
		List<List<Tile>> splitUpRegions = new List<List<Tile>>(); // horrible variable name 8^)
		int oldWood = villageToSplit.wood;
		int oldGold = villageToSplit.gold;
		Tile oldVillageLocation = villageToSplit.locatedAt;
		Player oldPlayer = villageToSplit.controlledBy;
		int oldPlayerColor = oldPlayer.color;
		Dictionary<Tile,bool> visitedDictionary = new Dictionary<Tile,bool> ();
		bool isVisited;
		foreach (Tile x in villageToSplit.controlledRegion) 
		{
			visitedDictionary.Add(x,false);
		}
		foreach (Tile n in splitTile.neighbours) 
		{
			if(visitedDictionary.TryGetValue(n,out isVisited))
			{
				if(n.myVillage == villageToSplit && isVisited == false)
				{
					List<Tile> newRegion = new List<Tile>();
					splitBFS (n,villageToSplit,newRegion,visitedDictionary);
					if(newRegion.Count >= 3)
					{
						splitUpRegions.Add (newRegion);
					}
					else if(newRegion.Count < 3)
					{
						foreach(Tile x in newRegion)
						{
							Unit u = x.occupyingUnit;
							Structure s = x.occupyingStructure;
							//TODO break relationship between tile and unit
							//destroy the unit gameobject
							//remove the tile owner
							//recolor the tile to neutral
							//TODO break relationship between tile and structure
							//destroy the structure gameobject
						}
					}

				}
			}
		}
		//recolor every tile to null/set their village to null
		foreach (Tile old in villageToSplit.controlledRegion) 
		{
			//villageToSplit.removeTile (old);
			old.myVillage = null;
			old.gameObject.networkView.RPC("setAndColor", RPCMode.AllBuffered, 2);	

		}
		oldVillageLocation.replace (null); // destroy the old village prefab
		if (splitUpRegions.Count == 0) 
		{
			return;
		}

		foreach (List<Tile> newRegion in splitUpRegions) 
		{
			if( newRegion.Contains (oldVillageLocation))
			{
				Vector3 hovelLocation = new Vector3(oldVillageLocation.point.x, 0, oldVillageLocation.point.y);
				GameObject hovel = Network.Instantiate(hovelPrefab, hovelLocation, hovelPrefab.transform.rotation, 0) as GameObject;
				Village newVillage = hovel.GetComponent<Village>();
				hovel.networkView.RPC ("setControlledByNet", RPCMode.AllBuffered, oldPlayerColor);
				oldVillageLocation.networkView.RPC ("replaceTilePrefabNet", RPCMode.AllBuffered, hovel.networkView.viewID);
				hovel.networkView.RPC("setLocatedAtNet", RPCMode.AllBuffered, oldVillageLocation.networkView.viewID);
				oldVillageLocation.networkView.RPC("setVillageNet", RPCMode.AllBuffered, hovel.networkView.viewID);
				hovel.networkView.RPC ("updateControlledRegionNet", RPCMode.AllBuffered);
				hovel.networkView.RPC ("setControlledByNet", RPCMode.AllBuffered, gameObject.networkView.viewID, oldPlayerColor);
				hovel.networkView.RPC("addGoldNet", RPCMode.AllBuffered, 200);
				hovel.networkView.RPC("addWoodNet", RPCMode.AllBuffered, 200);
				oldPlayer.gameObject.networkView.RPC ("addVillageNet", RPCMode.AllBuffered, newVillage.networkView.viewID);
			}
			else
			{

			}
		}

		//TODO implement PlayerManager.checkLoss(villageToSplitPlayer)
		//TODO implement PlayerManager.checkWin();
	}

	private void splitBFS (Tile tiletoSearch, Village villageToSplit,List<Tile> tilesToReturn, Dictionary<Tile, bool> visitedDictionary)
	{
		visitedDictionary [tiletoSearch] = true;
		tilesToReturn.Add (tiletoSearch);
		bool isVisited;
		foreach (Tile n in tiletoSearch.neighbours)
		{
			if (visitedDictionary.TryGetValue(n,out isVisited)){
				if(n.myVillage == villageToSplit && isVisited == false)
				{
					splitBFS(n,villageToSplit,tilesToReturn,visitedDictionary);
				}
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
		v.controlledRegion.Remove(t);
		t.myVillage = null;
	}

	public void hirePeasant(Village v,GameObject unitPrefab)
	{
		Tile tileAt = v.locatedAt;
		int villageGold = v.gold;
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
		Tile tileAt = v.locatedAt;
		int villageGold = v.gold;
		if (villageGold >= 20) {
			//Unit p = Unit.CreateComponent (UnitType.INFANTRY, tileAt, v, unitPrefab);
			GameObject newInfantry = Network.Instantiate(unitPrefab, new Vector3(tileAt.point.x, 0.15f, tileAt.point.y), tileAt.transform.rotation, 0) as GameObject;
			newInfantry.networkView.RPC("initUnitNet", RPCMode.AllBuffered, (int)UnitType.INFANTRY, tileAt.gameObject.networkView.viewID, v.gameObject.networkView.viewID);
			//p.gameObject.transform.FindChild("Peasant").gameObject.SetActive (false);
			//p.gameObject.transform.FindChild("Infantry").gameObject.SetActive (true);
			//p.gameObject.transform.FindChild("Soldier").gameObject.SetActive (false);
			//p.gameObject.transform.FindChild("Knight").gameObject.SetActive (false);
			newInfantry.networkView.RPC ("setActiveNet", RPCMode.AllBuffered, "Infantry");
			v.gameObject.networkView.RPC("addGoldNet", RPCMode.AllBuffered, -20);
			//v.addUnit (p);
			v.gameObject.networkView.RPC("addUnitNet", RPCMode.AllBuffered, newInfantry.networkView.viewID);
		} else {
			gameGUI.displayError (@"You do not have enough gold to train infantry. ¯\(°_o)/¯");
		}
	}

	public void hireSoldier(Village v, GameObject unitPrefab)
	{
		Tile tileAt = v.locatedAt;
		int villageGold = v.gold;
		if (villageGold >= 30) {
			if(v.myType >= VillageType.Town)
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
		Tile tileAt = v.locatedAt;
		int villageGold = v.gold;
		if (villageGold >= 40) {
			if(v.myType == VillageType.Fort)
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

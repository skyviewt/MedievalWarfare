using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class VillageManager : MonoBehaviour {
	
	public readonly int ZERO = 0;
	public readonly int ONE = 1;
	public readonly int EIGHT = 8;
	public readonly int TEN = 10;
	public readonly int TWENTY = 20;
	public readonly int THIRTY = 30;
	public readonly int FOURTY = 40;
	private InGameGUI gameGUI;
	// Use this for initialization
	public GameObject meadowPrefab;

	void Start () {
		gameGUI = GameObject.Find ("attachingGUI").GetComponent<InGameGUI>();
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
		Player myPlayer = myVillage.getPlayer ();
		VillageType myVillageType = myVillage.getMyType ();
		List<Village> villagesToMerge = new List<Village>();
		int size = ZERO;
		Village biggestVillage = null;
		VillageType biggestVillageType = VillageType.Hovel;
		foreach (Tile neighbour in neighbours) 
		{
			Village neighbourVillage = neighbour.getVillage ();
			if( neighbourVillage != null )
			{
				Player neighbourPlayer = neighbourVillage.getPlayer ();
				if((myPlayer == neighbourPlayer) && !(villagesToMerge.Contains(neighbourVillage)))
				{
					List<Tile> neighbourControlledRegion = neighbourVillage.getControlledRegion();
					int neighbourSize = neighbourControlledRegion.Count();
					VillageType neighbourVillageType = neighbourVillage.getMyType();
					if(((size < neighbourSize) && (biggestVillageType == neighbourVillageType)) || biggestVillageType < neighbourVillageType)
					{
						size = neighbourSize;
						biggestVillage = neighbourVillage;
						biggestVillageType = neighbourVillageType;
					}
					villagesToMerge.Add(neighbourVillage);
				}

			}
	
		}
		int totalGold = ZERO;
		int totalWood = ZERO;
		List<Tile> totalRegion = new List<Tile> ();
		foreach (Village village in villagesToMerge) 
		{
			if(village != biggestVillage)
			{
				totalGold += village.getGold ();
				totalWood += village.getWood ();
				List<Tile> villageRegion = village.getControlledRegion();
				Tile villageLocation = village.getLocatedAt();
				List<Unit> villageUnits = village.getControlledUnits();
				totalRegion.AddRange(villageRegion);
				foreach(Unit u in villageUnits)
				{
					u.setVillage(biggestVillage);
					biggestVillage.addUnit(u);
				}
				//destroying a village from the game
				//Destroy (//prefab for village);
				Destroy (villageLocation.prefab);
				villageLocation.setLandType (LandType.Meadow);
				//This MeadowPrefab has no networkviewID
				villageLocation.prefab = Instantiate(meadowPrefab, new Vector3(villageLocation.point.x, 0.2f, villageLocation.point.y), meadowPrefab.transform.rotation) as GameObject;

			}
			biggestVillage.addGold(totalGold);
			biggestVillage.addWood(totalWood);
			biggestVillage.addRegion(totalRegion);
		}
	}

	/*
	 * Function adds village to invader. If the dest had a village prefab on it, then we take all resources
	 */ 
	public void takeoverTile(Village invader, Tile dest)
	{
		Village invadedVillage = dest.getVillage ();
		if(dest.checkVillagePrefab())
		{
			int pillagedWood = invadedVillage.getWood ();
			int pillagedGold = invadedVillage.getGold ();
			invader.addWood(pillagedWood);
			invader.addGold(pillagedGold);
			respawnHovel (invadedVillage);
		}
		invader.addTile(dest);
		splitRegion(dest, invadedVillage);
	}

	public void respawnHovel(Village v)
	{

	}

	public void splitRegion(Tile splitTile, Village v)
	{

	}

	public void removeUnitFromVillage(Village v,Unit u)
	{
		v.removeUnit(u);
	}

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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
			gameGUI.displayError("The Fort is your strongest village!");
		}
		else if ((vType != VillageType.Fort) && (vWood >= 8) && (vAction == VillageActionType.ReadyForOrders)) 
		{
			v.upgrade ();
		} 
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
				villageLocation.setLandType (LandType.Meadow);
			}
			biggestVillage.addGold(totalGold);
			biggestVillage.addWood(totalWood);
			biggestVillage.addRegion(totalRegion);
		}
	}
	
	public void takeoverTile(Tile Destination)
	{
		
	}
	
	public void hirePeasant(Village v,GameObject unitPrefab)
	{
		Tile tileAt = v.getLocatedAt ();
		int villageGold = v.getGold ();
		if (villageGold >= 10) 
		{
			Unit p = Unit.CreateComponent (UnitType.PEASANT, tileAt, v, unitPrefab);
			p.gameObject.transform.FindChild("Peasant").gameObject.SetActive (true);
			p.gameObject.transform.FindChild("Infantry").gameObject.SetActive (false);
			p.gameObject.transform.FindChild("Soldier").gameObject.SetActive (false);
			p.gameObject.transform.FindChild("Knight").gameObject.SetActive (false);
			v.setGold (villageGold - TEN);
			v.addUnit (p);
		} else {
			gameGUI.displayError ("Wow you're broke, can't even afford a peasant?");
		}

	}

	public void hireInfantry(Village v,GameObject unitPrefab)
	{
		Tile tileAt = v.getLocatedAt ();
		int villageGold = v.getGold ();
		if (villageGold >= 20) {
			Unit p = Unit.CreateComponent (UnitType.INFANTRY, tileAt, v, unitPrefab);
			p.gameObject.transform.FindChild("Peasant").gameObject.SetActive (false);
			p.gameObject.transform.FindChild("Infantry").gameObject.SetActive (true);
			p.gameObject.transform.FindChild("Soldier").gameObject.SetActive (false);
			p.gameObject.transform.FindChild("Knight").gameObject.SetActive (false);
			v.setGold (villageGold - TWENTY);
			v.addUnit (p);
		} else {
			gameGUI.displayError ("You do not have enough gold to train infantry.");
		}
	}

	public void hireSoldier(Village v, GameObject unitPrefab)
	{
		Tile tileAt = v.getLocatedAt ();
		int villageGold = v.getGold ();
		if (villageGold >= 30) {
			if(v.getMyType() >= VillageType.Town)
			{
				Unit p = Unit.CreateComponent (UnitType.SOLDIER, tileAt, v, unitPrefab);
				p.gameObject.transform.FindChild("Peasant").gameObject.SetActive (false);
				p.gameObject.transform.FindChild("Infantry").gameObject.SetActive (false);
				p.gameObject.transform.FindChild("Soldier").gameObject.SetActive (true);
				p.gameObject.transform.FindChild("Knight").gameObject.SetActive (false);
				v.setGold (villageGold - THIRTY);
				v.addUnit (p);
			}
			else
			{
				gameGUI.displayError("Please upgrade your village to a Town first.");
			}
		} else {
			gameGUI.displayError("You can't afford a soldier.");
		}
	}
	public void hireKnight(Village v, GameObject unitPrefab)
	{
		Tile tileAt = v.getLocatedAt ();
		int villageGold = v.getGold ();
		if (villageGold >= 40) {
			if(v.getMyType() == VillageType.Fort)
			{
				Unit p = Unit.CreateComponent (UnitType.KNIGHT, tileAt, v, unitPrefab);
				p.gameObject.transform.FindChild("Peasant").gameObject.SetActive (false);
				p.gameObject.transform.FindChild("Infantry").gameObject.SetActive (false);
				p.gameObject.transform.FindChild("Soldier").gameObject.SetActive (false);
				p.gameObject.transform.FindChild("Knight").gameObject.SetActive (true);
				v.setGold (villageGold - FOURTY);
				v.addUnit (p);
			}
			else
			{
				gameGUI.displayError ("Please upgrade your village to a Fort first.");
			}
		} else {
			gameGUI.displayError("You don't have enough gold for a knight");
		}

	}
}

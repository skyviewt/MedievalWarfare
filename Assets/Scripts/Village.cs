using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum VillageActionType
{
	ReadyForOrders,
	BuildStageOne
};

public enum VillageType
{
	Hovel,
	Town,
	Fort
}


public class Village : MonoBehaviour {

	private List<Tile> controlledRegion;
	private Player controlledBy;
	private Tile locatedAt;
	private List<Unit> supportedUnits;
	private VillageType myType;
	private VillageActionType myAction;
	private int gold;
	private int wood;



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//constructor
	public static Village CreateComponent ( Player p, List<Tile> regions, Tile locatedAt, GameObject locationPrefab, GameObject g ) 
	{
		Village myVillage = g.AddComponent<Village>();
		myVillage.controlledRegion = regions;
		myVillage.controlledBy = p;
		myVillage.myType = VillageType.Hovel;
		myVillage.supportedUnits = new List<Unit> ();
		locatedAt.replace (locationPrefab);
		myVillage.locatedAt = locatedAt;

		myVillage.myAction = VillageActionType.ReadyForOrders;
		myVillage.gold = 0;
		myVillage.wood = 0;

		return myVillage;
	}
	
	//setters and getters
	public void setGold(int goldValue)
	{
		gold = goldValue;
	}

	public int getGold(){
		return gold;
	}
	public void setWood(int woodValue)
	{
		wood = woodValue;
	}
	
	public int getWood(){
		return wood;
	}

	public VillageType getMyType()
	{
		return myType;
	}

	public VillageActionType getAction()
	{
		return this.myAction;
	}

	public Player getPlayer()
	{
		return controlledBy;
	}

	public List<Tile> getControlledRegion()
	{
		return controlledRegion;
	}

	public List<Unit> getControlledUnits()
	{
		return supportedUnits;
	}

	public void setLocation(Tile t)
	{
		locatedAt = t;
	}

	public Tile getLocatedAt()
	{
		return locatedAt;
	}


	//increment / decrement

	public void addGold(int i)
	{
		gold += i;
	}
	public void removeGold(int i)
	{
		gold -= i;
	}
	public void addWood(int i)
	{
		wood += i;
	}

	public void removeWood(int i)
	{
		wood -= i;
	}


	public void addUnit(Unit u)
	{
		supportedUnits.Add (u);
		u.setVillage (this);
		//the following 2 lines may not be needed.
		Tile t = u.getLocation ();
		t.setOccupyingUnit (u);
	}
	//needs unit's setters and getters along with the Tombstone Landtype
	/*
	public void removeUnit(Unit u)
	{
		Tile unitLocation = u.getLocation ();
		unitLocation.setOccupyingUnit (null);
		u.setLocation (null);
		u.setVillage(null);
		unitLocation.setLandType(LandType.Tombstone);
		supportedUnits.Remove(u);
	}
*/

	//Needs setVillage in Tile. Remove comment once setVillage is implemented
	public void addTile(Tile t)
	{
		controlledRegion.Add (t);
		t.setVillage (this);
	}

	public void removeTile(Tile t)
	{
		controlledRegion.Remove (t);
	}
	
	public void addRegion(List<Tile> regions)
	{	
		//doing exactly what the gameManager.addregion(List<Tile>, Village village) is doing
		foreach (Tile t in regions) {
			t.setVillage(this);
			controlledRegion.Add(t);

			//if there is a unit on the tile
			Unit u = t.getOccupyingUnit();
			if(u != null){
				u.setVillage(this);
				supportedUnits.Add(u);
			}
		}
	}

	//needs getWage in Units

	public int getTotalWages()
	{
		int totalWage = 0;
		foreach (Unit u in supportedUnits) {
			int tempWage = u.getWage();
			totalWage += tempWage;
		}
		return totalWage;
	}


	//needs setLocation and setVillage and setOccupyingUnit in Tile

	public void retireAllUnits()
	{
		foreach (Unit u in supportedUnits) {
			Tile unitLocation = u.getLocation();
			unitLocation.setOccupyingUnit(null);
			unitLocation.setLandType(LandType.TombStone);
			u.setLocation(null);
			u.setVillage(null);
			supportedUnits.Remove(u);
		}
	}


	public void upgrade()
	{
		//assuming gamemanager (or any manager) is doing all the heavy lifting in upgradeVillage
		//only increments the villageType
		myType += 1;

	}
	//sets gold to 0 and returns the previous gold value
	public int pillageGold()
	{
		int previousGoldValue = gold;
		gold = 0;
		return previousGoldValue;
	}
	//sets wood to 0 and returns the previous wood value
	public int pillagewood()
	{
		int previousWoodValue = wood;
		wood = 0;
		return previousWoodValue;
	}

	public void addTile(Tile t)
	{
		this.controlledRegion.Add(t);
	}
}

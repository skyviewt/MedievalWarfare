using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum UnitType
{
	PEASANT,
	INFANTRY,
	SOLDIER,
	KNIGHT
};

public enum ActionType
{
	ReadyForOrders,
	Moved,
	BuildingRoad, 
	ChoppingTree,
	ClearingTombstone, 
	UpgradingCombining, 
	StartCultivating, 
	FinishCultivating
};

public class Unit : MonoBehaviour {
	
	public GameObject unitPrefab;
	public Tile locatedAt;
	public ActionType myAction;
	public UnitType myType;
	private Village myVillage;
	private readonly int TWO = 2;
	private readonly int SIX = 6;
	private readonly int EIGHTEEN = 18;
	private readonly int FIFTY_FOUR = 54;
	
	
	//constructor
	public static Unit CreateComponent ( UnitType unitType, Tile location, Village v, GameObject prefab, GameObject g ) 
	{
		Unit theUnit = g.AddComponent<Unit>();
		theUnit.locatedAt = location;
		theUnit.myType = unitType;
		theUnit.myVillage = v;
		theUnit.myAction = ActionType.ReadyForOrders;
		
		theUnit.unitPrefab = Instantiate(prefab) as GameObject;
		
		return theUnit;
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void setVillage(Village v)
	{
		this.myVillage = v;
	}
	
	public Tile getLocation()
	{
		return this.locatedAt;
	}

	public void setLocation(Tile t)
	{
		this.locatedAt = t;
	}
	
	public void setAction(ActionType action)
	{
		this.myAction = action;
	}
	
	public ActionType getAction()
	{
		return this.myAction;
	}
	public int getWage()
	{
		if(this.myType == UnitType.PEASANT)
		{
			return TWO;
		}
		else if(this.myType == UnitType.INFANTRY)
		{
			return SIX;
		}
		else if(this.myType == UnitType.SOLDIER)
		{
			return EIGHTEEN;
		}
		else
		{
			return FIFTY_FOUR;
		}
	}
}

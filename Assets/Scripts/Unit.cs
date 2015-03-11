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
	FinishCultivating,
	CapturingNeutral,
	CapturingEnemy
	
};

public class Unit : MonoBehaviour {

	public Tile locatedAt;
	public ActionType myAction;
	public UnitType myType;
	private Village myVillage;
	private readonly int TWO = 2;
	private readonly int SIX = 6;
	private readonly int EIGHTEEN = 18;
	private readonly int FIFTY_FOUR = 54;

	private Shader outline;

	//constructor
	public static Unit CreateComponent ( UnitType unitType, Tile location, Village v, GameObject PeasantPrefab ) 
	{
		Tile toplace = null;
		foreach (Tile a in location.neighbours) 
		{
			if(a.prefab == null && a.getOccupyingUnit() == null && a.getColor() == location.getColor())
			{
				toplace = a;
			}
		}
		if(toplace == null)
		{
			toplace = location;
		}
		GameObject o = Instantiate(PeasantPrefab, new Vector3(toplace.point.x, 0.15f, toplace.point.y), toplace.transform.rotation) as GameObject;
		Unit theUnit = o.AddComponent<Unit>();
		theUnit.locatedAt = location;

		theUnit.myType = unitType;
		theUnit.myVillage = v;
		theUnit.myAction = ActionType.ReadyForOrders;

		location.setOccupyingUnit (theUnit);
		return theUnit;
	}
	// Use this for initialization
	void Start () 
	{
		outline = Shader.Find("Glow");
	}

	void OnMouseEnter()
	{
		this.renderer.material.shader = outline;
	}

	public void movePrefab(Vector3 vector)
	{
		this.transform.localPosition = vector;
	}

	void OnMouseExit()
	{
		this.renderer.material.shader = Shader.Find("Diffuse");
	}

	// Update is called once per frame
	void Update () 
	{
		
	}
	
	public void setVillage(Village v)
	{
		this.myVillage = v;
	}
	
	public Village getVillage()
	{
		return this.myVillage;
	}
	
	public Tile getLocation()
	{
		return this.locatedAt;
	}
	
	public UnitType getUnitType()
	{
		return this.myType;
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

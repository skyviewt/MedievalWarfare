using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public enum UnitType
{
	PEASANT,
	INFANTRY,
	SOLDIER,
	KNIGHT,
	CANNON
};

[System.Serializable]
public enum UnitActionType
{
	ReadyForOrders,
	Moved,
	CannonMoved,
	BuildingRoad, 
	ChoppingTree,
	ClearingTombstone, 
	UpgradingCombining, 
	StartedCultivating, 
	FinishedCultivating,
	CapturingNeutral,
	CapturingEnemy,
};

[System.Serializable]
public class Unit : MonoBehaviour {

	private Tile locatedAt;
	public UnitActionType myAction;
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
		foreach (Tile a in location.getNeighbours()) 
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
		theUnit.locatedAt = toplace;

		theUnit.myType = unitType;
		theUnit.myVillage = v;
		theUnit.myAction = UnitActionType.ReadyForOrders;

		location.setOccupyingUnit (theUnit);
		return theUnit;
	}

	public Unit(){
	}

//	[RPC]
//	void setActiveNet(string unitClass){
//
//		gameObject.transform.FindChild ("Peasant").gameObject.SetActive (false);
//		gameObject.transform.FindChild ("Infantry").gameObject.SetActive (false);
//		gameObject.transform.FindChild ("Soldier").gameObject.SetActive (false);
//		gameObject.transform.FindChild ("Knight").gameObject.SetActive (false);
//
//		gameObject.transform.FindChild (unitClass).gameObject.SetActive (true);
//
//		if (!(unitClass == "Peasant" || unitClass == "Infantry" || unitClass == "Soldier" || unitClass == "Knight")) {
//			Debug.Log("Invalid Unit.SetActive() parameter: " + unitClass);
//		}
//	}

	[RPC]
	void changeMapLayer( int mapNum )
	{
		switch (mapNum) {
		case 0:
			this.gameObject.layer = LayerMask.NameToLayer ("map1");
			break;
		case 1:
			this.gameObject.layer = LayerMask.NameToLayer ("map2");
			break;
			//TODO: do this for units and villages.
		case 2:
			this.gameObject.layer = LayerMask.NameToLayer("loadedMap");
			this.gameObject.tag = "LoadedMap";			
			break;
		}
	}


	[RPC]
	void initUnitNet(int unitTypeID, NetworkViewID locationTileID, NetworkViewID villageID){
		//Getting all the parameters
		UnitType unitType = (UnitType)unitTypeID;
		Tile location = NetworkView.Find (locationTileID).gameObject.GetComponent<Tile>();
		Village v = NetworkView.Find (villageID).gameObject.GetComponent<Village>();

		//CreateComponent
		Tile toplace = null;
		foreach (Tile a in location.getNeighbours()) 
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
		//BE CAREFUL!!! If the order of Tiles in neighbors are not the same, the position of the new unit will be different!!
		gameObject.transform.position = new Vector3(toplace.point.x, 0.15f, toplace.point.y);

		locatedAt = toplace;
		myType = unitType;
		myVillage = v;
		myAction = UnitActionType.ReadyForOrders;
		locatedAt.setOccupyingUnit (this);
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

	void OnMouseExit()
	{
		this.renderer.material.shader = Shader.Find("Diffuse");
	}

	// Update is called once per frame
	void Update () 
	{
		
	}
	[RPC]
	void setVillageNet(NetworkViewID villageID)
	{
		this.myVillage = NetworkView.Find (villageID).gameObject.GetComponent<Village> ();
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

	[RPC]
	void setLocationNet(NetworkViewID tileID)
	{
		Tile t = NetworkView.Find (tileID).gameObject.GetComponent<Tile> ();
		this.locatedAt = t;
	}
	[RPC]
	void setActionNet(int action)
	{
		this.myAction = (UnitActionType)action;
	}

//	public void setAction(UnitActionType action)
//	{
//		this.myAction = action;
//	}
	[RPC]
	void setUnitTypeNet(int type)
	{
		myType = (UnitType)type;
	}
	[RPC]
	void switchUnitPrefabNet(int pType)
	{
		UnitType newType = (UnitType)pType;
		if (newType == UnitType.PEASANT) 
		{
			this.transform.FindChild("Peasant").gameObject.SetActive (true);
			this.transform.FindChild("Infantry").gameObject.SetActive (false);
			this.transform.FindChild("Soldier").gameObject.SetActive (false);
			this.transform.FindChild("Knight").gameObject.SetActive (false);
		}
		else if (newType == UnitType.INFANTRY) 
		{
			this.transform.FindChild("Peasant").gameObject.SetActive (false);
			this.transform.FindChild("Infantry").gameObject.SetActive (true);
			this.transform.FindChild("Soldier").gameObject.SetActive (false);
			this.transform.FindChild("Knight").gameObject.SetActive (false);
		}
		else if (newType == UnitType.SOLDIER) 
		{
			this.transform.FindChild("Peasant").gameObject.SetActive (false);
			this.transform.FindChild("Infantry").gameObject.SetActive (false);
			this.transform.FindChild("Soldier").gameObject.SetActive (true);
			this.transform.FindChild("Knight").gameObject.SetActive (false);
		}
		else if (newType == UnitType.KNIGHT) 
		{
			this.transform.FindChild("Peasant").gameObject.SetActive (false);
			this.transform.FindChild("Infantry").gameObject.SetActive (false);
			this.transform.FindChild("Soldier").gameObject.SetActive (false);
			this.transform.FindChild("Knight").gameObject.SetActive (true);
		}
	}

		[RPC]
		void DontDestroy(NetworkViewID tileID){
			DontDestroyOnLoad(NetworkView.Find (tileID).gameObject);
		}

	public UnitActionType getAction()
	{
		return this.myAction;
	}

	public int getWage()
	{
		switch (myType) {
		case UnitType.PEASANT:
			return 2;
		case UnitType.INFANTRY:
			return 6;
		case UnitType.SOLDIER:
			return 18;
		case UnitType.KNIGHT:
			return 54;
		case UnitType.CANNON:
			return 5;
		default:
			return 0;
		}
	}

//	public void upgrade(UnitType newLevel)
//	{
//		this.myType = newLevel;
//		//this.myAction = UnitActionType.UpgradingCombining;
//
//		switch (myType) {
//		case UnitType.INFANTRY:
//			this.transform.FindChild("Peasant").gameObject.SetActive (false);
//			this.transform.FindChild("Infantry").gameObject.SetActive (true);
//			this.transform.FindChild("Soldier").gameObject.SetActive (false);
//			this.transform.FindChild("Knight").gameObject.SetActive (false);
//			break;
//		case UnitType.SOLDIER:
//			this.transform.FindChild("Peasant").gameObject.SetActive (false);
//			this.transform.FindChild("Infantry").gameObject.SetActive (false);
//			this.transform.FindChild("Soldier").gameObject.SetActive (true);
//			this.transform.FindChild("Knight").gameObject.SetActive (false);
//			break;
//		case UnitType.KNIGHT:
//			this.transform.FindChild("Peasant").gameObject.SetActive (false);
//			this.transform.FindChild("Infantry").gameObject.SetActive (false);
//			this.transform.FindChild("Soldier").gameObject.SetActive (false);
//			this.transform.FindChild("Knight").gameObject.SetActive (true);
//			break;
//		}
//	}

}

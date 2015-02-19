using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum UnitType
{
	Peasant,
	Infantry,
	Soldier,
	Knight
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



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

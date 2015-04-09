using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Structure : MonoBehaviour {

	private UnitType myType = UnitType.INFANTRY;
	public GameObject structurePrefab;
	public Village myVillage;
	public Tile locatedAt;
}

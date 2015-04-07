using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Structure : MonoBehaviour {

	private UnitType myType = UnitType.SOLDIER;
	public GameObject structurePrefab;
	// private Tile locatedAt is the relation to its parent Tile
	// this is not kept in script, but object hierarchy

}

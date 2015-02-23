using UnityEngine;
using System.Collections;

public enum UnitType
{
	Peasant,
	Infantry,
	Soldier,
	Knight
};

public class Structure : MonoBehaviour {

	private UnitType myType;
	// private Tile locatedAt is the relation to its parent Tile
	// this is not kept in script, but object hierarchy

	// awake called on script instantiation
	void Awake () {
		myType = UnitType.Soldier; //assuming tower is of soldier level
	}
}

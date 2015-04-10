using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Structure : MonoBehaviour {

	private UnitType myType = UnitType.INFANTRY;
	public Village myVillage;
	public Tile locatedAt;

	[RPC]
	public void initTower(NetworkViewID village, NetworkViewID tile){
		transform.localScale = new Vector3 (0.03f, 0.03f, 0.03f);
		transform.eulerAngles = new Vector3 (-90, 0, 0);
		Village v = NetworkView.Find (village).GetComponent<Village>();
		Tile t = NetworkView.Find (tile).GetComponent<Tile>();
		myVillage = v;
		locatedAt = t;
	}

}

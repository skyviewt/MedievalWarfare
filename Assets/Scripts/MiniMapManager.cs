using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MiniMapManager : MonoBehaviour {

	public GameObject miniPrefab;

	public bool isMap1;

	void Start(){

		GameObject mini = Instantiate (miniPrefab) as GameObject;
		mini.transform.parent = transform;
		MapGenerator m1 = mini.GetComponent<MapGenerator> ();
		m1.minimap = true;
		m1.isMap1 = isMap1;
		m1.initMap ();

	} 
}

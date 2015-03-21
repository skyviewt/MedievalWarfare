using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MiniMapManager : MonoBehaviour {

	public List<GameObject> minimaps;
	public GameObject miniPrefab;

	void Start(){

		for (int i=0; i<5; i++) {
			GameObject mini = Instantiate (miniPrefab) as GameObject;
			mini.transform.parent = transform;
			minimaps.Add (mini);
			MapGenerator m1 = mini.GetComponent<MapGenerator> ();
			m1.minimap = true;
			m1.initMap ();

			mini.SetActive(false);
		}

	} 



}

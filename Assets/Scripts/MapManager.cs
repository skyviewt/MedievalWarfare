using UnityEngine;
using System.Collections;

public class MapManager : MonoBehaviour {

	private System.Random rand = new System.Random();

	// Use this for initialization
	void Start () {

		initializeMap ();
	}

	void initializeMap ()
	{
		MapGenerator gen = new MapGenerator ();
		Graph map = gen.getMap ();
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

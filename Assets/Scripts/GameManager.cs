using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {

	public GameObject pp;

	// Use this for initialization
	void Start () 
	{
		Player p1 = Player.CreateComponent ("Sky", "123", gameObject);
		Player p2 = Player.CreateComponent ("Joerg", "456", gameObject);
		List<Player> participants = new List<Player>();
		participants.Add (p1);
		participants.Add (p2);

		MapGenerator gen = gameObject.GetComponent<MapGenerator> ();

		gen.initializeVillagesOnMap (participants);

	}

	// Update is called once per frame
	void Update () {
	
	}
}

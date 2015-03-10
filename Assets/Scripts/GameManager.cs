using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {

	public GameObject pp;

	public string ipAddress;
	public int port = 25000;
	public bool isServer = true;

	// Use this for initialization
	void Start () 
	{
		if (isServer) {

			Network.InitializeServer(32, port);

			Player p1 = Player.CreateComponent ("Sky", "123", gameObject);
			Player p2 = Player.CreateComponent ("Joerg", "456", gameObject);
			List<Player> participants = new List<Player> ();
			participants.Add (p1);
			participants.Add (p2);

			MapGenerator gen = gameObject.GetComponent<MapGenerator> ();

			gen.initializeVillagesOnMap (participants);
		} else {
			Network.Connect(ipAddress, port);
		}

	}

	// Update is called once per frame
	void Update () {
	
	}
}

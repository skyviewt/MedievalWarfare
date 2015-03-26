using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class GameManager : MonoBehaviour {

	public string ipAddress;
	public int port = 25000;
	public bool isServer = true;

	// Use this for initialization
	void Start () 
	{
		this.initGame (ipAddress, port);
	}
	public void initGame(string ip, int pPort)
	{
		print ("in initGame");
		if (isServer) {

			Network.InitializeServer (32, port);
			print ("in isServer----"); 
			Player p1 = Player.CreateComponent ("Sky", "123", gameObject);
			p1.setColor(0);
			Player p2 = Player.CreateComponent ("Joerg", "456", gameObject);
			p2.setColor(1);

//			gameObject.networkView.RPC ("initPlayers", RPCMode.AllBuffered);

//			Player[] pls = gameObject.GetComponents<Player>();
			List<Player> participants = new List<Player> ();
//			participants.Add (pls[0]);
//			participants.Add (pls[1]);

			participants.Add (p1);
			participants.Add (p2);

			MapGenerator gen = gameObject.GetComponent<MapGenerator> ();
			for ( int i = 0; i<participants.Count; i++)
			{
				gen.initMap (i);
//				gen.initializeVillagesOnMap (participants,i);
				print ("the i: "+i);
			}
			Debug.Log ("-------------here for maps---------------");
			for(int i = 0; i<gen.maps.Count; ++i)
			{
			
				Debug.Log (gen.getMap(i));
			}
			Debug.Log ("-----------------------------------------");

		} else {
			Network.Connect (ip, pPort);
		}
	}
	public void setIsServer(bool b)
	{
		this.isServer = b;
	}
	public bool getIsServer()
	{
		return this.isServer;
	}

	public void setIpAddress(string ip)
	{
		this.ipAddress = ip;
	}
	public string getIpAddress()
	{
		return this.ipAddress;
	}
	public void setPort(int pPort)
	{
		this.port = pPort;
	}
	public int getPort()
	{
		return this.port;
	}


	[RPC]
	void initPlayers(){
		Player[] pls = gameObject.GetComponents<Player>();
		pls [0].initPlayer ("P1", "Pass");
		pls[0].setColor(0);
		pls [1].initPlayer ("P2", "Pass");
		pls [1].setColor (1);

	}

	// Update is called once per frame
	void Update () {
	
	}
}

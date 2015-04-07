using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class GameManager : MonoBehaviour {

	public string ipAddress;
	public int port = 25000;
	public bool isServer = true;
	public List<Player> players = new List<Player>();

	public int finalMapChoice = -1;

	public MapGenerator MapGen;

	public Game theGame;
	public Graph finalMap = null;
	
	// Use this for initialization
	void Start () 
	{
	}

	public NetworkConnectionError initGame(string ip, int pPort)
	{
		print ("in initGame");
		if (isServer) {

			Network.InitializeServer (32, port);
			print ("in isServer----"); 


			MapGen = gameObject.GetComponent<MapGenerator> ();

			for ( int i = 0; i<2; i++)
			{
				MapGen.initMap (i);
			}
			return NetworkConnectionError.NoError;
		} else {
			return Network.Connect (ip, pPort);
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
	
	public void addPlayer(Player p)
	{
		this.players.Add (p);
	}

	[RPC]
	public void addPlayerNet(string name, string pass, int color, int loss, int win)
	{
		bool isExist = (this.players.Where(player => ((player.getName() == name) && (player.getPassword() == pass) )).Count() > 0);
		if (!isExist) 
		{
			Player p = Player.CreateComponent (name, pass, win, loss, color, gameObject);
			this.players.Add (p);	
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}

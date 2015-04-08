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
	public Graph finalMap = null;

	public int finalMapChoice = -1;
	public MapGenerator mapGen;

	public Game game;

	private Player localPlayer; // has getter
	private int localTurn; //has getter and setter
	private VillageManager villageManager;

	// Use this for initialization
	void Start () 
	{
		villageManager = GameObject.Find ("VillageManager").GetComponent<VillageManager> ();
	}

	public NetworkConnectionError initGame(string ip, int pPort)
	{
		print ("in initGame");
		if (isServer) {

			Network.InitializeServer (32, port);
			print ("in isServer----"); 


			mapGen = gameObject.GetComponent<MapGenerator> ();

			for ( int i = 0; i<2; i++)
			{
				mapGen.initMap (i);
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

	public List<Player> getPlayers()
	{
		return this.players;
	}

	public void InitializeFinalMap ()
	{
		this.finalMap = mapGen.getMap(finalMapChoice);
		mapGen.initializeColorAndVillagesOnMap(players, finalMapChoice, this.finalMap); // this needs to be RPC
		mapGen.gameObject.networkView.RPC("perserveFinalMap", RPCMode.AllBuffered, finalMapChoice);
	}


	public void createNewGame ()
	{
		game = Game.CreateComponent (this.players,this.finalMap,this.gameObject); // this needs to be RPC
	}

	private void beginNextTurn()
	{
		Player p = game.getCurrentPlayer ();
		List<Village> villagesToUpdate = p.getVillages ();
		foreach (Village v in villagesToUpdate)
		{
			villageManager.updateVillages(v);
		}
	}
	
	public int getLocalTurn()
	{
		return this.localTurn;
	}

	//TODO RPC
	public void setLocalTurn(int turnNumber)
	{
		this.localTurn = turnNumber;
	}

	//TODO networking
	public void setNextPlayerInTurnOrder()
	{
		int currentTurn = game.getCurrentTurn();
		int numberOfPlayers = game.getPlayers().Count;
		List<PlayerStatus> playerStatuses = game.getPlayerStatuses();

		for(int i = 0; i < numberOfPlayers; i++)
		{
			int nextPlayerTurn = (currentTurn + i) % numberOfPlayers;
			if(playerStatuses[nextPlayerTurn] == PlayerStatus.PLAYING)
			{
				game.setTurn (nextPlayerTurn);
				beginNextTurn();
				break;
			}
			else
			{
				continue;
			}
		}
	}


	[RPC]
	public void addPlayerNet(string name, string pass, int color, int loss, int win, string ip)
	{
		bool isExist = (this.players.Where(player => ((player.getName() == name) && (player.getPassword() == pass) )).Count() > 0);
		Player p = Player.CreateComponent (name, pass, win, loss, color, gameObject);

		if (!isExist && !players.Contains( p )) 
		{
			p.ipAddress = ip;
			
			this.players.Add (p);	
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class GameManager : MonoBehaviour {

	public static int ERROR = -1;
	public bool isInGame = false;
	private InGameGUI gameGUI;

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
	private int turnsSoFar;
	private VillageManager villageManager;

	// Use this for initialization
	void Start () 
	{
		villageManager = GameObject.Find ("VillageManager").GetComponent<VillageManager> ();
		game = gameObject.GetComponent<Game> ();
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

	public NetworkConnectionError initOldGame(string ip, int pPort)
	{
		print ("in initOldGame");
		if (isServer) {
			print ("in isServer----");
			Network.InitializeServer (32, port);
			mapGen = gameObject.GetComponent<MapGenerator> ();
			//mapGen.initOldMap (i);
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

	public void initializeSelectedMap ()
	{
		mapGen.initializeColorAndVillagesOnMap(players, finalMapChoice, this.finalMap);
	}

	public Graph getMap()
	{
		return this.finalMap;
	}

	public void preserveMostVotedMap()
	{
		mapGen.gameObject.networkView.RPC("preserveFinalMap", RPCMode.AllBuffered, finalMapChoice);
	}

	[RPC]
	public void setFinalMap(int finalMapChoice)
	{
		this.finalMap = mapGen.getMap(finalMapChoice);
	}

	[RPC]
	public void createNewGame ()
	{
		Debug.Log ("In create new game");
		game.gameObject.networkView.RPC ("setMap",RPCMode.AllBuffered);
		game.gameObject.networkView.RPC ("setPlayers",RPCMode.AllBuffered);
		game.gameObject.networkView.RPC ("initializeStatuses",RPCMode.AllBuffered);
		game.gameObject.networkView.RPC ("setStartingPlayer",RPCMode.AllBuffered,0);
		game.gameObject.networkView.RPC ("setTurnsPlayed",RPCMode.AllBuffered,0);
		turnsSoFar = game.getTurnsPlayed();
	}
	[RPC]
	public void createSavedGame()
	{

	}

	[RPC]
	public void setNextPlayer(int nextTurn)
	{
		Debug.Log ("inSetNextPlayer");
		game.setTurn(nextTurn);
	}

	public int findNextPlayer()
	{

		gameGUI.disableInteractions ();
		int currentTurn = game.getCurrentTurn();
		int numberOfPlayers = game.getPlayers().Count;
		List<PlayerStatus> playerStatuses = game.getPlayerStatuses();
		
		for(int i = 0; i < numberOfPlayers; i++)
		{
			int nextPlayerTurn = (currentTurn + i) % numberOfPlayers;
			if(playerStatuses[nextPlayerTurn] == PlayerStatus.PLAYING)
			{
				Debug.Log ("Next Player Turn is: " + nextPlayerTurn);
				Debug.Log ("My player turn is: " + localTurn);
				return nextPlayerTurn;
			}
			else
			{
				continue;
			}
		}
		Debug.Log ("this shouldnt get printed");
		return ERROR; // ERROR = -1
	}


	public void initializeNextPlayersVillages()
	{
		Debug.Log ("in initialize next player villages");
		Player p = game.getCurrentPlayer ();
		Debug.Log ("current player in turn to play is: " + p);
		List<Village> villagesToUpdate = p.getVillages ();
		Debug.Log ("number of villages of current player :" +villagesToUpdate.Count);
		foreach (Village v in villagesToUpdate)
		{
			Debug.Log ("in update village loop");
			Debug.Log ("updating village: "+v);
			villageManager.gameObject.networkView.RPC("updateVillageNet",RPCMode.AllBuffered,v.gameObject.networkView.viewID);
		}
	}

	public Player getLocalPlayer()
	{
		return this.localPlayer;
	}
	public int getLocalTurn()
	{
		return this.localTurn;
	}

	[RPC]
	public void setLocalTurnAndPlayer(int turnNumber)
	{
		this.localTurn = turnNumber;
		List<Player> temp = game.getPlayers ();
		this.localPlayer = temp[turnNumber];
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
		if( isInGame && (gameGUI == null) )
		{
			Debug.Log ("finding attaching GUI");
			gameGUI = GameObject.Find ("attachingGUI").GetComponent<InGameGUI>();
			Debug.Log (gameGUI);
		}
	}
}

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
	private List<Player> tempList = new List<Player> ();
	public Graph finalMap = null;

	public int finalMapChoice = -1;
	public MapGenerator mapGen;

	public Game game;

	private Player localPlayer; // has getter
	private int localTurn; //has getter and setter
	private int turnsSoFar;
	public VillageManager villageManager;

	public bool printList;

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
	

	public void setFinalMap(Graph finalMap)
	{
		this.finalMap = finalMap;
	}
	
	public void createNewGame ()
	{
		game.gameObject.networkView.RPC ("setMap",RPCMode.AllBuffered);
		game.gameObject.networkView.RPC ("setPlayers",RPCMode.AllBuffered);
		game.gameObject.networkView.RPC ("initializeStatuses",RPCMode.AllBuffered);
		game.gameObject.networkView.RPC ("setStartingPlayer",RPCMode.AllBuffered,0);
		game.gameObject.networkView.RPC ("setTurnsPlayed",RPCMode.AllBuffered,0);
		gameObject.networkView.RPC ("setTurnsSoFar",RPCMode.AllBuffered,0);
	}
	[RPC]
	void setTurnsSoFar(int turnNumber)
	{
		this.turnsSoFar = turnNumber;
	}

		                       
	[RPC]
	void createSavedGame()
	{

	}

	public void setNextPlayer(int nextTurn)
	{
		game.gameObject.networkView.RPC ("setNextPlayerNet", RPCMode.AllBuffered, nextTurn);
	}



	public void updateTurnsPlayed()
	{
		game.gameObject.networkView.RPC ("incrementTurnsPlayedInGameNet", RPCMode.AllBuffered);
		gameObject.networkView.RPC ("updateTurnsPlayedInManagerNet", RPCMode.AllBuffered);
	}

	[RPC]
	void updateTurnsPlayedInManagerNet()
	{
		this.turnsSoFar = game.getTurnsPlayed ();
	}

	public int findNextPlayer()
	{
		int currentTurn = game.getCurrentTurn();
		int numberOfPlayers = game.getPlayers().Count;
		List<PlayerStatus> playerStatuses = game.getPlayerStatuses();
		
		for(int i = 1; i <= numberOfPlayers; i++)
		{
			int nextPlayerTurn = (currentTurn + i) % numberOfPlayers;
			if(playerStatuses[nextPlayerTurn] == PlayerStatus.PLAYING)
			{
				Debug.Log ("Next Player Turn is: " + nextPlayerTurn);
				Debug.Log ("My player turn is: " + localTurn);
				return nextPlayerTurn;
			}
		}
		Debug.Log ("this shouldnt get printed");
		return ERROR; // ERROR = -1
	}


	public void initializeNextPlayersVillages()
	{
		Debug.Log ("in initialize next player villages");
		Player p = game.getCurrentPlayer ();
		List<Village> villagesToUpdate = p.getVillages ();
		Debug.Log ("current player in turn to play is: " + p);
		Debug.Log ("number of villages of current player :" +villagesToUpdate.Count);
		foreach (Village v in villagesToUpdate)
		{
			Debug.Log ("updating village: "+v);
			villageManager.updateVillage(v);
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
	void setLocalTurnAndPlayerNet(int turnNumber)
	{
		Debug.Log ("in set local turn and player");
		this.localTurn = turnNumber;
		//List<Player> temp = game.getPlayers ();
		this.localPlayer = players[turnNumber];
		Debug.Log ("local turn: " + localTurn);
		Debug.Log ("player name: " + localPlayer);
	}
	public void setLocalTurnAndPlayer(int turnNumber)
	{
		this.localTurn = 0;
		this.localPlayer = players [0];
	}

	[RPC]
	void addPlayerNet(string name, string pass, int color, int loss, int win, string ip)
	{
		bool isExist = (this.players.Where(player => ((player.getName() == name) && (player.getPassword() == pass) )).Count() > 0);
		Player p = Player.CreateComponent (name, pass, win, loss, color, gameObject);
		if (!isExist && !players.Contains( p )) 
		{
			p.ipAddress = ip;
			this.players.Add (p);	
		}
		for(int i = 0; i < players.Count; i++)
		{
			Debug.Log (p.getName ());
		}
	}
	public int findPlayerIndex(Player p)
	{
		int index = players.FindIndex (i => i.getName () == p.getName ());
		return index;
	}

	// Update is called once per frame
	void Update () {
		if( isInGame && (gameGUI == null) )
		{
			Debug.Log ("finding attaching GUI");
			gameGUI = GameObject.Find ("attachingGUI").GetComponent<InGameGUI>();
			Debug.Log (gameGUI);
		}
		if (printList) {
			print (localPlayer.getName ());
			printList = false;
		}
	}

	[RPC]
	void setPlayerColorsNet(string name, int color){
		Player p = players.Find(i => i.getName() == name); 
		p.setColor (color);
		tempList.Add (p);
	}

	[RPC]
	void overWritePlayerList(){
		players = tempList;
	}

	public void checkLoss(Player p)
	{
		int numVillages = p.getVillages().Count();
		if (numVillages == 0)
		{
			int curPlayerIndex = findPlayerIndex (p);
			game.networkView.RPC ("setPlayerStatus",RPCMode.AllBuffered,(int)PlayerStatus.LOST,curPlayerIndex);
			p.addLoss(); //update database
			gameGUI.displayError (@""+ p.getName ()+" has lost and is out of the game.");

		}

		int loserCounter = 0;
		int winnerIndex = 0;
		for(int i = 0; i < game.getPlayerStatuses().Count; i++)
		{
			if(game.getPlayerStatuses()[i] == PlayerStatus.LOST)
			{
				loserCounter++;
			}
			else if (game.getPlayerStatuses()[i] == PlayerStatus.PLAYING)
			{
				winnerIndex = i;
			}
		}
		if (loserCounter == players.Count ()-2) 
		{
			game.networkView.RPC ("setPlayerStatus",RPCMode.AllBuffered,(int)PlayerStatus.WIN,winnerIndex);
		}
	}
	
	public void checkWin()
	{
		//retrieve number of players in the list
		int winner = 100;
		for(int i = 0; i < game.getPlayerStatuses().Count;i++)
		{
			if(game.getPlayerStatuses()[i] == PlayerStatus.WIN)
			{
				winner = i;
			}
		}
		if(winner != 100)
		{
			Player winningPlayer = this.players[winner];
			winningPlayer.addWin();
			gameGUI.displayError (@""+ winningPlayer.getName ()+" wins!");
		}

	}
}

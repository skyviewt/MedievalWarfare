using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public enum PlayerStatus
{
	PLAYING,
	LOST,
	WIN,
};

[System.Serializable]
public class Game : MonoBehaviour
{
	private Graph gameMap;
	private List<Player> players;														//stores the list of players in the game
	private List<PlayerStatus> playerStatuses = new List<PlayerStatus> ();				//stores the status of players in the game
	private int currentTurn;
	private int turnsPlayed;
	private GameManager GM;
	public bool printList;


	void Update(){
		if (printList) {
			foreach (Player p in players){
				print (p.getName ());
			}
			printList = false;
		}
	}

	//constructor
//	public static Game CreateComponent ( List<Player> participants, Graph map,  GameObject g) 
//	{
//		Game theGame = g.AddComponent<Game>();
//		theGame.players = participants;
//		theGame.gameMap = map;
//		print (theGame.players.Count);
//		for(int i = 0; i < theGame.players.Count; i++) 
//		{
//			theGame.playerStatuses.Add(PlayerStatus.PLAYING);
//		}
//
//		theGame.setTurn(0);
//		return theGame;
//	}
	void Start()
	{
		GM = GameObject.Find("preserveGM").GetComponent<GameManager>();
	}

	[RPC]
	void setMap()
	{
		this.gameMap = GM.getMap();
	}

	[RPC]
	void setPlayers()
	{
		this.players = GM.getPlayers();
	}

	[RPC]
	void initializeStatuses()
	{
		for(int i = 0; i < players.Count; i++) 
		{
			playerStatuses.Add(PlayerStatus.PLAYING);
		}
	}	

	[RPC]
	void setStartingPlayer(int playerTurn)
	{
		this.currentTurn = playerTurn;
	}
	[RPC]
	void setTurnsPlayed(int turnsPlayed)
	{
		this.turnsPlayed = turnsPlayed;
	}
	[RPC]
	void incrementTurnsPlayedInGameNet()
	{
		this.turnsPlayed++;
	}

	[RPC]
	void setNextPlayerNet(int nextPlayer)
	{
		this.currentTurn = nextPlayer;
	}

	[RPC]
	void setPlayerStatus(int status,int playerIndex)
	{
		playerStatuses [playerIndex] = (PlayerStatus)status;
	}

	/********* GETTERS ****************/
	public List<Player> getPlayers()
	{
		return this.players;
	}

	public Graph getMap()
	{
		return this.gameMap;
	}

	public int getCurrentTurn()
	{
		return this.currentTurn;
	}
	public Player getCurrentPlayer()
	{
		return this.players [currentTurn];
	}

	public List<PlayerStatus> getPlayerStatuses()
	{
		return this.playerStatuses;
	}
	
	public void setTurn(int turnNumber)
	{
		this.currentTurn = turnNumber;
	}
	public int getTurnsPlayed()
	{
		return this.turnsPlayed;
	}
	//Remove player from List<Player> Players
	public void removePlayer(Player p)
	{
		this.players.Remove (p);
	}

}
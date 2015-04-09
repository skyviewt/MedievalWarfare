using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public enum PlayerStatus
{
	PLAYING,
	LOSE,
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
		GM = GameObject.Find("perserveGM").GetComponent<GameManager>();
	}
	[RPC]
	public void setMap()
	{
		this.gameMap = GM.getMap();
	}

	[RPC]
	public void setPlayers()
	{
		this.players = GM.getPlayers();
	}

	[RPC]
	public void initializeStatuses()
	{
		for(int i = 0; i < players.Count; i++) 
		{
			playerStatuses.Add(PlayerStatus.PLAYING);
		}
	}	

	[RPC]
	public void setStartingPlayer(int playerTurn)
	{
		this.currentTurn = playerTurn;
	}
	[RPC]
	public void setTurnsPlayed(int turnsPlayed)
	{
		this.turnsPlayed = turnsPlayed;
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
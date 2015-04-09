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
	private Player currentPlayer;
	private int currentTurn;
	private GameManager GM = GameObject.Find("perserveGM").GetComponent<GameManager>();
	
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

	[RPC]
	public void setMap(Graph map)
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
		this.currentPlayer = players [playerTurn];
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
		return this.currentPlayer;
	}

	public List<PlayerStatus> getPlayerStatuses()
	{
		return this.playerStatuses;
	}

	//Sets the turn to be Player p
	public void setTurn(int turnNumber)
	{
		this.currentTurn = turnNumber;
		this.currentPlayer = players[turnNumber];
	}

	//Remove player from List<Player> Players
	public void removePlayer(Player p)
	{
		this.players.Remove (p);
	}



}
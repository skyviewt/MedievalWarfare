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
	private List<Player> players;							//stores the list of players in the game
	private List<PlayerStatus> playerStatuses;				//stores the status of players in the game
	private int numberOfPlayers;
	private Player currentPlayer;
	private int currentTurn;								
	
	//constructor
	public static Game CreateComponent ( List<Player> participants, Graph map,  GameObject g) 
	{
		Game theGame = g.AddComponent<Game>();
		theGame.players = participants;
		theGame.gameMap = map;
		theGame.numberOfPlayers = participants.Count;
		theGame.playerStatuses = new List<PlayerStatus> ();
		for (int i = 0; i < theGame.numberOfPlayers; i++) 
		{
			print ("player count: "+ i);
			theGame.playerStatuses[i] = PlayerStatus.PLAYING;
		}

		theGame.setTurn(0);
		return theGame;
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

	public List<PlayerStatus> getStatuses()
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
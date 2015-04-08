using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Game : MonoBehaviour
{
	private Graph gameMap;
	private List<Player> Players;
	private Player turnOf;
	
	//constructor
	public static Game CreateComponent ( List<Player> Participants, GameObject g ) 
	{
		Game theGame = g.AddComponent<Game>();
		theGame.Players = Participants;
		theGame.turnOf = Participants [0];
		return theGame;
	}

	/********* GETTERS ****************/
	public List<Player> getPlayers()
	{
		return this.Players;
	}

	public Graph getMap()
	{
		return this.gameMap;
	}

	//Sets the turn to be Player p
	public void setTurn(Player p)
	{
		this.turnOf = p;
	}

	//Remove player from List<Player> Players
	public void removePlayer(Player p)
	{
		this.Players.Remove (p);
	}
}
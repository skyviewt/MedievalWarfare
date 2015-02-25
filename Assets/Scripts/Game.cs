using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Game : MonoBehaviour
{
	private Graph gameMap;
	private List<Player> Players;
	private int colorIndex;
	private int mapSize;	//randomly generated, between 300 and 400
	private Player turnOf;


	public Game(List<Player> Participants)
	{
		this.Players = Participants;
		this.colorIndex = Participants.Count - 1; //used to generate color later
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
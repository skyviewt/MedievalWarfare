//Digidestined, Initialized by Harvey Yang (Feb 21, 2015), Version 1.0
//Game.cs class

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
	private int probability; //This is the probability of tiles, randomly generated between 1 and 100
	private Player turnOf;
	private Dictionary<Tile, int> colorDictionary;//Dictionary<Tile, int>
	private Dictionary<Tile, bool> visitedDictionary;//Dictionary<Tile, bool>

	public Game(List<Player> Participants)
	{
		this.Players = Participants;
		this.colorIndex = Participants.Count - 1; //used to generate color later
		this.mapSize = Random.Range(300,401); //max is excluded in Unity C#
		this.colorDictionary = new Dictionary<Tile, int>();
		this.visitedDictionary = new Dictionary<Tile, bool>();

		foreach (Tile tile in this.gameMap.vertices)
		{
			this.probability = Random.Range (1, 101); //max is excluded in Unity C#
			if (this.probability > 0 && this.probability <= 20)
			{
				tile.setLandType(LandType.Tree);
			}
			if (this.probability > 20 && this.probability <= 30)
			{
				tile.setLandType(LandType.Meadow);
			}
			else
			{
				tile.setLandType(LandType.Grass);
			}

			int color = Random.Range(0, this.colorIndex+1);
			this.colorDictionary.Add(tile, color);
			this.visitedDictionary.Add(tile,false);
		}
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

	public Dictionary<Tile, int> getColorDictionary(){
		return this.colorDictionary;
	}
	
	public Dictionary<Tile, bool> getVisitedDictionary(){
		return this.visitedDictionary;
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
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Player : MonoBehaviour{
	
	private string username;
	private string password;
	private int wins;
	private int losses;
	private List<Village> myVillages;
	private Game aGame;
	private int color;

	//constructor
	public static Player CreateComponent ( string pName, string pPass, int pWins, int pLosses, GameObject g ) 
	{
		Player thePlayer = g.AddComponent<Player>();
		thePlayer.username = pName;
		thePlayer.password = pPass;
		thePlayer.wins = pWins;
		thePlayer.losses = pLosses;
		thePlayer.myVillages = new List<Village> ();
		return thePlayer;
	}


	public static Player CreateComponent ( string pName, string pPass, GameObject g ) 
	{
		Player thePlayer = g.AddComponent<Player>();
		thePlayer.username = pName;
		thePlayer.password = pPass;
		thePlayer.wins = 0;
		thePlayer.losses = 0;
		thePlayer.myVillages = new List<Village> ();
		return thePlayer;
	}

	public void addWin()
	{
		this.wins++;
	}
	
	public void addLoss()
	{
		this.losses++;
	}
	
	public int getWins()
	{
		return wins;
	}
	public int getLosses()
	{
		return losses;
	}
	public void setGame(Game pGame)
	{
		this.aGame = pGame;
	}
	public Game getGame()
	{
		return this.aGame;
	}
	public string getName()
	{
		return username;
	}
	public void addVillage(Village v)
	{
		myVillages.Add (v);
	}
	public Village getVillage(int i)
	{
		return myVillages [i];
	}

	public List<Village> getVillages()
	{
		return myVillages;
	}
	public void setColor(int i)
	{
		this.color = i;
	}
	public int getColor()
	{
		return this.color;
	}
	/*
	 * This function will be to update the database.
	 * Should be called by the controller after a player has finished with a game.
	 */
	public void updateDatabase()
	{
		//TODO
	}
	
}
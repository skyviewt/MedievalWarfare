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
//	private Game aGame;
	
	public Player(string pName, string pPass, int pWins, int pLosses)
	{
		this.username = pName;
		this.password = pPass;
		this.wins = pWins;
		this.losses = pLosses;
		myVillages = new List<Village>();
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
//	public void setGame(Game pGame)
//	{
//		this.aGame = pGame;
//	}
	
	
	/*
	 * This function will be to update the database.
	 * Should be called by the controller after a player has finished with a game.
	 */
	public void updateDatabase()
	{
		//TODO
	}
	
}
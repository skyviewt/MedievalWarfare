using UnityEngine;
using System.Collections;

public class Player{

	private String username;
	private String password;
	private int wins;
	private int losses;
	private List<Village> myVillages;
	private aGame;

	public Player(String pName, String pPass, int pWins, int pLosses)
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
	public void setGame(Game pGame)
	{
		this.aGame = pGame;
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

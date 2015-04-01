using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Player : MonoBehaviour{
	
	public string username;
	public string password;
	public int wins;
	public int losses;
	public List<Village> myVillages;
	public Game aGame;
	public int color;

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
	//constructor 
	public Player(){
		wins = 0;
		losses = 0;
		myVillages = new List<Village>();
	}
	
	public void initPlayer(string pName, string pPass){
		username = pName;
		password = pPass;
	}
	[RPC]
	void addVillageNet(NetworkViewID villageID){
		Village vil = NetworkView.Find(villageID).gameObject.GetComponent<Village>();
		myVillages.Add (vil);
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
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Player : MonoBehaviour{
	
	public string username;
	private string password;
	private int wins;
	private int losses;
	public List<Village> myVillages;
	private Game aGame;
	private int color;
	public string ipAddress;

	//constructor
	public static Player CreateComponent ( string pName, string pPass, int pWins, int pLosses, int pColor, GameObject g ) 
	{
		Player thePlayer = g.AddComponent<Player>();
		thePlayer.username = pName;
		thePlayer.password = pPass;
		thePlayer.wins = pWins;
		thePlayer.losses = pLosses;
		thePlayer.color = pColor;
		thePlayer.myVillages = new List<Village> ();
		return thePlayer;
	}


	public static Player CreateComponent ( string pName, string pPass, string ipAddress, int color, GameObject g ) 
	{
		Player thePlayer = g.AddComponent<Player>();
		thePlayer.username = pName;
		thePlayer.password = pPass;
		thePlayer.wins = 0;
		thePlayer.losses = 0;
		thePlayer.ipAddress = ipAddress;
		thePlayer.color = color;
		thePlayer.myVillages = new List<Village> ();
		return thePlayer;
	}
	//constructor 
	public Player(){
		wins = 0;
		losses = 0;
		myVillages = new List<Village>();
	}
	
	public void initPlayer(string pName, string pPass, int pColor){
		username = pName;
		password = pPass;
		color = pColor;
	}
	[RPC]
	void addVillageNet(NetworkViewID villageID){
		Village vil = NetworkView.Find(villageID).gameObject.GetComponent<Village>();
		myVillages.Add (vil);
	}
	[RPC]
	void removeVillageNet(NetworkViewID villageID)
	{
		Village vil = NetworkView.Find(villageID).gameObject.GetComponent<Village>();
		myVillages.Remove (vil);
	}
	
	[RPC]
	void ColorPlayer(NetworkViewID pID, int c)
	{
		NetworkView.Find (pID).gameObject.GetComponent<Player> ().color = c;
	}
	public void addWin()
	{
		this.wins++;
		WWWForm form = new WWWForm();
		form.AddField("user", username);
		WWW w = new WWW("http://iconstanto.com/updateWinner.php", form);
		StartCoroutine(addWinI(w));
	}
	
	IEnumerator addWinI(WWW w)
	{
		yield return w;
		if (w.error == null) 
		{			
			print ("Error encountered.");
		} 
		else 
		{
			print ("player wins added");
		}
	}
	
	public void addLoss()
	{
		this.losses++;
		WWWForm form = new WWWForm();
		form.AddField("user", username);
		WWW w = new WWW("http://iconstanto.com/updateLoser.php", form);
		StartCoroutine(addLossI(w));
	}
	
	IEnumerator addLossI(WWW w)
	{
		yield return w;
		if (w.error == null) 
		{			
			print ("Error encountered.");
		} 
		else 
		{
			print ("player losses added");
		}
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
	public string getPassword()
	{
		return password;
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
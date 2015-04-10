using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class mainMenu : MonoBehaviour {

	public Canvas ExitCanvas;
	public Canvas JoinGameCanvas;
	public Canvas MiniMapCanvas;
	public Canvas MainMenuCanvas;
	public Canvas LobbyCanvas;
	public Canvas LoginCanvas;
	public Canvas RegisterCanvas;
	public Canvas LoginBoxCanvas;
	public Canvas ErrorCanvas;
	public Canvas StatsCanvas;
	public Canvas LoadGameCanvas;

	public Transform HostText;
	public Transform JoinText;
	public Transform StatsText;
	public Transform LogoutText;
	public Transform LoadGameText;

	public Transform LaunchText;
	public Text ErrorLobbyMsg;

	public List<Text> statsTableTexts;
	public List<Text> gameNameTexts;

	public Text ErrorJoinRegisterLoginMsg;

	public List<Text> connectedPlayerText;
	public List<Text> connectedPlayerMapText;

	public Text map1PlayerCount;
	public Text map2PlayerCount;
	public Text chosenMapText;

	public Camera cam1;
	public Camera cam2;

	public Camera resCam1;
	public Camera resCam2;
	public Camera loadCam;

	public int[] countMapChoices = new int[2];

	// 1-based
	public int mapChoice = -1;

	public bool isALoadGame = false;

	// game load choice
	public int gameLoadChoice = -1;

	public Text _ipInput;
	public Text _portInput;

	public Text RegisterUserNameInput;
	public Text RegisterPassword1;
	public Text RegisterPassword2;

	public Text LoginUserName;
	public Text LoginPassword;

	// Elements
	public GameObject PrefabFire;
	public GameObject PrefabLight;

	public GameManager GM;

	// Use this for initialization
	void Start () {
		Instantiate(PrefabFire);
		Instantiate(PrefabLight);
		GM = GameObject.Find("preserveGM").GetComponent<GameManager>();
		LoginCanvas.enabled = true;
		MainMenuCanvas.enabled = false;
		ExitCanvas.enabled = false;
		JoinGameCanvas.enabled = false;
		MiniMapCanvas.enabled = false;
		LobbyCanvas.enabled = false;
		RegisterCanvas.enabled = false;
		LoginBoxCanvas.enabled = false;
		LoadGameCanvas.enabled = false;
		StatsCanvas.enabled = false;
		ErrorCanvas.enabled = true;
		cam1.enabled = false;
		cam2.enabled = false;
		resCam1.enabled = false;
		resCam2.enabled = false;
		loadCam.enabled = false;
		ErrorLobbyMsg.enabled = false;
		ErrorJoinRegisterLoginMsg.enabled = false;
		LaunchText.GetComponent<Button>().enabled = false;
	}

	public void quitTextPressed()
	{
		ExitCanvas.enabled = true;
		MainMenuCanvas.enabled = false;
		hideStartGameButtons ();
	}

	public void doNotQuitPressed()
	{
		ExitCanvas.enabled = false;
		MainMenuCanvas.enabled = true;
		showStartGameButtons ();
	}


	public void registerButtonPressed()
	{
		RegisterCanvas.enabled = true;
	}

	public void loginButtonPressed()
	{
		LoginBoxCanvas.enabled = true;
	}

	public void actualLoginPressed()
	{
		ErrorJoinRegisterLoginMsg.enabled = false;
		if( LoginUserName.text == "" )
		{
			ErrorJoinRegisterLoginMsg.text = "Username cannot be null!";
			ErrorJoinRegisterLoginMsg.enabled = true;
			return;
		}
		else if( LoginPassword.text == "" )
		{
			ErrorJoinRegisterLoginMsg.text = "Password cannot be null!";
			ErrorJoinRegisterLoginMsg.enabled = true;
			return;
		}
		WWWForm form = new WWWForm();
		form.AddField("user", LoginUserName.text);
		form.AddField("password", LoginPassword.text);
		WWW w = new WWW("http://iconstanto.com/login.php", form);
		StartCoroutine(login(w));
	}

	public void statsButtonPressed()
	{
		WWWForm form = new WWWForm();
		form.AddField("user", "test");
		WWW w = new WWW("http://iconstanto.com/showStats.php", form);
		StartCoroutine(stats(w));
	}
	
	public void returnToLoginPressed()
	{
		ErrorJoinRegisterLoginMsg.enabled = false;
		LoginBoxCanvas.enabled = false;
		RegisterCanvas.enabled = false;
		LoginCanvas.enabled = true;
	}


	public void clearStatsTable()
	{
		for(int i=0; i< statsTableTexts.Count; i++)
		{
			statsTableTexts[i].text = "";
		}
	}

	IEnumerator stats(WWW w)
	{
		ErrorJoinRegisterLoginMsg.enabled = false;
		this.clearStatsTable ();
		yield return w;
		if (w.error == null)
		{
			StatsCanvas.enabled = true;
			string returntext = w.text;
			string[] words = returntext.Split('#');
			foreach (string word in words)
			{
				string[] parts = word.Split('-');
				for(int i=0; i<parts.Length; i++)
				{
					string s = parts[i];
					string[] latterParts = s.Split(':');
					statsTableTexts[i].text += (latterParts[1] + '\n');
				}
			}
		}
		else
		{
			ErrorJoinRegisterLoginMsg.text += "ERROR: " + w.error;
			ErrorJoinRegisterLoginMsg.enabled = true;
		}
	}


			
	IEnumerator login(WWW w)
	{
		ErrorJoinRegisterLoginMsg.enabled = false;
		yield return w;
		if (w.error == null)
		{
			if (w.text == "login-SUCCESS")
			{
				LoginCanvas.enabled = false;
				LoginBoxCanvas.enabled = false;
				MainMenuCanvas.enabled = true;
				showStartGameButtons ();
			}
			else
			{
				ErrorJoinRegisterLoginMsg.text = w.text;
				ErrorJoinRegisterLoginMsg.enabled = true;
			}
		}
		else
		{
			ErrorJoinRegisterLoginMsg.text = "ERROR: " + w.error;
			ErrorJoinRegisterLoginMsg.enabled = true;
		}
	}

	IEnumerator registerFunc(WWW w)
	{
		yield return w;
		if (w.error == null)
		{
			ErrorJoinRegisterLoginMsg.text = w.text;
			Debug.Log (w.text);
			ErrorJoinRegisterLoginMsg.enabled = true;
		}
		else
		{
			ErrorJoinRegisterLoginMsg.text += "ERROR: " + w.error;
			Debug.Log (w.error);
			ErrorJoinRegisterLoginMsg.enabled = true;
		}

	}

	void resetSavedGameTexts()
	{
		for (int i =0; i<gameNameTexts.Count; i++) 
		{
			gameNameTexts[i].text = ("Game " + i.ToString() + ": ");	
		}
	}

	public void actualLoadGamePressed()
	{
		resetSavedGameTexts ();
		isALoadGame = true;
		LoadGameCanvas.enabled = false;
		SaveLoad sl = GameObject.Find("SaveLoad").GetComponent<SaveLoad>();
		sl.loadThisGame (gameLoadChoice);
		showLobby();
	}
	public void actualRegistrationPressed()
	{
		ErrorJoinRegisterLoginMsg.enabled = false;
		if( RegisterUserNameInput.text == "" )
		{
			ErrorJoinRegisterLoginMsg.text = "Username field cannot be empty";
			ErrorJoinRegisterLoginMsg.enabled = true;
			return;
		}
		else if( RegisterPassword1.text == "" || RegisterPassword2.text == "" )
		{
			ErrorJoinRegisterLoginMsg.text = "Password field cannot be empty";
			ErrorJoinRegisterLoginMsg.enabled = true;
			return;
		}
		else if( RegisterPassword1.text != RegisterPassword2.text )
		{
			ErrorJoinRegisterLoginMsg.text = "Passwords entered do not match";
			ErrorJoinRegisterLoginMsg.enabled = true;
			return;
		}
		else if(RegisterUserNameInput.text.Contains("-") || RegisterUserNameInput.text.Contains(":") ||
		        RegisterUserNameInput.text.Contains("#") || RegisterPassword1.text.Contains("-") ||
		        RegisterPassword1.text.Contains(":") || RegisterPassword1.text.Contains("#"))
		{
			ErrorJoinRegisterLoginMsg.text = "Inputs cannot contains symbols -, : or #. ";
			ErrorJoinRegisterLoginMsg.enabled = true;
			return;
		}
		WWWForm form = new WWWForm();
		form.AddField("user", RegisterUserNameInput.text);
		form.AddField("password", RegisterPassword1.text);
		form.AddField ("ipaddress", Network.player.ipAddress);
		WWW w = new WWW("http://iconstanto.com/register.php", form);
		StartCoroutine(registerFunc(w));
	}


	public void showStartGameButtons()
	{
		HostText.GetComponent<Button>().enabled = true;
		JoinText.GetComponent<Button>().enabled = true;
		StatsText.GetComponent<Button>().enabled = true;
		LoadGameText.GetComponent<Button>().enabled = true;
		LogoutText.GetComponent<Button>().enabled = true;
	}

	public void hideStartGameButtons()
	{
		HostText.GetComponent<Button>().enabled = false;
		JoinText.GetComponent<Button>().enabled = false;
		StatsText.GetComponent<Button>().enabled = false;
		LogoutText.GetComponent<Button>().enabled = false;
		LoadGameText.GetComponent<Button>().enabled = false;
	}
	public void loadGameButtonPressed()
	{
		for (int i =0; i<gameNameTexts.Count; i++) 
		{
			SaveLoad sl = GameObject.Find("SaveLoad").GetComponent<SaveLoad>();
			gameNameTexts[i].text += sl.getSaveName(i+1);	
		}
		LoadGameCanvas.enabled = true;
	}

	public void game1Pressed()
	{
		gameLoadChoice = 1;
	}
	public void game2Pressed()
	{
		gameLoadChoice = 2;
	}
	public void game3Pressed()
	{
		gameLoadChoice = 3;
	}
	public void game4Pressed()
	{
		gameLoadChoice = 4;
	}
	public void game5Pressed()
	{
		gameLoadChoice = 5;
	}

	public void game6Pressed()
	{
		gameLoadChoice = 6;
	}
	public void game7Pressed()
	{
		gameLoadChoice = 7;
	}
	public void game8Pressed()
	{
		gameLoadChoice = 8;
	}
	public void game9Pressed()
	{
		gameLoadChoice = 9;
	}
	public void game10Pressed()
	{
		gameLoadChoice = 10;
	}

	public void returntoMainMenuCanvas()
	{
		StatsCanvas.enabled = false;
		JoinGameCanvas.enabled = false;
		LoadGameCanvas.enabled = false;
		MainMenuCanvas.enabled = true;
		showStartGameButtons ();
		GM.setIsServer (true);
	}

	public void joinGameButtonPressed()
	{
		ErrorJoinRegisterLoginMsg.enabled = false;
		hideStartGameButtons ();
		MainMenuCanvas.enabled = false;
		JoinGameCanvas.enabled = true;
		GM.setIsServer (false);
	}

	public void connectButtonPressed()
	{
		ErrorJoinRegisterLoginMsg.enabled = false;

		print(_ipInput.text);
		if (_ipInput.text != "")
		{
	
			GM.setIpAddress(_ipInput.text);
			GM.setPort(System.Int32.Parse (_portInput.text));
			NetworkConnectionError res = GM.initGame (_ipInput.text, System.Int32.Parse (_portInput.text));

			if(res != NetworkConnectionError.NoError)
			{
				ErrorJoinRegisterLoginMsg.text = res.ToString();
				ErrorJoinRegisterLoginMsg.enabled = true;
				return;
			}
		}
		else 
		{
			ErrorJoinRegisterLoginMsg.text = "The IP address cannot be empty";
			ErrorJoinRegisterLoginMsg.enabled = true;
			return;
		}

		showMiniMapMenu ();
	}

	public void hostButtonPressed()
	{
		GM.setIsServer (true);
		GM.initGame (GM.ipAddress, GM.port);
		GM.networkView.RPC ("addPlayerNet", 
		                    RPCMode.AllBuffered, 
		                    LoginUserName.text,
		                    LoginPassword.text,
		                    GM.players.Count+1, 
		                    0, 
		                    0,
		                    Network.player.ipAddress);
		showMiniMapMenu ();
	}

//	public void hostSavedGameButtonPressed()
//	{
//		GM.setIsServer (true);
//		GM.initGame (GM.ipAddress, GM.port);
//
//	}

	public void showMiniMapMenu()
	{

		JoinGameCanvas.enabled = false;
		MainMenuCanvas.enabled = false;
		MiniMapCanvas.enabled = true;
		cam1.enabled = true;
		cam2.enabled = true;
	
	}

	public void increaseMapChoice1()
	{
		this.networkView.RPC ("increaseMapChoiceNet", RPCMode.AllBuffered, 0);
		mapChoice = 1;
		MiniMapCanvas.enabled = false;
		cam1.enabled = false;
		cam2.enabled = false;
		showLobby ();
		
	}

	public void logoutButtonPressed()
	{
		WWWForm form = new WWWForm();
		form.AddField("user", LoginUserName.text);
		WWW w = new WWW("http://iconstanto.com/logoff.php", form);
		StartCoroutine(logoff(w));
	}

	IEnumerator logoff(WWW w)
	{
		ErrorJoinRegisterLoginMsg.enabled = false;
		yield return w;
		if (w.error == null) 
		{
			
			MainMenuCanvas.enabled = false;
			LoginCanvas.enabled = true;
		} 
		else 
		{
			ErrorJoinRegisterLoginMsg.text = "ERROR: " + w.error;
			ErrorJoinRegisterLoginMsg.enabled = true;	
		}

	}
	[RPC]
	public void increaseMapChoiceNet(int i)
	{
		this.countMapChoices [i] += 1;
	}

	public void increaseMapChoice2()
	{
		this.networkView.RPC ("increaseMapChoiceNet", RPCMode.AllBuffered, 1);
		mapChoice = 2;
		MiniMapCanvas.enabled = false;
		cam1.enabled = false;
		cam2.enabled = false;
		showLobby ();
	}

	public void showLobby()
	{
		LobbyCanvas.enabled = true;
	}

	[RPC]
	public void startLevel()
	{
		Application.LoadLevel("scene1");
	}

	public void ExitGame()
	{
		Application.Quit ();
	}

	private void ClearScene()
	{
		foreach(GameObject obj in Object.FindObjectsOfType<GameObject>()){
			Destroy(obj);
		}
	}

	// when a client connects
	void OnConnectedToServer()
	{
		GM.networkView.RPC ("addPlayerNet", 
		                    RPCMode.AllBuffered, 
		                    LoginUserName.text,
		                    LoginPassword.text,
		                    GM.players.Count+1, 
		                    0, 
		                    0,
		                    Network.player.ipAddress);
	}

	// PLEASE READ TO UNDERSTAND HOW A GAME IS LAUNCHED FROM SCRATCH
	// 1. [host] gets the list of players from GM and the map with the most votes
	// 2. [RPC] to all clients to tell them which map was selected
	// 3. [host] performs [RPC] calls for initializing the assets on the graph
	// 4. 

	[RPC]
	void DontDestroy(NetworkViewID mID)
	{
		DontDestroyOnLoad(NetworkView.Find (mID).gameObject);
	}
	public void launchGamePressed()
	{	if (GM.isServer) 
		{
			if(!isALoadGame)
			{
				Graph finalMap = GM.mapGen.getMap (GM.finalMapChoice);
				Debug.Log (finalMap);
				print ("final map choice" + GM.finalMapChoice);
				GM.setFinalMap (finalMap);
				GM.initializeSelectedMap (); //initializes the graph 

				GM.mapGen.preserveFinalMap (GM.finalMapChoice); // preserves the choice 
			}
			else
			{
				GameObject[] allTiles = GameObject.FindGameObjectsWithTag("LoadedMap");
				foreach(GameObject o in allTiles)
				{
					networkView.RPC("DontDestroy", RPCMode.AllBuffered, o.gameObject.networkView.viewID);
				}

			}
			networkView.RPC("DontDestroy", RPCMode.AllBuffered, GM.villageManager.gameObject.networkView.viewID);

			TileManager tileManager =  GameObject.Find ("TileManager").GetComponent<TileManager> ();
			networkView.RPC("DontDestroy", RPCMode.AllBuffered, tileManager.gameObject.networkView.viewID);

			//setting up the colors properly
			for(int i=0; i<GM.players.Count; i++)
			{
				Player p = GM.players[i];
				p.networkView.RPC ("ColorPlayer", RPCMode.AllBuffered, p.gameObject.networkView.viewID, i+1);
			}
		}

		List<Player> players = GM.getPlayers();
		GM.createNewGame();
		//now we need to give every connection on the network a unique "int turn". Host is always turn 0.
		for (int i = 0; i < Network.connections.Length; i++) {
			GM.gameObject.networkView.RPC ("setLocalTurnAndPlayer",Network.connections[i],i);
		}
		this.networkView.RPC("startLevel", RPCMode.AllBuffered);
	}

//	void OnPlayerConnected()
//	{
//		GM.networkView.RPC ("addPlayerNet", 
//                    RPCMode.AllBuffered, 
//                    LoginUserName.text,
//                    LoginPassword.text,
//                    GM.players.Count + 1, 
//                    0, 
//                    0,
//                    Network.player.ipAddress);
//	}

	void Update(){
		//updates the lobby
		if ( LobbyCanvas.enabled == true ) 
		{
			if(!isALoadGame)
			{
				bool isMap1Chosen = (countMapChoices [0] >= countMapChoices [1]);
				if (isMap1Chosen) {
						resCam1.enabled = true;	
						chosenMapText.text = "Map 1";
						GM.finalMapChoice = 0;
				} else {
						resCam2.enabled = true;	
						chosenMapText.text = "Map 2";
						GM.finalMapChoice = 1;
				}
			}
			//loaded map
			else 
			{
				resCam1.enabled = true;	
				chosenMapText.text = "Loaded Map";
			}
			if(!isALoadGame)
			{
				map1PlayerCount.text = countMapChoices [0].ToString ();
				map2PlayerCount.text = countMapChoices [1].ToString ();
			}

			if (GM.isServer) 
			{
				Player p = GM.players.Where(player=> (player.ipAddress == Network.player.ipAddress)).FirstOrDefault();
				this.networkView.RPC ("changePlayerTextNet", RPCMode.AllBuffered, 0, p.getName());
				if(!isALoadGame)
				{
					this.networkView.RPC ("changePlayerMapTextNet",RPCMode.AllBuffered, 0, "Map " + mapChoice.ToString());
				}

				// only counting the joining players.
				for (int i = 0; i<Network.connections.Length; i++) 
				{
					
					print ("-----joining players ip-----");
					Debug.Log (Network.connections[i].ipAddress);
					//get the player with the same ipAddress
					Debug.Log (GM.players.Count);
					for(int j = 0; j<GM.players.Count; j++)
					{
						print ("what is j:"+j);
						Player playa = GM.players[j];
						if( playa.ipAddress == Network.connections[i].ipAddress )
						{
							this.networkView.RPC ("changePlayerTextNet",RPCMode.AllBuffered, i+1, playa.getName());
							break;
						}
					}
				}
			
				LaunchText.GetComponent<Button> ().enabled = true;	
			}
			else
			{
				LaunchText.GetComponent<Button> ().enabled = false;
				if(!isALoadGame)
				{
					for(int i = 0; i<GM.players.Count; i++)
					{
						for(int j =0; j<connectedPlayerText.Count; j++)
						{
							if(GM.players[i].getName () == connectedPlayerText[j].text)
							{
								this.networkView.RPC ("changePlayerMapTextNet",RPCMode.AllBuffered, j, "Map " + mapChoice.ToString());
								break;
							}
						}
					}
				}
					
			}
		}
	}

	[RPC]
	public void changePlayerTextNet(int i, string s)
	{
		this.connectedPlayerText [i].text = s;
	}

	[RPC]
	public void changePlayerMapTextNet(int i, string s)
	{
		this.connectedPlayerMapText [i].text = s;
	}
}

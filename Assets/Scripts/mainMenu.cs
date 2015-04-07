using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class mainMenu : MonoBehaviour {

	public Canvas ExitCanvas;
	public Canvas JoinGameCanvas;
	public Canvas MiniMapCanvas;
	public Canvas MainMenuCanvas;
	public Canvas LobbyCanvas;
	public Canvas LoginCanvas;
	public Canvas RegisterCanvas;
	public Canvas ErrorCanvas;
	public Transform HostText;
	public Transform JoinText;
	public Transform StatsText;
	public Transform QuitText;

	public Transform LaunchText;
	public Text ErrorLobbyMsg;

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

	public int curPlayers = 0;
	public bool updatedLobby = false;

	public int[] countMapChoices = new int[2];

	// 1-based
	public int mapChoice = -1;

	public Text _ipInput;
	public Text _portInput;

	public Text RegisterUserNameInput;
	public Text RegisterPassword1;
	public Text RegisterPassword2;

	// Elements
	public GameObject PrefabFire;
	public GameObject PrefabLight;

	public GameManager GM;

	// Use this for initialization
	void Start () {
		Instantiate(PrefabFire);
		Instantiate(PrefabLight);
		GM = GameObject.Find("perserveGM").GetComponent<GameManager>();
		LoginCanvas.enabled = true;
		MainMenuCanvas.enabled = false;
		ExitCanvas.enabled = false;
		JoinGameCanvas.enabled = false;
		MiniMapCanvas.enabled = false;
		LobbyCanvas.enabled = false;
		RegisterCanvas.enabled = false;
		ErrorCanvas.enabled = true;
		cam1.enabled = false;
		cam2.enabled = false;
		resCam1.enabled = false;
		resCam2.enabled = false;
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

	public void registerButtonRessed()
	{
		RegisterCanvas.enabled = true;
	}
	public void returnToLoginPressed()
	{
		RegisterCanvas.enabled = false;
		LoginCanvas.enabled = true;
	}

	public void actualRegistrationPressed()
	{
		ErrorJoinRegisterLoginMsg.enabled = false;
		if( RegisterUserNameInput.text == "" )
		{
			ErrorJoinRegisterLoginMsg.text = "Username cannot be null!";
			ErrorJoinRegisterLoginMsg.enabled = true;
			return;
		}
		else if( RegisterPassword1.text == "" || RegisterPassword2.text == "" )
		{
			ErrorJoinRegisterLoginMsg.text = "Password cannot be null!";
			ErrorJoinRegisterLoginMsg.enabled = true;
			return;
		}
		else if( RegisterPassword1.text != RegisterPassword2.text )
		{
			ErrorJoinRegisterLoginMsg.text = "Passwords does not match";
			ErrorJoinRegisterLoginMsg.enabled = true;
			return;
		}
		Debug.Log (Network.player.ipAddress);
		Player p = Player.CreateComponent (RegisterUserNameInput.text,
		                                   RegisterPassword1.text, 
		                                   Network.player.ipAddress, 
		                                  GM.players.Count, GM.gameObject);
		GM.addPlayer (p);
		GM.curPlayer = p;
		RegisterCanvas.enabled = false;
		LoginCanvas.enabled = false;
		MainMenuCanvas.enabled = true;
		showStartGameButtons ();
	}


	public void showStartGameButtons()
	{
		HostText.GetComponent<Button>().enabled = true;
		JoinText.GetComponent<Button>().enabled = true;
		StatsText.GetComponent<Button>().enabled = true;
		QuitText.GetComponent<Button>().enabled = true;
	}

	public void hideStartGameButtons()
	{
		HostText.GetComponent<Button>().enabled = false;
		JoinText.GetComponent<Button>().enabled = false;
		StatsText.GetComponent<Button>().enabled = false;
		QuitText.GetComponent<Button>().enabled = false;
	}

	public void returntoMainMenuCanvas()
	{
		JoinGameCanvas.enabled = false;
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
			ErrorJoinRegisterLoginMsg.text = "The IP address cannot be null!";
			ErrorJoinRegisterLoginMsg.enabled = true;
			return;
		}
		showMiniMapMenu ();
	}

	public void hostButtonPressed()
	{
		GM.setIsServer (true);
		GM.initGame (GM.ipAddress, GM.port);
		showMiniMapMenu ();
	}

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
		countMapChoices [0] += 1;
		mapChoice = 1;
		MiniMapCanvas.enabled = false;
		cam1.enabled = false;
		cam2.enabled = false;
		showLobby ();
		
	}

	public void increaseMapChoice2()
	{
		countMapChoices [1] += 1;
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

	public void StartLevel()
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

	public void lauchGamePressed()
	{
		if (GM.finalMapChoice != -1) 
		{
			GM.finalMap = GM.MapGen.getMap(GM.finalMapChoice);
			GM.MapGen.initializeColorAndVillagesOnMap(GM.players, GM.finalMapChoice, GM.finalMap);
			GM.MapGen.gameObject.networkView.RPC("perserveFinalMap", RPCMode.AllBuffered, GM.finalMapChoice);
			StartLevel();
		}

	}
	

	void Update()
	{
		//updates the lobby
		if (LobbyCanvas.enabled == true && (curPlayers != GM.players.Count || !updatedLobby) ) 
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
			//add all players for all GM
			for(int i=0; i<GM.players.Count; i++)
			{
				Player p = GM.players[i];
				GM.networkView.RPC ("addPlayerNet", 
				                    RPCMode.AllBuffered, 
				                    p.getName (),
				                    p.getPassword(),
				                    p.getColor(), 
				                    p.getLosses(), 
				                    p.getWins() );
			}
			Debug.Log (GM.players.Count);
			Debug.Log(Network.connections.Length);
			if (GM.isServer) 
			{
				Player p = GM.players.Where(player=> (player.ipAddress == Network.player.ipAddress)).First();
				this.networkView.RPC ("changePlayerTextNet", RPCMode.AllBuffered, 0, p.getName());
//				connectedPlayerText[0].text = p.getName ();
				this.networkView.RPC ("changePlayerMapTextNet",RPCMode.AllBuffered, 0, "Map " + mapChoice.ToString());
//				connectedPlayerMapText[0].text = "Map " + mapChoice.ToString();
				// only counting the joining players.
				for (int i = 1; i<Network.connections.Length; i++) 
				{
					//get the player with the same ipAddress
					Player playa = GM.players.Where(player=> (player.ipAddress == Network.connections[i].ipAddress)).First();
					this.networkView.RPC ("changePlayerTextNet",RPCMode.AllBuffered, 0, playa.getName());
//					connectedPlayerText[i].text = playa.getName ();

				}
			
				LaunchText.GetComponent<Button> ().enabled = true;	
				updatedLobby = true;
			}
			else
			{
				for(int i = 0; i<GM.players.Count; i++)
				{
					if(GM.players[i].getName () == connectedPlayerText[i].text)
					{
						this.networkView.RPC ("changePlayerMapTextNet",RPCMode.AllBuffered, i, "Map " + mapChoice.ToString());
					}
				}
				LaunchText.GetComponent<Button> ().enabled = false;	
			}


			map1PlayerCount.text = countMapChoices [0].ToString ();
			map2PlayerCount.text = countMapChoices [1].ToString ();

			curPlayers = GM.players.Count;
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

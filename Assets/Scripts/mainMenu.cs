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
	public Canvas LoginBoxCanvas;
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

	public int[] countMapChoices = new int[2];

	// 1-based
	public int mapChoice = -1;

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
		GM = GameObject.Find("perserveGM").GetComponent<GameManager>();
		LoginCanvas.enabled = true;
		MainMenuCanvas.enabled = false;
		ExitCanvas.enabled = false;
		JoinGameCanvas.enabled = false;
		MiniMapCanvas.enabled = false;
		LobbyCanvas.enabled = false;
		RegisterCanvas.enabled = false;
		LoginBoxCanvas.enabled = false;
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
		WWW w = new WWW("http://medievalwarfare.site90.net/login.php", form);
		StartCoroutine(login(w));
	}

	public void returnToLoginPressed()
	{
		ErrorJoinRegisterLoginMsg.enabled = false;
		LoginBoxCanvas.enabled = false;
		RegisterCanvas.enabled = false;
		LoginCanvas.enabled = true;
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
		WWWForm form = new WWWForm();
		form.AddField("user", RegisterUserNameInput.text);
		form.AddField("password", RegisterPassword1.text);
		form.AddField ("ipaddress", Network.player.ipAddress);
		WWW w = new WWW("http://medievalwarfare.site90.net/register.php", form);
		StartCoroutine(registerFunc(w));
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
			ErrorJoinRegisterLoginMsg.text = "The IP address cannot be empty";
			ErrorJoinRegisterLoginMsg.enabled = true;
			return;
		}
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

	public void launchGamePressed()
	{
		if (GM.finalMapChoice != -1) 
		{	
			List<Player> players = GM.getPlayers();
			Graph finalMap = GM.mapGen.getMap(GM.finalMapChoice);
			GM.networkView.RPC("setFinalMap",RPCMode.AllBuffered, GM.finalMapChoice);
			GM.mapGen.initializeColorAndVillagesOnMap(players, GM.finalMapChoice, finalMap);
			if(GM.isServer)
			{
				GM.mapGen.networkView.RPC("perserveFinalMap", RPCMode.AllBuffered, GM.finalMapChoice);
			}
			GM.createNewGame();
			this.networkView.RPC("startLevel", RPCMode.AllBuffered);
		}
	}

	public void launchSavedGamePressed()
	{

	}

	void Update()
	{
		//updates the lobby
		if ( LobbyCanvas.enabled == true ) 
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
		
			if (GM.isServer) 
			{
				Player host = GM.players.Where(player=> (player.ipAddress == Network.player.ipAddress)).FirstOrDefault();
				this.networkView.RPC ("changePlayerTextNet", RPCMode.AllBuffered, 0, host.getName());
				this.networkView.RPC ("changePlayerMapTextNet",RPCMode.AllBuffered, 0, "Map " + mapChoice.ToString());

				int currTextIdx = 1;
				for (int i = 0; i<GM.players.Count; i++) 
				{
					for(int j = 0; j<connectedPlayerText.Count; j++)
					{
						if(connectedPlayerText[j].text == "")
						{
							currTextIdx = j;
						}
					}

					if(GM.players[i].getName() != host.getName())
					{
						this.networkView.RPC ("changePlayerTextNet", RPCMode.AllBuffered, currTextIdx, GM.players[i].getName());
					}
				}
			
				LaunchText.GetComponent<Button> ().enabled = true;	
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

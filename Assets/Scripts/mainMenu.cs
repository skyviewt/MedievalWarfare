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
	public Transform HostText;
	public Transform JoinText;
	public Transform StatsText;
	public Transform QuitText;

	public Transform LaunchText;
	public Text ErrorLobbyMsg;

	public Text ErrorJoinMsg;

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

	// Elements
	public GameObject PrefabFire;
	public GameObject PrefabLight;

	public GameManager GM;

	// Use this for initialization
	void Start () {
		Instantiate(PrefabFire);
		Instantiate(PrefabLight);
		GM = GameObject.Find("perserveGM").GetComponent<GameManager>();
		MainMenuCanvas.enabled = true;
		ExitCanvas.enabled = false;
		JoinGameCanvas.enabled = false;
		MiniMapCanvas.enabled = false;
		LobbyCanvas.enabled = false;
		cam1.enabled = false;
		cam2.enabled = false;
		resCam1.enabled = false;
		resCam2.enabled = false;
		ErrorLobbyMsg.enabled = false;
		ErrorJoinMsg.enabled = false;
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
		ErrorJoinMsg.enabled = false;
		hideStartGameButtons ();
		MainMenuCanvas.enabled = false;
		JoinGameCanvas.enabled = true;
		GM.setIsServer (false);
	}

	public void connectButtonPressed()
	{
		ErrorJoinMsg.enabled = false;

		print(_ipInput.text);
		if (_ipInput.text != "")
		{
	
			GM.setIpAddress(_ipInput.text);
			GM.setPort(System.Int32.Parse (_portInput.text));
			NetworkConnectionError res = GM.initGame (_ipInput.text, System.Int32.Parse (_portInput.text));

			if(res != NetworkConnectionError.NoError)
			{
				ErrorJoinMsg.text = res.ToString();
				ErrorJoinMsg.enabled = true;
				return;
			}
		}
		else 
		{
			ErrorJoinMsg.text = "The IP address cannot be null!";
			ErrorJoinMsg.enabled = true;
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
		if( LobbyCanvas.enabled == true )
		{
			print ("in Update");
			bool isMap1Chosen = (countMapChoices [0] >= countMapChoices [1]);
			if (isMap1Chosen) 
			{
				resCam1.enabled = true;	
				chosenMapText.text = "Map 1";
				GM.finalMapChoice = 0;
			} 
			else 
			{
				resCam2.enabled = true;	
				chosenMapText.text = "Map 2";
				GM.finalMapChoice = 1;
			}

			// only counting the joining players.
			for(int i = 0; i<GM.players.Count; i++)
			{
				print (GM.players[i].getName());
				connectedPlayerText[i].text = GM.players[i].getName();

//				if(Network.player.ipAddress == Network.connections[0].ipAddress)
//				{
//					connectedPlayerMapText[i+1].text = "Map " + mapChoice.ToString ();
//				}
			}
		}
		if (GM.isServer) 
		{
			LaunchText.GetComponent<Button>().enabled = true;	
		}
		map1PlayerCount.text = countMapChoices [0].ToString ();
		map2PlayerCount.text = countMapChoices [1].ToString ();
	}
}

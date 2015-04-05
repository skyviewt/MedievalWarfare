using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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

	public Text ErrorLobbyMsg;
	public Text ErrorJoinMsg;

	public Camera cam1;
	public Camera cam2;

	public Camera resCam1;
	public Camera resCam2;

	public int[] countMapChoices = new int[2];
	
	public Text _ipInput;
	public Text _portInput;

	// Elements
	public GameObject PrefabFire;
	public GameObject PrefabLight;

	private GameManager GM;
	// Use this for initialization
	void Start () {
		Instantiate(PrefabFire);
		Instantiate(PrefabLight);
		GM = GameObject.Find("mainMenu").GetComponent<GameManager>();
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
			if(GM.isConnectable())
			{
				GM.setIpAddress(_ipInput.text);
				GM.setPort(System.Int32.Parse (_portInput.text));
				GM.initGame (_ipInput.text, System.Int32.Parse (_portInput.text));
			}
			else
			{
				ErrorJoinMsg.text = "Cannot connect to specified IP. Please check if host is on or if the IP is valid.";
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
		print (countMapChoices [0]);
		MiniMapCanvas.enabled = false;
		cam1.enabled = false;
		cam2.enabled = false;
		showLobby ();
		
	}

	public void increaseMapChoice2()
	{
		countMapChoices [1] += 1;
		print (countMapChoices [1]);
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
}

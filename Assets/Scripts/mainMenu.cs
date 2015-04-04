using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

[System.Serializable]
public class mainMenu : MonoBehaviour {

	public Canvas ExitCanvas;
	public Canvas JoinGameCanvas;
	public Canvas MiniMapCanvas;
	public Canvas BackgroundCanvas;
	public Canvas MainMenuCanvas;
	public Transform HostText;
	public Transform JoinText;
	public Transform StatsText;
	public Transform QuitText;

	public Camera cam1;
	public Camera cam2;

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
		BackgroundCanvas.enabled = false;
		cam1.enabled = false;
		cam2.enabled = false;
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

	public void returnFromJoinCanvas()
	{
		JoinGameCanvas.enabled = false;
		MainMenuCanvas.enabled = false;
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
		GM.setIpAddress(_ipInput.text);
		GM.setPort(System.Int32.Parse (_portInput.text));
		GM.initGame(_ipInput.text, System.Int32.Parse (_portInput.text));
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
//		BackgroundCanvas.enabled = true;
		cam1.enabled = true;
		cam2.enabled = true;
	
	}

	public void increaseMapChoice1()
	{
		countMapChoices [0] += 1;
	}

	public void increaseMapChoice2()
	{
		countMapChoices [1] += 1;
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

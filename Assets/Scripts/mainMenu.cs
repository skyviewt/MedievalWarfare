using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class mainMenu : MonoBehaviour {

	public Canvas ExitCanvas;
	public Canvas JoinGameCanvas;
	public Canvas MiniMapCanvas;
	public Transform HostText;
	public Transform JoinText;
	public Transform StatsText;
	public Transform QuitText;

//	private Text _ipPlaceholder;
//	private Text _portPlaceholder;
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
//		_ipPlaceholder = GameObject.Find ("JoinGameCanvas").transform.Find ("ipPlaceholder").GetComponent<Text>();
//		_portPlaceholder = GameObject.Find ("JoinGameCanvas").transform.Find ("portPlaceholder").GetComponent<Text>();
		GM = GameObject.Find("mainMenu").GetComponent<GameManager>();
		JoinGameCanvas = JoinGameCanvas.GetComponent<Canvas>();
		ExitCanvas = ExitCanvas.GetComponent<Canvas> ();
		ExitCanvas.enabled = false;
		JoinGameCanvas.enabled = false;
		MiniMapCanvas.enabled = false;
	}

	public void quitTextPressed()
	{
		ExitCanvas.enabled = true;
		HostText.GetComponent<Button>().enabled = false;
		JoinText.GetComponent<Button>().enabled = false;
		StatsText.GetComponent<Button>().enabled = false;
		QuitText.GetComponent<Button>().enabled = false;
	}

	public void doNotQuitPressed()
	{
		ExitCanvas.enabled = false;
		HostText.GetComponent<Button>().enabled = true;
		JoinText.GetComponent<Button>().enabled = true;
		StatsText.GetComponent<Button>().enabled = true;
		QuitText.GetComponent<Button>().enabled = true;
	}
	public void returnFromJoinCanvas()
	{
		JoinGameCanvas.enabled = false;
		HostText.GetComponent<Button>().enabled = true;
		JoinText.GetComponent<Button>().enabled = true;
		StatsText.GetComponent<Button>().enabled = true;
		QuitText.GetComponent<Button>().enabled = true;
		GM.setIsServer (true);
//		_ipPlaceholder.text = "Enter IP  ...";
//		_portPlaceholder.text = "Enter port...";

	}
	public void joinGameButtonPressed()
	{
		JoinGameCanvas.enabled = true;
		GM.setIsServer (false);
		HostText.GetComponent<Button>().enabled = false;
		JoinText.GetComponent<Button>().enabled = false;
		StatsText.GetComponent<Button>().enabled = false;
		QuitText.GetComponent<Button>().enabled = false;
	}

	public void connectButtonPressed()
	{
		GM.setIpAddress(_ipInput.text);
		GM.setPort(System.Int32.Parse (_portInput.text));
		Application.LoadLevel("scene2");
		GM.initGame(_ipInput.text, System.Int32.Parse (_portInput.text));
	}
	
	public void StartLevel()
	{
		Application.LoadLevel("scene1");
	}

	//TODO
/*	public void OpenStats()
	{
		Application.LoadLevel("stats");
	}*/

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

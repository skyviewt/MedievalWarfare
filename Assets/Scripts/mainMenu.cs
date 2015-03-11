using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class mainMenu : MonoBehaviour {

	public Canvas ExitCanvas;
	public Transform HostText;
	public Transform JoinText;
	public Transform StatsText;
	public Transform QuitText;
	
	// Elements
	public GameObject PrefabFire;
	public GameObject PrefabLight;
	
	// Use this for initialization
	void Start () {
		Instantiate(PrefabFire);
		Instantiate(PrefabLight);
		ExitCanvas = ExitCanvas.GetComponent<Canvas> ();
		ExitCanvas.enabled = false;
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
}

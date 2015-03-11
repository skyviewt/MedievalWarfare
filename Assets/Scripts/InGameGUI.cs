using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InGameGUI : MonoBehaviour {
	public Camera myCamera;
	public Canvas VillageCanvas;
	public Canvas UnitCanvas;
	public Canvas HUDCanvas;

	// prefabs
	public GameObject PeasantPrefab;

	//selections
	private GameObject _Village;
	private GameObject _Unit;
	private GameObject _Tile;
	private GameObject _WoodValue;
	private GameObject _GoldValue;

	public Text _WoodText;
	public Text _GoldText;

	public VillageManager villageManager;
	// Use this for initialization
	void Start () 
	{
		myCamera =  GameObject.FindGameObjectWithTag("MainCamera").camera;
		villageManager = GameObject.Find("VillageManager").GetComponent<VillageManager>();
		HUDCanvas.enabled = true;
		VillageCanvas.enabled = false;

	}
	
	//Functions for when Village is pressed
	public void peasantPressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.hirePeasant (v,PeasantPrefab);
		VillageCanvas.enabled = false;
	}
	public void villageUpgradePressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.upgradeVillage (v);
		VillageCanvas.enabled = false;
	}
	public void closeVillagePressed()
	{
		VillageCanvas.enabled = false;
	}
	// Update is called once per frame
	void Update()
	{

		Ray ray = myCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		//if clicked
		if (Input.GetMouseButtonDown(0))
		{

			if (Physics.Raycast(ray, out hit)){

				switch(hit.collider.tag)
				{
					case "Hovel": case "Town": case "Fort":
					{
						VillageCanvas.enabled = true;
						_Village = hit.collider.gameObject;
						Village v = _Village.GetComponent<Village>();
						int redrawWood = v.getWood();
						int redrawGold = v.getGold();
						_WoodText.text = redrawWood.ToString();
						_GoldText.text = redrawGold.ToString();
						break;
					}
					case "Peasant": case "Infantry": case "Soldier": case "Knight":
					{
						_Unit = hit.collider.gameObject;
						Unit u = _Unit.GetComponent<Unit>();
						Village v = u.getVillage();
						int redrawWood = v.getWood();
						int redrawGold = v.getGold();
						_WoodText.text = redrawWood.ToString();
						_GoldText.text = redrawGold.ToString();
						break;
					}
				}

			}
		}

		if (Input.GetKeyDown (KeyCode.Escape)) 
		{
			if(VillageCanvas.enabled = true)
			{
				VillageCanvas.enabled = false;
			}
			//TODO: bring up the esc menu
			else{

			}
		}
	}

}

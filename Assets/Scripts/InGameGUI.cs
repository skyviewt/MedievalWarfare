using UnityEngine;
using System.Collections;

public class InGameGUI : MonoBehaviour {
	public Camera myCamera;
	public Canvas VillageCanvas;
	public Canvas UnitCanvas;

	// prefabs
	public GameObject PeasantPrefab;

	//selections
	private GameObject _Village;
	private GameObject _Unit;
	private GameObject _Tile;

	public VillageManager villageManager;
	// Use this for initialization
	void Start () {
		myCamera =  GameObject.FindGameObjectWithTag("MainCamera").camera;
		villageManager = GameObject.Find("VillageManager").GetComponent<VillageManager>();
		VillageCanvas.enabled = false;
	}

	public void peasantPressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.hirePeasant (v,PeasantPrefab);
		VillageCanvas.enabled = false;
	}
	
	// Update is called once per frame
	void Update(){

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
						break;
					}

					case "Peasant": case "Infantry": case "Soldier": case "Knight":
					{
						_Unit = hit.collider.gameObject;
						break;
					}

				}
//				if(hit.collider.tag == "Meadow" )
//				{
//					// hit.collider.gameObject
//					print("Meadow");
//
//				}
//
//				else if(hit.collider.tag == "Trees" )
//				{
//					print("Trees");
//				}
//
//				else if(hit.collider.tag == "Grass" )
//				{
//					print("Grass");
//				}

			}
		}
	}

}

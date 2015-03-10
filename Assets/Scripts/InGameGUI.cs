using UnityEngine;
using System.Collections;

public class InGameGUI : MonoBehaviour {
	public Camera myCamera;
	public Canvas VillageCanvas;
	public Transform PeasantText;
	public Transform InfantryText;
	public Transform SoldierText;
	public Transform KnightText;

	public GameObject PeasantPrefab;
	private GameObject selection;
	
	// Use this for initialization
	void Start () {
		myCamera =  GameObject.FindGameObjectWithTag("MainCamera").camera;
		VillageCanvas.enabled = false;
	}

	public void peasantPressed()
	{
		Village v = selection.GetComponent<Village> ();
		Tile tileAt = v.getLocatedAt ();

		Unit p = Unit.CreateComponent (UnitType.PEASANT, tileAt, v, PeasantPrefab);
	
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

				if(hit.collider.tag == "Meadow" )
				{
					// hit.collider.gameObject
					print("Meadow");

				}

				else if(hit.collider.tag == "Trees" )
				{
					print("Trees");
				}

				else if(hit.collider.tag == "Grass" )
				{
					print("Grass");
				}
				if( hit.collider.tag == "Hovel" )
				{
					VillageCanvas.enabled = true;
					selection = hit.collider.gameObject;
					
				}

			}
		}
	}

}

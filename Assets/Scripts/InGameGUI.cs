using UnityEngine;
using System.Collections;

public class InGameGUI : MonoBehaviour {
	public Camera myCamera;

	// Use this for initialization
	void Start () {
		myCamera =  GameObject.FindGameObjectWithTag("MainCamera").camera;
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

			}
		}
	}

}

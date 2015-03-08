using UnityEngine;
using System.Collections;

public class Mouser : MonoBehaviour {
	void Update() {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit, 100)) 
		{
			Debug.DrawLine (ray.origin, hit.point);
			if (Input.GetMouseButtonDown(0)){
				if (hit.collider.tag=="Tile")
				{
					Tile t = hit.collider.GetComponent<Tile>();
					Vector2 pos = t.coord;
					int size = t.myVillage.region.Count;
					string owner = t.myVillage.owner.GetComponent<Player>().myName;
					print ("Tile:"+pos+", Region Size:"+size+", Owner:"+owner);
				}
				//Tile t = hit.collider.GetComponent<Tile>();
				//print (t.coord+" "+t.dir[1]);
				//print (t.coord+" "+t.neighbours.Count);
			}
		}
	}
}

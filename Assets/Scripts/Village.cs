using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Village : MonoBehaviour {
	
	public List<GameObject> region;
	public GameObject owner;
	public int myColor;

	public void setOwner(GameObject p)
	{
		owner = p;
	}

}
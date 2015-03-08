using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum LandType
{
	Grass,
	Trees,
	Meadow
}

public class Tile : MonoBehaviour {

	public Vector2 coord;
	public List<GameObject> neighbours;
	public Vector2[] dir;
	public GameObject tree;
	public GameObject meadow;
	public int myColor;
	public LandType myType;
	public bool isChecked;
	public Village myVillage;
	//private System.Random rand = new System.Random();

	//assign landtype and add decoration
	public void setType(LandType type)
	{	
		GameObject GO;
		Vector3 scale;
		switch (type) 
		{
		case LandType.Trees:
			GO = (GameObject)Instantiate (tree, transform.position, Quaternion.identity); //instantiate decoration
			scale = GO.transform.localScale; //save scaling
			GO.transform.parent = this.transform; //set parent tile
			GO.transform.Translate (0f, 0.1f, 0f); //move the decoration up a bit
			GO.transform.localScale = scale; //rescale the decoration
			GO.transform.eulerAngles = new Vector3(0,Random.Range (0,360),0); //give it a random rotation
			myType = LandType.Trees; //set the landtype attribute
			break;
		case LandType.Meadow:
			GO = (GameObject)Instantiate (meadow, transform.position, Quaternion.identity);
			scale = GO.transform.localScale;
			GO.transform.parent = this.transform;
			GO.transform.Translate (0f, 0.1f, 0f);
			GO.transform.localScale = scale;
			GO.transform.eulerAngles = new Vector3(0,Random.Range (0,360),0);
			myType = LandType.Meadow;
			break;
		case LandType.Grass:
			myType = LandType.Grass;
			break;
		}
	}

	//randomly assign a color based on max number of players
	public void setColor(int color)
	{
		myColor = color; //set tile's color attribute
		switch (color) {
		case 1:
			renderer.material.color = Color.blue;
			break;
		case 2:
			renderer.material.color = Color.red;
			break;
		case 3:
			renderer.material.color = Color.green;
			break;
		case 4:
			renderer.material.color = Color.yellow;
			break;
		case 5:
			renderer.material.color = Color.cyan;
			break;
		case 6:
			renderer.material.color = Color.magenta;
			break;
		}
	}
}

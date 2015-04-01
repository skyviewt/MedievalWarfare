using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public enum LandType
{
	Grass,
	Trees,
	Meadow,
	Tombstone
}

//Tile Data Structure for building Graphs
public class Tile : MonoBehaviour
{
	public Vector2 point;
	public List<Tile> neighbours;
	public LandType myType;
	public Unit occupyingUnit;
	public Structure occupyingStructure;
	public Village myVillage;
	public int color;
	private Shader outline;
	public System.Random rand = new System.Random();
	public GameObject prefab; //holds decoration (tree/meadow)
	public bool hasRoad; // NEEDS TO GET IMPLEMENTED
	public bool visited; // for BFS

	//newly created constructor. This will be called whenever a gameobject containing Tile.cs gets instantiated
	public Tile (){
		visited = false;
		neighbours = new List<Tile>();
	}

	public void addNeighbour(Tile t)
	{
		if(this.neighbours.Where(
			n => n.point.x == t.point.x && n.point.y == t.point.y).Count() == 0)
		{
			this.neighbours.Add(t);
		}
	}

	public bool checkVillagePrefab()
	{
		if (this.myVillage.locatedAt = this)
			return true;
		else 
			return false;
	}

	void Start()
	{
		outline = Shader.Find("Glow");
	}

	public void replace(GameObject newPref)
	{
		Destroy (this.prefab);
		this.prefab = newPref;
	}

	public void colorTile()
	{
		if( this.color == 0 )
			gameObject.renderer.material.color = new Color(1.0f, 0.0f, 1.0f, 0.05f);
		else if ( this.color == 1 )
			gameObject.renderer.material.color = new Color(0.0f, 0.0f, 1.0f, 0.05f);
		else
			gameObject.renderer.material.color = Color.white;
	}
	
	void OnMouseEnter()
	{
		this.renderer.material.shader = outline;
	}

	void OnMouseExit()
	{
		this.renderer.material.shader = Shader.Find("Diffuse");
	}
	/* Is this method needed? Sure towers, but they act as "decoration prefabs" in place of trees and meadows
	public void setStructure(bool b)
	{
		if (b == true) {
			//instantiate an occupyingStructure
		} else {
			//destroy/remove occupyingSructure
		}
	}*/

	// DID NOT WANT TO MESS WITH RPC CALLS

	[RPC]
	public void setLandTypeNet(int type)
	{
		this.myType = (LandType)type;
	}

	[RPC]
	void destroyTile(NetworkViewID tileid){
		Destroy (NetworkView.Find (tileid).gameObject);
	}
	[RPC]
	void setPrefab (NetworkViewID prefID ){
		prefab = NetworkView.Find (prefID).gameObject;
	}
	[RPC]
	void setAndColor(int newColor){
		color = newColor;
		if( color == 0 )
		{
			gameObject.renderer.material.color = new Color(1.0f, 0.0f, 1.0f, 0.05f);
		}
		else if ( color == 1 )
		{
			gameObject.renderer.material.color = new Color(0.0f, 0.0f, 1.0f, 0.05f);
		}
		else if ( color == 2 )
		{
			gameObject.renderer.material.color = Color.white;
		}
	}
	
	[RPC]
	void setPointN(Vector3 pt){
		this.point.x = pt.x;
		this.point.y = pt.z;
	}
	
	[RPC]
	public void addNeighbourN(NetworkViewID tileID)
	{
		Tile t = NetworkView.Find (tileID).GetComponent<Tile>();
		if(this.neighbours.Where(
			n => n.point.x == t.point.x && n.point.y == t.point.y).Count() == 0)
		{
			this.neighbours.Add(t);
		}
	}
	[RPC]
	//replaces the prefab on this tile ie: replace tree with hovel
	void replaceTilePrefabNet(NetworkViewID prefID){
		Destroy (prefab);
		prefab = NetworkView.Find (prefID).gameObject;
	}

	[RPC]
	//sets the village of the tile to new village attached to that gameObject
	void setVillageNet(NetworkViewID villageID){
		myVillage = NetworkView.Find (villageID).gameObject.GetComponent<Village>();
	}

	[RPC]
	void changeMapLayer( bool isMap1)
	{
		if( isMap1 )
		{
			this.gameObject.layer = LayerMask.NameToLayer( "map1" );
		}else{
			this.gameObject.layer = LayerMask.NameToLayer( "map2" );
		}

	}

}
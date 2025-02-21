﻿using UnityEngine;
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
[System.Serializable]
public class Tile : MonoBehaviour
{
	public Vector2 point;
	private List<Tile> neighbours;
	private LandType myType;
	private Unit occupyingUnit;
	private Village myVillage;
	private int color;
	public Shader outline;
	public System.Random rand = new System.Random();
	public GameObject prefab;
	public GameObject myRoad;
	public bool hasRoad;
	public bool hasTower;


	private bool visited;
	public GameObject roadPrefab;
	public GameObject towerPrefab;


	//This function should not be used, the Tile component is now always attached to a Grass Tile
	public static Tile CreateComponent (Vector2 pt, GameObject g) {
		Debug.Log ("----------Tile.CreateComponent() ran--------------");
		Tile myTile = g.AddComponent<Tile>();
		myTile.point = pt;
		myTile.visited = false;
		myTile.neighbours = new List<Tile>();
		return myTile;
	}

	void Awake() {
		Network.minimumAllocatableViewIDs = 10000;
	}

	//newly created constructor. This will be called whenever a gameobject containing Tile.cs gets instantiated
	public Tile (){
		visited = false;
		neighbours = new List<Tile>();
	}
	

	public void addNeighbour(Tile t)
	{
		if(this.getNeighbours().Where(
			n => n.point.x == t.point.x && n.point.y == t.point.y).Count() == 0)
		{
			this.getNeighbours().Add(t);
		}
	}
	//This method shouldn't be called
	public void InstantiateTree( GameObject TreePrefab)
	{
		Debug.Log ("------Tile.InstanciateTree------");
		prefab = Instantiate(TreePrefab, new Vector3(this.point.x, 0.15f, this.point.y), TreePrefab.transform.rotation) as GameObject;
		this.setLandType( LandType.Trees );
	}
	[RPC]
	void switchTilePrefabNet(NetworkViewID gameobjectID)
	{
		this.prefab = NetworkView.Find (gameobjectID).gameObject;
	}
	public bool checkVillagePrefab()
	{
		if (prefab == null) 
		{
			return false;
		} 
		else if (this.prefab.CompareTag ("Town")) 
		{
			return true;
		} 
		else 
		{
			return false;
		}
	}

	//This method shouldn't be called
	public void InstantiateMeadow( GameObject MeadowPrefab )
	{
		Debug.Log ("-----Tile.InstanciateMeadow------");
		prefab = Instantiate(MeadowPrefab, new Vector3(this.point.x, 0.15f, this.point.y), MeadowPrefab.transform.rotation) as GameObject;
		this.setLandType( LandType.Meadow );
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
	
		if( color == 0 )
		{
			gameObject.renderer.material.color = Color.white;
		}
		else if( color == 1 )
		{
			gameObject.renderer.material.color = Color.red;
		}
		else if ( color == 2 )
		{
			gameObject.renderer.material.color = Color.blue;
		}
		else if ( color == 3 )
		{
			gameObject.renderer.material.color = Color.green;
		}
		else if ( color == 4 )
		{
			gameObject.renderer.material.color = Color.yellow;
		}
		else if ( color == 5 )
		{
			gameObject.renderer.material.color = Color.cyan;
		}
		else if ( color == 6 )
		{
			gameObject.renderer.material.color = Color.gray;
		}
	}
	
	void OnMouseEnter()
	{
		this.renderer.material.shader = outline;
	}

	void OnMouseExit()
	{
		this.renderer.material.shader = Shader.Find("Diffuse");
	}

	public void setLandType(LandType type)
	{
		this.myType = type;
	}
	public LandType getLandType()
	{
		return this.myType;
	}
	
	public Unit getOccupyingUnit()
	{
		return this.occupyingUnit;
	}

	public void setOccupyingUnit(Unit u)
	{
		this.occupyingUnit = u;
	}

	public Village getVillage()
	{
		return myVillage;
	}

	public void setVillage(Village v)
	{
		this.myVillage = v;
	}

	public bool checkTower()
	{
		return this.hasTower;
	}

	public List<Tile> getNeighbours()
	{
		return neighbours;
	}

	public int getColor()
	{
		return color;
	}
	
	public void setColor(int i)
	{
		this.color = i;
	}

	public bool getVisited()
	{
		return this.visited;
	}
	
	public void setVisited(bool isVisited)
	{
		this.visited = isVisited;
	}
	/*
	public void buildRoad()
	{
		this.isRoad = true;
		prefab = Instantiate(roadPrefab, new Vector3(this.point.x, 0.11f, this.point.y), roadPrefab.transform.rotation) as GameObject;
	}

	public bool checkRoad()
	{
		return this.isRoad;
	}*/
	[RPC]
	void removeOccupyingUnitNet()
	{
		this.occupyingUnit = null;
	}
	[RPC]
	void setOccupyingUnitNet(NetworkViewID unitID)
	{
		Unit unit = NetworkView.Find (unitID).gameObject.GetComponent<Unit> ();
		this.occupyingUnit = unit;
	}

	[RPC]
	 void setLandTypeNet(int type)
	{
		this.myType = (LandType)type;
	}

	[RPC]
	void destroyTile(NetworkViewID tileid){
		Destroy (NetworkView.Find (tileid).gameObject);
	}

//	[RPC]
//	void DontDestroyTile(NetworkViewID tileID){
//		DontDestroyOnLoad(NetworkView.Find (tileID).gameObject);
//	}

	[RPC]
	void setPrefab (NetworkViewID prefID ){
		prefab = NetworkView.Find (prefID).gameObject;
	}

	[RPC]
	void destroyPrefab ()
	{
		Destroy (prefab);
		prefab = null;
	}

	[RPC]
	void DontDestroyPrefab(NetworkViewID tileID){
		Tile t = NetworkView.Find (tileID).gameObject.GetComponent<Tile>();
		DontDestroyOnLoad(t.prefab);
	}

	[RPC]
	void setAndColor(int newColor){
		color = newColor;
		// 0 is the neutral color
		if( color == 0 )
		{
			gameObject.renderer.material.color = Color.white;
		}
		else if( color == 1 )
		{
			gameObject.renderer.material.color = Color.red;
		}
		else if ( color == 2 )
		{
			gameObject.renderer.material.color = Color.blue;
		}
		else if ( color == 3 )
		{
			gameObject.renderer.material.color = Color.green;
		}
		else if ( color == 4 )
		{
			gameObject.renderer.material.color = Color.yellow;
		}
		else if ( color == 5 )
		{
			gameObject.renderer.material.color = Color.cyan;
		}
		else if ( color == 6 )
		{
			gameObject.renderer.material.color = Color.gray;
		}
	}
	
	[RPC]
	void setPointN(Vector3 pt){
		this.point.x = pt.x;
		this.point.y = pt.z;
	}
	
	[RPC]
	void addNeighbourN(NetworkViewID tileID)
	{
		Tile t = NetworkView.Find (tileID).GetComponent<Tile>();
		if(this.getNeighbours().Where(
			n => n.point.x == t.point.x && n.point.y == t.point.y).Count() == 0)
		{
			this.getNeighbours().Add(t);
		}
	}

	//replaces the prefab on this tile ie: replace tree with hovel
	[RPC]
	void replaceTilePrefabNet(NetworkViewID prefID){
		Destroy (prefab);
		prefab = NetworkView.Find (prefID).gameObject;
	}

	[RPC]
	//sets the village of the tile to new village attached to that gameObject
	void setVillageNet(NetworkViewID villageID){
		this.myVillage = NetworkView.Find (villageID).gameObject.GetComponent<Village>();
	}
	[RPC]
	void neutralizeVillageNet()
	{
		this.myVillage = null;
	}

	[RPC]
	void changeMapLayer( int mapNum )
	{
		bool hasPref = this.getLandType () != LandType.Grass;
		switch (mapNum) {
				case 0:
						this.gameObject.layer = LayerMask.NameToLayer ("map1");
						if (hasPref)
							this.prefab.layer = LayerMask.NameToLayer ("map1");	
						break;
				case 1:
						this.gameObject.layer = LayerMask.NameToLayer ("map2");
						if (hasPref)
							this.prefab.layer = LayerMask.NameToLayer ("map2");
						break;
			//TODO: do this for units and villages.
				case 2:
						this.gameObject.layer = LayerMask.NameToLayer("loadedMap");
						this.gameObject.tag = "LoadedMap";
						if (hasPref)
						{
							this.prefab.layer = LayerMask.NameToLayer ("loadedMap");
							this.prefab.tag = "LoadedMap";
						}
							
						break;
				}
	}

	[RPC]
	void setRoadNet(bool b)
	{
		if (b==true) {
			GameObject road = Network.Instantiate (roadPrefab, new Vector3 (point.x, .11f, point.y), roadPrefab.transform.rotation, 0) as GameObject;
			myRoad = road;
			hasRoad = true;
		} else if (b==false) {
			Destroy (myRoad);
			hasRoad = false;
		}
	}

	[RPC]
	void setStructureNet(bool b){
		if (b==true) {
			this.hasTower = true;
		} else if (b==false) {
			Destroy (prefab);
			this.hasTower = false;
		}
	}

}
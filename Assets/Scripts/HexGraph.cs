using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HexGraph : MonoBehaviour {

	public GameObject hex;
	public GameObject hovel;
	public GameObject villageHolder;

	public int numPlayers;
	public bool addNeutral;
	private int maxPlayers;
	public int rows;
	public int cols;

	private Hashtable map;
	private List<GameObject> villages;
	private List<GameObject> playerList;

	private float hexWidth;
	private float hexHeight;

	//private System.Random rand = new System.Random();

	//determine size of hex
	void setSizes()
	{
		hexWidth = hex.renderer.bounds.size.x;
		hexHeight = hex.renderer.bounds.size.z;
	}

	//create grid and initialize tiles
	void createGrid(){
		for (int y=0; y<rows; y++) {
			float offset = 0;
			if (y%2!=0) offset = hexHeight/2; //offset the odd rows
			for (int x=0; x<cols; x++){
				Vector3 pos = new Vector3(y*hexWidth*3/4,0,x*hexHeight+offset);
				GameObject GO = (GameObject)Instantiate (hex, pos, Quaternion.identity);
				GO.transform.parent=this.transform; //attach new hexes to map object in scene
				Tile t = GO.GetComponent<Tile>(); 
				Vector2 key = new Vector2(x,y);

				//initialize the tile and it's list of neighbours
				t.neighbours = new List<GameObject>();
				t.isChecked = false;
				t.coord = key;
				setDir (t);

				//randomly assign Landtypes
				int type = Random.Range (1, 11);
				if (type < 3) // 20% trees
					t.setType (LandType.Trees);
				else if (type == 3) // 10% meadows
					t.setType (LandType.Meadow);
				else
					t.setType (LandType.Grass);  

				//setOwner/color here
				maxPlayers = numPlayers;
				if (addNeutral) //add extra neutral space
					maxPlayers++;
				int color = Random.Range (1, maxPlayers+1);
				t.setColor (color);

				//finally add tile to map listing
				map.Add (key, GO);
			}
		}
	}

	//set directions (aka coordinates) of neighbours
	void setDir(Tile t)
	{
		//tile coords = x,y
		t.dir = new Vector2[6];
		if (t.coord.y % 2 == 0) {
			t.dir [0] = new Vector2 (t.coord.x + 1, t.coord.y);
			t.dir [1] = new Vector2 (t.coord.x, t.coord.y - 1);
			t.dir [2] = new Vector2 (t.coord.x - 1, t.coord.y - 1);
			t.dir [3] = new Vector2 (t.coord.x - 1, t.coord.y);
			t.dir [4] = new Vector2 (t.coord.x - 1, t.coord.y + 1);
			t.dir [5] = new Vector2 (t.coord.x, t.coord.y + 1);
		} else {
			t.dir [0] = new Vector2 (t.coord.x + 1, t.coord.y);
			t.dir [1] = new Vector2 (t.coord.x + 1, t.coord.y - 1);
			t.dir [2] = new Vector2 (t.coord.x, t.coord.y - 1);
			t.dir [3] = new Vector2 (t.coord.x - 1, t.coord.y);
			t.dir [4] = new Vector2 (t.coord.x, t.coord.y + 1);
			t.dir [5] = new Vector2 (t.coord.x + 1, t.coord.y + 1);
		}
	}

	void setNeighbours()
	{
		foreach (DictionaryEntry DE in map) {
			GameObject GO = (GameObject)DE.Value;
			Tile t = GO.GetComponent<Tile> ();
			for (int i=0; i<6; i++) {
				GameObject n = (GameObject)map [t.dir [i]];
				if (n != null)
					t.neighbours.Add (n);
			}
		}
	}

	//search for connected regions, spawn villages for each
	void initializeRegions()
	{
		foreach (DictionaryEntry DE in map) 
		{
			GameObject GO = (GameObject)DE.Value;
			Tile t = GO.GetComponent<Tile> ();
			if (!t.isChecked){
				GameObject town = (GameObject)Instantiate (hovel, GO.transform.position, Quaternion.identity);
				town.transform.parent = villageHolder.transform;
				Village v = town.GetComponent<Village>();
				v.region = new List<GameObject>();
				v.myColor = t.myColor;
				BFS (GO, t, v);
				villages.Add (town);
			}
		}
	}

	void BFS(GameObject GO, Tile t, Village v)
	{
		if (!t.isChecked) 
		{
			t.isChecked=true;
			t.myVillage=v;
			v.region.Add (GO);
			foreach (GameObject n in t.neighbours)
			{
				Tile s = n.GetComponent<Tile> ();
				if (!s.isChecked && (t.myColor == s.myColor))
					BFS (n,s,v);
			}
		}
	}

	//remove regions that are small or controlled by neutral
	void removeRegions(){
		List<GameObject> toRemove = new List<GameObject> ();
		foreach (GameObject town in villages){
			Village v = town.GetComponent<Village>();
			if ((v.region.Count<3)||(addNeutral&&v.myColor==maxPlayers)) //if it's a bad region
			{
				foreach (GameObject tile in v.region) //white out each tile
				{
					tile.renderer.material.color = Color.white;
				}
				toRemove.Add (town); //add region to list to remove later
			}
		}
		foreach (GameObject v in toRemove) //remove region and destroy village object
		{
			villages.Remove (v);
			Destroy (v);
		}
	}

	//add good villages to player's control
	//could definitely rename a lot of the vars to be clearer
	//x=village object, v=village script
	//y=player object, p=player script
	//t=tile object
	void initializeVillages()
	{
		foreach (GameObject x in villages) {
			Village v = x.GetComponent<Village>();
			foreach (GameObject y in playerList){
				Player p = y.GetComponent<Player>();
				if (v.myColor == p.myColor){
					p.myVillages.Add (x);
					v.setOwner(y);
				}
			}
			//move the village to a random tile
			int maxTiles = v.region.Count;
			int ranTile = Random.Range (0,maxTiles);
			GameObject t = v.region[ranTile];
			x.transform.position = t.transform.position;
		}
	}

	void Start()
	{
		map = new Hashtable();
		villages = new List<GameObject>();
		playerList = new List<GameObject>();
		GameObject[] gos;
		gos = GameObject.FindGameObjectsWithTag ("Player");
		foreach (GameObject go in gos) {
			Player p = go.GetComponent<Player>();
			p.myVillages = new List<GameObject>();
			playerList.Add (go);
		}
		setSizes ();
		createGrid ();
		setNeighbours ();
		//remove outside tiles here
		initializeRegions ();
		removeRegions ();
		initializeVillages ();
		//print (villages.Count);
	}

}

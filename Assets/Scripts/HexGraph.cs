using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HexGraph : MonoBehaviour {

	public GameObject hex;

	public int numPlayers;
	public bool addNeutral;
	private int maxPlayers;
	public int rows;
	public int cols;

	public Hashtable map;
	public List<Village> villages;

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
				//t.initialize (key,maxPlayers+1);
				t.neighbours = new List<GameObject>();
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
					//randomly generate number
					//then call setOwner with param
						//tile will recolor itself and set its owner
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

	void initializeRegions()
	{
		foreach (DictionaryEntry DE in map) 
		{
			GameObject GO = (GameObject)DE.Value;
			Tile t = GO.GetComponent<Tile> ();
			if (!t.isChecked){
				Village v = new Village();
				v.region = new List<GameObject>();
				BFS (GO, t, v);
				villages.Add (v);
			}
		}
	}

	void BFS(GameObject GO, Tile t, Village v)
	{
		if (!t.isChecked) 
		{
			t.isChecked=true;
			v.region.Add (GO);
			foreach (GameObject n in t.neighbours)
			{
				Tile s = n.GetComponent<Tile> ();
				if (!s.isChecked && (t.myColor == s.myColor))
					BFS (n,s,v);
			}
		}
	}

	void removeRegions(){
		foreach (Village v in villages) {
			if (v.region.Count<3)
			{
				foreach (GameObject GO in v.region)
				{
					GO.renderer.material.color = Color.white;
				}
			}
		}
	}

	void Start()
	{
		map = new Hashtable();
		villages = new List<Village>();
		maxPlayers = numPlayers;
		if (addNeutral)
			maxPlayers++;
		setSizes ();
		createGrid ();
		setNeighbours ();
		//remove outside tiles here
		initializeRegions ();
		removeRegions ();
		//print (villages.Count);
	}

}

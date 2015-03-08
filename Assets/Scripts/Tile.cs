using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum LandType
{
	Grass,
	Trees,
	Meadow,
	TombStone
}

//Tile Data Structure for building Graphs
public class Tile : MonoBehaviour
{
	public Vector2 point;
	public List<Tile> neighbours;
	private LandType myType;
	private Unit occupyingUnit;
	private bool visited;
	private int color;
	private bool isRoad;
	private Village myVillage;
	public Shader outline;
	private System.Random rand = new System.Random();
	public GameObject prefab;
	private Structure occupyingStructure;


	public Tile()
	{
		this.point = new Vector2();
		neighbours = new List<Tile>();
		visited = false;
	}

	public static Tile CreateComponent (Vector2 pt, GameObject g) {
		Tile myTile = g.AddComponent<Tile>();
		myTile.point = pt;
		myTile.visited = false;
		myTile.color = myTile.rand.Next (0, 3);

		return myTile;
	}

	public Tile(Vector2 pt)
	{
		this.point = pt;
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

	public void InstantiateTree( GameObject TreePrefab)
	{
		prefab = Instantiate(TreePrefab, new Vector3(this.point.x, 0, this.point.y), TreePrefab.transform.rotation) as GameObject;
		this.setLandType( LandType.Trees );
		this.colorTile(prefab);
	}

	public void InstantiateMeadow( GameObject MeadowPrefab )
	{
		prefab = Instantiate(MeadowPrefab, new Vector3(this.point.x, 0, this.point.y), MeadowPrefab.transform.rotation) as GameObject;
		this.setLandType( LandType.Meadow );
		this.colorTile( prefab );
	}

	public void InstantiateGrass( GameObject GrassPrefab )
	{
		prefab = Instantiate(GrassPrefab, new Vector3(this.point.x, 0, this.point.y), GrassPrefab.transform.rotation) as GameObject;
		this.setLandType( LandType.Grass );
		this.colorTile( prefab );
	}

	void Start()
	{
		outline = Shader.Find("Glow");
	}

	public void replace(GameObject pref)
	{
		Destroy (this.prefab);
		GameObject newPref = Instantiate(pref, new Vector3(this.point.x, 0, this.point.y), pref.transform.rotation) as GameObject;
		newPref.AddComponent("Tile");
		this.colorTile ( newPref );
		this.prefab = newPref;
	}

	public void colorTile(GameObject pref)
	{

		if( this.getLandType() != LandType.Grass )
		{
			Transform child = pref.transform.Find("Grass");
			print (child);
			if( this.color == 0 )
			{
				child.renderer.material.color = new Color(1.0f, 0.0f, 1.0f, 0.05f);
			}
			else if ( this.color == 1 )
			{
				child.renderer.material.color = new Color(0.0f, 0.0f, 1.0f, 0.05f);
			}
			return;
		}
		else if (this.getLandType() == LandType.Grass )
		{
			if(this.color == 0 )
			{
				pref.renderer.material.color = new Color(1.0f, 0.0f, 1.0f, 0.05f);
			}
			else if ( this.color == 1 )
			{
				pref.renderer.material.color = new Color(0.0f, 0.0f, 1.0f, 0.05f);
			}
		}
	}
	
	void OnMouseEnter()
	{
		switch (this.tag) {
		case "Grass":
			this.renderer.material.shader = outline;
			break;
		case "Trees":
		case "Meadow":
		case "Hovel":
			Transform child = transform.Find("Grass");
			child.renderer.material.shader = outline;
			break;
		}
	}

	void OnMouseExit()
	{
		switch (this.tag) {
		case "Grass":
			this.renderer.material.shader = Shader.Find("Diffuse");
			print (this.color);
			break;
		case "Trees":
		case "Meadow":
		case "Hovel":
			Transform child = transform.Find("Grass");
			child.renderer.material.shader = Shader.Find("Diffuse");
			print (this.color);
			break;
		}
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

	public List<Tile> getNeighbours(Tile t)
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

	public void buildRoad()
	{
		this.isRoad = true;
	}

	public bool canUnitMove(UnitType type)
	{
		if(occupyingStructure == null || myType != LandType.Trees)
		{
			return true;
		}
		else if(occupyingStructure != null || (type == UnitType.KNIGHT && myType == LandType.Trees) || occupyingUnit != null)
		{
			return false;
		}
	}
}
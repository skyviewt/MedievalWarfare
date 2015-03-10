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
	public System.Random rand = new System.Random();
	public GameObject prefab;
	private Structure occupyingStructure;



	public static Tile CreateComponent (Vector2 pt, GameObject g) {
		Tile myTile = g.AddComponent<Tile>();
		myTile.point = pt;
		myTile.visited = false;
		myTile.neighbours = new List<Tile>();
		return myTile;
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
		prefab = Instantiate(TreePrefab, new Vector3(this.point.x, 0.2f, this.point.y), TreePrefab.transform.rotation) as GameObject;
		this.setLandType( LandType.Trees );
	}

	public void InstantiateMeadow( GameObject MeadowPrefab )
	{
		prefab = Instantiate(MeadowPrefab, new Vector3(this.point.x, 0.2f, this.point.y), MeadowPrefab.transform.rotation) as GameObject;
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
		if( this.color == 0 )
		{
			gameObject.renderer.material.color = new Color(1.0f, 0.0f, 1.0f, 0.05f);
		}
		else if ( this.color == 1 )
		{
			gameObject.renderer.material.color = new Color(0.0f, 0.0f, 1.0f, 0.05f);
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

	void Update()
	{
		colorTile ();
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

	public void buildRoad()
	{
		this.isRoad = true;
	}

	public bool checkRoad()
	{
		return this.isRoad;
	}

	public bool canUnitMove(UnitType type)
	{
		if(occupyingStructure == null || myType != LandType.Trees)
		{
			return true;
		}
		else //if(occupyingStructure != null || (type == UnitType.KNIGHT && myType == LandType.Trees) || occupyingUnit != null)
		{
			return false;
		}
	}
}
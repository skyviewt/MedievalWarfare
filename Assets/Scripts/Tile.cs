using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum LandType
{
	Grass,
	Trees,
	Meadow
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
	private Village myVillage;
	public Shader outline;


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

	void Start()
	{
		outline = Shader.Find("Glow");
	}

	void OnMouseEnter()
	{
		this.renderer.material.shader = outline;
	}

	void OnMouseExit()
	{
		this.renderer.material.shader = Shader.Find("Transparent/Diffuse");;
	}

	public void setLandType(LandType type)
	{
		this.myType = type;
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

	public int getColor()
	{
		return color;
	}
	
	public void setColor(int i)
	{
		this.color = i;
	}

}
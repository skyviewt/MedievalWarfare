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
public class Tile
{
	public Vector2 point;
	public List<Tile> neighbours;
	private LandType myType;
	private Unit occupyingUnit;
	private bool visited;
	private int color;
	private Village myVillage;


	public Tile()
	{
		this.point = new Vector2();
		neighbours = new List<Tile>();
		visited = false;
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

}
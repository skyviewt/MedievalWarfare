using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Tile Data Structure for building Graphs
public class Tile
{
	public Vector2 point;
	public List<Tile> neighbours;
	
	public Tile()
	{
		this.point = new Vector2();
		neighbours = new List<Tile>();
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
}
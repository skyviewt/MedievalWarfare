﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//The Graph Class used to store the map as a grid
[System.Serializable]
public class Graph
{
	private List<Tile> vertices;
	private Tile start;
	private Tile end;
	
	public Graph()
	{
		this.vertices = new List<Tile>();
	}
	
	public Graph(Tile firstTile, Tile endTile)
	{
		this.vertices = new List<Tile>();
		this.start = firstTile;
		this.end = endTile;
		this.vertices.Add(firstTile);
		
	}
	public Tile getStartingTile()
	{
		return this.start;
	}
	public Tile getEndingTile()
	{
		return this.end;
	}
	public List<Tile> getVertices()
	{
		return this.vertices;
	}
	public bool addTileUnique(Tile t)
	{
		if (!this.Contains(t.point.x, t.point.y)) 
		{
			this.vertices.Add(t);
			return true;
		}
		return false;
	}

	public bool Contains(float x, float y)
	{
		return this.vertices.Where(tile => (tile.point.x == x && tile.point.y == y)).Count() > 0;
	}
	
	public Tile GetTile(float x, float y)
	{
		return this.vertices.Where(tile => (tile.point.x == x && tile.point.y == y)).FirstOrDefault();
	}
	
	public bool isEnd(Tile t)
	{
		return t.point.x == this.end.point.x && t.point.y == this.end.point.y;
	}
	
	public bool isStart(Tile t)
	{
		return t.point.x == this.start.point.x && t.point.y == this.start.point.y;
	}
}
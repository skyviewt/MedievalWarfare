﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class TileManager : MonoBehaviour {

	public bool isInGame;
	// Use this for initialization
	void Start () {
			
	}

	public bool checkNeighboursForGuards(Tile center, Unit unit)
	{
		List<Tile> neighbours = center.getNeighbours ();
		UnitType unitType = unit.getUnitType();
		Village unitVillage = unit.getVillage ();
		Player unitVillagePlayer = unitVillage.getPlayer ();

		foreach (Tile n in neighbours) 
		{
			Unit neighbouringUnit = n.getOccupyingUnit();
			if(neighbouringUnit != null)
			{
				UnitType neighbourUnitType = neighbouringUnit.getUnitType();
				Village neighbourVillage = neighbouringUnit.getVillage ();
				Player neighbourPlayer = neighbourVillage.getPlayer ();
				if(unitType <= neighbourUnitType && unitVillagePlayer != neighbourPlayer)
				{
					return true;
				}
			}
			bool hasTower = n.checkTower ();
			if (hasTower && unitType > UnitType.INFANTRY){
				return true;
			}
		}
		return false;
	}

	[RPC]
	void DontDestroyTileManager(NetworkViewID mID)
	{
		DontDestroyOnLoad(NetworkView.Find (mID).gameObject);
	}

	[RPC]
	void destroyTile(NetworkViewID tileID){
		Destroy (NetworkView.Find (tileID).gameObject);
	}
	
	[RPC]
	void DontDestroyTile(NetworkViewID tileID){
		DontDestroyOnLoad(NetworkView.Find (tileID).gameObject);
	}

	/* DEPRECATED
	public void removeUnitFromTile(Tile t, Unit u)
	{
		t.setOccupyingUnit (null);
		u.setLocation (null);
		t.setLandType(LandType.Tombstone);
	}*/
}

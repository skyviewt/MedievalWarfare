using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class TileManager : MonoBehaviour {

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
				if(unitType < neighbourUnitType && unitVillagePlayer != neighbourPlayer)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void removeUnitFromTile(Tile t, Unit u)
	{
		t.setOccupyingUnit (null);
		u.setLocation (null);
		t.setLandType(LandType.Tombstone);
	}
}

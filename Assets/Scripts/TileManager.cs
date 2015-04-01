using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class TileManager : MonoBehaviour {

	public bool checkNeighboursForGuards(Tile center, Unit unit)
	{
		List<Tile> neighbours = center.neighbours;
		UnitType unitType = unit.myType;
		Village unitVillage = unit.myVillage;
		Player unitVillagePlayer = unitVillage.controlledBy;

		foreach (Tile n in neighbours) 
		{
			Unit neighbouringUnit = n.occupyingUnit;
			if(neighbouringUnit != null)
			{
				UnitType neighbourUnitType = neighbouringUnit.myType;
				Village neighbourVillage = neighbouringUnit.myVillage;
				Player neighbourPlayer = neighbourVillage.controlledBy;
				if(unitType <= neighbourUnitType && unitVillagePlayer != neighbourPlayer)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void removeUnitFromTile(Tile t, Unit u)
	{
		t.occupyingUnit = null;
		u.locatedAt = null;
		t.myType = LandType.Tombstone;
	}
}

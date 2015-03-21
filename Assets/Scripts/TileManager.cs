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
		UnitType centerUnitType = unit.getUnitType ();
		Village centerUnitVillage = unit.getVillage ();
		Player centerVillagePlayer = centerUnitVillage.getPlayer ();

		foreach (Tile n in neighbours) 
		{
			Unit neighbouringUnit = n.getOccupyingUnit();
			if(neighbouringUnit != null)
			{
				UnitType neighbourUnitType = neighbouringUnit.getUnitType();
				Village neighbourVillage = neighbouringUnit.getVillage ();
				Player neighbourPlayer = neighbourVillage.getPlayer ();
				if(centerUnitType < neighbourUnitType && centerVillagePlayer != neighbourPlayer)
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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TurnManager : MonoBehaviour {

	public void beginTurn(Game g, Player p)
	{
		g.setTurn (p);
		List<Village> villagesToUpdate = p.getVillages ();
		foreach (Village v in villagesToUpdate)
		{
			this.updateVillages(v);
		}
	}

	public void updateVillages(Village v)
	{
		List<Tile> controlledRegion = v.getControlledRegion ();
		foreach (Tile tile in controlledRegion)
		{
			LandType type = tile.getLandType();
			if (type == LandType.TombStone)
			{
				tile.setLandType(LandType.Trees);
			}

			Unit unitOnTile = tile.getOccupyingUnit(); //grabs the occupying unit on tile
			if (unitOnTile != null)
			{
				UnitActionType action = unitOnTile.getAction(); //get the action of the unit on tile

				if (action == UnitActionType.StartCultivating)
				{
					unitOnTile.setAction(UnitActionType.FinishCultivating);
				}
				if (action == UnitActionType.FinishCultivating)
				{
					unitOnTile.setAction(UnitActionType.ReadyForOrders);
					tile.setLandType(LandType.Meadow);
				}
				if (action == UnitActionType.BuildingRoad)
				{
					unitOnTile.setAction(UnitActionType.ReadyForOrders);
					tile.buildRoad();
				}
			}

			if (type == LandType.Grass)
			{
				v.addGold(1); //add gold by 1

			}
			if (type == LandType.Meadow)
			{

				v.addGold(2); //add gold by 2
			}
		}

		int totalWages = v.getTotalWages ();
		int villageGold = v.getGold ();
		if (villageGold >= totalWages) { //means have enough money to pay units
			villageGold = villageGold - totalWages;
		} 
		else {
			v.retireAllUnits();
		}
	}
}

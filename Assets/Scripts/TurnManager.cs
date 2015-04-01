using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class TurnManager : MonoBehaviour {

	public void beginTurn(Game g, Player p)
	{
		g.setTurn (p);
		List<Village> villagesToUpdate = p.myVillages;
		foreach (Village v in villagesToUpdate)
		{
			this.updateVillages(v);
		}
	}

	public void updateVillages(Village v)
	{
		List<Tile> controlledRegion = v.controlledRegion;
		foreach (Tile tile in controlledRegion)
		{
			LandType type = tile.myType;
			if (type == LandType.Tombstone)
			{
				tile.myType=(LandType.Trees);
			}

			Unit unitOnTile = tile.occupyingUnit; //grabs the occupying unit on tile
			if (unitOnTile != null)
			{
				UnitActionType action = unitOnTile.myAction; //get the action of the unit on tile

				if (action == UnitActionType.StartCultivating)
				{
					unitOnTile.myAction=(UnitActionType.FinishCultivating);
				}
				if (action == UnitActionType.FinishCultivating)
				{
					unitOnTile.myAction=(UnitActionType.ReadyForOrders);
					tile.myType=(LandType.Meadow);
				}
				if (action == UnitActionType.BuildingRoad)
				{
					unitOnTile.myAction=(UnitActionType.ReadyForOrders);
					tile.hasRoad=true;
				}
			}

			if (type == LandType.Grass)
			{
				v.gold+=1; //add gold by 1

			}
			if (type == LandType.Meadow)
			{

				v.gold+=(2); //add gold by 2
			}
		}

		int totalWages = v.getTotalWages ();
		int villageGold = v.gold;
		if (villageGold >= totalWages) { //means have enough money to pay units
			villageGold = villageGold - totalWages;
		} 
		else {
			v.retireAllUnits();
		}
	}
}

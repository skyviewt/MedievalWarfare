using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class UnitManager : MonoBehaviour {

	private VillageManager villageManager;
	private TileManager tileManager;
	private InGameGUI gameGUI;
	private readonly int TEN = 10;
	// Use this for initialization

	void Start () {
		villageManager = GameObject.Find ("VillageManager").GetComponent<VillageManager>();
		tileManager = GameObject.Find ("TileManager").GetComponent<TileManager> ();
		gameGUI = GameObject.Find ("attachingGUI").GetComponent<InGameGUI>();
	}

	[RPC]
	void moveUnitNet(NetworkViewID unitID, NetworkViewID tileID){
		Unit unitToMove = NetworkView.Find (unitID).gameObject.GetComponent<Unit>();
		Tile dest = NetworkView.Find (tileID).gameObject.GetComponent<Tile>();
		moveUnit (unitToMove, dest);
	}
	
	public void moveUnit(Unit unit, Tile dest)
	{
		//print ("----in move unit----");
		Village destVillage = dest.myVillage;
		Village srcVillage = unit.myVillage;
		
		Unit destUnit = dest.occupyingUnit;
		UnitType srcUnitType = unit.myType;
		
		bool unitPermitted = canUnitMove (srcUnitType, dest);
		
		//if the move is allowed to move onto the tile
		if (unitPermitted == true ) 	
		{
			Tile originalLocation = unit.locatedAt;
			// moving within your region
			if (srcVillage == destVillage)
			{
				performMove(unit,dest);
				originalLocation.occupyingUnit = null;
			}
			else if (srcVillage != destVillage)
			{
				// taking over neutral tiles
				if (destVillage == null)
				{
					srcVillage.addTile(dest);
					performMove(unit,dest);
					villageManager.MergeAlliedRegions(dest);
					unit.myAction = (UnitActionType.CapturingNeutral);
					originalLocation.occupyingUnit = null;
				}

				// taking over enemy tiles
				//TODO this part of the code needs network components
				else if (srcUnitType != UnitType.PEASANT)
				{
					bool isGuardSurrounding = tileManager.checkNeighboursForGuards(dest,unit);
					print("Guard surrounding is " + isGuardSurrounding);
					if (isGuardSurrounding == false)
					{
						if (destUnit != null)
						{
							print ("made it here 3");
							UnitType destUnitType = destUnit.myType;
							if (srcUnitType > destUnitType)
							{
								villageManager.removeUnitFromVillage(destVillage,destUnit); // remove relationship between V and U
								villageManager.removeTileFromVillage(destVillage,dest);		// remove relationship vetween V and T
								tileManager.removeUnitFromTile(dest,destUnit);				// remove relationship between T and U
								//TODO destroy the unit prefab
								//TODO create a tombstone prefab ontop of Tile
								villageManager.takeoverTile(srcVillage,dest);
								performMove(unit,dest);
								unit.myAction = (UnitActionType.CapturingEnemy);
								villageManager.MergeAlliedRegions(dest);
								originalLocation.occupyingUnit = null;
							}
							else if(srcUnitType <= destUnitType)
							{
								print("your unit is equal or weaker than the unit on the tile");
							}
						}
						else if (destUnit == null)
						{
							bool destHasVillagePrefab = dest.checkVillagePrefab();
							if(destHasVillagePrefab && srcUnitType <= UnitType.INFANTRY)
							{
								print("infantry is not brave enough to invade a village");
								return;
							}
							else if(destHasVillagePrefab && srcUnitType > UnitType.INFANTRY)
							{
								villageManager.plunderVillage(srcVillage, destVillage,dest);
								performMove(unit,dest);
								villageManager.takeoverTile(srcVillage,dest);
							}
							else
							{
								performMove(unit,dest);
								villageManager.takeoverTile(srcVillage,dest);
							}
							unit.myType = (UnitActionType.CapturingEnemy);
							villageManager.MergeAlliedRegions(dest);
							originalLocation.occupyingUnit = null;
						}
						print ("after");
					}
				}
			}
		}

	}

	private void movePrefab(Unit u, Vector3 vector)
	{
		u.transform.localPosition = vector;
	}

	private void performMove(Unit unit, Tile dest)
	{
		dest.occupyingUnit = (unit);
		unit.locatedAt = (dest);
		Village srcVillage = unit.myVillage;
		UnitType srcUnitType = unit.myType;
		LandType destLandType = dest.myType;

		if (srcUnitType == UnitType.KNIGHT) 
		{
			if (destLandType == LandType.Meadow && !dest.hasRoad) 
			{
				dest.myType=LandType.Grass;
				Destroy (dest.prefab);
			}
			unit.myAction = (UnitActionType.Moved);
		} 
		else
		{
			//Debug.LogError("HERREEE in else");
			if (destLandType == LandType.Trees)
			{
				//print ("entered cutting trees");
				unit.myAction=(UnitActionType.ChoppingTree);
				//unit.animation.CrossFade("attack");
				Destroy (dest.prefab);
				dest.prefab = null;

				//unit.animation.CrossFade("idle");
				srcVillage.wood++;
				dest.myType=LandType.Grass;
			}
			else if (destLandType == LandType.Tombstone)
			{
				unit.myAction=(UnitActionType.ClearingTombstone);
				dest.myType=LandType.Grass;
			}
		}
		movePrefab (unit, new Vector3 (dest.point.x, 0.15f,dest.point.y));
	}

	private bool canUnitMove(UnitType type, Tile dest)
	{
		if (dest.occupyingStructure == null && dest.occupyingUnit == null && dest.myType != LandType.Trees) 
		{
			return true;
		} 
		else if(dest.myType == LandType.Trees && type != UnitType.KNIGHT)
		{
			return true;
		} 
		else if (dest.occupyingStructure != null) 
		{
			gameGUI.displayError (@"The tower doesn't want you to stand ontop of it. ¯\(°_o)/¯");
			return false;
		} 
		else if (type == UnitType.KNIGHT && dest.myType==LandType.Trees) 
		{
			gameGUI.displayError (@"Your Knight is out of shape. It cannot cut down this tree. ¯\(°_o)/¯");
			return false;
		} 
		else if (dest.occupyingUnit != null) 
		{
			gameGUI.displayError (@"There is a unit already standing there!!! ¯\(°_o)/¯");
			return false;
		}

		return false;
	}

	public void upgradeUnit(Unit u, UnitType newLevel)
	{
		Village unitVillage = u.myVillage;
		VillageType unitVillageLevel = unitVillage.myType;
		UnitType unitType = u.myType;
		UnitActionType unitAction = u.myAction;
		int goldAvailable = unitVillage.gold;
		int goldRequired = (newLevel - unitType) * TEN;
		if (unitType == UnitType.KNIGHT) {
			gameGUI.displayError (@"The Knight is already your strongest warrior! ¯\(°_o)/¯");
		}
		else if((goldAvailable >= goldRequired)&&(newLevel > unitType)&&(unitAction == UnitActionType.ReadyForOrders || unitAction == UnitActionType.Moved))
		{
			if(newLevel == UnitType.SOLDIER && unitVillageLevel < VillageType.Town)
			{
				gameGUI.displayError (@"Please upgrade your village to a Town first. ¯\(°_o)/¯");
			}
			else if(newLevel == UnitType.KNIGHT && unitVillageLevel < VillageType.Fort)
			{
				gameGUI.displayError (@"Please upgrade your village to a Fort first. ¯\(°_o)/¯");
			}
			else
			{
				unitVillage.gold = (goldAvailable - goldRequired);
				u.upgrade(newLevel);
			}
		}
	}

	[RPC]
	void upgradeUnitNet(NetworkViewID unitID, int newlvl){
		Unit u = NetworkView.Find (unitID).gameObject.GetComponent<Unit>();
		upgradeUnit (u, (UnitType)newlvl);
	}
}

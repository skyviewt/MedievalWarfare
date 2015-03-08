using UnityEngine;
using System.Collections;

public class VillageManager : MonoBehaviour {
	public readonly int EIGHT = 8;
	public readonly int ONE = 1;
	// Use this for initialization
	void Start () {
		
	}

	public void upgradeVillage(Village v)
	{
		int vWood = v.getWood ();
		VillageType vType = v.getMyType ();
		VillageActionType vAction = v.getAction ();
		if ((vType != VillageType.Fort) && (vWood >= EIGHT) && (vAction == VillageActionType.ReadyForOrders)) 
		{
			v.upgrade ();
		}
	}

	public void takeOverTile(Tile destination)
	{

	}
}

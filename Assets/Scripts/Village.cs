using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public enum VillageActionType
{
	ReadyForOrders,
	BuildStageOne
};

[System.Serializable]
public enum VillageType
{
	Hovel,
	Town,
	Fort,
	Castle
}

[System.Serializable]
public class Village : MonoBehaviour {

	public List<Tile> controlledRegion;
	public Player controlledBy;
	public Tile locatedAt;
	public List<Unit> supportedUnits;
	public VillageType myType;
	public VillageActionType myAction;
	public int gold;
	public int wood;
	public Shader outline;
	public VillageManager vm;
	
	// Use this for initialization
	void Start()
	{
		outline = Shader.Find("Glow");
		GameObject go = GameObject.Find("VillageManager");
		vm = go.GetComponent<VillageManager> ();

	}

	void OnMouseEnter()
	{
		this.renderer.material.shader = outline;
		foreach (Transform child in transform)
		{
			child.renderer.material.shader = outline;
			foreach (Transform childchild in child)
			{
				childchild.renderer.material.shader = outline;
			}
		}
	}
	
	void OnMouseExit()
	{
		this.renderer.material.shader = Shader.Find("Diffuse");
		foreach (Transform child in transform)
		{
			child.renderer.material.shader = Shader.Find("Diffuse");
			foreach (Transform childchild in child)
			{
				childchild.renderer.material.shader = Shader.Find("Diffuse");
			}
		}
	}

	//constructor
	public static Village CreateComponent ( Player p, List<Tile> regions, Tile location, GameObject locationPrefab ) 
	{
		Debug.Log ("-----Village.CreateComponent() CALLED--------");
		Village myVillage = locationPrefab.AddComponent<Village>();
		myVillage.controlledRegion = regions;//todo
		myVillage.controlledBy = p;//TODO: Currently set in Mapgenerator
		myVillage.myType = VillageType.Hovel;
		myVillage.supportedUnits = new List<Unit> ();
		locatedAt.replace (locationPrefab);//to check : Set in Mapgenerator
		myVillage.locatedAt = locatedAt;//done
		locatedAt = location;//done
		myVillage.myAction = VillageActionType.ReadyForOrders;
		myVillage.gold = 0;
		myVillage.wood = 0;
		//TODO: set controlledRegions in tiles : Call updateControlledRegionNet()
		foreach (Tile t in myVillage.controlledRegion) 
		{
			t.myVillage = myVillage;
		}
		return myVillage;
	}

	//new constructor
	public Village(){
		myType = VillageType.Hovel;
		supportedUnits = new List<Unit> ();
		//need to be set over network: controlledRegion, controlledBy, locatedAt.Replace(), locatedAt, locatedAt.setVillage(), pdateControlledRegionNet()
		controlledRegion = new List<Tile> ();
		myAction = VillageActionType.ReadyForOrders;
		gold = 0;
		wood = 0;
		//set controlled regions
	}

	[RPC]
	//adds a single tile to the controlledRegion. To add multiple tiles, use a loop
	void addTileNet(NetworkViewID tileID){
		Tile t = NetworkView.Find (tileID).GetComponent<Tile>();
		controlledRegion.Add (t);
	}

	[RPC]
	//set the location of the village by getting the Tile component of the GrassTile Prefab with that tileID 
	void setLocatedAtNet(NetworkViewID tileID){
		locatedAt = NetworkView.Find (tileID).GetComponent<Tile>();
	}

	[RPC]
	//All tiles in controlledRegion will be set to belong to this village. The village MUST have a controlledRegion set first
	void updateControlledRegionNet(){
		foreach (Tile t in controlledRegion) 
		{
			t.myVillage=this;
		}
	}

	[RPC]
	//adding gold values
	void addGoldNet(int goldToAdd){
		gold += goldToAdd;
	}
	[RPC]
	//adding wood values
	void addWoodNet(int woodToAdd){
		wood += woodToAdd;
	}
	
	public void addUnit(Unit u)
	{
		supportedUnits.Add (u);
		u.myVillage = this;
	}


	public void removeUnit(Unit u)
	{
		supportedUnits.Remove(u);
		u.myVillage = null;
	}

	/*
	 * Function adds t to village region and colors.
	 * this function does NOT remove the tile from the old village.
	 */
	public void addTile(Tile t)
	{
		controlledRegion.Add(t);
		t.myVillage = this;
		int color = this.controlledBy.color;
		t.color = color;
		t.colorTile ();
	}
	
	public void addRegion(List<Tile> region)
	{	
		//doing exactly what the gameManager.addregion(List<Tile>, Village village) is doing
		foreach (Tile t in region) {
			t.myVillage = this;
			controlledRegion.Add(t);

			//if there is a unit on the tile
			Unit u = t.occupyingUnit;
			if(u != null && u.myVillage!=this){
				u.myVillage = this;
				supportedUnits.Add(u);
			}
		}
	}
	
	public int getTotalWages()
	{
		int totalWage = 0;
		foreach (Unit u in supportedUnits) {
			int tempWage = u.getWage();
			totalWage += tempWage;
		}
		return totalWage;
	}
	
	public void retireAllUnits()
	{
		foreach (Unit u in supportedUnits) {
			Tile unitLocation = u.locatedAt;
			unitLocation.occupyingUnit = null;
			unitLocation.myType = LandType.Tombstone;
			u.locatedAt = null;
			u.myVillage = null;
			supportedUnits.Remove(u);
		}
	}


	public void upgrade()
	{
		//TODO uncomment following line after demo
		//myAction = VillageActionType.BuildStageOne;

		wood -= 8;
		if (myType == VillageType.Hovel) 
		{
			this.transform.FindChild("Hovel").gameObject.SetActive (false);
			this.transform.FindChild("Town").gameObject.SetActive (true);
			myType = VillageType.Town;
		}
		else if (myType == VillageType.Town) 
		{
			transform.FindChild("Town").gameObject.SetActive (false);
			transform.FindChild("Fort").gameObject.SetActive (true);
			myType = VillageType.Fort;
		}
	}

	[RPC]
	void addUnitNet(NetworkViewID unitID){
		Unit unitToAdd = NetworkView.Find (unitID).gameObject.GetComponent<Unit>();
		supportedUnits.Add (unitToAdd);
		unitToAdd.myVillage = this;
	}

	[RPC]
	void setControlledByNet(NetworkViewID objectId, int playerIndex){
		Player[] pls = NetworkView.Find(objectId).gameObject.GetComponents<Player>();
		controlledBy = pls [playerIndex];
	}

}

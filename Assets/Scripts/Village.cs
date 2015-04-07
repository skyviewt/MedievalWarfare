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
	private Tile locatedAt;
	private List<Unit> supportedUnits;
	private VillageType myType;
	public VillageActionType myAction;
	private int gold;
	private int wood;
	private Shader outline;
	private VillageManager vm;
	
	// Use this for initialization
	void Start()
	{
		outline = Shader.Find("Glow");
		GameObject go = GameObject.Find("VillageManager");
		vm = go.GetComponent<VillageManager> ();
		Debug.Log (vm);
	}
	
	// Update is called once per frame
	void Update () {
		
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
	public static Village CreateComponent ( Player p, List<Tile> regions, Tile locatedAt, GameObject villagePrefab ) 
	{
		Debug.Log ("-----Village.CreateComponent() CALLED--------");
		Village myVillage = villagePrefab.AddComponent<Village>();
		myVillage.controlledRegion = regions;//todo
		myVillage.controlledBy = p;//TODO: Currently set in Mapgenerator
		myVillage.myType = VillageType.Hovel;
		myVillage.supportedUnits = new List<Unit> ();
		locatedAt.replace (villagePrefab);//to check : Set in Mapgenerator
		myVillage.locatedAt = locatedAt;//done
		locatedAt.setVillage (myVillage);//done
		myVillage.myAction = VillageActionType.ReadyForOrders;
		myVillage.gold = 0;
		myVillage.wood = 0;
		//TODO: set controlledRegions in tiles : Call updateControlledRegionNet()
		foreach (Tile t in myVillage.controlledRegion) 
		{
			t.setVillage(myVillage);
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
			t.setVillage(this);
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
	
	//setters and getters
	public void setGold(int goldValue)
	{
		gold = goldValue;
	}

	public int getGold()
	{
		return gold;
	}

	public void setWood(int woodValue)
	{
		wood = woodValue;
	}
	
	public int getWood(){
		return wood;
	}

	public VillageType getMyType()
	{
		return myType;
	}

	public void setMyType(VillageType vt)
	{
		myType = vt;
	}

	public VillageActionType getAction()
	{
		return this.myAction;
	}

	public Player getPlayer()
	{
		return controlledBy;
	}

	public List<Tile> getControlledRegion()
	{
		return controlledRegion;
	}

	public int getRegionSize()
	{
		return this.controlledRegion.Count;
	}

	public List<Unit> getControlledUnits()
	{
		return supportedUnits;
	}

	public int getUnitSize()
	{
		return this.getControlledUnits().Count;
	}

	public void setLocation(Tile t)
	{
		locatedAt = t;
	}

	public Tile getLocatedAt()
	{
		return locatedAt;
	}


	//increment / decrement
	// used only until calling method is networked
	public void addGold(int i)
	{
		gold += i;
	}
	public void addWood(int i)
	{
		wood += i;
	}

	public void addUnit(Unit u)
	{
		supportedUnits.Add (u);
		u.setVillage (this);
	}


	public void removeUnit(Unit u)
	{
		supportedUnits.Remove(u);
		u.setVillage(null);
	}

	/*
	 * Function adds t to village region and colors.
	 * this function does NOT remove the tile from the old village.
	 */
	public void addTile(Tile t)
	{
		controlledRegion.Add(t);
		t.setVillage(this);
		int color = this.getPlayer ().getColor ();
		t.setColor( color );
		t.colorTile ();
	}

	public void removeTile(Tile t)
	{
		controlledRegion.Remove (t);
	}


	public void addRegion(List<Tile> regions)
	{	
		//doing exactly what the gameManager.addregion(List<Tile>, Village village) is doing
		foreach (Tile t in regions) {
			t.setVillage(this);
			controlledRegion.Add(t);

			//if there is a unit on the tile
			Unit u = t.getOccupyingUnit();
			if(u != null && u.getVillage()!=this){
				u.setVillage(this);
				supportedUnits.Add(u);
			}
		}
	}

	//needs getWage in Units

	public int getTotalWages()
	{
		int totalWage = 0;
		foreach (Unit u in supportedUnits) {
			int tempWage = u.getWage();
			totalWage += tempWage;
		}
		return totalWage;
	}


	//needs setLocation and setVillage and setOccupyingUnit in Tile

	public void retireAllUnits()
	{
		foreach (Unit u in supportedUnits) {
			Tile unitLocation = u.getLocation();
			unitLocation.setOccupyingUnit(null);
			unitLocation.setLandType(LandType.Tombstone);
			u.setLocation(null);
			u.setVillage(null);
			GameObject tombPrefab = vm.tombPrefab;
			GameObject tomb = Instantiate (tombPrefab, new Vector3 (unitLocation.point.x, 0.4f, unitLocation.point.y), tombPrefab.transform.rotation) as GameObject;
			unitLocation.setLandType(LandType.Tombstone);
			//unitLocation.replace (tomb);
			//unitLocation.networkView.RPC ("setPrefab", RPCMode.AllBuffered, tomb.networkView.viewID);
			//unitLocation.networkView.RPC ("setLandTypeNet", RPCMode.AllBuffered, (int)LandType.Tombstone);

			supportedUnits.Remove(u);
			Destroy (u.gameObject);
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
			setMyType (VillageType.Town);
		}
		else if (myType == VillageType.Town) 
		{
			transform.FindChild("Town").gameObject.SetActive (false);
			transform.FindChild("Fort").gameObject.SetActive (true);
			setMyType (VillageType.Fort);
		}
	}
	//sets gold to 0 and returns the previous gold value
	public int pillageGold()
	{
		int previousGoldValue = gold;
		gold = 0;
		return previousGoldValue;
	}
	//sets wood to 0 and returns the previous wood value
	public int pillagewood()
	{
		int previousWoodValue = wood;
		wood = 0;
		return previousWoodValue;
	}

	public void setControlledBy(Player p){
		controlledBy = p;
	}

	[RPC]
	void addUnitNet(NetworkViewID unitID){
		Unit unitToAdd = NetworkView.Find (unitID).gameObject.GetComponent<Unit>();
		supportedUnits.Add (unitToAdd);
		unitToAdd.setVillage (this);
	}

	[RPC]
	void setControlledByNet(NetworkViewID objectId, int playerIndex){
		Player[] pls = NetworkView.Find(objectId).gameObject.GetComponents<Player>();
		controlledBy = pls [playerIndex];
	}

}

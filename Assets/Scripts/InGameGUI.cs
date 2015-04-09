using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class InGameGUI : MonoBehaviour {
	public Camera myCamera;
	public Canvas VillageCanvas;
	public Canvas UnitCanvas;
	public Canvas HUDCanvas;
	public Canvas ErrorCanvas;
	public Canvas YourTurnCanvas;

	public int myTurn;
	public int turnOrder = 0;

	// prefabs
	public GameObject UnitPrefab;

	//selections
	public GameObject _Village;
	public GameObject _Unit;
	public GameObject _Tile;
	private GameObject _WoodValue;
	private GameObject _GoldValue;

	public bool _isAUnitSelected;
	public bool _isVillageSelected;

	public Text _WoodText;
	public Text _GoldText;
	public Text _RegionText;
	public Text _UnitsText;
	public Text _ErrorText;
	public Transform EndButton;

	private Game _game;
	private Tile _move;

	private VillageManager villageManager;
	private UnitManager unitManager;
	private GameManager gameManager;

	private bool menuUp;
	// Use this for initialization
	void Start () 
	{
		myCamera =  GameObject.FindGameObjectWithTag("MainCamera").camera;
		villageManager = GameObject.Find("VillageManager").GetComponent<VillageManager>();
		villageManager.isInGame = true;
		unitManager = GameObject.Find("UnitManager").GetComponent<UnitManager>();
		gameManager = GameObject.Find("perserveGM").GetComponent<GameManager> ();
		HUDCanvas.enabled = true;
		VillageCanvas.enabled = false;
		UnitCanvas.enabled = false;
		ErrorCanvas.enabled = false;
		YourTurnCanvas.enabled = false;
		menuUp = false;
		myTurn = 0;
		//gameObject.networkView.RPC ("setOtherToTurn0", RPCMode.OthersBuffered);
	}

	[RPC]
	void setOtherToTurn1(){
		myTurn = 1;
	}

	[RPC]
	void incrementTurnOrderNet(){
		turnOrder = (turnOrder+1)%2;

		GameObject[] allUnits = GameObject.FindGameObjectsWithTag("Unit");
		foreach (GameObject u in allUnits) {
			Debug.Log("Found 1 unit");
			u.GetComponent<Unit>().setAction(UnitActionType.ReadyForOrders);
		}
		disableAllCanvases ();
		if(myTurn == turnOrder)
		{
			EndButton.GetComponent<Button>().interactable = true;
			notifyTurnStart ();
		}
		else
		{
			EndButton.GetComponent<Button>().interactable = false;
		}


	}

	void OnConnectedToServer(){
		gameObject.networkView.RPC ("setOtherToTurn1", RPCMode.Others);
	}

	//Functions on the HUD

	public void endTurnPressed()
	{
		//disableAllCanvases ();
		gameManager.setNextPlayerInTurnOrder ();
		gameObject.networkView.RPC ("incrementTurnOrderNet", RPCMode.AllBuffered);
	}
	private void disableAllCanvases()
	{
		VillageCanvas.enabled = false;
		UnitCanvas.enabled = false;
		ErrorCanvas.enabled = false;
		menuUp = false;
	}
	//Functions for when a Village is selected
	public void trainPeasantPressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.hirePeasant (v,UnitPrefab);
		int redrawUnits = v.getUnitSize();
		int redrawGold = v.getGold();
		_UnitsText.text = redrawUnits.ToString();
		_GoldText.text = redrawGold.ToString();
		VillageCanvas.enabled = false;
		menuUp = false;
	}

	public void trainInfantryPressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.hireInfantry (v,UnitPrefab);
		int redrawUnits = v.getUnitSize();
		int redrawGold = v.getGold();
		_UnitsText.text = redrawUnits.ToString();
		_GoldText.text = redrawGold.ToString();
		VillageCanvas.enabled = false;
		menuUp = false;
	}

	public void trainSoldierPressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.hireSoldier (v,UnitPrefab);
		int redrawUnits = v.getUnitSize ();
		int redrawGold = v.getGold();
		_UnitsText.text = redrawUnits.ToString();
		_GoldText.text = redrawGold.ToString();
		VillageCanvas.enabled = false;
		menuUp = false;
	}

	public void trainKnightPressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.hireKnight (v,UnitPrefab);
		int redrawUnits = v.getUnitSize ();
		int redrawGold = v.getGold();
		_UnitsText.text = redrawUnits.ToString();
		_GoldText.text = redrawGold.ToString();
		VillageCanvas.enabled = false;
		menuUp = false;
	}


	public void villageUpgradePressed()
	{
		Village v = _Village.GetComponent<Village> ();
		//villageManager.upgradeVillage (v);
		villageManager.gameObject.networkView.RPC ("upgradeVillageNet", RPCMode.AllBuffered, v.gameObject.networkView.viewID);
		int redrawWood = v.getWood();
		_WoodText.text = redrawWood.ToString();
		VillageCanvas.enabled = false;
		menuUp = false;
	}
	public void closeVillagePressed()
	{
		VillageCanvas.enabled = false;
		menuUp = false;
	}

	//Functions for when a Unit is selected
	public void cultivatePressed()
	{
		Unit u = _Unit.GetComponent<Unit>();

		unitManager.cultivateMeadow(u);

		UnitCanvas.enabled = false;
		menuUp = false;
	}
	public void unitPressed()
	{
		UnitCanvas.enabled = true;
		menuUp = false;
	}

	public void cancelUnitPressed()
	{
		UnitCanvas.enabled = false;
		menuUp = false;
		ClearSelections ();
	}
	public void moveUnitPressed()
	{
		UnitCanvas.enabled = false;
		_isAUnitSelected = true;
		this.displayError("Please select a friendly or neutral tile 1 distance away to move to. (*￣ー￣*)");
		menuUp = false;

	}
	public void unitUpgradeInfantryPressed()
	{
		//When you upgrade a unit, you only need to redraw the gold on the HUD
		Unit u = _Unit.GetComponent<Unit>();
		//unitManager.upgradeUnit(u,UnitType.INFANTRY);
		unitManager.gameObject.networkView.RPC ("upgradeUnitNet", RPCMode.AllBuffered, u.gameObject.networkView.viewID, (int)UnitType.INFANTRY);
		Village v = u.getVillage();
		int redrawGold = v.getGold();
		_GoldText.text = redrawGold.ToString();
		UnitCanvas.enabled = false;
		menuUp = false;

	}

	public void unitUpgradeSoldierPressed()
	{
		Unit u = _Unit.GetComponent<Unit>();
		//unitManager.upgradeUnit(u,UnitType.SOLDIER);
		unitManager.gameObject.networkView.RPC ("upgradeUnitNet", RPCMode.AllBuffered, u.gameObject.networkView.viewID, (int)UnitType.SOLDIER);
		Village v = u.getVillage();
		int redrawGold = v.getGold();
		_GoldText.text = redrawGold.ToString();
		UnitCanvas.enabled = false;
		menuUp = false;

	}

	public void unitUpgradeKnightPressed()
	{
		Unit u = _Unit.GetComponent<Unit>();
		//unitManager.upgradeUnit(u,UnitType.KNIGHT);
		unitManager.gameObject.networkView.RPC ("upgradeUnitNet", RPCMode.AllBuffered, u.gameObject.networkView.viewID, (int)UnitType.KNIGHT);
		Village v = u.getVillage();
		int redrawGold = v.getGold();
		_GoldText.text = redrawGold.ToString();
		UnitCanvas.enabled = false;
		menuUp = false;

	}

	public void displayError(string error)
	{
		VillageCanvas.enabled = false;
		UnitCanvas.enabled = false;
		_ErrorText.text = error;
		ErrorCanvas.enabled = true;

	}
	public void notifyTurnStart()
	{
		disableAllCanvases ();
		ClearSelections ();
		YourTurnCanvas.enabled = true;
	}
	public void beginTurnPressed()
	{
		YourTurnCanvas.enabled = false;
	}
	void validateMove(RaycastHit hit)
	{
		//print ("in validateMove");
		if (_isAUnitSelected && ( _Unit.GetComponent<Unit> ().myAction == UnitActionType.ReadyForOrders || 
					_Unit.GetComponent<Unit> ().myAction == UnitActionType.Moved )) 
		{
			_Tile = hit.collider.gameObject;
			Tile selection = _Tile.GetComponent<Tile> ();
			print (selection != null);
			//Debug.Log (_Unit.GetComponent<Unit> ().getLocation ().neighbours);
			if (_Unit.GetComponent<Unit> ().getLocation ().getNeighbours().Contains (selection)) {
					_move = selection;
			}
			//Debug.LogWarning (_move);
			if (_move != null) 
			{
					UnitCanvas.enabled = false;
					Unit u = _Unit.GetComponent<Unit> ();
					Village v = u.getVillage ();
		
					//print ("doing the move now");

					//unitManager.moveUnit (u, _move);
					gameObject.networkView.RPC ("moveUnitNet", RPCMode.AllBuffered, u.gameObject.networkView.viewID, _move.gameObject.networkView.viewID);
					
					if (selection.getVillage () == null) {
							v.addTile (selection);
							int redrawRegion = v.getRegionSize();
							_RegionText.text = redrawRegion.ToString ();
					}
					
/*					if (selection.getLandType () == LandType.Trees) {
	
							int redrawWood = v.getWood ();
							_WoodText.text = redrawWood.ToString ();

					}*/

					int redrawWood = v.getWood ();
					_WoodText.text = redrawWood.ToString ();
					ClearSelections ();
			}
			else
			{
				this.displayError (@"Invalid Move. ¯\(°_o)/¯");
				ClearSelections ();//can delete this line later if we want, added it to help simplify turns
			}

		} 
		else if( _isAUnitSelected && !(_Unit.GetComponent<Unit> ().myAction == UnitActionType.ReadyForOrders || 
		                             _Unit.GetComponent<Unit> ().myAction == UnitActionType.Moved)) 
		{
			this.displayError (@"Already Moved. ¯\(°_o)/¯");
			ClearSelections();
		}
	}
	
	void ClearSelections()
	{
		_Unit = null;
		_move = null;
		_Tile = null;
		_isAUnitSelected = false;
		_Village = null;
		_isVillageSelected = false;
	}
	// Update is called once per frame
	void Update()
	{

		Ray ray = myCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		//if clicked
		if (Input.GetMouseButtonDown(0)&& !menuUp)
		{

			if (Physics.Raycast(ray, out hit)){
	
				switch(hit.collider.tag)
				{
					case "Town":
					{
					Debug.Log("inTown");
						_Village = hit.collider.gameObject;
						Village v = _Village.GetComponent<Village>();
						int redrawWood = v.getWood();
						int redrawGold = v.getGold();
						int redrawRegion = v.getRegionSize();
						int redrawUnits = v.getUnitSize();
						_WoodText.text = redrawWood.ToString();
						_GoldText.text = redrawGold.ToString();
						_RegionText.text = redrawRegion.ToString();
						_UnitsText.text = redrawUnits.ToString();
						if(myTurn != turnOrder)
						{
							VillageCanvas.enabled = true;
							menuUp = true;
						}
						break;
					}

					case "Unit":
					{
						_Unit = hit.collider.gameObject;
						
						Unit u = _Unit.GetComponent<Unit>();
						Village v = u.getVillage();
						int redrawWood = v.getWood();
						int redrawGold = v.getGold();
						int redrawRegion = v.getRegionSize();
						int redrawUnits = v.getUnitSize();
						_WoodText.text = redrawWood.ToString();
						_GoldText.text = redrawGold.ToString();
						_RegionText.text = redrawRegion.ToString();
						_UnitsText.text = redrawUnits.ToString();

						//Tile onIt = _Unit.GetComponent<Unit>().getLocation();
						if(myTurn != turnOrder)
						{
							UnitCanvas.enabled = true;
							menuUp = true;
						}
						break;
					}
					case "Grass":
					{
						if (_isAUnitSelected == true){
							ErrorCanvas.enabled = true;
							validateMove(hit);
						} else if (_isVillageSelected==true){
							ErrorCanvas.enabled = true;
							validateBuild(hit);
						} else {
							_Tile = hit.collider.gameObject;
							Tile t = _Tile.GetComponent<Tile>();
							Village v = t.getVillage ();
							if (v!=null){
								Debug.Log(v);
								int redrawWood = v.getWood();
								int redrawGold = v.getGold();
								int redrawRegion = v.getRegionSize();
								int redrawUnits = v.getUnitSize();
								_WoodText.text = redrawWood.ToString();
								_GoldText.text = redrawGold.ToString();
								_RegionText.text = redrawRegion.ToString();
								_UnitsText.text = redrawUnits.ToString();
							}
						}

						ClearSelections();
						break;
					}
				}

			}
		}

		if (Input.GetKeyDown (KeyCode.Escape)) 
		{
			if(VillageCanvas.enabled == true)
			{
				VillageCanvas.enabled = false;
				menuUp = false;
			}
			if(UnitCanvas.enabled == true)
			{
				UnitCanvas.enabled = false;
				_isAUnitSelected = false;
				menuUp = false;
			}
			if(ErrorCanvas.enabled == true)
			{
				ErrorCanvas.enabled = false;
				menuUp = false;
			}

			//TODO: bring up the esc menu
			else
			{

			}
		}

	}

	public void buildTowerPressed()
	{
		VillageCanvas.enabled = false;
		Village v = _Village.GetComponent<Village> ();
		if (v.getWood () >= 5) {
			_isVillageSelected = true;
			this.displayError ("Please select an empty tile to build on");
		} else {
			this.displayError ("You need at least 5 wood to build a tower");
		}
		menuUp = false;
	}

	void validateBuild(RaycastHit hit)
	{
		//print ("in validateBuild");
		//print (_isVillageSelected);
		if (_isVillageSelected && _Village.GetComponent<Village> ().getAction() == VillageActionType.ReadyForOrders) 
		{
			//print ("inside inside validate build");
			Village v = _Village.GetComponent<Village> ();
			_Tile = hit.collider.gameObject;
			Tile selection = _Tile.GetComponent<Tile> ();
			if (!v.getControlledRegion().Contains (selection)){
				this.displayError ("You must build inside your controlled region");
			} else if (selection.getStructure ()!=null){
				this.displayError ("There is already a tower there");
			} else if (selection.getOccupyingUnit()!=null){
				this.displayError ("We arent building homes! A unit is standing there");
			} else if (selection == v.getLocatedAt()){
				this.displayError ("Towers go AROUND your village");
			} else {
				villageManager.buildTower(v, selection);
			}
			//selection.gameObject.renderer.material.color = Color.yellow;
		}
		ClearSelections();
	}

	public void buildRoadPressed()
	{
		Unit u = _Unit.GetComponent<Unit>();
		//Village v = u.getVillage();
		Tile t = u.getLocation ();
		ErrorCanvas.enabled = true;
		if (u.getUnitType () != UnitType.PEASANT) {
			this.displayError ("Only peasants can build roads");
		} else if (t.checkRoad ()) {
			this.displayError ("This tile already has a road");
		} else {
			//TODO RPC this, delay until next turn
			t.buildRoad ();
			u.setAction(UnitActionType.BuildingRoad);
		}

		UnitCanvas.enabled = false;
		menuUp = false;
		ClearSelections();
	}

	public void buildCastlePressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.buildCastle (v);
		int redrawWood = v.getWood();
		_WoodText.text = redrawWood.ToString();
		VillageCanvas.enabled = false;
		menuUp = false;
	}

	public void buildCannonPressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.buildCannon (v,UnitPrefab);
		int redrawUnits = v.getUnitSize ();
		int redrawGold = v.getGold();
		_UnitsText.text = redrawUnits.ToString();
		_GoldText.text = redrawGold.ToString();
		VillageCanvas.enabled = false;
		menuUp = false;
	}

}

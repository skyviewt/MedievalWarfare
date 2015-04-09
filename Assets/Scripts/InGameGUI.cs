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
	public int turnOrder;

	// prefabs
	public GameObject UnitPrefab;
	public GameObject cannonPrefab;

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
	public Text _TurnText;
	public Transform EndButton;
	
	private Tile _move;

	private VillageManager villageManager;
	private UnitManager unitManager;
	private GameManager gameManager;

	private bool menuUp;

	private bool currentlyUpdatingGame; // boolean is used to prevent players from touching the map while it is being updated

	// Use this for initialization
	void Start () 
	{
		myCamera =  GameObject.FindGameObjectWithTag("MainCamera").camera;
		villageManager = GameObject.Find("VillageManager").GetComponent<VillageManager>();
		villageManager.isInGame = true;
		unitManager = GameObject.Find("UnitManager").GetComponent<UnitManager>();
		gameManager = GameObject.Find("perserveGM").GetComponent<GameManager> ();
		gameManager.isInGame = true;
		HUDCanvas.enabled = true;
		disableAllCanvases ();
		myTurn = gameManager.getLocalTurn ();
	}
	
	//Functions on the HUD
	public void endTurnPressed()
	{	
		Debug.Log ("inEndTurn");
		gameManager.networkView.RPC ("setNextPlayer", RPCMode.AllBuffered,gameManager.findNextPlayer()); // disables gui interaction while map is updating
		gameManager.initializeNextPlayersVillages();
		this.networkView.RPC ("setTurnButton", RPCMode.AllBuffered);					//reenables gui interaction after map is updated
	}

	public void disableInteractions()
	{
		currentlyUpdatingGame = true;
	}
	public void allowInteractions()
	{
		this.currentlyUpdatingGame = false;
	}

	[RPC]
	public void setTurnButton()
	{
		disableAllCanvases ();
		turnOrder = gameManager.game.getCurrentTurn ();
		Debug.Log ("Turn order is: "+turnOrder);
		if (turnOrder == myTurn) 
		{
			Debug.Log ("setTurnButton My Turn");
			EndButton.GetComponent<Button> ().interactable = true;
		}
		else 
		{
			Debug.Log ("setTurnButton Not My Turn");
			EndButton.GetComponent<Button>().interactable = false;
		}
		notifyTurnStart();
		this.allowInteractions();
	}

	public void notifyTurnStart()
	{
		disableAllCanvases ();
		ClearSelections ();
		if (turnOrder == myTurn) 
		{
			Debug.Log ("notifyTurnStart My Turn");
			_TurnText.text = "ヽ(≧Д≦)ノ\nIt is now your turn!";
		}
		else if (turnOrder != myTurn)
		{
			Debug.Log ("notifyTurnStart Not my Turn");
			_TurnText.text = "ヽ(≧Д≦)ノ\nIt is now Player "+(turnOrder+1).ToString()+"'s turn!";
		}
		YourTurnCanvas.enabled = true;
		menuUp = true;
	}
	
	private void disableAllCanvases()
	{
		VillageCanvas.enabled = false;
		UnitCanvas.enabled = false;
		ErrorCanvas.enabled = false;
		YourTurnCanvas.enabled = false;
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
		UnitType unitType = u.getUnitType ();
		LandType landType = u.getLocation ().getLandType ();
		if (unitType == UnitType.PEASANT) 
		{
			if(u.getLocation ().checkVillagePrefab() == true)
			{
				displayError (@"It would be unwise to replace your town with a meadow.");
			}
			else if(landType == LandType.Grass)
			{
				unitManager.gameObject.networkView.RPC("cultivateMeadowNet",RPCMode.AllBuffered,u.gameObject.networkView.viewID);
			}
			else if (landType == LandType.Meadow)
			{
				displayError (@"There is already a lovely meadow here.");
			}
			else
			{
				displayError (@"WARNING: ERROR with the logic if this message is displayed");
			}
		} 
		else 
		{
			displayError (@"Only peasants are willing to cultivate meadows.");
		}
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

	public void beginTurnPressed()
	{
		YourTurnCanvas.enabled = false;
		menuUp = false;
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
		if (Input.GetMouseButtonDown(0)&& !menuUp && !currentlyUpdatingGame)
		{

			if (Physics.Raycast(ray, out hit)){
	
				switch(hit.collider.tag)
				{
					case "Town":
					{
						Debug.Log("inTown");
						_Village = hit.collider.gameObject;
						Village v = _Village.GetComponent<Village>();
						VillageActionType action = v.getAction ();
						Player owningPlayer = v.getPlayer ();
						Player localPlayer = gameManager.getLocalPlayer ();
						Debug.Log ("Village Owner: "+owningPlayer);
						Debug.Log ("trying to access: " +localPlayer);
						int redrawWood = v.getWood();
						int redrawGold = v.getGold();
						int redrawRegion = v.getRegionSize();
						int redrawUnits = v.getUnitSize();
						_WoodText.text = redrawWood.ToString();
						_GoldText.text = redrawGold.ToString();
						_RegionText.text = redrawRegion.ToString();
						_UnitsText.text = redrawUnits.ToString();
						
						if(myTurn == turnOrder) //&& owningPlayer == localPlayer)
						{
							if(action == VillageActionType.ReadyForOrders)
							{
								VillageCanvas.enabled = true;
								menuUp = true;
							}
							else
							{
								displayError (@"This village cannot perform actions until it is done upgrading.");
							}
						}
						break;
					}

					case "Unit":
					{
						_Unit = hit.collider.gameObject;
						
						Unit u = _Unit.GetComponent<Unit>();
						UnitActionType action = u.getAction ();
						Village v = u.getVillage();
						Player owningPlayer = v.getPlayer ();
						Player localPlayer = gameManager.getLocalPlayer ();
						int redrawWood = v.getWood();
						int redrawGold = v.getGold();
						int redrawRegion = v.getRegionSize();
						int redrawUnits = v.getUnitSize();
						_WoodText.text = redrawWood.ToString();
						_GoldText.text = redrawGold.ToString();
						_RegionText.text = redrawRegion.ToString();
						_UnitsText.text = redrawUnits.ToString();
						
						if(myTurn == turnOrder) //&& owningPlayer == localPlayer)
						{
							if(action == UnitActionType.ReadyForOrders || action == UnitActionType.Moved)
							{
								UnitCanvas.enabled = true;
								menuUp = true;
							}
							else
							{
								displayError (@"This unit has already performed an action this turn.");
							}
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
							else if(v == null){
								Debug.Log ("neutral tile");
								_WoodText.text = "";
								_GoldText.text = "";
								_RegionText.text = "";
								_UnitsText.text = "";
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
			if(YourTurnCanvas.enabled == true)
			{
				YourTurnCanvas.enabled = false;
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
		villageManager.buildCannon (v,cannonPrefab);
		int redrawUnits = v.getUnitSize ();
		int redrawGold = v.getGold();
		_UnitsText.text = redrawUnits.ToString();
		_GoldText.text = redrawGold.ToString();
		VillageCanvas.enabled = false;
		menuUp = false;
	}

}

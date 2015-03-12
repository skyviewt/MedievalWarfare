using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;


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
	private GameObject _Village;
	private GameObject _Unit;
	private GameObject _Tile;
	private GameObject _WoodValue;
	private GameObject _GoldValue;

	private bool _isAUnitSelected;

	public Text _WoodText;
	public Text _GoldText;
	public Text _RegionText;
	public Text _UnitsText;
	public Text _ErrorText;
	public Transform EndButton;

	private Tile _move;

	private VillageManager villageManager;
	private UnitManager unitManager;

	private bool menuUp;
	// Use this for initialization
	void Start () 
	{
		myCamera =  GameObject.FindGameObjectWithTag("MainCamera").camera;
		villageManager = GameObject.Find("VillageManager").GetComponent<VillageManager>();
		unitManager = GameObject.Find("UnitManager").GetComponent<UnitManager>();
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
			EndButton.GetComponent<Button>().enabled = true;
			notifyTurnStart ();
		}
		else
		{
			EndButton.GetComponent<Button>().enabled = false;
		}


	}

	void OnConnectedToServer(){
		gameObject.networkView.RPC ("setOtherToTurn1", RPCMode.Others);
	}

	//Functions on the HUD

	public void endTurnPressed()
	{
		//turnOrder = (turnOrder+1)%2;
		//disableAllCanvases ();
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
		int redrawUnits = v.getControlledUnits().Count();
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
		int redrawUnits = v.getControlledUnits().Count();
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
		int redrawUnits = v.getControlledUnits().Count();
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
		int redrawUnits = v.getControlledUnits().Count();
		int redrawGold = v.getGold();
		_UnitsText.text = redrawUnits.ToString();
		_GoldText.text = redrawGold.ToString();
		VillageCanvas.enabled = false;
		menuUp = false;
	}


	public void villageUpgradePressed()
	{
		Village v = _Village.GetComponent<Village> ();
		villageManager.upgradeVillage (v);
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
		this.displayError("Please select a friendly or neutral tile 1 distance away to move to.");
		menuUp = false;

	}
	public void unitUpgradeInfantryPressed()
	{
		//When you upgrade a unit, you only need to redraw the gold on the HUD
		Unit u = _Unit.GetComponent<Unit>();
		unitManager.upgradeUnit(u,UnitType.INFANTRY);
		Village v = u.getVillage();
		int redrawGold = v.getGold();
		_GoldText.text = redrawGold.ToString();
		UnitCanvas.enabled = false;
		menuUp = false;

	}

	public void unitUpgradeSoldierPressed()
	{
		Unit u = _Unit.GetComponent<Unit>();
		unitManager.upgradeUnit(u,UnitType.SOLDIER);
		Village v = u.getVillage();
		int redrawGold = v.getGold();
		_GoldText.text = redrawGold.ToString();
		UnitCanvas.enabled = false;
		menuUp = false;

	}

	public void unitUpgradeKnightPressed()
	{
		Unit u = _Unit.GetComponent<Unit>();
		unitManager.upgradeUnit(u,UnitType.KNIGHT);
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
	private void notifyTurnStart()
	{
		disableAllCanvases ();
		ClearSelections ();
		YourTurnCanvas.enabled = true;
	}
	public void okBeginTurnPressed()
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
			if (_Unit.GetComponent<Unit> ().getLocation ().neighbours.Contains (selection)) {
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
							int redrawRegion = v.getControlledRegion ().Count;
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
						
						_Village = hit.collider.gameObject;
						Village v = _Village.GetComponent<Village>();
						int redrawWood = v.getWood();
						int redrawGold = v.getGold();
						int redrawRegion = v.getControlledRegion().Count();
						int redrawUnits = v.getControlledUnits().Count();
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
						int redrawRegion = v.getControlledRegion().Count();
						int redrawUnits = v.getControlledUnits().Count();
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
						if(_isAUnitSelected == false)
						{
							_Tile = hit.collider.gameObject;
							Tile t = _Tile.GetComponent<Tile>();
							Village v = t.getVillage ();
							int redrawWood = v.getWood();
							int redrawGold = v.getGold();
							int redrawRegion = v.getControlledRegion().Count();
							int redrawUnits = v.getControlledUnits().Count();
							_WoodText.text = redrawWood.ToString();
							_GoldText.text = redrawGold.ToString();
							_RegionText.text = redrawRegion.ToString();
							_UnitsText.text = redrawUnits.ToString();
						}
						ErrorCanvas.enabled = false;
						validateMove(hit);
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
			else{

			}
		}

	}

}

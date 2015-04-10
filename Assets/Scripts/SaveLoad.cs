using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SaveLoad : MonoBehaviour {
	
	public bool saveGame = false;
	public bool loadGame = false;
	public int numberOfSaves;
	public int saveGameID = 1;
	
	//Prefabs:
	public GameObject GrassPrefab;
	public GameObject MeadowPrefab;
	public GameObject TreePrefab;
	public GameObject HovelPrefab;
	public float offset = 3.0f;
	
	//names of the saved files:

	public string saveName;
	public int saveID = 1;
	public string firstSavedGameName;



	//Saves the gameobjects instanciated
	//DELETE THIS LIST EVERY TIME THE GAME LOADS
	public List<Tile> tileList;
	public List<Village> villageList;
	public List<Player> playerList;

	//public SaveLoad(){
	//	numberOfSaves = PlayerPrefs.GetInt ("NumberOfSaves");
//		firstSavedGameName = PlayerPrefs.GetString("NameByID"+"1");
//	}


	// Use this for initialization
	void Start () {
		tileList = new List<Tile>();
		villageList = new List<Village>();
		playerList = new List<Player>();
	}

	void Awake() {
		Network.minimumAllocatableViewIDs = 10000;
	}
	// Update is called once per frame
	void Update () {
		if (saveGame) {
			saveThisGame(saveName);

			saveGame = false;
		}
		if (loadGame) {
			loadThisGame(saveGameID);

			loadGame = false;
		}
	}

	//returns the number of saved games
	public int saveNumber(){
		return PlayerPrefs.GetInt("NumberOfSaves");
	}

	//Get the name of the saved game with an integer
	public string getSaveName(int saveID){
		return PlayerPrefs.GetString ("NameByID"+saveID, name);
	}

	public void saveThisGame(string name){
		//get number of saves:
		int numberOfSaves = PlayerPrefs.GetInt("NumberOfSaves");

		if (numberOfSaves >= 10) {
			Debug.LogError("Max number of games exceeded");
		}

		//The saveID is incremented and is used as id:
		int saveID = numberOfSaves + 1;
		PlayerPrefs.SetInt ("NumberOfSaves", saveID);
		//set the name to ID
		PlayerPrefs.SetString ("NameByID"+saveID, name);

		saveTiles(saveID.ToString());
		savePlayerAndVillages (saveID.ToString());
	}
	
	public void loadThisGame(int gameID){
		loadTiles(gameID.ToString());
		loadPlayerAndVillages (gameID.ToString());
		tileList = new List<Tile>();
		villageList = new List<Village>();
		playerList = new List<Player>();
	}
	
	
	public void saveTiles(string gameID){
		//BE CAREFUL!!!!!! DELETES EVERYTHING
		PlayerPrefs.DeleteAll();
		
		Debug.Log ("Saving Tiles!!");
		GameObject[] tileGO = GameObject.FindGameObjectsWithTag("Grass");
		string id = gameID;
		string name = "savedName";
		string tileNB = "tNB";
		string land = "LandType";
		string color = "Color";
		string road = "Road";
		
		PlayerPrefs.SetInt (id+name+tileNB, tileGO.Length);
		//PlayerPrefs.SetInt(id + name ,300);
		//PlayerPrefs.SetInt("SavedGameNumber",3);
		
		//Index of first tile
		int startIndex = 1;
		
		Debug.Log (tileGO.Length);
		foreach (GameObject t in tileGO) {
			Tile curTile = t.GetComponent<Tile>();
			Debug.Log(curTile.point);
			
			//setting points
			PlayerPrefs.SetFloat (id+name+tileNB+startIndex+'x', curTile.point.x);
			PlayerPrefs.SetFloat (id+name+tileNB+startIndex+'y', curTile.point.y);
			
			
			//setting landtypes:
			PlayerPrefs.SetInt(id+name+tileNB+startIndex+land, (int)curTile.getLandType());
			
			//set color:
			PlayerPrefs.SetInt(id+name+tileNB+startIndex+color, curTile.getColor());
			//setRoad:
			if (curTile.hasRoad){
				PlayerPrefs.SetInt(id+name+tileNB+startIndex+road, 1);
			} else{
				PlayerPrefs.SetInt(id+name+tileNB+startIndex+road, 0);
			}
			
			//UPDATE INDEX LAST!!!
			startIndex++;
		}
	}
	
	public void loadTiles(string gameID){
		string id = gameID;
		string name = "savedName";
		string tileNB = "tNB";
		string land = "LandType";
		string color = "Color";
		string road = "Road";
		Debug.Log (PlayerPrefs.GetInt (id + name));
		int nbTile = PlayerPrefs.GetInt (id + name + tileNB);
		Debug.Log (nbTile);
		
		for(int i=1; i<=nbTile;i++){
			float x = PlayerPrefs.GetFloat (id+name+tileNB+i+'x');
			float y = PlayerPrefs.GetFloat (id+name+tileNB+i+'y');
			//instanciate the tiles
			Vector3 tilePosition = new Vector3(x, offset, y);
			GameObject curtile = Network.Instantiate(GrassPrefab, tilePosition, GrassPrefab.transform.rotation, 0) as GameObject;
			
			//setting tile points by RPC:
			curtile.networkView.RPC("setPointN", RPCMode.AllBuffered, tilePosition);
			//appending tile to global tileList:
			tileList.Add(curtile.GetComponent<Tile>());
			
			//setting colors
			int tcol = PlayerPrefs.GetInt(id+name+tileNB+i+color);
			curtile.networkView.RPC("setAndColor", RPCMode.AllBuffered, tcol);
			
			LandType landtp = (LandType)PlayerPrefs.GetInt(id+name+tileNB+i+land);
			//set landtype:
			
			//check if it is tree or meadow. TODO: Tombstones, Roads, etc
			Vector3 prefabPosition = new Vector3(x, offset, y);
			if(landtp==LandType.Trees){
				GameObject tpref = Network.Instantiate(TreePrefab, prefabPosition, TreePrefab.transform.rotation, 0) as GameObject;
				//Not over network
				//tpref.transform.eulerAngles = new Vector3(0,Random.Range (0,360),0); //give it a random rotation
				curtile.networkView.RPC ("setPrefab", RPCMode.AllBuffered, tpref.networkView.viewID);
				curtile.networkView.RPC ("setLandTypeNet", RPCMode.AllBuffered, (int)LandType.Trees);
			} 
			if(landtp==LandType.Meadow){
				GameObject tpref = Network.Instantiate(MeadowPrefab, prefabPosition, MeadowPrefab.transform.rotation, 0) as GameObject;
				//Not over network 
				//tpref.transform.eulerAngles = new Vector3(0,Random.Range (0,360),0); //give it a random rotation
				curtile.networkView.RPC ("setPrefab", RPCMode.AllBuffered, tpref.networkView.viewID);
				curtile.networkView.RPC ("setLandTypeNet", RPCMode.AllBuffered, (int)LandType.Meadow);
			}
			//set neighbour:
			setsNeighbors(tileList);
			
		}
		
		//Coloring. TODO: set prefabs
		
		//		GameObject[] tileGO = GameObject.FindGameObjectsWithTag("Grass");
		//		foreach (GameObject t in tileGO) {
		//			int color = t.GetComponent<Tile>().getColor();
		//			Debug.LogError("TileColor = " + color);
		//			t.networkView.RPC("setAndColor", RPCMode.AllBuffered, color);
		//		}
		
		
	}
	
	public void savePlayerAndVillages(string gameID){
		//player data names
		string id = gameID;
		string name = "savedName";
		string pID = "PlayerID";
		string pNum = "NumberOfPlayers";
		string clr = "Color";
		
		//village data names
		string vID = "VillageID";
		string vNum = "NumberOfVillage";
		
		string gold = "Gold";
		string wood = "Wood";
		string health = "Health";
		
		string locationx= "Locationx";
		string locationy= "Locationy";
		
		string vType = "vType";
		string vGold = "vGold";
		string vWood = "vWood";
		string vWage = "vWage";
		string vHealth = "vHealth";
		string vActType= "vActionType";

		//Get villages by the order of players
		Debug.Log ("Saving villages!!");
		GameObject GM = GameObject.Find("preserveGM");
		Game game = GM.GetComponent<GameManager>().game;
		List<Player> playerList = game.getPlayers ();
		//TODO: What happens when a player loses and is out of the game??
		PlayerPrefs.SetInt (id+name+pNum, playerList.Count);
		
		int playerNb = 1;
		//save player
		foreach (Player t in playerList) {
			PlayerPrefs.SetInt(id+name+pID+clr, t.getColor());
			
			//getting the villages
			List<Village> villageList = t.getVillages();
			//save the number of villages
			PlayerPrefs.SetInt(id+name+pID+vNum, villageList.Count);

			//Printing the number of villages:
			Debug.LogError(villageList.Count);

			//saving each village of their respective player
			int villageNb = 1;
			foreach (Village v in villageList){
				//LocationTile:
				PlayerPrefs.SetFloat(id+name+pID+playerNb+vID+villageNb+locationx, v.getLocatedAt().point.x);
				PlayerPrefs.SetFloat(id+name+pID+playerNb+vID+villageNb+locationy, v.getLocatedAt().point.y);

				//VillageType:

				PlayerPrefs.SetInt(id+name+pID+playerNb+villageNb+vType, (int)v.getMyType());
				PlayerPrefs.SetInt(id+name+pID+playerNb+villageNb+vGold, v.getGold());
				PlayerPrefs.SetInt(id+name+pID+playerNb+villageNb+vWage, v.getTotalWages());
				PlayerPrefs.SetInt(id+name+pID+playerNb+villageNb+vWood, v.getWood());
				PlayerPrefs.SetInt(id+name+pID+playerNb+villageNb+vHealth, v.health);
				PlayerPrefs.SetInt(id+name+pID+playerNb+villageNb+vActType, (int)v.getAction());

				//increment villageNb
				villageNb++;
			}
			
			//LAST: increment playerNb:
			playerNb++;


		}
	}

	public void setsNeighbors(List<Tile> tiles)
	{	
		foreach(Tile curr in tiles)
		{
			foreach (Tile tmp in tiles)
			{			
				if(curr.point.x == tmp.point.x + 1 && curr.point.y == tmp.point.y)
				{
					curr.gameObject.networkView.RPC("addNeighbourN", RPCMode.AllBuffered, tmp.gameObject.networkView.viewID);
				}
				
				if(curr.point.x == tmp.point.x - 1 && curr.point.y == tmp.point.y)
				{
					curr.gameObject.networkView.RPC("addNeighbourN", RPCMode.AllBuffered, tmp.gameObject.networkView.viewID);
				}
				
				
				if(curr.point.x == tmp.point.x + 0.5f && curr.point.y == tmp.point.y + 0.75f)
				{
					curr.gameObject.networkView.RPC("addNeighbourN", RPCMode.AllBuffered, tmp.gameObject.networkView.viewID);
				}
				
				if(curr.point.x == tmp.point.x + 0.5f && curr.point.y == tmp.point.y - 0.75f)
				{
					curr.gameObject.networkView.RPC("addNeighbourN", RPCMode.AllBuffered, tmp.gameObject.networkView.viewID);
				}
				
				if(curr.point.x == tmp.point.x - 0.5f && curr.point.y == tmp.point.y + 0.75f)
				{
					curr.gameObject.networkView.RPC("addNeighbourN", RPCMode.AllBuffered, tmp.gameObject.networkView.viewID);
				}
				
				if(curr.point.x == tmp.point.x - 0.5f && curr.point.y == tmp.point.y - 0.75f)
				{
					curr.gameObject.networkView.RPC("addNeighbourN", RPCMode.AllBuffered, tmp.gameObject.networkView.viewID);
				}
			}			
		}
	}
	
	
	public void loadPlayerAndVillages(string gameID){
		//player data names
		string id = gameID;
		string name = "savedName";
		string pID = "PlayerID";
		string pNum = "NumberOfPlayers";
		string clr = "Color";
		
		//village data names
		string vID = "VillageID";
		string vNum = "NumberOfVillage";
		
		string gold = "Gold";
		string wood = "Wood";
		string health = "Health";
		
		string locationx= "Locationx";
		string locationy= "Locationy";

		string vType = "vType";
		string vGold = "vGold";
		string vWood = "vWood";
		string vWage = "vWage";
		string vHealth = "vHealth";
		string vActType= "vActionType";

		//total number of players:
		int nbOfPlayer = PlayerPrefs.GetInt (id+name+pNum);
		Debug.LogError ("NUMBEROFPLAYERS" + nbOfPlayer);
		//get player and color
		for (int playerID=1; playerID <=nbOfPlayer; playerID++) {
			int color = PlayerPrefs.GetInt(id+name+pID+clr);
			//Debugging. Can be removed
			if (color ==0){
				Debug.LogError("SaveLoad-> Player Color is 0");
			}

			//ASSUMING PreserveGM is there and has a GAME component (only need players)
			GameObject GM = GameObject.Find("preserveGM");
			Game game = GM.GetComponent<GameManager>().game;


			Player newPlayer = game.getPlayers()[playerID-1];
			newPlayer.setColor(color);
			playerList.Add(newPlayer);
			
			//Villages:
			int nbOfVillages = PlayerPrefs.GetInt(id+name+pID+vNum);
			Debug.Log(nbOfVillages);
			for (int vIndex = 1; vIndex <= nbOfVillages; vIndex++){
				//hovel Location:
				float x = PlayerPrefs.GetFloat(id+name+pID+nbOfPlayer+vID+vIndex+locationx);
				float y = PlayerPrefs.GetFloat(id+name+pID+nbOfPlayer+vID+vIndex+locationy);
				Tile myLocation = getVillageTile(x, y);
				//Create Villages:
				Debug.LogError("TileLocation: " + myLocation.point);
				myLocation.networkView.RPC("setLandTypeNet", RPCMode.AllBuffered, (int) LandType.Grass);
				Vector3 hovelLocation = new Vector3(myLocation.point.x, 0.1f + offset, myLocation.point.y);
				GameObject hovel = Network.Instantiate(HovelPrefab, hovelLocation, HovelPrefab.transform.rotation, 0) as GameObject;
				//Village newVillage = Village.CreateComponent(p, TilesToReturn, location, hovel );
				Village newVillage = hovel.GetComponent<Village>();
				villageList.Add (newVillage);

				//actiontypes, gold, wood, wage, .....
				int tp = PlayerPrefs.GetInt(id+name+pID+playerID+vIndex+vType);
				newVillage.networkView.RPC("setVillageTypeNet", RPCMode.AllBuffered, tp);

				int villagegold = PlayerPrefs.GetInt(id+name+pID+playerID+vIndex+vGold);
				newVillage.networkView.RPC("setGoldNet", RPCMode.AllBuffered, villagegold);

				int vilWood = PlayerPrefs.GetInt(id+name+pID+playerID+vIndex+vWood);
				newVillage.networkView.RPC("setWoodNet", RPCMode.AllBuffered, vilWood);

				int vilWage = PlayerPrefs.GetInt(id+name+pID+playerID+vIndex+vWage);
				newVillage.networkView.RPC("setWageNet", RPCMode.AllBuffered, vilWood);

				int vilHealth = PlayerPrefs.GetInt(id+name+pID+playerID+vIndex+vHealth);
				newVillage.networkView.RPC("setHealthNet", RPCMode.AllBuffered, vilHealth);

				int vilAT = PlayerPrefs.GetInt(id+name+pID+playerID+vIndex+vActType);
				newVillage.networkView.RPC("setVillageActionNet", RPCMode.AllBuffered, vilAT);

				//set locatedAt tile for the village
				newVillage.networkView.RPC("setLocatedAtNet", RPCMode.AllBuffered, myLocation.networkView.viewID);
			}
		}
	}
	
	public Tile getVillageTile(float x, float y){
		foreach(Tile t in tileList){
			if (t.point.x==x && t.point.y==y){
				return t;
			}
		}
		Debug.LogError ("SaveLoad.cs -> getVillageTile did not return any tile!!");
		return null;
	}
	
	
	public void loadVillage(){
		string id = "1";
		string name = "savedName";
		string pID = "PlayerID";
		string pNum = "NumberOfPlayers";
		string clr = "Color";
		
		//village data names
		string vID = "VillageID";
		string vNum = "NumberOfVillage";
		string gold = "Gold";
		string wood = "Wood";
		string health = "Health";
		
		string locationx= "Locationx";
		string locationy= "Locationy";
		
		//get number of villages per player:
		
		
		
	}
	
}

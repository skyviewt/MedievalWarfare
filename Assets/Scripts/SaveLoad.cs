using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SaveLoad : MonoBehaviour {
	
	public bool saveGame = false;
	public bool loadGame = false;
	public int saveGameID = 1;
	
	//Prefabs:
	public GameObject GrassPrefab;
	public GameObject MeadowPrefab;
	public GameObject TreePrefab;
	public GameObject HovelPrefab;
	public float offset = 3.0f;
	
	
	//Saves the gameobjects instanciated
	//DELETE THIS LIST EVERY TIME THE GAME LOADS
	public List<Tile> tileList;
	public List<Village> villageList;
	public List<Player> playerList;
	// Use this for initialization
	void Start () {
		tileList = new List<Tile>();
		villageList = new List<Village>();
		playerList = new List<Player>();
	}
	
	// Update is called once per frame
	void Update () {
		if (saveGame) {
			saveThisGame(saveGameID);
			saveGame = false;
		}
		if (loadGame) {
			loadThisGame(saveGameID);
			loadGame = false;
		}
	}
	
	public void saveThisGame(int gameID){
		saveTiles(gameID);
		
	}
	
	public void loadThisGame(int gameID){
		loadTiles(gameID);
		
		tileList = new List<Tile>();
		villageList = new List<Village>();
		playerList = new List<Player>();
	}
	
	
	public void saveTiles(int gameID){
		//BE CAREFUL!!!!!! DELETES EVERYTHING
		PlayerPrefs.DeleteAll();
		
		Debug.Log ("Saving Tiles!!");
		GameObject[] tileGO = GameObject.FindGameObjectsWithTag("Grass");
		string id = "1";
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
			if (curTile.isRoad){
				PlayerPrefs.SetInt(id+name+tileNB+startIndex+road, 1);
			} else{
				PlayerPrefs.SetInt(id+name+tileNB+startIndex+road, 0);
			}
			
			//UPDATE INDEX LAST!!!
			startIndex++;
		}
		
	}
	
	public void loadTiles(int gameID){
		string id = "1";
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
			//set prefab and landType:
			
			
		}
		
		//Coloring. TODO: set prefabs
		
		//		GameObject[] tileGO = GameObject.FindGameObjectsWithTag("Grass");
		//		foreach (GameObject t in tileGO) {
		//			int color = t.GetComponent<Tile>().getColor();
		//			Debug.LogError("TileColor = " + color);
		//			t.networkView.RPC("setAndColor", RPCMode.AllBuffered, color);
		//		}
		
		
	}
	
	public void savePlayerAndVillages(){
		//player data names
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
		
		
		//Get villages by the order of players
		Debug.Log ("Saving villages!!");
		GameObject GM = GameObject.Find("PreserveGM");
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
			
			//saving each village of their respective player
			int villageNb = 1;
			foreach (Village v in villageList){
				//LocationTile:
				PlayerPrefs.SetFloat(id+name+pID+playerNb+vID+villageNb+locationx, v.getLocatedAt().point.x);
				PlayerPrefs.SetFloat(id+name+pID+playerNb+vID+villageNb+locationy, v.getLocatedAt().point.y);
			}
			
			//LAST: increment playerNb:
			playerNb++;
		}
	}
	//Does NOT load Villages
	public void loadPlayerAndVillages(){
		//player data names
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
		
		//total number of players:
		int nbOfPlayer = PlayerPrefs.GetInt (id+name+pNum);
		//get player and color
		for (int playerID=1; playerID <=nbOfPlayer; playerID++) {
			int color = PlayerPrefs.GetInt(id+name+pID+clr);
			//Debugging. Can be removed
			if (color ==0){
				Debug.LogError("SaveLoad-> Player Color is 0");
			}
			
			Player newPlayer = new Player();
			newPlayer.setColor(color);
			playerList.Add(newPlayer);
			
			//Villages:
			int nbOfVillages = PlayerPrefs.GetInt(id+name+pID+vNum);
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

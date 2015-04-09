using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SaveLoad : MonoBehaviour {
	
	public bool saveGame = false;
	public bool loadGame = false;

	//Prefabs:
	public GameObject GrassPrefab;
	public GameObject MeadowPrefab;
	public GameObject TreePrefab;
	public GameObject HovelPrefab;
	public float offset = 3.0f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (saveGame) {
			saveTiles();
			saveGame = false;
		}
		if (loadGame) {
			loadTiles();
			loadGame = false;
		}
	}
	
	public void saveTiles(){
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
			Tile a = t.GetComponent<Tile>();
			Debug.Log(a.point);
			//setting points
			PlayerPrefs.SetFloat (id+name+tileNB+startIndex+'x', a.point.x);
			PlayerPrefs.SetFloat (id+name+tileNB+startIndex+'y', a.point.y);

			
			//setting landtypes:
			PlayerPrefs.SetInt(id+name+tileNB+startIndex+land, (int)a.getLandType());
			
			//set color:
			PlayerPrefs.SetInt(id+name+tileNB+startIndex+color, a.getColor());
			//setRoad:
			if (a.isRoad){
				PlayerPrefs.SetInt(id+name+tileNB+startIndex+road, 1);
			} else{
				PlayerPrefs.SetInt(id+name+tileNB+startIndex+road, 0);
			}

			//UPDATE INDEX LAST!!!
			startIndex++;
		}
		
	}
	
	public void loadTiles(){
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
			GameObject curtile = Network.Instantiate(GrassPrefab, new Vector3(x, offset, y), GrassPrefab.transform.rotation, 0) as GameObject;
			//setting colors
			int tcol = PlayerPrefs.GetInt(id+name+tileNB+i+color);
			curtile.networkView.RPC("setAndColor", RPCMode.AllBuffered, tcol);

			LandType landtp = (LandType)PlayerPrefs.GetInt(id+name+tileNB+i+land);
			//set landtype:
			curtile.networkView.RPC("setAndColor", RPCMode.AllBuffered, (int)landtp);
			//check if it is tree or meadow. TODO: Tombstones, Roads, etc
			Vector3 position = new Vector3(x, offset, y);
			if(landtp==LandType.Trees){
				GameObject tpref = Network.Instantiate(TreePrefab, position, TreePrefab.transform.rotation, 0) as GameObject;
				tpref.transform.eulerAngles = new Vector3(0,Random.Range (0,360),0); //give it a random rotation
				curtile.networkView.RPC ("setPrefab", RPCMode.AllBuffered, tpref.networkView.viewID);
				curtile.networkView.RPC ("setLandTypeNet", RPCMode.AllBuffered, (int)LandType.Trees);
			} 
			else if(landtp==LandType.Meadow){
				GameObject tpref = Network.Instantiate(MeadowPrefab, position, TreePrefab.transform.rotation, 0) as GameObject;
				tpref.transform.eulerAngles = new Vector3(0,Random.Range (0,360),0); //give it a random rotation
				curtile.networkView.RPC ("setPrefab", RPCMode.AllBuffered, tpref.networkView.viewID);
				curtile.networkView.RPC ("setLandTypeNet", RPCMode.AllBuffered, (int)LandType.Trees);
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
		string pNum = "PlayerNumber";
		string clr = "Color";
		
		//village data names
		string vID = "VillageID";
		//string pID = "PlayerID";
		string gold = "Gold";
		string wood = "Wood";
		string health = "Health";
		
		
		//Get villages by the order of players
		Debug.Log ("Saving villages!!");
		GameObject GM = GameObject.Find("PreserveGM");
		Game game = GM.GetComponent<GameManager>().game;
		List<Player> playerList = game.getPlayers ();
		
		PlayerPrefs.SetInt (id+name+pNum, playerList.Count);
		
		int playerNb = 1;
		//save player
		foreach (Player t in playerList) {
			PlayerPrefs.SetInt(id+name+pID+clr, t.getColor());
			
			//getting the villages
			List<Village> villageList = t.getVillages();
			//saving each village of their respective player
			int villageNb = 1;
			foreach (Village v in villageList){
				
			}
		}
	}
	
	public void loadVillages(){
		
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveLoadManager
{
	public static List<Game> savedGames = new List<Game>();
	
	public static void SaveGame(Game currentGame)
	{
		SaveLoadManager.savedGames.Add (currentGame);
		BinaryFormatter binformatter = new BinaryFormatter();
		
		//Application.persistentDataPath is a string, therefore can be in Debug.Log()
		FileStream file = File.Create(Application.persistentDataPath + "/savedGames.gd");
		binformatter.Serialize(file, SaveLoadManager.savedGames);
		file.Close();
	}
	
	public static void Load()
	{
		if (File.Exists(Application.persistentDataPath + "/savedGames.gd"))
		{
			BinaryFormatter binformatter = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/savedGames.gd", FileMode.Open);
			SaveLoadManager.savedGames = (List<Game>)binformatter.Deserialize(file);
			file.Close();		
		}
	}
}

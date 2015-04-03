using UnityEngine;
using System.Collections;
using MySql.Data;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

public class DatabaseManager : MonoBehaviour {

	//MySQL instance specific items
	string constr = "Server=localhost;Database=demo;User ID=demo;Password=demo;Pooling=true";
	// connection object
	MySqlConnection con = null;
	// command object
	MySqlCommand cmd = null;
	// reader object
	MySqlDataReader rdr = null;
	// error object
	MySqlError er = null;
	// object collection array
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

using UnityEngine;
using System.Collections;
using IniParser;
using System;

public class BMX_Server_LoadConfig : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		LoadConfig ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void LoadConfig()
	{ 
		Debug.Log("readConfigData");
		FileIniDataParser parser = new FileIniDataParser();
		IniData data = new IniData();
		try
		{
			data = parser.LoadFile(Application.dataPath + "/StreamingAssets/Config.ini");
		}
		catch
		{
			Debug.Log("Not Found Config File.... create Config.ini");
			SectionData section;
			
			section = new SectionData("Option");
			section.Keys.AddKey("3D", "0");
			section.Keys.AddKey("FreeMode", "0");
			section.Keys.AddKey("Total_Coin", "0");
			section.Keys.AddKey("OneGameCoin", "1");
			section.Keys.AddKey("Difficult", "0");
			section.Keys.AddKey("Training_Time", "5");
            section.Keys.AddKey("AutoMode ", "0");
            section.Keys.AddKey("AutoModeSpeed ", "5");

            data.Sections.SetSectionData( "Option", section);
			
			section = new SectionData("Hardware");
			section.Keys.AddKey("Port", "COM4");
			section.Keys.AddKey("Speed1", "40");
			section.Keys.AddKey("Speed2", "80");
			section.Keys.AddKey("MotorSpeed", "0");
			section.Keys.AddKey("RFID", "0");
			data.Sections.SetSectionData( "Hardware", section);
			
			section = new SectionData("Network");
			section.Keys.AddKey("BMX_Server_Ip", "192.168.0.50");
			section.Keys.AddKey("BMX_Server_WaitTime", "60");
			section.Keys.AddKey("Use_Server", "1");
			section.Keys.AddKey("Use_Network", "1");
			section.Keys.AddKey("ServerNoTime", "0");
			data.Sections.SetSectionData( "Network", section);
			
			parser.SaveFile(Application.dataPath + "/StreamingAssets/Config.ini", data);
			return;
		}
		
		int _3d = Int32.Parse(data["Option"]["3D"]);
		if (_3d == 0) GameData._3D = false;
		else GameData._3D = true;
		
		int freemode = Int32.Parse(data["Option"]["FreeMode"]);
		if (freemode == 0) GameData.FREE_MODE = false;
		else GameData.FREE_MODE = true;
		
		GameData.TOTAL_COIN = Int32.Parse(data["Option"]["Total_Coin"]);
		GameData.ONEGAMECOIN = Int32.Parse(data["Option"]["OneGameCoin"]);
		GameData.DIF = Int32.Parse(data["Option"]["Difficult"]);
		GameData.TRAINING_TIME = Int32.Parse(data["Option"]["Training_Time"]);
        GameData.autoMode = data["Option"]["AutoMode"];
        GameData.autoModeSpeed = data["Option"]["AutoModeSpeed"];

        GameData.Bike_Port = data ["Hardware"] ["Port"];
		GameData.SPEED_1 = Int32.Parse(data["Hardware"]["Speed1"]);
		GameData.SPEED_2 = Int32.Parse(data["Hardware"]["Speed2"]);
		GameData.MOTOR_SPEED = Int32.Parse(data["Hardware"]["MotorSpeed"]);
		int rfid = Int32.Parse(data["Hardware"]["RFID"]);
		if (rfid == 0) GameData.USE_RFID = false;
		else GameData.USE_RFID = true;
		
		GameData.SERVER_IP = data ["Network"] ["BMX_Server_Ip"];
		GameData.SERVER_WAIT_TIME = Int32.Parse(data["Network"]["BMX_Server_WaitTime"]);
		int useserver = Int32.Parse(data["Network"]["Use_Server"]);
		if (useserver == 0) GameData.USE_SERVER = false;
		else GameData.USE_SERVER = true;
		int usenetwork = Int32.Parse(data["Network"]["Use_Network"]);
		if (usenetwork == 0) GameData.USE_NETWORK = false;
		else GameData.USE_NETWORK = true;

		GameData.noTime = Int32.Parse(data["Network"]["ServerNoTime"]);
		
		if (Application.platform == RuntimePlatform.WindowsEditor)
		{
			Caching.ClearCache();
			return;
		}
	}
}

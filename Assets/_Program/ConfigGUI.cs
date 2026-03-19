using UnityEngine;
using System.Collections;
using System;
using System.Net;
using IniParser;
using System;

public class ConfigGUI : MonoBehaviour {

	// Use this for initialization
    bool[] state = new bool[11];
    int stateValue;
    Rect rect1;
    Rect rect2;
    //bool on_off;

	void Awake()
	{
		LoadConfig ();
	}

//    void Start()
//    {
//        if (PlayerPrefs.GetInt("3D") == 0) GameData._3D = false;
//        else if (PlayerPrefs.GetInt("3D") == 1) GameData._3D = true;
//
//        if (PlayerPrefs.GetInt("FREE_MODE") == 0) GameData.FREE_MODE = false;
//        else if (PlayerPrefs.GetInt("FREE_MODE") == 1) GameData.FREE_MODE = true;
//
//        GameData.TOTAL_COIN = PlayerPrefs.GetInt("TOTAL_COIN");
//		if (PlayerPrefs.GetInt ("ONEGAMECOIN") > 0)
//						GameData.ONEGAMECOIN = PlayerPrefs.GetInt ("ONEGAMECOIN");
//				else
//						GameData.ONEGAMECOIN = 1;
//        
//        if (PlayerPrefs.GetFloat("Speed1") < 20) PlayerPrefs.SetFloat("Speed1", 40.0f);
//        if (PlayerPrefs.GetFloat("Speed2") < 20) PlayerPrefs.SetFloat("Speed2", 80.0f);
//
//        //if (PlayerPrefs.GetInt("USE_RFID") == 0) GameData.USE_RFID = false;
//        //else if (PlayerPrefs.GetInt("USE_RFID") == 1) GameData.USE_RFID = true;
//
//        GameData.MOTOR_SPEED = PlayerPrefs.GetInt("MOTOR_SPEED");
//
//		GameData.DIF = PlayerPrefs.GetInt("DIFFICULTY");
//
//        GameData.TRAINING_TIME_IDX = PlayerPrefs.GetInt("Training Time");
//
//        if (PlayerPrefs.GetInt("USE_SERVER") == 0) GameData.USE_SERVER = false;
//        else if (PlayerPrefs.GetInt("USE_SERVER") == 1) GameData.USE_SERVER = true;
//
//        if (PlayerPrefs.GetInt("USE_NETWORK") == 0) GameData.USE_NETWORK = false;
//        else if (PlayerPrefs.GetInt("USE_NETWORK") == 1) GameData.USE_NETWORK = true;
//
//        
//    }
	bool configBtn = false;
	float configBtnTime = 0;
	// Update is called once per frame
	void Update () {        
        if (GameData.ISCONFIG)
        {
            rect1 = new Rect(Screen.width / 2 - Screen.width / 4, Screen.height / 2 - Screen.height / 4, Screen.width / 2, Screen.height / 2);
            rect2 = new Rect(Screen.width / 2 - Screen.width / 6, Screen.height / 2 - Screen.height / 6, Screen.width / 3, Screen.height / 2);
            for (int i = 0; i < state.Length; i++)
            {
                if (stateValue == i) state[i] = true;
                else state[i] = false;
            }
            if (Input.GetKeyDown(KeyCode.D) || CBikeSerial.GetNewButton(0))
            {
                stateValue++;
                if (stateValue >= state.Length)
                    stateValue = 0;
            }

            switch (stateValue)
            {
                case 0:
                    if (Input.GetKeyDown(KeyCode.Space) || CBikeSerial.GetNewButton(1))
                    {
                        if (GameData._3D) GameData._3D = false;
                        else GameData._3D = true;
                    }
                    break;
                case 1:
                    if (Input.GetKeyDown(KeyCode.Space) || CBikeSerial.GetNewButton(1))
                    {
                        //GameData.DIF = (GameData.DIF+1)%10;
                        GameData.FREE_MODE = !GameData.FREE_MODE;
                    }
                    break;
                case 2:
                    if (Input.GetKeyDown(KeyCode.Space) || CBikeSerial.GetNewButton(1))
                    {
                        GameData.TOTAL_COIN = 0;
                    }
                    break;
                case 3:
                    if (Input.GetKeyDown(KeyCode.Space) || CBikeSerial.GetNewButton(1))
                    {
                        //GameData.ONEGAMECOIN = (GameData.ONEGAMECOIN) % 9;
                        //GameData.ONEGAMECOIN += 1;                        
                        GameData.MOTOR_SPEED = (GameData.MOTOR_SPEED + 1) % 40;
                    }
                    break;
                case 4:
                    if (Input.GetKeyDown(KeyCode.Space) || CBikeSerial.GetNewButton(1))
                    {
//                        float value = PlayerPrefs.GetFloat("Speed1");
                        GameData.SPEED_1 += 1.0f;
						if (GameData.SPEED_1 > 100) GameData.SPEED_1 = 20.0f;
//                        PlayerPrefs.SetFloat("Speed1", value);  
                    }
                    break;
				case 5:
                    if (Input.GetKeyDown(KeyCode.Space) || CBikeSerial.GetNewButton(1))
                    {
//                        float value = PlayerPrefs.GetFloat("Speed2");
						GameData.SPEED_2 += 1.0f;
						if (GameData.SPEED_2 > 100) GameData.SPEED_2 = 20.0f;
//                        PlayerPrefs.SetFloat("Speed2", value);
                    }
                    break;
                case 6:
                    if (Input.GetKeyDown(KeyCode.Space) || CBikeSerial.GetNewButton(1))
                    {
                        GameData.DIF = (GameData.DIF + 1) % 10;
                    }
                    break;
                case 7:
                    if (Input.GetKeyDown(KeyCode.Space) || CBikeSerial.GetNewButton(1))
                    {
						GameData.TRAINING_TIME = (GameData.TRAINING_TIME + 1);
						if (GameData.TRAINING_TIME > 60) GameData.TRAINING_TIME = 1;
                    }
                    break;
                case 8:
                    if (Input.GetKeyDown(KeyCode.Space) || CBikeSerial.GetNewButton(1))
                    {
                        GameData.USE_NETWORK = !GameData.USE_NETWORK;                        
                        GameData.USE_SERVER = GameData.USE_NETWORK;
                    }
                    break;
                case 9:
                    if (Input.GetKeyDown(KeyCode.Space) || CBikeSerial.GetNewButton(1))
                    {   
                        GameData.USE_SERVER = !GameData.USE_SERVER;
                        if (GameData.USE_SERVER)
                            GameData.USE_NETWORK = GameData.USE_SERVER;
                    }
                    break;
				case 10:
					if (Input.GetKeyDown(KeyCode.Space) || CBikeSerial.GetNewButton(1))
					{   
						GameData.ONEGAMECOIN  = (GameData.ONEGAMECOIN + 1) % 10;					
					}
					break;
			}
        }

		if (configBtn) 
		{
			configBtnTime += Time.deltaTime;
			if (configBtnTime >= 2.0f) 
			{
				configBtnTime = 0;
				configBtn = false;
			}
		}

        
        if ((Input.GetKeyDown(KeyCode.P) || CBikeSerial.GetButton(3) == 1) && !configBtn)
        {
			configBtn = true;
			Debug.Log ("Open Config");
            if (GameData.ISCONFIG)
            {
//                if (GameData._3D) 
//                    PlayerPrefs.SetInt("3D", 1);
//                else 
//                    PlayerPrefs.SetInt("3D", 0);
//
//                if (GameData.FREE_MODE)
//                    PlayerPrefs.SetInt("FREE_MODE", 1);
//                else
//				{
//                    PlayerPrefs.SetInt("FREE_MODE", 0);
//					GameData.NOW_CREDIT = 0;
//				}
//
//                PlayerPrefs.SetInt("TOTAL_COIN", 0);
//
//				PlayerPrefs.SetInt("ONEGAMECOIN", GameData.ONEGAMECOIN);
//
//                PlayerPrefs.SetInt("MOTOR_SPEED", GameData.MOTOR_SPEED);
//
//                PlayerPrefs.SetInt("Difficulty", GameData.DIF);
//                PlayerPrefs.SetInt("Training Time", GameData.TRAINING_TIME_IDX);
//
//                if (GameData.USE_NETWORK)
//                    PlayerPrefs.SetInt("USE_NETWORK", 1);
//                else
//                    PlayerPrefs.SetInt("USE_NETWORK", 0);
//
//                if (GameData.USE_SERVER)
//                    PlayerPrefs.SetInt("USE_SERVER", 1);
//                else
//                    PlayerPrefs.SetInt("USE_SERVER", 0);
				FileIniDataParser parser = new FileIniDataParser();
				IniData data = new IniData();
				data = parser.LoadFile(Application.dataPath + "/StreamingAssets/Config.ini");
                //SectionData section;

                //section = new SectionData("Option");

                if (GameData._3D)
                    data["Option"]["3D"] = "1";
                else
                    data["Option"]["3D"] = "0";
				if (GameData.FREE_MODE)
                    data["Option"]["FreeMode"] = "1";
                else
                {
                    data["Option"]["FreeMode"] = "0";
                    GameData.NOW_CREDIT = 0;
                } 
                data["Option"]["Total_Coin"] = GameData.TOTAL_COIN.ToString();
                data["Option"]["OneGameCoin"] = GameData.ONEGAMECOIN.ToString();
                data["Option"]["Difficult"] = GameData.DIF.ToString();
                data["Option"]["Training_Time"] = GameData.TRAINING_TIME.ToString();

                data["Hardware"]["Port"] = GameData.Bike_Port;
                data["Hardware"]["Speed1"] = GameData.SPEED_1.ToString();
                data["Hardware"]["Speed2"] = GameData.SPEED_2.ToString();
                data["Hardware"]["MotorSpeed"] = GameData.MOTOR_SPEED.ToString();


                if (GameData.USE_RFID)
                    data["Hardware"]["RFID"] = "1";
                else data["Hardware"]["RFID"] = "0";

                data["Network"]["BMX_Server_Ip"] = GameData.SERVER_IP;
                data["Network"]["BMX_Server_WaitTime"] = GameData.SERVER_WAIT_TIME.ToString();
                
				if (GameData.USE_SERVER)
                    data["Network"]["Use_Server"] = "1";
                else data["Network"]["Use_Server"] = "0";

                if (GameData.USE_NETWORK)
                    data["Network"]["Use_Network"] = "1";
                else data["Network"]["Use_Network"] = "0";
                data["Network"]["ServerNoTime"] = GameData.noTime.ToString();

				
				parser.SaveFile(Application.dataPath + "/StreamingAssets/Config.ini", data);

                GameData.ISCONFIG = false;

                gameObject.AddComponent<Change3D>();
                StateControl.gameMng.SetState(typeof(Change3D));
            }
            else
            {
                // Unity6 Migration: Network.Disconnect() removed (Phase 4: re-implement via UDP)
                if (GetComponent<BMX_Client_Data>())
                {
                    DestroyImmediate(GetComponent<BMX_Client_Data>());
                }
                if (GetComponent<MTB_Client_Data>())
                {
                    DestroyImmediate(GetComponent<MTB_Client_Data>());
                }
                if (GetComponent<BMX_S_Data>())
                {
                    DestroyImmediate(GetComponent<BMX_S_Data>());
                }
                if (GetComponent<MTB_S_Data>())
                {
                    DestroyImmediate(GetComponent<MTB_S_Data>());
                }
                if (GetComponent<Training_Data>())
                {
                    DestroyImmediate(GetComponent<Training_Data>());
                }
                GameData.ISCONFIG = true;
            }
        }       
	}

    void OnGUI()
    {
        if (GameData.ISCONFIG)
        {
            GUI.Box(rect1, "V-Helk ���� �޴�");
            GUI.BeginGroup(rect2);

            GUI.Box(new Rect(0, 0, 150, 25),        "3D Vision");
            GUI.Toggle(new Rect(0, 0, 150, 25),     state[0], ""); 
            GUI.Label(new Rect(160, 0, 50, 25),     GameData._3D.ToString());

            GUI.Box(new Rect(0, 30, 150, 25),       "Free Mode"); 
            GUI.Toggle(new Rect(0, 30, 25, 25),     state[1], ""); 
            GUI.Label(new Rect(160, 30, 200, 25),   GameData.FREE_MODE.ToString());

            GUI.Box(new Rect(0, 60, 150, 25),       "InCome"); 
            GUI.Toggle(new Rect(0, 62.5f, 25, 25),  state[2], "");
            GUI.Label(new Rect(160, 60, 50, 25),    GameData.TOTAL_COIN.ToString());                        
            GUI.Label(new Rect(160, 90, 200, 25),   " (Total InCome : " + PlayerPrefs.GetInt("ESC_MEMORY_COIN") + ")");

            GUI.Box(new Rect(0, 120, 150, 25),      " Motor Speed :");
            GUI.Toggle(new Rect(0, 122.5f, 25, 25), state[3], "");
            GUI.Label(new Rect(160, 120, 50, 25),   GameData.MOTOR_SPEED.ToString());

            GUI.Box(new Rect(0, 150, 150, 25),      "Pedal Basic :");        
            GUI.Toggle(new Rect(0, 152.5f, 25, 25), state[4], "");
            GUI.Label(new Rect(160, 150, 50, 25),   GameData.SPEED_1.ToString());

            GUI.Box(new Rect(0, 180, 150, 25),      "Pedal variable :");     
            GUI.Toggle(new Rect(0, 182.5f, 25, 25), state[5], "");
            GUI.Label(new Rect(160, 180, 50, 25),   GameData.SPEED_2.ToString());

            GUI.Box(new Rect(0, 210, 150, 25),      "Difficulty :");
            GUI.Toggle(new Rect(0, 212.5f, 25, 25), state[6], "");
            GUI.Label(new Rect(160, 210, 50, 25),   GameData.DIF.ToString());

            GUI.Box(new Rect(0, 240, 150, 25), "Training Time :");
            GUI.Toggle(new Rect(0, 242.5f, 25, 25), state[7], "");
            GUI.Label(new Rect(160, 240, 50, 25), GameData.TRAINING_TIME.ToString());

            GUI.Box(new Rect(0, 270, 150, 25), "Use Network :");
            GUI.Toggle(new Rect(0, 272.5f, 25, 25), state[8], "");
            GUI.Label(new Rect(160, 270, 50, 25), GameData.USE_NETWORK.ToString());

            GUI.Box(new Rect(0, 300, 150, 25), "Use Server :");
            GUI.Toggle(new Rect(0, 302.5f, 25, 25), state[9], "");
            GUI.Label(new Rect(160, 300, 50, 25), GameData.USE_SERVER.ToString());

			GUI.Box(new Rect(0, 330, 150, 25), "Need Coin Count :");
			GUI.Toggle(new Rect(0, 332.5f, 25, 25), state[10], "");
			GUI.Label(new Rect(160, 330, 50, 25), GameData.ONEGAMECOIN.ToString());
            GUI.EndGroup();
        }
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
        CBikeSerial.tonggap = float.Parse(data["Hardware"]["Tong"]);
        CBikeSerial.jBtn = data["Hardware"]["JButton"];
        CBikeSerial.jSpeed = float.Parse(data["Hardware"]["JSpeed"]);

        Debug.Log("jBtn = " + CBikeSerial.jBtn);
        Debug.Log("jSpeed = " + CBikeSerial.jSpeed);

        if (Application.platform == RuntimePlatform.WindowsEditor)
		{
			Caching.ClearCache();
			return;
		}
	}
}


using UnityEngine;
using System;
using System.Collections;
using System.Net;
using UnityEngine.SceneManagement; // Unity6 Migration

[RequireComponent(typeof(GameMng))]
[RequireComponent(typeof(GameFunctions))]

public class StateControl : MonoBehaviour {

    public static System.Type m_State;
    
    #region Member 

    private static GameMng m_GameMng;
    public static GameMng gameMng
    {
        get { return m_GameMng; }
    }

    private static GameFunctions m_GameFunctions;
    public static GameFunctions gameFunctions
    {
        get { return m_GameFunctions; }
    }

    public enum Select_Network
    {
        Server, Client
    }
    public Select_Network select_Network = Select_Network.Server;


    public string stateName;

    #endregion

    #region Functions

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void OnDestroy()
    {
        Debug.Log("STOP");
        //CRFID.Close();
    }

    void Start()
    {
		Cursor.visible = false; // Unity6 Migration: Screen.showCursor removed
        m_GameMng = GetComponent<GameMng>();
        m_GameFunctions = GetComponent<GameFunctions>();

        const int size = 1024 * 64;
        System.Object[] tmp = new System.Object[size];
        for (int i = 0; i < size; i++) tmp[i] = new byte[1024];
        tmp = null;

        // Unity6 Migration: Dns.GetHostByName removed; use GetHostEntry + IPv4 filter
        IPHostEntry IPHost = Dns.GetHostEntry(Dns.GetHostName());
        System.Net.IPAddress ipv4 = System.Array.Find(
            IPHost.AddressList,
            ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
        );
        GameData.MY_IP = (ipv4 != null) ? ipv4.ToString() : "127.0.0.1";
        string[] listIP = GameData.MY_IP.Split('.');
        Debug.Log(GameData.MY_IP);
        GameData.number = System.Convert.ToInt32(listIP[3]);
        GameData.number = (GameData.number - 1) % 10;
        if (GameData.number < 0) GameData.number = 9;
        //Debug.Log(GameData.MY_IP);
        //Debug.Log(GameData.number);
        //GameData.number = 0;

        //if (GameData.number >= 60) GameData.number = GameData.number - 50;
        //else if (GameData.number < 60) GameData.number = GameData.number % 10;
        //GameData.number = GameData.number - 1;

        if (GameData.TEST_MODE)
        {
            GameData.m_bLock = true;           
        }
        
//        if (!GameData.m_bLock)
//        {
//            bool n = CheckBoard.Check_Lock();
//
//            if (n)
//            {
//                GameData.m_bLock = true;
//                //Debug.Log("���");
//            }
//            else
//            {
//                GameData.m_bLock = false;
//                //Application.Quit();
//            }
//
//            CheckBoard.Check_System();
//        }



        //CRFID.Init();
		
        if (GameData.TEST_MODE)
        {
            // TestBootstrap이 상태 초기화를 담당하므로 여기서는 건너뜀
            return;
        }

        if (select_Network == Select_Network.Server)
        {
            ServerStart();
        }
        else
        {
			CBikeSerial.Init();
            gameObject.AddComponent<Menu_SelectGame>();
            m_GameMng.SetState(typeof(Menu_SelectGame));
        }
		
    }

    void ServerStart()
    {
        GameData.BMXServer = true;
        gameObject.AddComponent<BMX_Server_Data>();
        gameObject.AddComponent<BMX_Server_Wait>();
        m_GameMng.SetState(typeof(BMX_Server_Wait));
    }

    void Update()
    {
        //CRFID.Update();

		CBikeSerial.FrameLock(Time.deltaTime);
        if (m_State != null && stateName != m_State.ToString()) stateName = m_State.ToString();


        int coin = GameData.TEST_MODE ? 0 : CBikeSerial.GetBill(); // Unity6 Migration: skip DLL in TEST_MODE

        if (Input.GetKeyDown(KeyCode.C))
        {
            coin++;
        }

        if (coin > 0)
        {
            GameData.NOW_CREDIT += coin;
            GameData.TOTAL_COIN += coin;
            int a = PlayerPrefs.GetInt("ESC_MEMORY_COIN");
            a += coin;
            PlayerPrefs.SetInt("ESC_MEMORY_COIN", a);
            AudioCtr.Play(AudioCtr.snd_coin[0]);
        }

		if( GameData.FREE_MODE )
			GameData.NOW_CREDIT = GameData.ONEGAMECOIN;


//        if (Input.GetKeyDown(KeyCode.P) || CBikeSerial.GetButton(3) == 1)
//        {
//			Debug.Log ("Open Config");
//        //    Network.Disconnect();
//        //    if (GetComponent<BMX_Client_Data>())
//        //    {
//        //        DestroyImmediate(GetComponent<BMX_Client_Data>());
//        //    }
//        //    if (GetComponent<MTB_Client_Data>())
//        //    {
//        //        DestroyImmediate(GetComponent<MTB_Client_Data>());
//        //    }
//        //    if (GetComponent<BMX_S_Data>())
//        //    {
//        //        DestroyImmediate(GetComponent<BMX_S_Data>());
//        //    }
//        //    if (GetComponent<MTB_S_Data>())
//        //    {
//        //        DestroyImmediate(GetComponent<MTB_S_Data>());
//        //    }
//        //    if (GetComponent<Training_Data>())
//        //    {
//        //        DestroyImmediate(GetComponent<Training_Data>());
//        //    }
//        //    //gameObject.AddComponent<ConfigControl>();
//        //    //StateControl.gameMng.SetState(typeof(ConfigControl));
//        //    //Destroy(this);
//            GameData.ISCONFIG = true;
//        }
    }

//    void OnGUI()
//    {
//        if (m_State != null) GUI.Label(new Rect(Screen.width - 100, 0, 100, 50), m_State.ToString()); //���� ������Ʈ �� ǥ��
//    }

    #endregion
}

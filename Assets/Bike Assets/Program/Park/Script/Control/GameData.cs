using UnityEngine;
using System.Collections;

public class GameData : MonoBehaviour
{
    public static bool isServer = false;
    public static bool BMXServer = false;
    public static bool _3D = false;
    public static bool m_bLock = false;
    public static int GameState = 0; // 1 = MTB 0 = BMX
    public static int number;
    public static string[] Way = new string[] { "_Waypoint" };
    public static bool inGame = false;
    public static string[] map = new string[] { "Map1", "Map2_01", "Map2_02", "Map5" }; // Unity6 Migration: fixed scene name mismatch
    public static float[] remainTime = new float[] { 60.0f, 60.0f, 60.0f };
    public static int TraningMap = 0;
    public static int MTBMap = 0;
    public static Vector3[] ArrowSize_MTB = new Vector3[] { new Vector3(12, 1, 12), new Vector3(12, 1, 12), new Vector3(12, 1, 12) };
    public static string[] bmxWay = new string[] { "_Waypoint", "_Waypoint1", "_Waypoint2", "_Waypoint3" };
    public static string[] bmxStart = new string[] { "_Waypoint", "_Startpoint1", "_Startpoint2", "_Startpoint3" };
    public static int BMXMap = 0;
    public static Vector3[] ArrowSize_BMX = new Vector3[] { new Vector3(12, 1, 12), new Vector3(3, 1, 3), new Vector3(3, 1, 3), new Vector3(3, 1, 3) };
    public static int TRAINING_TIME_IDX = 0;
//    public static float[] TRAINING_TIME = new float[] { 3, 5, 10, 15, 20, 25, 30, 40, 50, 60 };
	public static int TRAINING_TIME = 3;
    #region BMX ���� ���� ����
    public static float ServerWaitTime = 60.0f;
    #endregion

    public static int MAX_PLAYER = 10;
    public static string SERVER_IP = "192.168.0.50";
    public static string DB_IP = "192.168.0.100";//"192.168.0.240:8770";
    public static string MY_IP;
    public static int FinishPlayer;
    public static float SERVER_WAIT_TIME = 60;//5.0f;
    public static float SERVER_READY_TIME = 5;//5.0f;
    public static float SERVER_FINISH_TIME = 10;//10.0f;

    public static int NOW_CREDIT = 0;

    public static int[] RANK_POINT = new int[MAX_PLAYER];
    public static bool TEST_MODE = true;

    public static bool FREE_MODE = false;
    public static bool USE_RFID = false;

    public static int demo = 0;
    public static string[] demoMap = new string[] {"ShowDemo01", "ShowDemo02"};

    public static int[] BMX_FutureRank = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    //public static int[] BMX_AIRank = new int[10];// { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    public static bool ISCONFIG = false;
    public static int DIF = 0;
    public static int TOTAL_COIN = 0;    
    public static int ONEGAMECOIN = 0;

    public static int MOTOR_SPEED = 0;
	public static float SPEED_1 = 40;
	public static float SPEED_2 = 80;

    public static bool USE_SERVER = true;
    public static bool USE_NETWORK = true;
	public static int noTime = 0;

	public static string Bike_Port = "COM2";

    public static string autoMode = "0";
    public static string autoModeSpeed = "5";
}
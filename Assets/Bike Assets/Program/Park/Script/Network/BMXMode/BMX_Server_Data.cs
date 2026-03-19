using UnityEngine;
using System.Collections;
using System;
using System.Text;

public class BMX_Server_Data : MonoBehaviour
{
    #region ���� ���� ������

    public bool _The_server_has_been_made = false;

    public float _ServerTime;
    public BMX_Champ _GUI;
    public BMX_Server_GUI _GameGUI;
    public FinishLine finishLine;

    //public float _CurrentTime;

    [System.Serializable]
    public class PlayerInfo
    {
// Unity6: public NetworkPlayer player;
        public int number = -1;
        public bool ready = false;		
        //public Cycle_Control cycle;
    }
    public PlayerInfo[] _PlayerInfo = new PlayerInfo[GameData.MAX_PLAYER];

    public int _ServerState = 0; //0 = ���. 1 = �غ� ī��Ʈ, 2 = ���� ī��Ʈ �ٿ�, 3 = ����, 4 = 2�� ���ͼ� ī��Ʈ �ٿ�, 5 = ī��Ʈ ������ ���
   
    #endregion

    #region ��ŷ ���� ������
    public Cycle_Control[] AI = new Cycle_Control[GameData.MAX_PLAYER]; //��ü AI ����Ʈ
    public Cycle_Control[] cycles = new Cycle_Control[GameData.MAX_PLAYER]; //���� �����ϴ� ����Ŭ ���� (��ȣ ������ ����)
    public RankData _RankData;
    #endregion

    #region ��¼� ���� �����ֱ�
    [System.Serializable]
    public class Replay
    {
        public Vector3[] position = new Vector3[3];
        public Quaternion[] rotation = new Quaternion[3];
        public float [] realSpeed = new float[3];
        public float [] pedalSpeed = new float[3];
        public float [] steer = new float[3];
        public int[] deadState = new int[3];
    }
    public Replay[] replay = new Replay[GameData.MAX_PLAYER];

    public void ReplaySave(int count)
    {
        for (int i = 0; i < replay.Length; i++)
        {   
            replay[i].position[count] = cycles[i].transform.position;
            replay[i].rotation[count] = cycles[i].transform.rotation;
            replay[i].realSpeed[count] = cycles[i].moveValue.realSpeed;
            //replay[i].pedalSpeed[count] = 0;
            replay[i].steer[count] = cycles[i].moveValue.steer;
            //replay[i].deadState[count] = cycles[i].deadState;
        }
    }

    public void ReplayShow(int count)
    {
        for (int i = 0; i < GameData.MAX_PLAYER; i++)
        {
            // Unity6: SendReplayPos removed (was a network RPC) -- replay not supported in Unity6 migration
            // cycles[i].SendReplayPos(replay[i].position[count], replay[i].rotation[count], replay[i].realSpeed[count], 0, replay[i].steer[count]);
        }
    }

    #endregion


// Unity6: NetworkPlayer[] m_Player;
    int m_nPlayerCount;
    // Use this for initialization
	
	public IEnumerator SendWebData( int i )
	{
		string url = GameData.DB_IP;
		switch( i )
		{
		case 0:
			url += "/admin/app_game_start_do.php?race_sn=";
	        url += System.DateTime.Now.ToString("yyyyMMdd_HH_mm_ss");	        
	        url += "&su=10&track=";
	        url += "Deajeon_Race";
	        url += "&sec=";
	        url += GameData.SERVER_READY_TIME.ToString();
			break;
		case 1:
			url += "/admin/app_game_act_do.php";
			break;
		case 2:
            //url += "/admin/app_game_result_do.php?gamerst=";
            //url += _RankData.ranklist[0].MyNumber+1;
            //url += ",";
            //url += _RankData.ranklist[1].MyNumber+1;            
            //url += ",";
            //url += _RankData.ranklist[2].MyNumber+1;

            url += "/admin/app_hsmob3_game_result_do.php?gamerst=";
            url += _RankData.ranklist[0].MyNumber + 1;
            url += ",";
            url += _RankData.ranklist[1].MyNumber + 1;
            url += ",";
            url += _RankData.ranklist[2].MyNumber + 1;
            url += ",";
            url += _RankData.ranklist[3].MyNumber + 1;
            url += ",";
            url += _RankData.ranklist[4].MyNumber + 1;		

			break;
		case 3:
			url += "/admin/app_ranklist.php?limit=20";
			break;
        case 4:
            //url += "/admin/app_ratecalc_do.php";
            url += "/admin/app_ratecalc_do.php?rlist=";
            for (int a = 0; a < GameData.BMX_FutureRank.Length; a++)
            {
                url += GameData.BMX_FutureRank[a];
                if (a < GameData.BMX_FutureRank.Length - 1)
                    url += ",";
            }
            url += "&player=";//0,0,0,0,0,0,0,0,0,0";
            for (int a = 0; a < _PlayerInfo.Length; a++)
            {
                if (_PlayerInfo[a].ready)
                    url += "1";
                else
                    url += "0";
                if (a < _PlayerInfo.Length - 1)
                    url += ",";
            }
            url += "&mode=0";
            break;
        case 5:
            //url += "/admin/app_ratecalc_do.php";
            url += "/admin/app_ratecalc_do.php?rlist=";
            for (int a = 0; a < GameData.BMX_FutureRank.Length; a++)
            {
                url += GameData.BMX_FutureRank[a];
                if (a < GameData.BMX_FutureRank.Length - 1)
                    url += ",";
            }
            url += "&player=";//0,0,0,0,0,0,0,0,0,0";
            for (int a = 0; a < _PlayerInfo.Length; a++)
            {
                if (_PlayerInfo[a].ready)
                    url += "1";
                else
                    url += "0";
                if (a < _PlayerInfo.Length - 1)
                    url += ",";
            }
            url += "&mode=1";
            break;
		}
        //Debug.Log(url);
        WWW www = new WWW(url);
		
		//Debug.Log(url);
		
		yield return www;
		//Debug.Log(www.text);

        try
        {
            switch (i)
            {
                case 0:
                    GameData.SERVER_WAIT_TIME = Convert.ToInt32(www.text);
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    //if (www.text.Length > 0)
                    //{
                    //    string[] strList = www.text.Split('/');
                    //    string[] strList_1 = strList[1].Split(',');
                    //    string[] strList_2 = strList[2].Split(',');
                    //    for (int k = 0; k < GameData.MAX_PLAYER; k++)
                    //    {
                    //        GameData.RANK_POINT[k] = Convert.ToInt32(strList_1[k]) * 3;
                    //        GameData.RANK_POINT[k] += Convert.ToInt32(strList_2[k]);
                    //        //Debug.Log( GameData.RANK_POINT[k] );
                    //    }
                    //}
                    break;
                case 5:
                    if (www.text.Length > 0)
                    {
                        string[] strList = www.text.Split(',');
                        Debug.Log(www.text);
                        for (int num = 0; num < 10; num++)
                            GameData.BMX_FutureRank[num] = Convert.ToInt32(strList[num]);
                    }
                    break;
            }
        }
        catch
        {
            //StartCoroutine( SendWebData(i) );
        }
	}
		
    void Start()
    {
        GameData.BMXMap = 2;
    }

// Unity6: void OnPlayerConnected(NetworkPlayer player)�÷��̾ �������� ���
//  {
//      PlayerAssign(player, false);
//  }

// Unity6: void OnPlayerDisconnected(NetworkPlayer player) //�÷��̾ ���� ������ ���
//  {
//      m_Player[m_nPlayerCount] = player;
//      m_nPlayerCount++;
//      PlayerAssign(player, true);
// Unity6: //Network.DestroyPlayerObjects(player);
//  }

// Unity6: void PlayerAssign(NetworkPlayer target, bool remove) -- removed (NetworkPlayer unavailable)

    public void CreatAI()
    {
        for (int i = 0; i < AI.Length; i++)
        {
// Unity6: Network.Instantiate removed — use regular Instantiate for single-player stub
            GameObject obj = Instantiate((GameObject)Resources.Load("Prefeb/Cycle " + i), new Vector3(-100, 0, -100) + Vector3.right * i, Quaternion.identity);
            AI[i] = obj.GetComponent<Cycle_Control>();
            //AI[i].GetComponent<NetworkRigidbody>().m_InterpolationBackTime = 0;
            //AI[i].GetComponent<NetworkRigidbody>().m_ExtrapolationLimit = 1;
            AI[i].name = "Cycle_AI_" + (i + 1).ToString();
            AI[i].Rb.isKinematic = true;
            AI[i].gameStart = false;
            AI[i].gameFinish = false;
            AI[i].User = false;
            AI[i].cycle_Move = false;
            AI[i].cycle_AI = false;
            AI[i].cycle_Impact = false;
            AI[i].checkRespawn = false;            
        }
        //cycles = AI;

// Unity6: m_Player = new NetworkPlayer[GameData.MAX_PLAYER];
    }

    public void AIPositionZero()
    {
        m_nPlayerCount = 0;

        finishLine.count = 0;
        finishLine.pastCount = 0;
        //finishLine.act = false;
        for (int i = 0; i < AI.Length; i++)
        {
            AI[i].Rb.isKinematic = true;
            AI[i].transform.position = new Vector3(-100, 0, -100) + Vector3.right * i;
            AI[i].transform.rotation = Quaternion.identity;
            AI[i].findWay = false;
            AI[i].GetComponent<Cycle_AI>().start = false;
            AI[i].deadState = 0;
            AI[i].wayName = "";
            AI[i].gameStart = false;
            AI[i].gameFinish = false;
            AI[i].rank = 0;
            AI[i].User = false;
            AI[i].cycle_Move = false;
            AI[i].cycle_AI = false;
            AI[i].cycle_Impact = false;
            AI[i].checkRespawn = false;
            AI[i].moveValue.heightAngle = 0;
            AI[i].moveValue.lrAngle = 0;
            AI[i].gameObject.layer = 2;
            AI[i].InitGame();
        }
        //cycles = AI;
    }

    public void AIStartPosition()
    {
        Renderer[] obj = GameObject.Find(GameData.bmxStart[GameData.BMXMap]).GetComponentsInChildren<Renderer>();
		foreach( Renderer p in  obj)
		{
		        p.GetComponent<Renderer>().enabled = false;
		}
        Transform[] sp = GameObject.Find(GameData.bmxStart[GameData.BMXMap]).GetComponentsInChildren<Transform>() as Transform[];
        finishLine = GameObject.Find("_Finish" + GameData.BMXMap).GetComponent<FinishLine>();
        //finishLine.Init();
        //finishLine.act = true;
        Minimap(GameData.BMXMap);
        for (int i = 0; i < GameData.MAX_PLAYER; i++)
        {
            if (!_PlayerInfo[i].ready)
            {
                AI[i].wayName = GameData.bmxWay[GameData.BMXMap];
                AI[i].transform.position = sp[i + 1].position;
                AI[i].transform.rotation = sp[i + 1].rotation;
                AI[i].gameStart = true;
                AI[i].Rb.isKinematic = true;
                AI[i].checkRespawn = false;
                AI[i].deadState = 0;
                AI[i].moveValue.heightAngle = 0;
                AI[i].moveValue.lrAngle = 0;
                //AI[i].minimapArrow.arrow.localScale = GameData.ArrowSize_BMX[GameData.BMXMap];
            }
        }
    }

    public void SyncServerTime()
    {
        _ServerTime -= Time.deltaTime;
    }

    public void FindPlayingPlayer()
    {
        Cycle_Control[] all = FindObjectsOfType(typeof(Cycle_Control)) as Cycle_Control[];
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].gameStart)
			{
                cycles[all[i].MyNumber] = all[i];
				cycles[all[i].MyNumber].bRecordTime = true;
			}
			
        }
        for (int i = 0; i < cycles.Length; i++)
        {
            if (cycles[i] == null)
            {
                cycles[i] = AI[i];
                //print("error = " + i.ToString());
            }
        }

        _RankData = GameObject.Find(GameData.bmxWay[GameData.BMXMap]).GetComponent<RankData>();
        _RankData.cycles = cycles;
        _RankData.Init();
    }

    public int finishPlayer;

    public int FindFinishPlayer()
    {
        int count = 0;
        for (int i = 0; i < cycles.Length; i++)
        {
            if (cycles[i].gameFinish)
                count++;
        }

        //if (finishPlayer != count)
        //{
        //    if (count == 1)
        //    {
        //        ReplaySave(0);
        //    }
        //    else if (count == 2)
        //    {
        //        ReplaySave(1);
        //    }
        //    else if (count == 3)
        //    {
        //        ReplaySave(2);
        //    }
        //    print("Save pos");
        //    finishPlayer = count;
        //}
        return count;
    }

    public void SaveScreenShot()
    {
        if (finishLine.count != finishLine.pastCount)
        {
            if (finishLine.count == 1)
            {
                ReplaySave(0);
            }
            else if (finishLine.count == 2)
            {
                ReplaySave(1);
            }
            else if (finishLine.count == 3)
            {
                ReplaySave(2);
            }
            finishLine.pastCount = finishLine.count;
        }
    }

    public void SendState(int state, float time)
    {
        for (int i = 0; i < _PlayerInfo.Length; i++)
        {
            // if (_PlayerInfo[i].ready) // Unity6: networkView.RPC("RPC_SendState") removed
        }
    }

    public void SendResult()
    {
        for (int i = 0; i < _PlayerInfo.Length; i++)
        {
            // if (_PlayerInfo[i].ready) // Unity6: networkView.RPC("RPC_ShowResult") removed
        }
    }

    public void EndState(int state, float time)
    {
        for (int i = 0; i < m_nPlayerCount; i++)
        {
// Unity6: Network.DestroyPlayerObjects(m_Player[i]);
        }
        m_nPlayerCount = 0;

        _RankData.initComplete = false;
		_RankData.ClearData ();
        for (int i = 0; i < _PlayerInfo.Length; i++)
        {
            if (_PlayerInfo[i].ready)
            {
// Unity6: networkView.RPC("RPC_EndGame", _PlayerInfo[i].player);
                _PlayerInfo[i].ready = false;
            }
            else if (_PlayerInfo[i].number != -1 && !_PlayerInfo[i].ready)
            {
                _PlayerInfo[i].ready = true;
// Unity6: networkView.RPC("RPC_SendState", _PlayerInfo[i].player, state, time, GameData.BMXMap);
            }
        }
        _RankData.cycles = null;
        _RankData = null;
        gameObject.AddComponent<BMX_Server_Wait>();
        StateControl.gameMng.SetState(typeof(BMX_Server_Wait));
    }

    // [RPC] removed Unity6 Migration
    void RPC_ShowResult()
    {
    }

    // [RPC] removed Unity6 Migration
    void RPC_SendInfo(int map, int number, float time){}

    //// [RPC] removed Unity6 Migration
    //void RPC_ReadyPosition(int state, float time) { }

    // [RPC] removed Unity6 Migration
    void RPC_SendState(int state, float time, int map) { }

    // [RPC] removed Unity6 Migration
    void RPC_EndGame() { }

    // [RPC] removed Unity6 Migration
    void RPC_SendStartpointPosition(string name,  int _0, int _1, int _2, int _3, int _4, int _5, int _6, int _7, int _8, int _9)
    {

    }

    //void OnGUI()
    //{
        //GUI.skin = (GUISkin)Resources.Load("skin");
        //GUILayout.Label("1�� ��ȣ = " + (GameData.BMX_FutureRank[0]).ToString());
        //GUILayout.Label("2�� ��ȣ = " + (GameData.BMX_FutureRank[1]).ToString());
        //GUILayout.Label("3�� ��ȣ = " + (GameData.BMX_FutureRank[2]).ToString());

        //GUILayout.Label("3�� �ȿ� �������� ��ȣ = " + (GameData.BMX_FutureRank[7]).ToString() + ", " + (GameData.BMX_FutureRank[8]).ToString() + ", " + (GameData.BMX_FutureRank[9]).ToString());
//        return;
//		GUILayout.Label("�� ���� = " + GameData.bmxWay[GameData.BMXMap]);
//        switch (_ServerState)
//        {
//            case 0:
//                GUILayout.Label("0. ��� �� ---> " + "�����ð� " + _ServerTime);
//                break;
//            case 1:
//                GUILayout.Label("1. ���� ���� �� �غ� �ð� ---> " + "ī��Ʈ �ٿ� " + _ServerTime);
//                break;
//            case 2:
//                GUILayout.Label("2. ���� ���� ī��Ʈ �ٿ�  " + _ServerTime);
//                break;
//            case 3:
//                GUILayout.Label("3. ���� ��");
//                break;
//            case 4:
//                GUILayout.Label("4. 2���� ���ͼ� 10�� ī��Ʈ �ٿ� ���� ---> " + "ī��Ʈ �ٿ� " + _ServerTime);
//                break;
//            case 5:
//                GUILayout.Label("5. ���â �����ֱ� ---> " + "��� �����ִ� �ð� " + _ServerTime);
//                break;
//        }
//
//        for (int i = 0; i < _PlayerInfo.Length; i++)
//        {
//            if (_PlayerInfo[i] != null)
//            {
//                if (_PlayerInfo[i].number != -1)
//                    GUILayout.Label(i + " �� --> " + "���� ���� " + _PlayerInfo[i].ready);
//                else
//                    GUILayout.Label(i + " �� --> " + "���� ����");
//            }
//        }
    //}

    public void Minimap(int i)
    {
        GameObject obj = GameObject.Find("MinimapCam");

        if (i == 0)
        {
            obj.GetComponent<Camera>().orthographicSize = 500;
            GameObject.Find("Minimap").GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Minimap/map_5_full"));
        }
        else if (i == 3)
        {
            obj.transform.position = new Vector3(70, 100, 330);
            obj.transform.eulerAngles = new Vector3(90, -90, 0);
            obj.GetComponent<Camera>().orthographicSize = 145;
            GameObject.Find("Minimap").GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Minimap/map5_03"));
        }
        else if (i == 1)
        {
            obj.transform.position = new Vector3(309, 100, 210);
            obj.transform.eulerAngles = new Vector3(90, 180, 0);
            obj.GetComponent<Camera>().orthographicSize = 100;
            GameObject.Find("Minimap").GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Minimap/map5_01"));
        }
        else if (i == 2)
        {
            obj.transform.position = new Vector3(460, 100, 376);
            obj.transform.eulerAngles = new Vector3(90, -90, 0);
            obj.GetComponent<Camera>().orthographicSize = 130;
            GameObject.Find("Minimap").GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Minimap/map5_02"));
        }
    }
}

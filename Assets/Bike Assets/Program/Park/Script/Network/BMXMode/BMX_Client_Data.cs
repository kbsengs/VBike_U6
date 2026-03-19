using UnityEngine;
using System.Collections;

public class BMX_Client_Data : MonoBehaviour {

    public Cycle_Control _User;
    public int _ServerState = 0;
    public float _ServerTime;
    //public float _CurrentTime;
    public bool _Result;

    public InGameGUI _GUI;
    public Cycle_Control[] cycles = new Cycle_Control[GameData.MAX_PLAYER];
    
    public void CreatCycle()
    {
        Transform[] sp = GameObject.Find(GameData.bmxStart[0]).GetComponentsInChildren<Transform>() as Transform[];
        int target = (int)Random.Range(1, sp.Length);
// Unity6: Network.Instantiate removed — use regular Instantiate for single-player stub
        GameObject obj = Instantiate((GameObject)Resources.Load("Prefeb/Cycle " + GameData.number), sp[target].position, sp[target].rotation);
        _User = obj.GetComponent<Cycle_Control>();
        //_User.GetComponent<NetworkRigidbody>().m_InterpolationBackTime = 0.01;
        //_User.GetComponent<NetworkRigidbody>().m_ExtrapolationLimit = 1;
        _User.wayName = GameData.bmxWay[0];
        _User.gameStart = false;
        _User.gameFinish = false;
        _User.User = true;
        _User.cycle_Move = true;
        _User.cycle_AI = false;
        _User.cycle_Impact = true;
        _User.Rb.isKinematic = false;
        _GUI.Minimap(0);
        _User.minimapArrow.arrow.localScale = GameData.ArrowSize_BMX[0];
        CycleCam cam = FindObjectOfType(typeof(CycleCam)) as CycleCam;
        cam.SetTarget(_User.cameraTarget, 1);
        //GameObject.Find("Eye").GetComponent<CycleCam>().SetTarget(_User.cameraTarget, 1);
        Destroy(GameObject.Find("_Loading"));
    }

    public void UserStartPosition()
    {
        Transform sp = GameObject.Find(GameData.bmxStart[GameData.BMXMap]).transform;//.GetComponentsInChildren<Transform>() as Transform[];
        _User.findWay = false;
        _User.wayName = GameData.bmxWay[GameData.BMXMap];
// Unity6: _User.networkView.RPC("WayName", RPCMode.All, _User.wayName);
        _User.deadState = 0;
        _User.moveValue.heightAngle = 0;
        _User.moveValue.lrAngle = 0;
        _User.gameStart = true;
        _User.gameFinish = false;
        _User.cycle_Move = false;
        _User.cycle_Impact = false;
        _User.checkRespawn = false;
        _User.Rb.isKinematic = true;
        _GUI.Minimap(GameData.BMXMap);
        _User.minimapArrow.arrow.localScale = GameData.ArrowSize_BMX[GameData.BMXMap];
        _User.transform.position = sp.Find((GameData.number + 1).ToString()).position;
        _User.transform.rotation = sp.Find((GameData.number + 1).ToString()).rotation;
    }

    public void SyncServerTime()
    {
        _ServerTime -= Time.deltaTime;
    }

    #region RPC
    // [RPC] removed Unity6 Migration
    void RPC_SendInfo(int map, int number, float time)
    {
        Debug.Log("�޾ҳ�?");
        GameData.BMXMap = map;
        GameData.number = number;
        _ServerTime = time;
        CreatCycle();
    }

    //// [RPC] removed Unity6 Migration
    //void RPC_ReadyPosition(int state, float time)
    //{
    //    _ServerState = state;
    //    _ServerTime = time;
    //    _User.deadState = 0;
    //    _User.gameStart = true;
    //    _User.cycle_Move = false;
    //    _User.cycle_AI = false;
    //}

    // [RPC] removed Unity6 Migration
    void RPC_SendState(int state, float time, int map)
    {
        GameData.BMXMap = map;
        if (_ServerState != 5)
        {
            _ServerState = state;
            _ServerTime = time;
        }
        if (state == 1)
        {
            _GUI.Count5();
            _User.deadState = 2;
        }
    }

    // [RPC] removed Unity6 Migration
    void RPC_ShowResult()
    {
        _Result = true;
    }

    // [RPC] removed Unity6 Migration
    void RPC_EndGame()
    {
// Unity6: Network.RemoveRPCs(Network.player);
// Unity6: Network.DestroyPlayerObjects(Network.player);
// Unity6: Network.Disconnect();
        //gameObject.AddComponent<Menu_SelectGame>();
        //StateControl.gameMng.SetState(typeof(Menu_SelectGame));
    }

    // [RPC] removed Unity6 Migration
    void RPC_SendStartpointPosition(string name, int _0, int _1, int _2, int _3, int _4, int _5, int _6, int _7, int _8, int _9)
    {
        StartPointControl sp = GameObject.Find(GameData.bmxStart[GameData.BMXMap]).GetComponent<StartPointControl>();
        sp.num[0] = _0;
        sp.num[1] = _1;
        sp.num[2] = _2;
        sp.num[3] = _3;
        sp.num[4] = _4;
        sp.num[5] = _5;
        sp.num[6] = _6;
        sp.num[7] = _7;
        sp.num[8] = _8;
        sp.num[9] = _9;
        sp.SetPos();
    }

    #endregion

//    void OnGUI()
//    {
//        GUILayout.Label("�� ���� = " + GameData.bmxWay[GameData.BMXMap]);
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
//                GUILayout.Label("5. ���â �����ֱ�");
//                break;
//        }
//    }

    public void FindCycles()
    {
        Cycle_Control[] all = FindObjectsOfType(typeof(Cycle_Control)) as Cycle_Control[];
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].gameStart)
                cycles[all[i].MyNumber] = all[i];
        }
    }

// Unity6: void OnDisconnectedFromServer(NetworkDisconnection info)
//  {
//      gameObject.AddComponent<Menu_SelectGame>();
//      StateControl.gameMng.SetState(typeof(Menu_SelectGame));
//      DestroyImmediate(this);
//  }

	// Unity6: OnFailedToConnect removed (legacy network callback)
	// void OnFailedToConnect(NetworkConnectionError error)
	// {
	//     Debug.Log("Connect Failed");
	//     gameObject.AddComponent<Menu_SelectGame>();
	//     StateControl.gameMng.SetState(typeof(Menu_SelectGame));
	//     DestroyImmediate(this);
	// }
}

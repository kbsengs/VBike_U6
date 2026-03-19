using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class MTB_Client_Data : MonoBehaviour
{
    #region Game
    public float _ServerTime = GameData.ServerWaitTime; //���� ���� �ð�
    public int _ReadyPlayer; //���� �غ�� �÷��̾�
    public int _ServerState; //���� ����

    public Cycle_Control MyCharacter;
    public MTB_Champ _GUI;
    public InGameGUI _GameGUI;

    public Cycle_Control[] cycles = new Cycle_Control[GameData.MAX_PLAYER];
    public Cycle_Control[] ai;

    public RankData _RankData;

    public bool InGameState = false;

    public int[] players = new int[GameData.MAX_PLAYER];

    public void Synctime()
    {
        _ServerTime -= Time.deltaTime;
    }
    #endregion

    // Unity6: OnFailedToConnect removed (legacy network callback)
    // void OnFailedToConnect(NetworkConnectionError error) { ... }

// Unity6: //void OnDisconnectedFromServer(NetworkDisconnection info)
    //{
    //    gameObject.AddComponent<Menu_SelectGame>();
    //    StateControl.gameMng.SetState(typeof(Menu_SelectGame));
    //    DestroyImmediate(this);
    //}

    public void CreatBike()
    {
        Transform startpoint = GameObject.Find("_Startpoint/" + (GameData.number + 1).ToString()).GetComponent<Transform>();//�ӽ���
// Unity6: Network.Instantiate removed — use regular Instantiate for single-player stub
        GameObject obj = Instantiate((GameObject)Resources.Load("Prefeb/Cycle " + GameData.number), startpoint.position, startpoint.rotation);
        MyCharacter = obj.GetComponent<Cycle_Control>();
        MyCharacter.wayName = "_Waypoint";
        MyCharacter.Rb.isKinematic = true;
        MyCharacter.User = true;
        MyCharacter.cycle_Impact = false;
        MyCharacter.minimapArrow.arrow.localScale = GameData.ArrowSize_MTB[GameData.MTBMap];
        CycleCam cam = FindObjectOfType(typeof(CycleCam)) as CycleCam;
        cam.SetTarget(MyCharacter.cameraTarget, 1);
        //GameObject.Find("Eye").GetComponent<CycleCam>().SetTarget(MyCharacter.cameraTarget, 1);
        Destroy(GameObject.Find("_Loading"));
    }

    public void CreatAI()
    {

        ai = new Cycle_Control[GameData.MAX_PLAYER - (_ReadyPlayer + 1)];
        int count = 0;
        for (int i = 0; i < GameData.MAX_PLAYER; i++)
        {
            if (players[i] == -1) //�ش� ����Ƽ �Ҵ��� �ȵǸ� AI ĳ���ͷ� ä���.
            {
                Transform startpoint = GameObject.Find("_Startpoint" + (i + 1).ToString()).GetComponent<Transform>();
// Unity6: Network.Instantiate removed — use regular Instantiate for single-player stub
                GameObject obj = Instantiate((GameObject)Resources.Load("Prefeb/Cycle " + i), startpoint.position, startpoint.rotation);
                ai[count] = obj.GetComponent<Cycle_Control>();
                ai[count].wayName = "_Waypoint";
                ai[count].Rb.isKinematic = true;
                ai[count].User = false;
                ai[count].cycle_Impact = false;
                ai[count].minimapArrow.arrow.localScale = GameData.ArrowSize_MTB[GameData.MTBMap];
                count++;
            }
        }
    }

    public void RankDataSetting()
    {
        Cycle_Control[] all = FindObjectsOfType(typeof(Cycle_Control)) as Cycle_Control[];
        for (int i = 0; i < all.Length; i++)
        {
            cycles[all[i].MyNumber] = all[i];
        }
// Unity6: if (Network.isServer)
        {
            _RankData = GameObject.Find("_Waypoint").GetComponent<RankData>();
            _RankData.cycles = cycles;
            _RankData.Init();
        }
    }

    public int FindFinishPlayer()
    {
        int count = 0;
        for (int i = 0; i < cycles.Length; i++)
        {
            if (cycles[i].gameFinish)
                count++;
        }
        return count;
    }

    // [RPC] removed Unity6 Migration
    void SendSyncState(int state, float time)
    {
        _ServerState = state;
        _ServerTime = time;
        if (state == 1)
        {
            _GUI.Countdown();
        }
    }
}


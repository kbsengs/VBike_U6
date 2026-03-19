using UnityEngine;
using System.Collections;

public class MTB_LobbyServer : MonoBehaviour
{

    #region ���� ���� ����

    public float _ServerTime = GameData.SERVER_WAIT_TIME;
    public string _ServerIP;
    public int _ServerNumber = -1;

    public int _ReadyPlayer;

    #endregion

    [System.Serializable]
    public class PlayerInfo
    {
// Unity6: public NetworkPlayer player;
        public int number = -1;
        public bool ready = false;
    }
    public PlayerInfo[] _PlayerInfo = new PlayerInfo[GameData.MAX_PLAYER];
    

    public int _ServerState = 0; //0 = ���, 1 = �غ� ī��Ʈ

    void Start()
    {
		
		Screen.SetResolution(640,480,false);
// Unity6: Network.incomingPassword = "MTB";
// Unity6: Network.InitializeServer(GameData.MAX_PLAYER, 52000, false);
    }

    void Update()
    {
        SetServer();
        if (_ReadyPlayer > 0)
            SyncTime();
        else
            _ServerTime = GameData.SERVER_WAIT_TIME;
        if (_ServerState == 0)
        {
            for (int i = 0; i < _PlayerInfo.Length; i++)
            {
                if (_PlayerInfo[i].ready)
                {
                    // Unity6: RPC_SendWhoServer removed (NetworkPlayer unavailable)
                // if (_ServerNumber == _PlayerInfo[i].number) { RPC true } else { RPC false }
                }
            }
            if (_ServerTime <= 0)
            {
// Unity6: Network.maxConnections = Network.connections.Length;
                _ServerState = 1;
                _ServerTime = GameData.SERVER_READY_TIME;
                SendState();
            }
        }
        else if (_ServerState == 1)
        {
            if (_ServerTime <= 0)
            {
                _ServerState = 2;
                _ServerTime = GameData.SERVER_READY_TIME;
                SendState();
            }
        }
        else if (_ServerState == 2)
        {
            _ServerState = 0;
// Unity6: Network.maxConnections = GameData.MAX_PLAYER;
            GameData.MTBMap = 0;
            _ServerTime = GameData.SERVER_WAIT_TIME;
            for (int i = 0; i < _PlayerInfo.Length; i++)
            {
                if (_PlayerInfo[i].number != -1 && !_PlayerInfo[i].ready)
                {
                    _PlayerInfo[i].ready = true;
// Unity6: networkView.RPC("RPC_SendState", _PlayerInfo[i].player, _ServerState, _ServerTime);
                }
            }
        }
    }

// Unity6: void OnPlayerConnected(NetworkPlayer player) //�÷��̾ �������� ���
//  {
//      PlayerAssign(player, false);
//  }

// Unity6: void OnPlayerDisconnected(NetworkPlayer player) //�÷��̾ ���� ������ ���
//  {
//      PlayerAssign(player, true);
// Unity6: Network.RemoveRPCs/DestroyPlayerObjects removed
//  }

    void SetServer()
    {
// Unity6: if (Network.connections.Length > 0)
        {
            for (int i = 0; i < _PlayerInfo.Length; i++)
            {
// Unity6: if (_PlayerInfo[i].ready && (_PlayerInfo[i].player == Network.connections[0]))
                // Unity6: NetworkPlayer removed — server assignment not available
                // { _ServerNumber = _PlayerInfo[i].number; _ServerIP = ...; }
            }
        }
        // else -- Unity6: Network.connections removed
        // { _ServerNumber = -1; _ServerIP = ""; }
    }

// Unity6: void PlayerAssign(NetworkPlayer target, bool remove) -- removed (NetworkPlayer unavailable)

    void SyncTime()
    {
        _ServerTime -= Time.deltaTime;
    }

    void SendState()
    {
        for (int i = 0; i < _PlayerInfo.Length; i++)
        {
            // if (_PlayerInfo[i].ready) // Unity6: networkView.RPC("RPC_SendState") removed
        }
    }

    // [RPC] removed Unity6 Migration
    void RPC_SendInfo(int map, int num, float time)
    {

    }

    // [RPC] removed Unity6 Migration
    void RPC_PlayerCount(int num)
    {

    }

    // [RPC] removed Unity6 Migration
    void RPC_SendWhoServer(string ip, bool isServer)
    {

    }

    // [RPC] removed Unity6 Migration
    void RPC_SendState(int state, float time)
    {
        _ServerState = state;
        _ServerTime = time;
    }

    // [RPC] removed Unity6 Migration
    void RPC_SyncMap(int map)
    {
        GameData.MTBMap = map;
    }

    void OnGUI()
    {
        for (int i = 0; i < _PlayerInfo.Length; i++)
        {
            if (_PlayerInfo[i] != null)
            {
                if (_PlayerInfo[i].number != -1)
                    GUILayout.Label(i + " �� --> " + "���� ���� " + _PlayerInfo[i].ready);
                else
                    GUILayout.Label(i + " �� --> " + "���� ����");
            }
        }
        GUILayout.Label("State " + _ServerState);
        GUILayout.Label("Server " + _ServerNumber);
        GUILayout.Label("ServerIP " + _ServerIP);
        GUILayout.Label(_ServerTime.ToString());
        GUILayout.Label("Map " + GameData.MTBMap);
    }
}

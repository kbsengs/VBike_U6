using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class BMX_Multi_Wait : GameState {

    #region UDP
    private UdpClient server = null;
    private UdpClient client = null;
    private IPEndPoint receivePoint;
    private string port = "9300";
    private int listenPort = 27000;
    private string ip = "0.0.0.0";
    private string ip_broadcast = "255.255.255.255";
    private int youServer = 0;
    private string server_name = "";
    private int clear_list = 0;
    private string myip;
    private float ftime = 0;
    private int start = 0;

    public void Close()
    {
        if (client != null) client.Close();
        if (server != null) server.Close();
    }

    public void LoadClient()
    {
        client = new UdpClient(System.Convert.ToInt32(port));
        receivePoint = new IPEndPoint(IPAddress.Parse(ip), System.Convert.ToInt32(port));
        Thread startClient = new Thread(new ThreadStart(start_client));
        startClient.Start();
        start = 1;
    }

    public void start_client()
    {
        bool continueLoop = true;

        try
        {
            while (continueLoop)
            {
                byte[] recData = client.Receive(ref receivePoint);
                System.Text.ASCIIEncoding encode = new System.Text.ASCIIEncoding();
                server_name = encode.GetString(recData);
                if (server_name != "")
                {
                    start = 4;
                    client.Close();
                    break;
                }
                if (start == 2) break;
            }
        }
        catch { }
    }

    public void start_server()
    {
        try
        {
            while (true)
            {
                System.Text.ASCIIEncoding encode = new System.Text.ASCIIEncoding();

                byte[] sendData = encode.GetBytes(myip);
                server.Send(sendData, sendData.Length, ip_broadcast, System.Convert.ToInt32(port));
                Thread.Sleep(100);
                if (start == 3) return;
            }
        }
        catch { }
    }

    void NewStartServer()
    {
        client.Close();
        Thread.Sleep(10);
        //GameData.isServer = true;
// Unity6: Network.incomingPassword = "BMX";
// Unity6: Network.InitializeServer(32, listenPort, false);

        _Data.players[GameData.number] = GameData.number;

        server = new UdpClient(System.Convert.ToInt32(port));
        receivePoint = new IPEndPoint(IPAddress.Parse(myip), System.Convert.ToInt32(port));
        Thread startServer = new Thread(new ThreadStart(start_server));
        startServer.Start();
        print("start server " + myip.ToString());
    }

    void StartNetwork()
    {
        //GameData.isServer = false;
        print(server_name);
        print(listenPort);
// Unity6: Network.Connect(server_name, listenPort, "BMX");
        print("connected !!!!");
    }

    void EndServer()
    {
        start = 3;
        server.Close();
    }

// Unity6: void OnPlayerConnected(NetworkPlayer player) //�÷��̾ �������� ���
//  {
//      _Data._ReadyPlayer++;
// Unity6: networkView.RPC("SendReadyPlayer", ...) removed
//  }

// Unity6: void OnPlayerDisconnected(NetworkPlayer player) //�÷��̾ ���� ������ ���
//  {
//      _Data._ReadyPlayer--;
// Unity6: networkView.RPC/Network.RemoveRPCs/DestroyPlayerObjects removed
//  }

	// Unity6: OnFailedToConnect removed (legacy network callback)
	// void OnFailedToConnect(NetworkConnectionError error) { ... }

// Unity6: //void OnServerInitialized()
    //{

    //}

// Unity6: void OnConnectedToServer()
//  {
// Unity6: networkView.RPC("SetMyInfo", RPCMode.Server, GameData.number, true);
//  }

// Unity6: void OnDisconnectedFromServer(NetworkDisconnection info)
//  {
// Unity6: networkView.RPC("SetMyInfo", RPCMode.Server, GameData.number, false);
//  }

    // [RPC] removed Unity6 Migration
    void SetMyInfo(int num, bool set)
    {
        if (set)
            _Data.players[num] = num;
        else
            _Data.players[num] = -1;

// Unity6: if (Network.isServer)
        {
            for (int i = 0; i < _Data.players.Length; i++)
            {
                if (_Data.players[i] != -1)
                {
// Unity6: networkView.RPC("SetMyInfo", RPCMode.Others, i, true);
                }
                else
                {
// Unity6: networkView.RPC("SetMyInfo", RPCMode.Others, i, false);
                }
            }
        }
    }
    // [RPC] removed Unity6 Migration
    void SendReadyPlayer(int num, float time)
    {
        _Data._ReadyPlayer = num;
        _Data._ServerTime = time;
    }

    // [RPC] removed Unity6 Migration
    void SendSyncMap(int map)
    {
        GameData.BMXMap = map;
    }
    #endregion

    #region Members
    BMX_Multi_Data _Data; //��Ƽ ������ ����
    #endregion

    #region OnActivate
    public override void OnActivate()
    {
        Debug.Log("Start");
        LoadClient();
// Unity6: myip = Network.player.ipAddress.ToString();
        Debug.Log(myip);

        _Data = GetComponent<BMX_Multi_Data>(); //������ ��������
        GameObject obj = Instantiate((GameObject)Resources.Load("Bmx_multi")) as GameObject;
        _Data._GUI = obj.GetComponent<MTB_Champ>();
        for (int i = 0; i < _Data.players.Length; i++)
        {
            _Data.players[i] = -1;
        }
        GameMng.m_StartUpdate = true;
    }
    #endregion

    #region OnUpdate
    public override void OnUpdate()
    {
        #region UDP
        if (start == 1)
        {
            ftime += Time.deltaTime;
            if (ftime > 2)
            {
                ftime = 0;
                start = 2;
                NewStartServer();
            }
        }
        else if (start == 4)
        {
            StartNetwork();
            start = 5;
        }

        if (clear_list++ > 200)
        {
            server_name = "";
            clear_list = 0;
        }
        #endregion
        if (_Data._ReadyPlayer > 0)
            _Data.Synctime();
        else
            _Data._ServerTime = GameData.ServerWaitTime;

        if (_Data._ServerState == 0)
        {
            _Data._GUI.WaitTime(_Data._ServerTime);
// Unity6: if (Network.isServer)
            {
                if (Input.GetKeyDown(KeyCode.D) || CBikeSerial.GetNewButton(2))
                {
                    GameData.BMXMap++;
                    if (GameData.BMXMap > 2)
                    {
                        GameData.BMXMap = 0;
                    }

                    _Data._GUI.TrackArrow(1);
                }
// Unity6: networkView.RPC("SendSyncMap", RPCMode.Others, GameData.BMXMap);
                if (Input.GetKeyDown(KeyCode.A) || CBikeSerial.GetNewButton(0))
                {
// Unity6: Network.Disconnect();
                    gameObject.AddComponent<Menu_SelectGame>();
                    StateControl.gameMng.SetState(typeof(Menu_SelectGame));
                    DestroyImmediate(_Data);
                }

                _Data._GUI.ImServer(true);
                if (_Data._ReadyPlayer >= 0)
                {
                    if (Input.GetKeyDown(KeyCode.Space) || CBikeSerial.GetNewButton(1) || _Data._ServerTime <= 0)
                    {
                        _Data._ServerState = 1;
                        _Data._ServerTime = GameData.SERVER_READY_TIME;
// Unity6: networkView.RPC("SendSyncState", RPCMode.All, _Data._ServerState, _Data._ServerTime);
                        Close();
                    }
                }
            }
            // else -- Unity6: Network.isServer check removed; ImServer(false) skipped
            // { _Data._GUI.ImServer(false); }
        }
        else if (_Data._ServerState == 1)
        {
            _Data._GUI.WaitTime(0);
// Unity6: if (Network.isServer && _Data._ServerTime <= 0)
            {
                _Data._ServerState = 2;
                _Data._ServerTime = GameData.SERVER_READY_TIME;
// Unity6: networkView.RPC("SendSyncState", RPCMode.All, _Data._ServerState, _Data._ServerTime);
            }
        }
        else if (_Data._ServerState == 2)
        {
            GameObject obj = Instantiate((GameObject)Resources.Load("Prefeb/Loading_BMX")) as GameObject;
            obj.name = "_Loading";
            gameObject.AddComponent<BMX_Multi_InGame>();
            StateControl.gameMng.SetState(typeof(BMX_Multi_InGame));
        }
// Unity6: _Data._GUI.Server_Client(Network.isServer);
        _Data._GUI.ShowPlayerNumber(GameData.number);
        _Data._GUI.TotalPlayer(_Data._ReadyPlayer + 1);
        _Data._GUI.TrackSelect(GameData.BMXMap);
    }
    #endregion

    #region OnDeactivate
    public override void OnDeactivate()
    {
        DestroyImmediate(this);
    }
    #endregion
}

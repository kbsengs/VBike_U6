using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;

// Unity6 Migration: All legacy Network.* / networkView.* / [RPC] calls removed.
// The custom UDP broadcast (UdpClient) logic is preserved.
// Network.InitializeServer / Network.Connect / OnPlayerConnected / [RPC] methods
// are stubbed behind #if LEGACY_NETWORK for Phase 4 reference.
// TODO Phase 4: Re-implement multiplayer using custom UDP message system.

public class UDPConnection : MonoBehaviour {

    private UdpClient server = null;
    private UdpClient client = null;
    private IPEndPoint receivePoint;
    private string port = "9200";
    private int listenPort = 26000;
    private string ip = "0.0.0.0";
    private string ip_broadcast = "255.255.255.255";
    private int youServer = 0;
    private string server_name = "";
    private int clear_list = 0;
    private string myip;
    private float ftime = 0;
    private int start = 0;

    public void Update()
    {
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
    }

    public void Start()
    {
        Debug.Log("UDPConnection Start");
        LoadClient();
        // Unity6 Migration: Network.player.ipAddress removed
        myip = GetLocalIPv4();
        Debug.Log("myip=" + myip);
    }

    // Unity6 Migration: replace Network.player.ipAddress
    private string GetLocalIPv4()
    {
        try
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ip4 = System.Array.Find(host.AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);
            return (ip4 != null) ? ip4.ToString() : "127.0.0.1";
        }
        catch { return "127.0.0.1"; }
    }

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
        // Unity6 Migration: Legacy Network.InitializeServer removed
        // TODO Phase 4: Re-implement server init via custom UDP
        // Network.incomingPassword = "MTB";
        // Network.InitializeServer(32, listenPort, false);

        server = new UdpClient(System.Convert.ToInt32(port));
        receivePoint = new IPEndPoint(IPAddress.Parse(myip), System.Convert.ToInt32(port));
        Thread startServer = new Thread(new ThreadStart(start_server));
        startServer.Start();
        Debug.Log("start server " + myip);
    }

    void StartNetwork()
    {
        // Unity6 Migration: Legacy Network.Connect removed
        // TODO Phase 4: Re-implement via custom UDP connect
        Debug.Log("StartNetwork: server=" + server_name + " port=" + listenPort);
        // Network.Connect(server_name, listenPort, "MTB");
    }

    void EndServer()
    {
        start = 3;
        server.Close();
    }

    [System.Serializable]
    public class PlayerInfo
    {
        public bool ready = false;
    }
    public PlayerInfo[] _PlayerInfo = new PlayerInfo[GameData.MAX_PLAYER];
    public int _ReadyPlayer;

    // Unity6 Migration: OnPlayerConnected/Disconnected, OnServerInitialized,
    // OnConnectedToServer, OnDisconnectedFromServer, [RPC] SetMyInfo all removed.
    // TODO Phase 4: Restore via custom UDP message handlers.

    void OnGUI()
    {
        GUILayout.Label("Is Server = " + GameData.isServer);
    }
}

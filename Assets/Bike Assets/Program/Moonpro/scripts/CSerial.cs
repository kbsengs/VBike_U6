using UnityEngine;
using System.Collections;
using System;
using System.IO.Ports;
   
public class CSerial : MonoBehaviour
{

    

    // port name
    public string PortName = "COM5";
    // port speed
    public int bps = 19200;
    // recevdata
    private string recvData = "";
    // port
    private static SerialPort port;

    private float starttime = 0;

#region Serial_Base
    void Awake()
    {
        DontDestroyOnLoad(this);
    }
	
	void Start () {
        starttime = Time.time + 3;
        comport_Open();
	}

    void OnApplicationQuit()
    {
        try
        {
            port.Close();
            if (!port.IsOpen) print("Serial Close OK!");
        }
        catch (Exception)
        { }
    }

    void comport_Open() 
    {
        try
        {
            // Sensor
            port = new SerialPort(PortName, bps, Parity.None, 8, StopBits.One);
            port.Open();
            port.ReadTimeout = 1;
            recvData = "";

            if (port.IsOpen) print(PortName + " open success!!");
        }
        catch (Exception)
        { }
    }


    private void comport_ReadData()
    {
        try
        {
            byte tmpByte = 0;
            do
            {
                tmpByte = (byte)port.ReadByte();
                if (0 >= tmpByte) return;
                recvData += (char)tmpByte;

                //print(tmpByte + " = " + (char)tmpByte);
            } while (tmpByte > 0);
        }
        catch (Exception)
        {
           // Debug.Log("Serial (" + SenderPortName + ") port Read Error");
        }
    }

    char get()
    {
        char b = (char)0;
        if (recvData.Length > 0)
        {
            b = recvData[0];
            recvData = recvData.Substring(1);
        }
        return b;
    }

    void Update()
    {

        if (starttime > Time.time) return;

        if (port == null) comport_Open();        
        comport_ReadData();
        process();
    }
    #endregion

    #region Bike Serial

    public static bool m_bCall = false;
    

    public static void write(String str)
    {
        try
        {
            port.Write(str);
        }
        catch (Exception)
        {
            Debug.Log("Send Data Fail");
        }
    }

    public static void Init_Data()
    {
        m_bCall = false;
    }

    float checkTime = 0;
    float tick = 0;
    public void process()
    {
        char C;

        while (recvData.Length > 0)
        {
            // Sensor
            C = recvData[0];
            switch (C)
            {
                case 'C':
                    {
                        if (recvData[1] == 1)
                        {
                            get();
                            get();
                            if( GameData.FREE_MODE )
                                GameData.NOW_CREDIT = GameData.ONEGAMECOIN;
                        }
                    }
                    break;
                default:
                    get();
                    break;
            }
        }

    }
    #endregion
}

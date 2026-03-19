using UnityEngine;
using System.Collections;
using System.Threading;
using System.IO.Ports;
using System.Text;
using System;
using System.Collections.Generic;


//namespace ZOIT
//{
public class zoSerial {

    bool isThreadRun = false;
    Thread loopThread;
    public SerialPort Port;
    private List<byte> pRecvData = new List<byte>();
    private Hashtable pProtocolList = new Hashtable();

    //private static ZOSerial s_instance = null;

    //public static ZOSerial instancelock
    //{
    //    get
    //    {
    //        if (null == s_instance)
    //        {
    //            s_instance = FindObjectOfType(typeof(ZOSerial)) as ZOSerial;
    //            if (null == s_instance)
    //            {
    //                Debug.Log("Fail to get Manager Instance");
    //            }
    //        }
    //        return s_instance;
    //    }
    //}

    public int state = 0;
    ~zoSerial()
    {
        Close();
    }

    public void zoUpdate()
    {
        while (isThreadRun)
        {
            //Thread.Sleep(1);

            ReadData();

            if (pRecvData.Count > 0)
            {
                if (state == 0)
                {
                    if (pRecvData[pRecvData.Count - 1] == 42)
                    {
                        state = 1;
                        // Debug.Log("start");
                    }
                }
                else
                {
                    if (pRecvData[pRecvData.Count - 1] == 42)
                    {
                        ProcessPacket(pRecvData.GetRange(2, pRecvData.Count - 3).ToArray());
                        pRecvData.RemoveRange(0, pRecvData.Count - 2);
                    }                    
                }
                /*
                if (pProtocolList.ContainsKey(pRecvData[0]))
                {
                    int t = Convert.ToInt32(pProtocolList[pRecvData[0]].ToString());
                    if (t <= pRecvData.Count)
                    {
                        ProcessPacket(pRecvData.GetRange(0, t).ToArray());
                        pRecvData.RemoveRange(0, t);
                    }
                }
                else
                    pRecvData.RemoveRange(0, 1);
                */
            }
        }
    }
    void ReadData()
    {
        try
        {

            pRecvData.Add((byte)Port.ReadByte());
        }
        catch (Exception)
        {
            //Debug.Log("ZOSerial port Read Error");
        }
    }

    public bool IsOpen
    {
        get
        {
            if (Port == null)
                return false;

            return Port.IsOpen;
        }
    }
    public void OpenPort(string portName, int nRate)
    {
        Port = new SerialPort(portName, nRate, Parity.None, 8, StopBits.One);
        Port.ReadTimeout = 2;
        Port.Encoding = Encoding.ASCII;
        Port.Open();

        if (Port.IsOpen)
            Debug.Log("ZOSerial Start");
        else
            Debug.Log("Port Open fail!");

        Port.WriteLine("start");


        loopThread = new Thread(new ThreadStart(zoUpdate));
        loopThread.IsBackground = true;
        isThreadRun = true;
        loopThread.Start();
        state = 0;
    }
    public void Close()
    {
        isThreadRun = false;
        if (Port != null && Port.IsOpen) Port.Close();
        if (loopThread != null && loopThread.IsAlive) loopThread.Abort();

    }
    public void ClearProtocol()
    {
        pProtocolList.Clear();
    }
    public bool add_Protocol(byte uHeader, int nLength)
    {
        if (pProtocolList.ContainsKey(uHeader) == false)
        {
            pProtocolList.Add(uHeader, nLength);
            return true;
        }
        Debug.Log("중복 : Key=" + uHeader + " : Value = " + nLength);
        return false;
    }
    public void ProcessPacket(byte[] pData)
    {
        if (PacketRecved != null) // 이벤트가 선언되었다면 
        {
            PacketRecved(this, pData);  // 이벤트 함수 실행.
        }
        else
            Debug.Log("Not Found at PacketRecved Function.  data size = " + pData.Length);
    }

    public delegate void RecvPacket(object sender, byte[] pData);
    public event RecvPacket PacketRecved; // 이벤트 선언

    public void SendData(byte[] p, int cnt)
    {
        if (IsOpen && Port != null && Port.IsOpen)
            Port.Write(p, 0, cnt);
    }
}
//}


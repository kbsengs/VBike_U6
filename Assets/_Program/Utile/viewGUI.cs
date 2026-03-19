using UnityEngine;
using System.Collections;
using System;
//using ZOIT;
using System.Runtime.InteropServices;
//[DllImport("zoSerial")]
public class viewGUI : MonoBehaviour
{
    

    public GUISkin[] s1;

    //private float hSliderValue = 0.0F;
    //private float vSliderValue = 0.0F;
    //private float hSValue = 0.0F;
    //private float vSValue = 0.0F;
    private int cont = 0;

    private bool bConnect = false;
    private string connectMessage;

    int selGridInt = 1;
    string[] selStrings = new string[] { "COM1", "COM2", "COM3", "COM4"};
    string selRate = "115200";

    private bool btBrake = false;
    private bool btSetup = false;
    private bool btUp = false;
    private bool btDown= false;
    private bool btAccount = false;

    //private float fAccel = 0.0f;
    //private float fHandle = 0.0f;
    public float fPitch = 0.0f;
	float foldPitch = 0.0f;
	
	public float fMspeed = 0.0f;
	float foldMspeed = 0.0f;
	
    public int m_nCount = 0;
	
	public int m_nAccel = 0;
	public int m_nHandle = 0;
	public int m_nBrake = 0;

    private zoSerial pSerial = new zoSerial();

    char[] m_Test;
    string m_strVal;
    Vector3 m_vecRot;
    Vector3 m_vecStart;
    void Start()
    {
        pSerial.PacketRecved += new zoSerial.RecvPacket(ProcessPacket);

        pSerial.add_Protocol((byte)('*'), 10);

        m_Test = new char[100];
    }
    void OnDisable()
    {
        pSerial.Close();
    }
 
    int nCnt = 0;
    void ProcessPacket(object sender, byte[] pData)
    {
        System.Array.Copy(pData, 0, m_Test, 0, pData.Length);
        m_strVal = new string(m_Test);

        string[] result = m_strVal.Split(new char[] { ',' });

        m_vecRot.x = float.Parse(result[0]);
        m_vecRot.y = float.Parse(result[1]);
        m_vecRot.z = float.Parse(result[2]);
        if (pSerial.state == 1)
        {
            m_vecStart = m_vecRot;
            pSerial.state = 2;
        }
    }

    bool once;
    void Update()
    {
        if (!once)
        {
            int nRate = 0;
            if (Int32.TryParse(selRate, out nRate))
                pSerial.OpenPort(selStrings[selGridInt], nRate);
            else
                selRate = "115200";
            reset();
            once = true;
        }

        transform.localEulerAngles= new Vector3(-m_vecRot.y + m_vecStart.y, m_vecRot.z - m_vecStart.z, -m_vecRot.x);
        //Vector3 pos = Vector3.zero;

        //pos.y = (m_vecRot.z - m_vecStart.z) * 0.002f + 1;
        //Camera.main.transform.position = pos;
    }

    void reset()
    {
        m_nCount = 0;
        
        btBrake = false;
        btSetup = false;
        btUp = false;
        btDown = false;
        btAccount = false;

        m_nAccel = 0;
        m_nHandle = 0;
        fPitch = 0.0f;

        nCnt = 0;

    }
    //void OnGUI()
    //{
    //    bConnect = pSerial.IsOpen;

    //    if (bConnect)
    //    {
    //        connectMessage = "Disconnect";
    //        GUI.Label(new Rect(10, 10, 100, 30), "연결되었습니다. nCnt = " + nCnt);
    //    }
    //    else
    //    {
    //        connectMessage = "Connection";
    //    }

    //    string str;
    //    str = "x = " + m_vecRot.x + " y = " + m_vecRot.y + " z = " + m_vecRot.z;
    //    GUI.Label(new Rect(800, 10, 200, 30), str);

    //    selGridInt = GUI.SelectionGrid(new Rect(10, 40, 400, 30), selGridInt, selStrings, 4);

    //    GUI.Label(new Rect(260, 10, 200, 30), "Baud Rate (must Number)!");
    //    selRate = GUI.TextField(new Rect(430, 5, 100, 30), selRate);
    //    if (GUI.Button(new Rect(430, 40, 100, 30), connectMessage))
    //    {
    //        if (bConnect)
    //        {
    //            pSerial.Close();
    //            selRate = "115200";
    //            selGridInt = 0;
    //        }
    //        else
    //        {
    //            int nRate = 0;
    //            if (Int32.TryParse(selRate, out nRate))
    //                pSerial.OpenPort(selStrings[selGridInt], nRate);
    //            else
    //                selRate = "115200";
    //            reset();
    //        }
    //    }
    //}
}
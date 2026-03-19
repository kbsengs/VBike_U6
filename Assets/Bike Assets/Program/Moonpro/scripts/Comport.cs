using UnityEngine;
using System.Collections;
using System;
using System.IO.Ports;
  
public class Comport : MonoBehaviour {
    public string portname = "COM3";
    public int bps = 19200;    
    public string recvData = "";

    private SerialPort port;
    private float starttime = 0;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }
	
	void Start () {
        starttime = Time.time + 3;
        raw_start();
	}

    void raw_start() 
    {
        port = new SerialPort(portname, bps, Parity.None, 8, StopBits.One);
        port.Open();
        port.ReadTimeout = 2;
        recvData = "";
    }
	
	void Update () {

        if (starttime > Time.time) return;
        if (port == null) raw_start();
        ReadData();
	}

    private void ReadData()
    {
        float t = 0;
        try
        {
            byte tmpByte = 0;
            do
            {   
                tmpByte = (byte) port.ReadByte();
                if( 0 >= tmpByte ) return;
                recvData += (char)tmpByte;
            }while(tmpByte > 0 );            
        }
        catch( Exception )
        {
            //Debug.Log("Serial (" + portname + ") port Read Error");
        }
    }
}

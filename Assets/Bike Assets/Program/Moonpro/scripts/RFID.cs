using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct MW_EasyPOD
{
    public uint VID;			        // Need to match user device's "Vendor ID".
    public uint PID;			        // Need to match user device's "Product ID".
    public uint ReadTimeOut;		    // Specifies the read data time-out interval, in milliseconds.
    public uint WriteTimeOut;		    // Specifies the write data time-out interval, in milliseconds.    
    public uint Handle;                 // Do not modify this value, reserved for DLL
    public uint FeatureReportSize;      // Do not modify this value, reserved for DLL
    public uint InputReportSize;        // Do not modify this value, reserved for DLL
    public uint OutputReportSize;       // Do not modify this value, reserved for DLL   
} 

    
class RFID :MonoBehaviour
{
    [DllImport("EasyPOD")]
	public static extern uint ConnectPOD(ref MW_EasyPOD pEasyPOD, uint Index);

    [DllImport("EasyPOD")]
	public static extern uint WriteData(ref MW_EasyPOD pEasyPOD, byte[] lpBuffer, uint nNumberOfBytesToWrite, ref uint lpNumberOfBytesWritten);

    [DllImport("EasyPOD")]
	public static extern uint ReadData(ref MW_EasyPOD pEasyPOD, byte[] lpBuffer, uint nNumberOfBytesToRead, ref uint lpNumberOfBytesRead);

    [DllImport("EasyPOD")]
	public static extern uint DisconnectPOD(ref MW_EasyPOD pEasyPOD);

    [DllImport("EasyPOD")]
    public static extern uint ClearPODBuffer(ref MW_EasyPOD pEasyPOD);
	
	
	MW_EasyPOD pPOD;
	bool bConnect = false;
	float tTime = 0.0f;
	byte[] pData;
	
	byte STX = 0x02;
 	byte ETX = 0x03;
	
	uint gID;
	byte[] ID;
	
	public bool bChecked = false;
 	//EXE_DELAY Sleep(100);
	void Start()
	{
		ID = new byte[4];
		pData = new byte[8];

		bConnect = false;
		pPOD = new MW_EasyPOD();

		if (GameData.TEST_MODE) return; // Unity6 Migration: skip EasyPOD DLL in TEST_MODE

		pPOD.VID = 0x0e6a;
		pPOD.PID = 0x317;

		try
		{
			uint r = ConnectPOD( ref pPOD, 1 );
			if( r == 0 )
			{
				bConnect = true;
				ClearPODBuffer(ref pPOD);
			}
		}
		catch (System.Exception e)
		{
			Debug.LogWarning("RFID.Start: EasyPOD DLL not available - " + e.Message);
		}
	}
	
	void OnDestroy()
	{
		DisconnectPOD( ref pPOD );
	}
	void Update()
	{
		if( !bConnect ) return;
		
		tTime += Time.deltaTime;
		
		if( tTime > 1.0f )
		{
			tTime = 0;
			uint nReadSize =0 ;
			uint nWriteSize=0;

			ReadData( ref pPOD, pData, 8, ref nReadSize );
			if( nReadSize > 0 ) 
			{
				if( nReadSize == 8 && pData[0] == STX)
				{					
					if( pData[1] == Convert.ToByte('i') && pData[2]	== 0 && pData[3] == 0 && pData[4] == 0 && pData[5] == 0 && pData[6] == ETX && pData[7] == Convert.ToByte('i') )
					{
						Debug.Log( "Card In" );	
						pData[0] = STX;
						pData[1] = Convert.ToByte('I');
						pData[2] = ETX;
						pData[3] = Convert.ToByte('I');
						WriteData( ref pPOD, pData, 4, ref nWriteSize );
						
						pData[0] = STX;
						pData[1] = Convert.ToByte('R');
						pData[2] = ETX;
						pData[3] = Convert.ToByte('R');
						WriteData( ref pPOD, pData, 4, ref nWriteSize );
						  
						ReadData( ref pPOD, pData, 8, ref nReadSize );
						
						if( nReadSize == 8 && pData[0] == STX)
						{
							if( pData[1] == Convert.ToByte('r') && pData[6] == ETX)
							{						
								ID[0] = pData[2];
								ID[1] = pData[3];
								ID[2] = pData[4];
								ID[3] = pData[5];
							
								gID = BitConverter.ToUInt32(ID,0);
								//Debug.Log("Card ID = "+ gID );
								
								StartCoroutine( GetWebData() );
							}
						}							
					}
					if( pData[1] == Convert.ToByte('o') && pData[2]	== 0 && pData[3] == 0 && pData[4] == 0 && pData[5] == 0 && pData[6] == ETX && pData[7] == Convert.ToByte('o') )
					{
						GameData.NOW_CREDIT = 0;
						Debug.Log("Card Out");
						pData[0] = STX;
						pData[1] = Convert.ToByte('O');
						pData[2] = ETX;
						pData[3] = Convert.ToByte('O');
						WriteData( ref pPOD, pData, 4, ref nWriteSize );
					}
				}
			}
			else if( GameData.NOW_CREDIT < 0 )
			{
				pData[0] = STX;
				pData[1] = Convert.ToByte('R');
				pData[2] = ETX;
				pData[3] = Convert.ToByte('R');
				WriteData( ref pPOD, pData, 4, ref nWriteSize );
				  
				ReadData( ref pPOD, pData, 8, ref nReadSize );
				
				if( nReadSize == 8 && pData[0] == STX)
				{
					if( pData[1] == Convert.ToByte('r') && pData[6] == ETX)
					{						
						ID[0] = pData[2];
						ID[1] = pData[3];
						ID[2] = pData[4];
						ID[3] = pData[5];
					
						gID = BitConverter.ToUInt32(ID,0);
						//Debug.Log("Card ID = "+ gID );
						
						StartCoroutine( GetWebData() );
					}
				}							
			}
		}
	}
	
	
	public IEnumerator GetWebData()
	{
		string url = GameData.DB_IP;
		
		url += "/admin/app_bmx_card_check.php?mode=1&cardid=";
		url += gID.ToString();
			
		
		
	    WWW www = new WWW(url);
		
		//Debug.Log(url);		
		
		yield return www;
		
		GameData.NOW_CREDIT =  int.Parse( www.text );		
		Debug.Log( GameData.NOW_CREDIT );
	}

	public IEnumerator IsStart()
	{
		bChecked = false;
		string url = GameData.DB_IP;
		
		url += "/admin/app_bmx_card_check.php?mode=2&cardid=";
		url += gID.ToString();
			
		
		
	    WWW www = new WWW(url);
		
		//Debug.Log(url);		
		
		yield return www;
		
		if( www.text == "ok" )
			bChecked = true;
		else
			bChecked = false;
		//Debug.Log( www.text );
	}
}


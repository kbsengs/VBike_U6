using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

public class CheckBoard {

    [DllImport("io")]
    private static extern void PortOut(int Port, byte Data);
    [DllImport("io")]
    private static extern void PortWordOut(int Port, short Data);
    [DllImport("io")]
    private static extern byte PortIn(int Port);
    [DllImport("io")]
    public static extern int PortWordIn(int Port);


    static int hio;
    
    static int IO_Index = 0x0295; //메모리가 아닌 I/O번지 입니다
    static int IO_Data = 0x0296;  //

    byte reg25;
	byte reg26;
	byte reg27;
    byte Vendor_ID_high;
    byte Vendor_ID_low;
    byte Chip_ID;
	
	public static char[] Data = new char[6];
    //public static int Init()
    //{
    //    hio = LoadLibrary("io");
    //    return hio;

    //}

    //public static void Unload()
    //{   
    //    FreeLibrary(hio);
    //}
    
    //////////////////////////////////////////
    // 레지스터에서 읽기   //
    //////////////////////////////////////////
    static byte Read_Reg(byte i)
    {
	    byte d;
	    PortWordOut(IO_Index, i);
	    d = PortIn(IO_Data);

	    //printf("데이타 읽기 = 0x%02X \n",d);


	    return d;
    }

    //////////////////////////////////////////
    // 레지스터에 쓰기   //
    //////////////////////////////////////////
    static void Write_Reg(byte i, byte d)
    {

	    //print("데이타 쓰기 < 0x%02X > < 0x%02X >\n", i, d);

	    PortWordOut(IO_Index, i);
	    PortWordOut(IO_Data, d);

	    //print("데이타 썼당 < 0x%02X > < 0x%02X >\n", i, d);
    }

    //////////////////////////////////////////////////
//      //
// 시스템 확인      //
// ===========     //
//      //
// Vendor ID : 0x5CA3    //
// Chip ID   : 0xC1    //
// 12V레지스터 : 50 이하의 값   //
//     수정전 보드에서는 200 이상  //
//      //
// 위 조건 중 전체 또는 일부 확인  //
//////////////////////////////////////////////////
    public static bool Check_Lock()
    {
		//byte Val_12V;
		byte Val_25;
		byte Val_26;
		byte Val_27;
		byte reg26;
		byte reg27;
	    byte Vendor_ID_high;
	    byte Vendor_ID_low;
	    byte Chip_ID;
			
        try
        {
            Write_Reg(0x4e, 0x80);
            Vendor_ID_high = Read_Reg(0x4f);
            Write_Reg(0x4e, 0x00);
            Vendor_ID_low = Read_Reg(0x4f);
            Chip_ID = Read_Reg(0x58);
            Val_25 = Read_Reg(0x25);
			Val_26 = Read_Reg(0x26);
			Val_27 = Read_Reg(0x27);

            if ((Vendor_ID_high == 0x5c)
                && (Vendor_ID_low == 0xa3)
                && (Chip_ID == 0xc1)
                && (Val_25 < 255)
				&& (Val_26 < 50)
			    && (Val_27 < 255)
			    )
            { 
                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        { 
            return false;
        }
    }
	
	public static char[] Check_System()
    {
//		byte reg25;
//		byte reg26;
//		byte reg27;
//	    byte Vendor_ID_high;
//	    byte Vendor_ID_low;
//	    byte Chip_ID;
		
		
        try
        {
            Write_Reg(0x4e, 0x80);
            Data[0] = (char)Read_Reg(0x4f);
            Write_Reg(0x4e, 0x00);
            Data[1] = (char)Read_Reg(0x4f);
            Data[2] = (char)Read_Reg(0x58);
            Data[3] = (char)Read_Reg(0x25);
			Data[4] = (char)Read_Reg(0x26);
			Data[5] = (char)Read_Reg(0x27);
			
//			foreach( byte b in Data )
//				Debug.Log( b );
			
			Data[3] = (char)30;
			Data[0] = (char)92;
			Data[4] = (char)30;
			Data[1] = (char)163;
			Data[5] = (char)120;
			Data[2] = (char)193;
			return Data;
        }
        catch
        { 
            return Data;
        }
    }
	//c		 3      0      4      1       5       2
	//67 188 30 225 92 163 30 225 163 92 120 135 193 62 xx ~xx
}

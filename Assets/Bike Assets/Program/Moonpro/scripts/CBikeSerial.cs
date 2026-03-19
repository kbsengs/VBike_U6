using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

public class CBikeSerial : MonoBehaviour
{
    [DllImport("BikeSerial")]
    private static extern void InitBikeSerial();
    [DllImport("BikeSerial")]
    private static extern bool OpenBikeSerial(string port);
    [DllImport("BikeSerial")]
    private static extern void EndBikeSerial();
    [DllImport("BikeSerial")]
    public static extern int GetTilt();
    [DllImport("BikeSerial")]
    public static extern int GetButton(int i);
    [DllImport("BikeSerial")]
    public static extern int GetSwitch1(int i);
    [DllImport("BikeSerial")]
    public static extern int GetSwitch2(int i);
    [DllImport("BikeSerial")]
    public static extern int GetBreak(int i);
    [DllImport("BikeSerial")]
    public static extern float GetSpeed();
    [DllImport("BikeSerial")]
    public static extern int GetHandle();
    [DllImport("BikeSerial")]
    public static extern int GetBill();
    [DllImport("BikeSerial")]
    private static extern void Send(char c);
    [DllImport("BikeSerial")]
    private static extern int GetPos();
    [DllImport("ZoLock")]
    private static extern int CheckLock(string app);

    public static float tonggap = 2.0f;
    public static string jBtn = "B";
    public static float jSpeed = 1;

    public static int OneHandle;
    public static int start = 0;

    public static float m_fFrame = 0;
    public static float m_fSerialHandle;
    public static float m_fPedalSpeed = 0;
    public static float m_fSteer;
    public static int b1, b2;

    public static float m_fBrakeTorque = 20;
    public static int saveenv;

    static bool button1, button2, button3;
    public static int m_nSetButton = 0;

    static int m_nAngleSpeed;
    static int m_nCurrAngle;

    public static int m_nJump;
    public static int m_nTong;
    static float m_fTongTime;
    static int m_nBreak;
    public static int m_nCrash;

    static float m_fLockTime;
    static int m_nLockState;

    static int m_nEnv = 0;

    static float m_fSpeed1 = 50, m_fSpeed2 = 80;

    static int Max_Angle_Speed = 6;

    public static bool Init()
    {
        button1 = false;
        button2 = false;
        button3 = false;
        if (start > 0) return false;

        m_nJump = 0;
        m_nTong = 0;
        m_nBreak = 0;
        m_nCrash = 0;

        // Unity6 Migration: Skip DLL calls when TEST_MODE (no hardware available)
        if (GameData.TEST_MODE)
        {
            start = 1;
            Debug.Log("CBikeSerial: TEST_MODE - hardware DLL skipped");
            return false;
        }

        try
        {
            EndBikeSerial();
            Thread.Sleep(100);
            start = 1;
            InitBikeSerial();
            bool b = OpenBikeSerial(GameData.Bike_Port);
            Debug.Log(GameData.Bike_Port);
            if (b) Debug.Log("serial open");
            Debug.Log("state=" + b);
            return b;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("CBikeSerial.Init failed (no hardware?): " + e.Message);
            start = 1;
            return false;
        }
    }

    public static int GetHandle2()
    {
        if (GameData.TEST_MODE) return 0; // Unity6 Migration: no hardware in TEST_MODE
        int handle = GetHandle() - 15;
        if (handle > 0) handle--;

        if (Mathf.Abs(handle) < 3) handle = 0;

        return -handle;
    }

    public static void Handle()
    {
        if (OneHandle == 0)
        {
            if (GetHandle2() < -5) OneHandle = -1;
            else if (GetHandle2() > 5) OneHandle = 1;
        }
        else if (OneHandle == -1)
        {
            if (GetHandle2() < -5) OneHandle = -2;
            else if (GetHandle2() > 5) OneHandle = 1;
            else OneHandle = 0;
        }
        else if (OneHandle == 1)
        {
            if (GetHandle2() < -5) OneHandle = -1;
            else if (GetHandle2() > 5) OneHandle = 2;
            else OneHandle = 0;
        }
        else
        {
            if (Mathf.Abs(GetHandle2()) < 5) OneHandle = 0;
        }
    }

    public static void FrameDead(float frame)
    {
        float tong = 0.2f;
        switch (m_nCrash)
        {
            case 1:
                AngleSpeed(3);
                FuncNormal(25.0f);
                m_nCrash = 2;
                m_fTongTime = 0;
                break;
            case 2:
                m_fTongTime += frame;
                if (m_fTongTime <= tong) return;
                m_fTongTime = 0;
                m_nCrash = 3;
                FuncNormal(-25.0f);
                break;
            case 3:
                m_fTongTime += frame;
                if (m_fTongTime <= tong) return;
                m_fTongTime = 0;
                m_nCrash = 0;
                FuncNormal(0);
                break;
        }
    }

    public static void FrameLock(float frame)
    {
        if (GameData.TEST_MODE) return; // Unity6 Migration: no hardware in TEST_MODE
        if (m_nLockState == 0) return;

        m_fLockTime += frame;

        if (m_nLockState == 1)
        {
            if (m_fLockTime > 3.0f)
            {
                m_fLockTime = 0;
                m_nLockState = 2;
                SendBreak(0);
            }
        }
        else
        {
            if (m_fLockTime > 250.0f)
            {
                m_fLockTime = 0;
                if (m_nLockState == 2)
                {
                    SendOk(2);
                    m_nLockState = 3;
                }
                else
                {
                    SendOk(1);
                    m_nLockState = 2;
                }
            }
        }
    }

    public static void InitGame()
    {
        if (GameData.TEST_MODE) return; // Unity6 Migration: no hardware in TEST_MODE
        SendOk(0);
        m_fLockTime = 0;
        m_nLockState = 1;
        ReadSpeed();
    }

    public static void SetEnv(int env)
    {
        m_nEnv = env;
    }

    public static void ReadSpeed()
    {
		m_fSpeed1 = GameData.SPEED_1;
		m_fSpeed2 = GameData.SPEED_2;
    }

    public static void FrameBike(float frame, float heightangle, float maxhandle, int env, float speed)
    {
        if (GameData.TEST_MODE) return; // Unity6 Migration: no hardware in TEST_MODE
        if (env == 10 && saveenv != 10 && m_nTong == 0) m_nTong = 1;
        else if (env != 10 && saveenv == 10 && m_nTong != 0) m_nTong = 0;

        FuncAngle(frame, speed, heightangle * 1.5f);
        // ���� ����
        //if(speed > 0) FrameBikeSerial2(frame);

        float max = 150;
        m_fPedalSpeed = GetSpeed() * 12.5f;
        if (m_fPedalSpeed > max) m_fPedalSpeed = max;

        b1 = GetBreak(0);
        b2 = GetBreak(1);


        float slow = 0;
        float val1 = 5, val2 = 5, val3 = 1, val4 = 0.2f;
        int b = b1 + b2;
        if (b1 > 2 && b2 > 2) b *= 2;
        slow = -val1 - b * val2 - heightangle * val3 - env * val4;
        slow *= 2;

        //
        saveenv = m_nEnv;

        m_fFrame += frame;

        int val;

        float f = 1;
        if (m_fPedalSpeed > 10) f = 0.5f;
        else f = 1 - m_fPedalSpeed * 0.05f;

        float br = (float)(b) / 16.0f;

        float s = m_fSpeed1 + m_fSpeed2;
        float v;
        float gap = 5.0f;
        // ���� 65����
        if (m_nEnv == 30)
        {
            v = m_fSpeed1 + m_fSpeed2 * 0.5f;
            val = (int)(v + (s - v + gap) * br);
        }
        // �� 60����
        else if (m_nEnv == 20)
        {
            v = m_fSpeed1 + m_fSpeed2 * 0.3f;
            val = (int)(v + (s - v + gap) * br);
        }
        // �׿�
        else
        {
            float h = heightangle * 1.2f;
            if (h > 30) h = 30;
            else if (h < -30) h = -30;
            h = h / 30.0f;
            if (m_nJump == 1 || m_nJump == 2) h = -1;

            v = m_fSpeed1 + m_fSpeed2 * h;
            val = (int)(v + (s - v + gap) * br);            
        }

        //val = (int)(heightangle * 0.5f) + (int)(10 * f) + (int)(b * 2) + (int)(env * 0.12f);
        if (val < 0) val = 0;
        if (val > 100) val = 100;

        m_fBrakeTorque = b * 30 + 20 + env;

        if (m_fFrame > 0.1f)
        {
            Break(val);
            m_fFrame = 0;
        }

        float currentVelocity = 0.0f;
        int a = 0;
        if (-GetHandle() > 0) a = 1;
        else a = -1;
        //m_fSteer = Mathf.SmoothDampAngle(maxhandle * -GetHandle2() / 15.0f, maxhandle * a, ref currentVelocity, 0.5f);
        m_fSteer = maxhandle * -GetHandle2() / 15.0f;
    }

    static void FuncAngle(float frame, float speed, float heightangle)
    {
        if( m_nCrash > 0 ) FrameDead(frame);
        else if (m_nJump > 0) FuncJump(frame);
        else if (m_nTong > 0)
        {
            if (speed <= 0.1) return;
            FuncTong(speed, frame, heightangle);
        }
        else
        {   
            AngleSpeed( (int)(Max_Angle_Speed * speed / 20) );
            FuncNormal(heightangle);
        }

        //Debug.Log("crash = " + m_nCrash + " jump = " + m_nJump + " Tong = " + m_nTong);
    }

    static void  FuncTong(float speed, float frame, float heightangle)
    {
        m_fTongTime += frame;
        float tong = 0.05f;

        if (m_fTongTime <= tong) return;

        m_fTongTime = 0;

        // �ӵ� 5 ~ 15
        AngleSpeed((int)(3 * speed / 20));
        switch (m_nTong)
        {
            case 1:
                FuncNormal(25.0f / tonggap);
                m_nTong = 2;
                break;
            case 2:
                FuncNormal(-25.0f / tonggap);
                m_nTong = 3;
                break;
            case 3:
                FuncNormal(heightangle);
                m_nTong = 1;
                break;
        }
    }

    static void FuncJump(float frame)
    {
        float tong = 0.2f;
        switch (m_nJump)
        {
            case 1:
                AngleSpeed(1);
                FuncNormal(-25.0f);
                m_nJump = 2;
                break;
            case 3:
                AngleSpeed(3);
                FuncNormal(25.0f);
                m_fTongTime += frame;
                if (m_fTongTime <= tong) return;
                m_fTongTime = 0;
                m_nJump = 4;
                m_fTongTime = 0;
                break;
            case 4:
                m_fTongTime += frame;
                if (m_fTongTime <= tong) return;
                m_fTongTime = 0;
                m_nJump = 5;
                FuncNormal(-25.0f);
                break;
            case 5:
                m_fTongTime += frame;
                if (m_fTongTime <= tong) return;
                m_fTongTime = 0;
                m_nJump = 0;
                break;
        }
    }

    public static void SendPos(int pos)
    {
//        if (!GameData.m_bLock)
//            pos = (int)(pos * 0.1f);
//
        char c = 'A';
        Send(c);
        Send((char)(~c));
        c = (char)pos;
        Send(c);
        Send((char)(~c));

        //Debug.Log(pos);
    }

    static void SendSpeed(int speed)
    {
//        if (!GameData.m_bLock)
//            speed = (int)(speed * 0.1f);

        char c = 'S';
        Send(c);
        Send((char)(~c));
        c = (char)speed;
        Send(c);
        Send((char)(~c));
    }

    static void SendBreak(int br)
    {
//        if (!GameData.m_bLock)
//            br = (int)(br * 0.1f);

        char c = 'B';
        Send(c);
        Send((char)(~c));
        c = (char)br;
        Send(c);
        Send((char)(~c));
    }

    static void SendOk(int state)
    {
        char c = 'C';
        Send(c);
        Send((char)(~c));

//        if (GameData.m_bLock)
//        {
            if (state < 2)
            {
                //c = CheckBoard.Data[3]; Send(c); Send((char)(~c));
                //c = CheckBoard.Data[0]; Send(c); Send((char)(~c));
                //c = CheckBoard.Data[4]; Send(c); Send((char)(~c));
                //c = CheckBoard.Data[1]; Send(c); Send((char)(~c));
                //c = CheckBoard.Data[5]; Send(c); Send((char)(~c));
                //c = CheckBoard.Data[2]; Send(c); Send((char)(~c));

                c = (char)30; Send(c); Send((char)(~c));
                c = (char)92; Send(c); Send((char)(~c));
                c = (char)30; Send(c); Send((char)(~c));
                c = (char)163; Send(c); Send((char)(~c));
                c = (char)120; Send(c); Send((char)(~c));
                c = (char)193; Send(c); Send((char)(~c));
                c = (char)Random.Range(0, 256); Send(c); Send((char)(~c));
            }
            else
            {
                int t = Random.Range(0, 2);

                if (t == 0)
                {
                    c = CheckBoard.Data[3]; Send(c); Send((char)(~c));
                    c = (char)Random.Range(0, 256); Send(c); Send((char)(~c));
                    c = CheckBoard.Data[4]; Send(c); Send((char)(~c));
                    c = (char)Random.Range(0, 256); Send(c); Send((char)(~c));
                    c = CheckBoard.Data[5]; Send(c); Send((char)(~c));
                    c = (char)Random.Range(0, 256); Send(c); Send((char)(~c));
                    c = (char)Random.Range(0, 256); Send(c); Send((char)(~c));
                }
                else
                {
                    c = (char)Random.Range(0, 256); Send(c); Send((char)(~c));
                    c = CheckBoard.Data[0]; Send(c); Send((char)(~c));
                    c = (char)Random.Range(51, 256); Send(c); Send((char)(~c));
                    c = CheckBoard.Data[1]; Send(c); Send((char)(~c));
                    c = (char)Random.Range(0, 256); Send(c); Send((char)(~c));
                    c = (char)Random.Range(0, 256); Send(c); Send((char)(~c));
                    c = (char)Random.Range(0, 256); Send(c); Send((char)(~c));
                }
            }
//        }
//        else
//        {
//            int t = Random.Range(0, 2);
//
//            if (t == 0)
//            {
//                c = CheckBoard.Data[3]; Send(c); Send((char)(~c));
//                c = (char)Random.Range(0, 256); Send(c); Send((char)(~c));
//                c = CheckBoard.Data[4]; Send(c); Send((char)(~c));
//                c = (char)Random.Range(0, 256); Send(c); Send((char)(~c));
//                c = CheckBoard.Data[5]; Send(c); Send((char)(~c));
//                c = (char)Random.Range(0, 256); Send(c); Send((char)(~c));
//                c = (char)Random.Range(0, 256); Send(c); Send((char)(~c));
//            }
//            else
//            {
//                c = (char)Random.Range(0, 256); Send(c); Send((char)(~c));
//                c = CheckBoard.Data[0]; Send(c); Send((char)(~c));
//                c = (char)Random.Range(51, 256); Send(c); Send((char)(~c));
//                c = CheckBoard.Data[1]; Send(c); Send((char)(~c));
//                c = (char)Random.Range(0, 256); Send(c); Send((char)(~c));
//                c = (char)Random.Range(0, 256); Send(c); Send((char)(~c));
//                c = (char)Random.Range(0, 256); Send(c); Send((char)(~c));
//            }
//        }
    }

    static void FuncNormal(float heightangle)
    {
        int angle = (int)(heightangle * 1.5f + 50.0f);
        if (angle < 10) angle = 10;
        else if (angle > 90) angle = 90;

        if (m_nCurrAngle == (int)(angle)) return;
        m_nCurrAngle = (int)(angle);

        //Debug.Log("h = "+ heightangle);
        SendPos(m_nCurrAngle);
    }

    static void AngleSpeed(int speed)
    {
//        if (!GameData.m_bLock)
//            speed = 1;
//        else
            speed = (int)(speed * 2.0f);// 0.065f);

        speed += +GameData.MOTOR_SPEED;
        //Debug.Log("AndgleSpeed = " + speed);

        if (m_nAngleSpeed == speed) return;

        m_nAngleSpeed = speed;
        SendSpeed(m_nAngleSpeed);
    }

    public static void Break(int val)
    {
        if (m_nBreak == val) return;

        float f = val;
        if (f > 100) f = 100;
        if (f < 0) f = 0;

        m_nBreak = (int)f;
        SendBreak(m_nBreak);
    }

    public static void InitIntro()
    {
        if (GameData.TEST_MODE) return; // Unity6 Migration: no hardware in TEST_MODE
        AngleSpeed(2);
        Thread.Sleep(10);
        SendPos(50);
        Thread.Sleep(10);
        Break(0);
        m_nLockState = 0;
    }

    public static bool GetNewSwitch1(int i)
    {
        if (GameData.TEST_MODE) return false; // Unity6 Migration: no hardware in TEST_MODE
        if (GetSwitch1(i) == 1) return true;
        else return false;
    }

    public static bool GetNewSwitch2(int i)
    {
        if (GameData.TEST_MODE) return false; // Unity6 Migration: no hardware in TEST_MODE
        if (GetSwitch2(i) == 1) return true;
        else return false;
    }

    public static bool GetNewButton(int i)
    {
        if (GameData.TEST_MODE) return false; // Unity6 Migration: no hardware in TEST_MODE
        switch (i)
        {
            case 0:
                if (!button1 && GetButton(0) == 1)
                {
                    button1 = true;
                    return true;
                }
                else if (GetButton(0) == 0) button1 = false;
                return false;
            case 1:
                if (!button2 && GetButton(1) == 1)
                {
                    button2 = true;
                    return true;
                }
                else if (GetButton(1) == 0) button2 = false;
                return false;
            case 2:
                if (!button3 && GetButton(2) == 1)
                {
                    button3 = true;
                    return true;
                }
                else if (GetButton(2) == 0) button3 = false;
                return false;
        }
        return false;
    }

    public static bool GetDrift()
    {
        if (GameData.TEST_MODE) return false; // Unity6 Migration: no hardware in TEST_MODE
        int v = GetBreak(0) + GetBreak(1);

        if (v > 0) return true;
        return false;
    }
}

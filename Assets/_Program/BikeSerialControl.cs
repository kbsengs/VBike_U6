using UnityEngine;
using System.Collections;

public class BikeSerialControl : MonoBehaviour {

	// Use this for initialization
	IEnumerator Start () {
       
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        CBikeSerial.Init();

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        CBikeSerial.InitIntro();
        CBikeSerial.InitGame();
    }
	
	// Update is called once per frame
	void Update () {
        CBikeSerial.FrameLock(Time.fixedDeltaTime);
    }

    void FixedUpdate()
    {
        Serial();
    }

    public float heightAngle;
    public float speed;
    public float steer;
    public float pedalSpeed;
    public float handle;
    
    private int breakold = 0;
    private bool handleAct = false;
    

    void Serial()
    {
        int b1 = 0, b2 = 0;

        CBikeSerial.FrameBike(Time.fixedDeltaTime, heightAngle, 35, 0, speed);
        b1 = CBikeSerial.b1;
        b2 = CBikeSerial.b2;

        if (breakold != (b1 + b2) && (b1 + b2) > 4)
        {
            breakold = (b1 + b2);
        }
        steer = CBikeSerial.m_fSteer;

        pedalSpeed = 130 * Input.GetAxis("Vertical");
        if (pedalSpeed == 0)
            pedalSpeed = CBikeSerial.GetSpeed();

        //if ((b1 != 0 && b2 == 0) || (b1 == 0 && b2 != 0) || Input.GetKey(KeyCode.Space))
        //{
        //    drift = true;
        //}
        //else
        //{
        //    drift = false;
        //}

        float Khandle = Input.GetAxis("Horizontal");

        if (Khandle == 0)
        {
            if (handleAct)
            {
                handle = -CBikeSerial.GetHandle2() * 0.04f;
            }
            else
            {
                if (handle != CBikeSerial.GetHandle2()) handleAct = true;
            }
        }
        else
            handle = Khandle;
    }

    void OnGUI()
    {
        GUILayout.Label("<color=black><size=30>Speed = " + speed + "</size></color>");
        GUILayout.Label("<color=black><size=30>breakold = " + breakold + "</size></color>");
        GUILayout.Label("<color=black><size=30>steer = " + steer + "</size></color>");
        GUILayout.Label("<color=black><size=30>pedalSpeed = " + pedalSpeed + "</size></color>");
        GUILayout.Label("<color=black><size=30>handle = " + handle + "</size></color>");
        GUILayout.Label("<color=black><size=30>Btn0 = " + CBikeSerial.GetButton(0) + "</size></color>");
        GUILayout.Label("<color=black><size=30>Btn1 = " + CBikeSerial.GetButton(1) + "</size></color>");
        GUILayout.Label("<color=black><size=30>Btn2 = " + CBikeSerial.GetButton(2) + "</size></color>");
        GUILayout.Label("<color=black><size=30>B1 = " + CBikeSerial.GetBreak(0) + "</size></color>");
        GUILayout.Label("<color=black><size=30>B2 = " + CBikeSerial.GetBreak(1) + "</size></color>");
        GUILayout.Label("<color=black><size=30>Brake = " + CBikeSerial.m_fBrakeTorque + "</size></color>");
    }
}

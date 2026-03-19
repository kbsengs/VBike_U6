using UnityEngine;
using System.Collections;

public class Cycle_AI : MoveModule {

    private Cycle_Control _control;
    public bool crossAI;
    public bool select;
    public bool start;

    float starttime, ftime;
    float speed;

	// Use this for initialization
	void Start () {
        _control = GetComponent<Cycle_Control>();
        Init(_control.moveValue.centerOfMass, _control.moveValue.frontTire, _control.moveValue.rearTire);
        FrictionValue_F(forwardFriction, _control.moveValue.forwardFriction.extremumSlip, _control.moveValue.forwardFriction.extremumValue
            , _control.moveValue.forwardFriction.asymptoteSlip, _control.moveValue.forwardFriction.asymptoteValue, _control.moveValue.forwardFriction.stiffness);
        FrictionValue_S(sideFriction, _control.moveValue.sideFriction.extremumSlip, _control.moveValue.sideFriction.extremumValue
           , _control.moveValue.sideFriction.asymptoteSlip, _control.moveValue.sideFriction.asymptoteValue, _control.moveValue.sideFriction.stiffness);
        SpringSet(suspension, _control.moveValue.suspension.spring, _control.moveValue.suspension.damper, _control.moveValue.suspension.position);
        FrictionSetting();
        rayStart = _control.raypoint;
	}

    float time;
    float totaltime;
    //float startTime;
	// Update is called once per frame

	void FixedUpdate () {
        SyncData_Receive();
        if (_control.cycle_AI)
        {
            ftime += Time.fixedDeltaTime;
            if (ftime < starttime) return;

            totaltime += Time.fixedDeltaTime;

            float settime = 0;
            if (GameData.BMXServer)
            {
                if (GameData.BMXMap == 1)
                {
                    settime = 60.0f;
                }
                else if (GameData.BMXMap == 2)
                {
                    settime = 85.0f;
                }
                else if (GameData.BMXMap == 3)
                {
                    settime = 80.0f;
                }
            }

            if (time == 0)
            {  
                if (GameData.BMXServer)
                {
                    if (GameData.BMXMap == 1)
                    {
                        if (totaltime < 10)
                            PedalSpeedControl_2();
                        else if (totaltime < 40)
                            PedalSpeedControl_1(3.0f, 2.0f);
                        //else if (totaltime < 45)
                        //    PedalSpeedControl_2();
                        else if (totaltime < 63)
                            PedalSpeedControl_2();
                        else
                            PedalSpeedControl_3();
                    }
                    if (GameData.BMXMap == 2)
                    {
                        if (totaltime < 10)
                            PedalSpeedControl_2();
                        else if (totaltime < 65)
                            PedalSpeedControl_1(3.0f, 2.0f);
                        //else if (totaltime < 70)
                        //    PedalSpeedControl_2();
                        else if (totaltime < 85)
                            PedalSpeedControl_2();
                        else
                            PedalSpeedControl_3();
                    }
                    if (GameData.BMXMap == 3)
                    {
                        if (totaltime < 10)
                            PedalSpeedControl_2();
                        else if (totaltime < 55)
                            PedalSpeedControl_1(3.0f, 2.0f);
                        //else if (totaltime < 60)
                        //    PedalSpeedControl_2();
                        else if (totaltime < 78)
                            PedalSpeedControl_2();
                        else
                            PedalSpeedControl_3();
                    }
                }
                else
                {
                    PedalSpeedControl_1(3.0f, 2.0f);
                }
            }
            time += Time.fixedDeltaTime;
            float gap = 3.0f;
            if (GameData.BMXServer)
            {
                if (totaltime > settime - 5) gap = 1.0f;
                if (totaltime > settime) gap = 0.05f;
            }
            if (time > gap) time = 0;
            PhysicsValue();
            if (_control.gameFinish) pedalSpeed = 0;
            Move(false);
            AIControl();
            SyncData_Send_Self();
            SyncData_Send();
        }
        else
        {
            handle = 0;
            steer = 0;
            pedalSpeed = 0;
            realSpeed = 0;

            time = 0;
            ftime = 1;
            totaltime = 0;
            starttime = Random.value * 0.3f;
            speed = Random.Range(30, 70);
        }

        //pedalSpeed *= 100 - (GameData.DIF * 5);
		pedalSpeed *= (1000 - (9-GameData.DIF)) * 0.001f;
	}

    void PedalSpeedControl_1(float gap1, float gap2)
    {
        if (_control.rank < 3)
        {
            if (_control.rank < 2)
            {
                float percent = Random.Range(0.0f, 1.0f);
                if (percent < 0.7f)
                {
                    pedalSpeed = Random.Range(pedalSpeed * 0.7f, pedalSpeed);
                    if (pedalSpeed < 55) pedalSpeed = 58.0f;
                    //Debug.Log("Decrease " + pedalSpeed);
                }
                if (pedalSpeed == 0) pedalSpeed = 58.0f;
            }
            else
                pedalSpeed = Random.Range(55.0f, 60.0f + (float)(_control.rank * gap1)) + _control.rank;
        }
        else
        {
            float r = Random.value;
            if (r < 0.1f) pedalSpeed = Random.Range(65.0f, 90.0f);
            else pedalSpeed = Random.Range(60.0f, 65.0f + (float)(_control.rank * gap2)) + _control.rank;
        }
    }

    void PedalSpeedControl_2()
    {
        for (int i = 0; i < GameData.BMX_FutureRank.Length; i++)
        {
            if (GameData.BMX_FutureRank[i] == _control.MyNumber + 1)
            {
                if (i == 0)
                {
                    _control.impotal = true;
                    if (_control.rank == 1)
                    {
                        pedalSpeed = Random.Range(63.0f, 66.0f);
                    }
                    else if (_control.rank > 1)
                    {
                        if (_control.rank < 3)
                            pedalSpeed = 72.0f;
                        else if (_control.rank < 6)
                            pedalSpeed = 75.0f;
                        else if (_control.rank < 8)
                            pedalSpeed = 80.0f;
                        else
                            pedalSpeed = 90.0f;
                    }
                }
                else if (i == 1)
                {
                    _control.impotal = true;
                    if (_control.rank == 2)
                    {
                        pedalSpeed = Random.Range(63.0f, 66.0f);
                    }
                    else if (_control.rank < 2) pedalSpeed = Random.Range(55.0f, 60.0f);
                    else if (_control.rank > 2)
                    {
                        if (_control.rank < 4)
                            pedalSpeed = 72.0f;
                        else if (_control.rank < 6)
                            pedalSpeed = 75.0f;
                        else if (_control.rank < 8)
                            pedalSpeed = 80.0f;
                        else
                            pedalSpeed = 90.0f;
                    }
                }
                else if (i == 2)
                {
                    _control.impotal = true;
                    if (_control.rank == 3)
                    {
                        pedalSpeed = Random.Range(63.0f, 66.0f);
                    }
                    else if (_control.rank < 3) pedalSpeed = Random.Range(55.0f, 60.0f);
                    else if (_control.rank > 3)
                    {
                        if (_control.rank < 5)
                            pedalSpeed = 72.0f;
                        else if (_control.rank < 6)
                            pedalSpeed = 75.0f;
                        else if (_control.rank < 8)
                            pedalSpeed = 80.0f;
                        else
                            pedalSpeed = 90.0f;
                    }
                }
                else
                {
                    _control.impotal = false;
                    PedalSpeedControl_1(1.0f, 2.0f);
                }
            }
        }
    }

    void PedalSpeedControl_3()
    {
        for (int i = 0; i < GameData.BMX_FutureRank.Length; i++)
        {
            if (GameData.BMX_FutureRank[i] == _control.MyNumber + 1)
            {
                if (i == 0)
                {
                    _control.impotal = true;
                    if (_control.rank == 1)
                    {
                        pedalSpeed = Random.Range(60.0f, 63.0f);
                    }
                    else if (_control.rank > 1)
                    {
                        if (_control.rank < 3)
                            pedalSpeed = 70.0f;
                        else if (_control.rank < 6)
                            pedalSpeed = 75.0f;
                        else if (_control.rank < 8)
                            pedalSpeed = 80.0f;
                        else
                            pedalSpeed = 90.0f;
                    }
                }
                else if (i == 1)
                {
                    _control.impotal = true;
                    if (_control.rank == 2)
                    {
                        pedalSpeed = Random.Range(60.0f, 63.0f);
                    }
                    else if (_control.rank < 2) pedalSpeed = 40.0f;
                    else if (_control.rank > 2)
                    {
                        if (_control.rank < 4)
                            pedalSpeed = 70.0f;
                        else if (_control.rank < 6)
                            pedalSpeed = 75.0f;
                        else if (_control.rank < 8)
                            pedalSpeed = 80.0f;
                        else
                            pedalSpeed = 90.0f;
                    }
                }
                else if (i == 2)
                {
                    _control.impotal = true;
                    if (_control.rank == 3)
                    {
                        pedalSpeed = Random.Range(60.0f, 63.0f);
                    }
                    else if (_control.rank < 3) pedalSpeed = 40.0f;
                    else if (_control.rank > 3)
                    {
                        if (_control.rank < 5)
                            pedalSpeed = 70.0f;
                        else if (_control.rank < 6)
                            pedalSpeed = 75.0f;
                        else if (_control.rank < 8)
                            pedalSpeed = 80.0f;
                        else
                            pedalSpeed = 90.0f;
                    }
                }
                else
                {
                    _control.impotal = false;
                    if (i == 7 ||
                        i == 8 ||
                        i == 9)
                    {
                        if (_control.rank < 7)
                            pedalSpeed = 35.0f;
                        else
                            pedalSpeed = Random.Range(50.0f, 60.0f);
                    }
                    else
                    {
                        if (_control.rank < 4) pedalSpeed = 35.0f;
                        else pedalSpeed = Random.Range(50.0f, 60.0f);
                    }
                }
            }
        }
    }

    #region AI

    public Transform rayStart;
    private float distance_long = 1.0f;
    private float distance_short = 1.2f;
    public float evasionAngle;
    private float angle1 = 60.0f;
    private float angle2 = 60.0f;
    private Vector3 evasionDir;

    private Vector3 wantpos;

    private bool leftObstacle = false;
    private bool rightObstacle = false;

    Vector3 dir_L1 = new Vector3(-1.0f, 0, 1.0f);
    Vector3 dir_L2 = new Vector3(-0.15f, 0, 1);

    Vector3 dir_R1 = new Vector3(1.0f, 0, 1.0f);
    Vector3 dir_R2 = new Vector3(0.15f, 0, 1);

    void CheckObstacle()
    {
        RaycastHit left;
        RaycastHit right;

        leftObstacle = Physics.Raycast(rayStart.position, rayStart.TransformDirection(dir_L2), out left, distance_long);
        rightObstacle = Physics.Raycast(rayStart.position, rayStart.TransformDirection(dir_R2), out right, distance_long);

        if (leftObstacle || rightObstacle) evasionAngle = angle2;

        if (!leftObstacle)
        {
            leftObstacle = Physics.Raycast(rayStart.position, rayStart.TransformDirection(dir_L1), out left, distance_short);
            if (leftObstacle) 
                evasionAngle = angle1;
        }
        if (!rightObstacle)
        {
            rightObstacle = Physics.Raycast(rayStart.position, rayStart.TransformDirection(dir_R1), out right, distance_short);
            if (rightObstacle) evasionAngle = angle1;
        }

        if (leftObstacle && rightObstacle)
        {
            if (left.distance < right.distance)
            {
                rightObstacle = false;
            }
            else
            {
                leftObstacle = false;
            }
            //pedalSpeed += 0.1f;
        }

        if (Mathf.Abs(_control.moveValue.lrAngle) > 15.0f)
        {
            leftObstacle = false;
            rightObstacle = false;
        }

        Debug.DrawRay(rayStart.position, rayStart.TransformDirection(dir_L2), Color.blue);
        Debug.DrawRay(rayStart.position, rayStart.TransformDirection(dir_R2), Color.red);

        if (leftObstacle)
        {
            float radian = -(evasionAngle) * Mathf.Deg2Rad;
            evasionDir = left.point - rayStart.position;
            evasionDir = new Vector3(evasionDir.x, 0, evasionDir.z);
            evasionDir = new Vector3(evasionDir.x * Mathf.Cos(radian) + evasionDir.z * (-Mathf.Sin(radian)), 0, evasionDir.z * Mathf.Cos(radian) + evasionDir.x * Mathf.Sin(radian));

            Debug.DrawLine(rayStart.position, left.point, Color.green);
            Debug.DrawRay(rayStart.position, evasionDir, Color.red);
        }
        else if (rightObstacle)
        {
            float radian = (evasionAngle) * Mathf.Deg2Rad;
            evasionDir = right.point - rayStart.position;
            evasionDir = new Vector3(evasionDir.x, 0, evasionDir.z);
            evasionDir = new Vector3(evasionDir.x * Mathf.Cos(radian) + evasionDir.z * (-Mathf.Sin(radian)), 0, evasionDir.z * Mathf.Cos(radian) + evasionDir.x * Mathf.Sin(radian));

            Debug.DrawLine(rayStart.position, right.point, Color.green);
            Debug.DrawRay(rayStart.position, evasionDir, Color.red);
        }
    }

    float wantSteer;

    void AIControl()
    {
        float gap = 0.0f;
        CheckObstacle();
        if (leftObstacle || rightObstacle)
        {
            wantpos = rayStart.position + evasionDir * 5.0f;
        }
        else
        {
            if (start)
            {
                if (_control.CP.cross)
                {
                    if (!select)
                    {
                        select = true;
                        int i = (int)Random.Range(0, 2);
                        if (i == 0) crossAI = false;
                        else crossAI = true;
                    }
                    if (crossAI)
                        wantpos = _control._waypoint.allways[_control.CP.crossStep].position + new Vector3(Random.Range(-gap, gap), 0, Random.Range(-gap, gap));
                    else
                        wantpos = _control._waypoint.allways[_control.CP.nextStep].position + new Vector3(Random.Range(-gap, gap), 0, Random.Range(-gap, gap));
                }
                else
                {
                    select = false;
                    crossAI = false;
                    wantpos = _control._waypoint.allways[_control.CP.nextStep].position + new Vector3(Random.Range(-gap, gap), 0, Random.Range(-gap, gap));
                }
            }
            else
            {
                wantpos = _control._waypoint.allways[_control._waypoint.startpoint].position + new Vector3(Random.Range(-gap, gap), 0, Random.Range(-gap, gap));
                if (_control.CP.currentStep >= _control._waypoint.startpoint) start = true;
            }
        }

        Vector3 RelativePosition = transform.InverseTransformPoint(new Vector3(wantpos.x, transform.position.y, wantpos.z));//방향 벡터 찾기
        float inputSteer = 100.0f * RelativePosition.x / RelativePosition.magnitude; //내가 꺾을 각도
        float damping = 3.0f;
        if (Mathf.Abs(inputSteer) > 45.0f)
        {
            //if (!leftObstacle && !rightObstacle)
            //{
            //    //Vector3 dir = wantpos - MyTransform.position;
            //    //float radian = (inputSteer / Mathf.Abs(inputSteer)) * 30.0f;
            //    //dir = new Vector3(dir.x * Mathf.Cos(radian) + dir.z * (-Mathf.Sin(radian)), 0, dir.z * Mathf.Cos(radian) + dir.x * Mathf.Sin(radian));
            //    //wantpos = MyTransform.position + dir * 5.0f;
            //    damping = 4.0f;
            //}
            //else
            //    damping = 4.0f;
            drift = true;
        }
        else drift = false;
        if (Mathf.Abs(inputSteer) > 30) inputSteer = inputSteer / Mathf.Abs(inputSteer) * 30;
        float velocity = 0.0f;
        wantSteer = Mathf.SmoothDampAngle(wantSteer, inputSteer, ref velocity, 0.1f);
        steer = wantSteer;

        if (pedalSpeed > 5)// && (groundR || groundF)) 
        {
            Quaternion rotation = Quaternion.LookRotation(new Vector3(wantpos.x, 0, wantpos.z) - new Vector3(transform.position.x, 0, transform.position.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, damping * Time.fixedDeltaTime);
        }


        ////Quaternion airotation = MyTransform.rotation;

        //MyTransform.rotation = Quaternion.Euler(MyTransform.eulerAngles.x, y, MyTransform.eulerAngles.z);

        Debug.DrawLine(transform.position, new Vector3(wantpos.x, transform.position.y, wantpos.z), Color.red);
    }
    #endregion

    void SyncData_Send()
    {
        _control.moveValue.realSpeed = realSpeed;
        _control.moveValue.heightAngle = heightAngle;
        _control.moveValue.lrAngle = lrAngle;

        _control.moveValue.groundF = groundF;
        _control.moveValue.groundR = groundR;
    }

    void SyncData_Send_Self()
    {
        _control.moveValue.pedalSpeed = pedalSpeed;
        
        _control.moveValue.steer = steer;
        _control.moveValue.handle = handle;
        _control.moveValue.drift = drift;
    }

    void SyncData_Receive()
    {
        resistance = _control.moveValue.resistance;
    }
}

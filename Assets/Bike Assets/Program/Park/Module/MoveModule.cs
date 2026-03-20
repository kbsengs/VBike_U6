using UnityEngine;
using System.Collections;

public class MoveModule : MonoBehaviour
{

    #region �����ӿ� �ʿ��� ������
    public class MaxValue
    {   
        public float MaxSteer = 35.0f;//�ڵ� ���̴� ����
        public float MaxSpeed = 100.0f;//�ִ� �ӵ�
    }
    public MaxValue maxValue = new MaxValue();

    [System.Serializable]
    public class TireSetting
    {
        public Transform front; //�� ����(�ִϸ��̼� ��)
        public Transform rear; //�� ����(�ִϸ��̼� ��)
    }
    public TireSetting tireSetting = new TireSetting();

    public class WheelSetting
    {
        public WheelCollider[] wheels;
        public WheelCollider front; //�չ���(Ground ���� �˱� ����)
        public WheelCollider rear;//�޹���(Ground ���� �˱� ����)
    }
    public WheelSetting wheelSetting = new WheelSetting();

    [System.Serializable]
    public class ForwardFriction
    {
        public float extremumSlip = 0.25f;
        public float extremumValue = 800;
        public float asymptoteSlip = 1.4f;
        public float asymptoteValue = 280;
        public float stiffness = 1.5f;
    }
    public ForwardFriction forwardFriction = new ForwardFriction();

    [System.Serializable]
    public class SideFriction
    {
        public float extremumSlip = 1;
        public float extremumValue = 500;
        public float asymptoteSlip = 6;
        public float asymptoteValue = 280;
        public float stiffness = 0.9f;
    }
    public SideFriction sideFriction = new SideFriction();

    [System.NonSerialized]
    public float steer; //ȸ�� ��(Degree)
    [System.NonSerialized]
    public float handle; //�Ϲ��� ȸ�� ��(���� �ִ� ���̹Ƿ� ��Ȯ�� Degree ������ �ƴϴ�)
    [System.NonSerialized]
    public float pedalSpeed; //���� �ִ� �� �ӵ�
    [System.NonSerialized]
    public float realSpeed; //���� ǥ�����ִ� �ӵ���
    [System.NonSerialized]
    private float brakeTorque = 10;
    [System.NonSerialized]
    public float resistance = 0.0f;
   
    // Unity6 Migration: cache Rigidbody (component shorthand 'rigidbody' removed)
    protected Rigidbody _rb;

    public Transform centerOfMass;

    private WheelFrictionCurve friction_F;
    private WheelFrictionCurve friction_S;
    private float extremumValue_S = 2000;
    private float asymptoteValue_S = 1000;
    private float extremumValue_Drift = 500;
    private float asymptoteValue_Drift = 100;

    [System.Serializable]
    public class Suspension
    {
        public float spring = 1000;
        public float damper = 25;
        public float position = 0;
    }
    public Suspension suspension = new Suspension();
    public JointSpring spring;

    [System.NonSerialized]
    public bool groundF;
    [System.NonSerialized]
    public bool groundR;
    [System.NonSerialized]
    public bool drift;
    [System.NonSerialized]
    public float heightAngle;
    [System.NonSerialized]
    public float lrAngle;

    [System.NonSerialized]
    private float[] gears = new float[] { 10.0f, 1.2f, 1.0f, 0.9f, 0.7f };
    private int gear;

    #endregion

    public void Init(Transform center, Transform frontTire, Transform rearTire)
    {
        _rb = GetComponent<Rigidbody>(); // Unity6 Migration
        centerOfMass = center;
        tireSetting.front = frontTire;
        tireSetting.rear = rearTire;
        if (centerOfMass)
        {
            _rb.centerOfMass = centerOfMass.localPosition;
        }
        wheelSetting.wheels = transform.GetComponentsInChildren<WheelCollider>();
        foreach (WheelCollider pos in wheelSetting.wheels)
        {
            // Unity6 Migration: "Wheel" tag must exist in Project Settings > Tags
            try { pos.tag = "Wheel"; } catch { Debug.LogWarning("Tag 'Wheel' not defined. Add it in Edit > Project Settings > Tags and Layers."); }
            string n = pos.name;
            if (n == "Front" || n == "Front_L" || n == "Front_R")
                wheelSetting.front = pos;
            else if (n == "Rear" || n == "Rear_L" || n == "Rear_R")
                wheelSetting.rear = pos;
        }

        // 이름 매칭 실패 시 위치 기반으로 앞/뒷바퀴 감지 (바퀴 이름이 달라도 동작)
        if (wheelSetting.front == null || wheelSetting.rear == null)
        {
            float maxZ = float.MinValue, minZ = float.MaxValue;
            foreach (var wc in wheelSetting.wheels)
            {
                float lz = transform.InverseTransformPoint(wc.transform.position).z;
                if (lz > maxZ) { maxZ = lz; wheelSetting.front = wc; }
                if (lz < minZ) { minZ = lz; wheelSetting.rear = wc; }
            }
        }

        // 바퀴 이름과 front/rear 감지 결과 출력 (디버깅용)
        string wheelNames = wheelSetting.wheels != null
            ? string.Join(", ", System.Array.ConvertAll(wheelSetting.wheels, w => w.name))
            : "none";
        Debug.Log("[MoveModule] WheelColliders: " + wheelNames
            + " | front=" + (wheelSetting.front != null ? wheelSetting.front.name : "null")
            + " | rear="  + (wheelSetting.rear  != null ? wheelSetting.rear.name  : "null"));
    }

    public void FrictionValue_F(ForwardFriction f, float eSlip, float eValue, float aSlip, float aValue, float stiff)//���� ����
    {
        f.extremumSlip = eSlip;
        f.extremumValue = eValue;
        f.asymptoteSlip = aSlip;
        f.asymptoteValue = aValue;
        f.stiffness = stiff;
    }

    public void FrictionValue_S(SideFriction f, float eSlip, float eValue, float aSlip, float aValue, float stiff)//���� ����
    {
        f.extremumSlip = eSlip;
        f.extremumValue = eValue;
        f.asymptoteSlip = aSlip;
        f.asymptoteValue = aValue;
        f.stiffness = stiff;
    }

    public void SpringSet(Suspension s, float spr, float dam, float pos)
    {
        s.spring = spr;
        s.damper = dam;
        s.position = pos;
    }

    public void FrictionSetting()//������ ����
    {
        friction_F.extremumSlip = forwardFriction.extremumSlip;
        friction_F.extremumValue = forwardFriction.extremumValue;
        friction_F.asymptoteSlip = forwardFriction.asymptoteSlip;
        friction_F.asymptoteValue = forwardFriction.asymptoteValue;
        friction_F.stiffness = forwardFriction.stiffness;

        friction_S.extremumSlip = sideFriction.extremumSlip;
        friction_S.extremumValue = sideFriction.extremumValue;
        friction_S.asymptoteSlip = sideFriction.asymptoteSlip;
        friction_S.asymptoteValue = sideFriction.asymptoteValue;
        friction_S.stiffness = sideFriction.stiffness;

        spring.spring = suspension.spring;
        spring.damper= suspension.damper;
        spring.targetPosition = suspension.position;

        foreach (WheelCollider pos in wheelSetting.wheels)
        {
            pos.forwardFriction = friction_F;
            pos.sidewaysFriction = friction_S;
            pos.suspensionSpring = spring;
            pos.brakeTorque = brakeTorque;
            pos.motorTorque = 0; // Unity6 Migration: 프리팹에 baked된 motorTorque 초기화 (스폰 즉시 자동 주행 방지)
        }
    }

    WheelHit hitWheels;

    public void PhysicsValue()
    {
        // 수평 속도만 사용 (y-velocity 제외: suspension 바운싱/낙하 속도가 속도계·댐핑에 반영되는 오류 방지)
        // Unity6 Migration: velocity→linearVelocity, drag→linearDamping
        Vector3 rawVel = _rb.linearVelocity;
        Vector3 hVel = Vector3.ProjectOnPlane(rawVel, Vector3.up);
        _rb.linearDamping = hVel.magnitude / 250.0f;
        _rb.mass = 100;

        realSpeed = hVel.magnitude * 4.0f;

        // 진단 로그: 속도 비정상시 출력 (컴파일 확인용 — 정상 동작 확인 후 제거)
        if (rawVel.magnitude > 10f)
            Debug.Log(string.Format("[MoveModule] 비정상속도 rawVel={0} hVel={1:F1} realSpeed={2:F1}",
                rawVel.ToString("F1"), hVel.magnitude, realSpeed));

        if (realSpeed < maxValue.MaxSpeed / 5) gear = 0;
        else if (realSpeed < maxValue.MaxSpeed * 2 / 5) gear = 1;
        else if (realSpeed < maxValue.MaxSpeed * 3 / 5) gear = 2;
        else if (realSpeed < maxValue.MaxSpeed * 4 / 5) gear = 3;
        else gear = 4;

        //if (wheelSetting.front.GetGroundHit(out hitWheels)) groundF = true;
        //else groundF = false;

        //if (wheelSetting.rear.GetGroundHit(out hitWheels)) groundR = true;
        //else groundR = false;

        // 전체 WheelCollider를 순회하여 앞/뒷바퀴 접지 여부 각각 판정
        // 바이크에 Front, Front_L, Front_R (3개) + Rear, Rear_L, Rear_R (3개) 구성인 경우
        // 단일 바퀴(Front_R만)로 판정하면 한 쪽이 뜰 때 groundF=false → Phase 1 타임아웃 반복
        groundF = false;
        groundR = false;
        if (wheelSetting.wheels != null)
        {
            foreach (WheelCollider pos in wheelSetting.wheels)
            {
                if (!pos.GetGroundHit(out hitWheels)) continue;
                bool isFront = (pos == wheelSetting.front)
                            || pos.name == "Front" || pos.name == "Front_L" || pos.name == "Front_R";
                if (isFront) groundF = true;
                else         groundR = true;
            }
        }


        if (centerOfMass)
        {
            if (!groundR && !groundF) _rb.centerOfMass = new Vector3(_rb.centerOfMass.x, 0, _rb.centerOfMass.z);
            else _rb.centerOfMass = centerOfMass.localPosition;
        }

        heightAngle = SlopeAngleCheck(transform.eulerAngles.x);
        lrAngle = SlopeAngleCheck(transform.eulerAngles.z);
      
         
        if (!groundF && !groundR)
            _rb.AddRelativeTorque(new Vector3(1, 0, 0) * 0.0525f, ForceMode.VelocityChange);

        tireSetting.front.Rotate(new Vector3(1.0f, 0, 0) * realSpeed * Time.fixedDeltaTime * 50.0f);
        tireSetting.rear.Rotate(new Vector3(1.0f, 0, 0) * realSpeed * Time.fixedDeltaTime * 50.0f);
    }

    public void Move(bool steerAngle)
    {
        //if (steerAngle)
        //{
            // Unity6 Migration: angularDrag → angularDamping
        if (!groundR && !groundF)
                _rb.angularDamping = 5.0f;
            else if (Mathf.Abs(steer) < 5.0f)
                _rb.angularDamping = 5.0f;
            else
                _rb.angularDamping = 20.0f;
        //}
        //else
        //{
        //    rigidbody.angularDrag = 5.0f;
        //}
        if (pedalSpeed < 1000.0f && realSpeed < maxValue.MaxSpeed)
        {
            if (groundR)
            {
                if (!steerAngle)
                {
                    // WheelCollider motorTorque 미사용 경로: 명시적으로 0 초기화
                    foreach (WheelCollider pos in wheelSetting.wheels)
                        pos.motorTorque = 0f;

                    if (pedalSpeed != 0)
                    {
                        Vector3 targetVelocity = transform.TransformDirection(Vector3.forward);
                        targetVelocity *= pedalSpeed;

                        // Apply a force that attempts to reach our target velocity
                        Vector3 velocity = _rb.linearVelocity;
                        Vector3 velocityChange = (targetVelocity - velocity);
                        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxValue.MaxSpeed, maxValue.MaxSpeed);
                        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxValue.MaxSpeed, maxValue.MaxSpeed);
                        velocityChange.y = 0;
                        _rb.AddForce(velocityChange * gears[gear] * 4.0f, ForceMode.Force);
                    }
                }
                else
                {
                    foreach (WheelCollider pos in wheelSetting.wheels)
                    {
                        // wheelSetting.front 참조 우선, 없으면 이름으로 판별
                        bool isFront = (pos == wheelSetting.front)
                                    || pos.name == "Front" || pos.name == "Front_L" || pos.name == "Front_R";
                        pos.motorTorque = isFront ? 0f : gears[gear] * pedalSpeed;
                    }
                }
            }
            else
            {
                // 공중 또는 뒷바퀴 미접지: motorTorque 초기화
                foreach (WheelCollider pos in wheelSetting.wheels)
                    pos.motorTorque = 0f;
            }
        }
        else
        {
            // 속도 한계 초과 또는 페달 과입력: motorTorque 초기화
            foreach (WheelCollider pos in wheelSetting.wheels)
                pos.motorTorque = 0f;
        }

        if (groundR || groundF)
        {
            float rotate = 2.0f;
            float dir = 1.0f;
            if (realSpeed < 0) dir = -1.0f;
            if (drift)
            {
                rotate = 3.0f;
                friction_S.extremumValue = extremumValue_Drift;
                friction_S.asymptoteValue = asymptoteValue_Drift;
                if (steerAngle) _rb.AddTorque(Vector3.up * handle * rotate * dir * 500, ForceMode.Force);
            }
            else
            {
                friction_S.extremumValue = Mathf.MoveTowards(friction_S.extremumValue, sideFriction.extremumValue, 200 * Time.fixedDeltaTime);
                friction_S.asymptoteValue = Mathf.MoveTowards(friction_S.asymptoteValue, sideFriction.asymptoteValue, 200 * Time.fixedDeltaTime);
                if (steerAngle) _rb.AddTorque(Vector3.up * handle * dir * rotate * 200, ForceMode.Force);
            }
            //if (steerAngle) MyTransform.Rotate(Vector3.up, Time.fixedDeltaTime * handle * rotate * 50.0f);
            
        }
        foreach (WheelCollider pos in wheelSetting.wheels)
        {
            pos.sidewaysFriction = friction_S;
        }

        foreach (WheelCollider pos in wheelSetting.wheels)
        {
            bool isFront = (pos == wheelSetting.front)
                        || pos.name == "Front" || pos.name == "Front_L" || pos.name == "Front_R";
            if (isFront)
                pos.steerAngle = steerAngle ? steer : 0f;
        }

        float brake = brakeTorque + resistance;// +heightAngle;
        if (brake < 1) brake = 1;

        foreach (WheelCollider pos in wheelSetting.wheels)
        {
            if (pedalSpeed > 0)
                pos.brakeTorque = brake;
            else
                pos.brakeTorque = brake * 2f;
        }
        //if (steerAngle) wheelSetting.front.steerAngle = Mathf.Clamp(steer, -maxValue.MaxSteer, maxValue.MaxSteer);
        //else wheelSetting.front.steerAngle = 0.0f;
    }

    public void KeyBoard()
    {
        // ↑/↓ 키: pedalSpeed를 점진적으로 증감 (즉시 최고속도 방지)
        // pedalSpeed 130 ≒ realSpeed 100 km/h 이므로 1 km/h ≒ 1.3 pedalSpeed
        // 초당 약 13 km/h씩 증감 (130을 10초에 걸쳐 도달)
        float accel = 13.0f * Time.fixedDeltaTime; // ≒ 1 km/h per frame at 10fps, 부드러운 가속

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            pedalSpeed = Mathf.Min(pedalSpeed + accel, 130f);
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            pedalSpeed = Mathf.Max(pedalSpeed - accel, -130f);
        }
        else
        {
            // 키 없을 때 자연 감속 (가속의 2배 속도)
            pedalSpeed = Mathf.MoveTowards(pedalSpeed, 0f, accel * 2f);
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            handle = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            handle = -1f;
        else
            handle = 0f;

        steer = maxValue.MaxSteer * handle;

        // 속도 0일 때 강한 브레이크 (경사 지형 정지 유지)
        if (Mathf.Approximately(pedalSpeed, 0f))
        {
            pedalSpeed = 0f;
            if (wheelSetting.wheels != null)
                foreach (WheelCollider pos in wheelSetting.wheels)
                    pos.brakeTorque = 500f;
            return;
        }

        if ((groundR || groundF) && Input.GetKeyDown(KeyCode.Q))
        {
            _rb.AddForce(transform.up * 250, ForceMode.Acceleration);
        }

        if (Input.GetKey(KeyCode.Space) || CBikeSerial.GetDrift())
            drift = true;
        else
            drift = false;
    }

    //���� üũ
    protected float SlopeAngleCheck(float slopeAngle)
    {
        float angle = 0;
        if (slopeAngle > 90)
        {
            return angle = 360 - slopeAngle;
        }
        else
        {
            return angle = -slopeAngle;
        }
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody>(); // Unity6 Migration: ensure cached before Init
        Init(centerOfMass, tireSetting.front, tireSetting.rear);
        FrictionValue_F(forwardFriction, forwardFriction.extremumSlip, forwardFriction.extremumValue
            , forwardFriction.asymptoteSlip, forwardFriction.asymptoteValue, forwardFriction.stiffness);
        FrictionValue_S(sideFriction, sideFriction.extremumSlip, sideFriction.extremumValue
           , sideFriction.asymptoteSlip, sideFriction.asymptoteValue, sideFriction.stiffness);
        FrictionSetting();
    }
}

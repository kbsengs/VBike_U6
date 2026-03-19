using UnityEngine;
using System.Collections;

public class Cycle_Control : MonoBehaviour {

    public bool User = false;
    public bool cycle_Move = false;
    public bool cycle_AI = false;
    public bool cycle_Impact = false;
    public int rank; //���� ����
    public string wayName = "_Waypoint";//���� ã�ƾ� �� ���� ����Ʈ
    public bool gameFinish = false; //���� ������ �ƴѰ�?
    public Transform cameraTarget; //���� ī�޶� Ÿ��
    private float step_check_distance = 10.0f; //���� ���� üũ�ϱ�~!

    public Transform[] camera; //�� ī�޶� Ÿ�� ��ġ ��

    [System.NonSerialized]
    public WaypointDefine _waypoint; //�� ��ǥ ��������Ʈ
    public Transform raypoint; //���̸� �� ��ġ

    [System.NonSerialized]
    public bool findWay = false; //���� ��������Ʈ ã����?
    //private Transform MyTransform;

    [System.Serializable]
    public class MoveValue
    {
        public Transform centerOfMass;
        public Transform frontTire;
        public Transform rearTire;

        public float steer; //ȸ�� ��(Degree)
        public float handle; //�Ϲ��� ȸ�� ��(���� �ִ� ���̹Ƿ� ��Ȯ�� Degree ������ �ƴϴ�)
        public float pedalSpeed; //���� �ִ� �� �ӵ�
        public float realSpeed; //���� ǥ�����ִ� �ӵ���
        public float resistance = 0.0f;

        public bool groundF;
        public bool groundR;
        public bool drift;

        public float heightAngle;
        public float lrAngle;

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

        [System.Serializable]
        public class Suspension
        {
            public float spring = 1000;
            public float damper = 25;
            public float position = 0;
        }
        public Suspension suspension = new Suspension();
    }
    public MoveValue moveValue = new MoveValue();
    [System.Serializable]
    public class CurrentPosition
    {
        public int currentStep; // ���� �ܰ�
        public int nextStep; // ���� �ܰ�
        public int crossroad; // ���° ��� �������ΰ�?
        public int crossStep;
        public int[] cross_SE; //���� ������ ������ ó�� ����
        public bool cross; //������?
    }
    public CurrentPosition CP = new CurrentPosition();

    public enum Environment
    {
        Normal = 0, Mud = 30, Water = 20, Gravel = 10
    }
    public Environment environment = Environment.Normal;

    public int deadState = 0; //���� ���� ���� �˷��ֱ�

    public Renderer[] cycleTexture; //������ �ý��� ��ȯ
    public int MyNumber; //���� �� ��ȣ
    public bool wrongWay; //�� �ݴ�� ���� �ִ°�?
    public bool gameStart; //��Ʈ��ũ �󿡼� ���� ���� �����ҰŶ�� �ǹ�, ȥ�� �Ҷ��� ���� ��� �ȴ�.
    public bool networkJoinGame;

    [System.Serializable]
    public class Replay
    {
        [System.NonSerialized]
        public Vector3[] pos = new Vector3[1000];
        [System.NonSerialized]
        public Quaternion[] rot = new Quaternion[1000];
        [System.NonSerialized]
        public float[] realSpeed = new float[1000];
        [System.NonSerialized]
        public float[] pedalSpeed = new float[1000];
        [System.NonSerialized]
        public float[] steer = new float[1000];
        [System.NonSerialized]
        public int[] deadState = new int[1000];
        public bool record;
        public bool play;
    }
    public Replay replay = new Replay();

    [System.Serializable]
    public class MinimapArrow
    {
        public Transform arrow;
        public Vector3 finalPos;
        public Vector3 finalRot;
        //public Color[] color = new Color[GameData.MAX_PLAYER];
    }
    public MinimapArrow minimapArrow = new MinimapArrow();
    //[System.NonSerialized]
    public bool isMine = true;
    [System.NonSerialized]
    public bool checkRespawn = true;
	
	
	public bool bRecordTime = false;
	public float fPlayTime = 0;
    public bool replayAct = false;
    public bool impotal = false;
    // Unity6 Migration: cache Rigidbody; expose as public property for external access
    private Rigidbody _rb;
    public Rigidbody Rb => _rb;

	void Start () {
        _rb = GetComponent<Rigidbody>();
        cameraTarget = camera[1];
        transform.tag = "Player";

        ChangeTexture();
        
        gameObject.AddComponent<Cycle_Move>();
        gameObject.AddComponent<Cycle_AI>();
        gameObject.AddComponent<Cycle_Impact>();
        gameObject.AddComponent<Cycle_Animation>();
        gameObject.AddComponent<Cycle_Smoke>();

        InitGame();
	}

    public void InitGame()
    {
        fPlayTime = 0;
        bRecordTime = false;
    }

    bool bLanding = false;
    public bool go = false;
    float jumpTime = 0.0f;

	// Update is called once per frame
	void Update () {

        if (GameData.GameState == 0 && !User)
        {
            minimapArrow.arrow.localScale = GameData.ArrowSize_BMX[GameData.BMXMap];
        }

		if( bRecordTime )
		{
			fPlayTime += Time.deltaTime;
		}
        if (isMine)
        {
            if (!findWay)
            {
                if (wayName != "") FindClosePoint();
            }
            else FindNextStep();
        }

        if (isMine)
        {
            if (gameFinish)
                replayAct = true;
            else
                replayAct = false;
            if (!cycle_AI && !cycle_Move)
            {
                if (!replayAct)
                {
                    moveValue.pedalSpeed = 0;
                    moveValue.realSpeed = 0;
                    moveValue.steer = 0;
                    moveValue.handle = 0;
                    moveValue.drift = false;
                }
                //if (!rigidbody.isKinematic)
                //    rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0);
            }
        }

        if (!cycle_AI && !cycle_Move)
        {
            _rb.isKinematic = true; // Unity6 Migration
        }

        // Unity6 Migration: Legacy Network removed; Phase 4 will re-implement via UDP
        // Network.peerType / networkView.RPC("SyncRank_RPC") removed
        ArrowPosition(rank);
        moveValue.resistance = (int)environment;

        if (!moveValue.groundF && !moveValue.groundR)
        {
            jumpTime += Time.fixedDeltaTime;
            // ���̰� 1���� �̻�
            bool hi = false;
            RaycastHit hit;
            bool b = Physics.Raycast(transform.position, new Vector3(0, -1, 0), out hit, 10);
            if (b && hit.distance > 0.5f) hi = true;
            float t = 0.4f;
            if (hit.distance < 1) t = t * hit.distance;

            if (hi && jumpTime > t)
            {
                if (!bLanding)
                {
                   
                    AudioSource.PlayClipAtPoint(AudioCtr.snd_jump[Random.Range(0, 3)], transform.position);
					if( isMine && !cycle_AI )
					{
						if (Random.Range(0, 1) == 0)
							AudioSource.PlayClipAtPoint(AudioCtr.snd_yahoo[Random.Range(0, 2)], transform.position);
						else
							AudioSource.PlayClipAtPoint(AudioCtr.snd_crash_voice[Random.Range(0, 6)], transform.position);
					}
                   
                }
                bLanding = true;
            }
        }
        else
        {
            jumpTime = 0;
            if (moveValue.groundF && moveValue.groundR && bLanding)
            {
                bLanding = false;
                AudioSource.PlayClipAtPoint(AudioCtr.snd_landing[2], transform.position);
            }
        }

        if (moveValue.drift)
        {
            if (!driftSound && !gameFinish)
                AudioCtr.Play(AudioCtr.snd_drift[0], transform.position);
            driftSound = true;
        }
        else
        {
            if (driftSound)
                AudioCtr.Stop(AudioCtr.snd_drift[0]);
            driftSound = false;
        }
	}

    bool driftSound = false;

    void FixedUpdate()
    {
        if (replay.record) Replay_Record();
        else if (replay.play) Replay_Play();
		
		if(User && Config._Serial == Config.SetSerial.True) CBikeSerial.SetEnv((int)(environment));
        environment = Environment.Normal;

        // Unity6 Migration: Legacy RPC removed; Phase 4 will re-implement
        // gameFinish sync was: networkView.RPC("SendGameFinish", RPCMode.Others, gameFinish)
    }

    #region ���� ��ġ���� �� �� �ִ� �Լ�
    void FindNextStep()
    {
        if (!CP.cross)
        {
            float angle = Angle_Check(transform, _waypoint.allways[CP.nextStep]);
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_waypoint.allways[CP.nextStep].position.x, _waypoint.allways[CP.nextStep].position.z)) < step_check_distance
                || (angle > -90 && angle < 90))
            {
                for (int i = 0; i < _waypoint.crossStart.Length; i++)
                {
                    if (CP.nextStep == _waypoint.crossStart[i])
                    {
                        CP.crossroad = i;
                        CP.cross = true;
                        switch (CP.crossroad)
                        {
                            case 0:
                                CP.cross_SE = _waypoint.crossways_1;
                                break;
                            case 1:
                                CP.cross_SE = _waypoint.crossways_2;
                                break;
                            case 2:
                                CP.cross_SE = _waypoint.crossways_3;
                                break;
                            case 3:
                                CP.cross_SE = _waypoint.crossways_4;
                                break;
                            case 4:
                                CP.cross_SE = _waypoint.crossways_5;
                                break;
                        }
                        CP.crossStep = CP.cross_SE[0];
                    }
                }
                CP.currentStep = CP.nextStep;
                CP.nextStep++;
                if (CP.nextStep > _waypoint.ways[1]) CP.nextStep = 0;
            }
            else if (User)
            {
                float wrongAngle = Angle_Check(transform, _waypoint.allways[CP.currentStep]);
                wrongWay = WrongWayCheck(angle, _waypoint.allways[CP.nextStep], transform);
            }
        }
        else
        {
            float angle1 = Angle_Check(transform, _waypoint.allways[CP.crossStep]);
            float angle2 = Angle_Check(transform, _waypoint.allways[CP.nextStep]);
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_waypoint.allways[CP.crossStep].position.x, _waypoint.allways[CP.crossStep].position.z)) < step_check_distance
                || (angle1 > -90 && angle1 < 90))
            {
                if (CP.crossStep == CP.cross_SE[1])
                {
                    findWay = false;
                    CP.cross = false;
                }
                else
                {
                    CP.currentStep = CP.crossStep;
                    CP.nextStep = CP.crossStep + 1;
                    CP.crossStep = CP.nextStep;
                }
            }
            else if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_waypoint.allways[CP.nextStep].position.x, _waypoint.allways[CP.nextStep].position.z)) < step_check_distance
                || (angle2 > -90 && angle2 < 90))
            {
                CP.cross = false;
            }
            else if (User)
            {
                float wrongAngle = Angle_Check(transform, _waypoint.allways[CP.currentStep]);
                wrongWay = WrongWayCheck(wrongAngle, _waypoint.allways[CP.nextStep], transform);
            }
        }
    }

    void FindClosePoint()
    {
        _waypoint = null;
        GameObject wpObj = GameObject.Find(wayName);
        if (wpObj == null) { findWay = true; return; } // Unity6 Migration: _Waypoint 없으면 건너뜀
        _waypoint = wpObj.GetComponent<WaypointDefine>();
        if (_waypoint == null || _waypoint.allways == null || _waypoint.allways.Length == 0) { findWay = true; return; }
        float fminDist = 10000.0f;
        float[] distance = new float[_waypoint.ways[1] + 1];
        for (int i = 0; i < distance.Length; i++)
        {
            distance[i] = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_waypoint.allways[i].position.x, _waypoint.allways[i].position.z));
            if (fminDist > distance[i])
            {
                fminDist = distance[i];
                CP.currentStep = i;
                CP.nextStep = i + 1;
                if (CP.nextStep > _waypoint.ways[1]) CP.nextStep = 0;
            }
        }
        findWay = true;
    }

    float Angle_Check(Transform target, Transform myTransform)
    {
        //Vector2 target_V2 = new Vector2(target.position.x, target.position.z);
        //Vector2 my_V2 = new Vector2(myTransform.position.x, myTransform.position.z);
        Vector3 my_Dir = myTransform.TransformDirection(Vector3.forward);
        Vector3 target_Dir = (target.position - myTransform.position).normalized;

        float x1 = my_Dir.x;
        float y1 = my_Dir.z;

        float x2 = target_Dir.x;
        float y2 = target_Dir.z;

        float angle = Mathf.Atan2(x2 * y1 - y2 * x1, x2 * x1 + y1 * y2) * Mathf.Rad2Deg;
        return angle;
    }

    bool WrongWayCheck(float dirAngle, Transform way, Transform target)
    {
        Vector3 dir = Vector3.left;
        if (dirAngle > 0) dir = Vector3.right;
        Vector3 wayDir = new Vector3(way.TransformDirection(dir).normalized.x, 0, way.TransformDirection(dir).normalized.z);
        Vector3 wayToTarget = new Vector3((target.position - way.position).normalized.x, 0, (target.position - way.position).normalized.z);
        Vector3 proj = Vector3.Project(wayToTarget, wayDir);
        Vector3 center = (proj - wayToTarget).normalized;
        Vector3 targetDir = new Vector3(target.TransformDirection(Vector3.forward).x, 0, target.TransformDirection(Vector3.forward).z);

        float angle = Vector3.Angle(center, targetDir);
        bool wrongway = false;
        if (angle > 90) wrongway = true;
        return wrongway;
    }
    #endregion

    #region ��Ʈ��ũ
    // Unity6 Migration: All legacy Network/RPC methods stubbed.
    // OnNetworkInstantiate, [RPC] attributes, networkView.*, BitStream, RPCMode all removed.
    // Phase 4 will re-implement multiplayer via custom UDP message system.

    // isMine defaults to true (single-player mode)

    void SendGameFinish_Stub(bool a)
    {
        gameFinish = a;
    }

    //[RPC]
    //void CallMyInfo()
    //{
    //    networkView.RPC("MyName", RPCMode.All, gameObject.name);
    //    networkView.RPC("WayName", RPCMode.All, wayName);
    //    //networkView.RPC("SyncMyTexture", RPCMode.All, MyNumber);
    //}

    //[RPC]
    //void MyName(string name) //�̸��� �����ֱ�
    //{
    //    gameObject.name = name;
    //    if (!networkView.isMine)
    //        networkView.RPC("MyName", networkView.owner, gameObject.name);
    //}

    // Unity6 Migration: WayName, SyncRank_RPC, SyncReplayPos, SendReplayPos
    // were [RPC] methods — stubbed for Phase 1. Phase 4 will restore via UDP.

    public void SyncReplayPos_Stub(Vector3 pos, Quaternion rot, float real, float pedal, float steer)
    {
        _rb.isKinematic = true; // Unity6 Migration
        cycle_Impact = false;
        Alpha(true);
        deadState = 0;
        cycle_Move = false;
        cycle_AI = false;
        transform.position = pos;
        transform.rotation = rot;
        moveValue.realSpeed = real;
        moveValue.pedalSpeed = 0;
        moveValue.steer = steer;
    }

    //[RPC]
    //void SyncMyTexture(int number)
    //{
    //    MyNumber = number;
    //    ChangeTexture();
    //    if (!networkView.isMine)
    //        networkView.RPC("SyncMyTexture", networkView.owner, MyNumber);
    //}


    // Unity6 Migration: OnSerializeNetworkView(BitStream) removed with legacy networking.
    // Serialization logic preserved as SerializeBikeState/DeserializeBikeState for Phase 4.
#if false
    void OnSerializeNetworkView_REMOVED(object stream, object info)
    {
        float syncSteer = 0.0f;
        float syncHandle = 0.0f;
        float syncPedalSpeed = 0.0f;
        float syncRealSpeed = 0.0f;

        bool syncGroundF = false;
        bool syncGroundR = false;

        float syncHeightAngle = 0.0f;
        float syncLrAngle = 0.0f;
        int syncDeadState = 0;

        bool syncFinish = false;
        bool syncGameStart = false;

        bool syncDrift = false;
        bool syncReplay = false;

        if (stream.isWriting)
        {
            syncSteer = moveValue.steer;
            syncHandle = moveValue.handle;
            syncPedalSpeed = moveValue.pedalSpeed;
            syncRealSpeed = moveValue.realSpeed;

            syncGroundF = moveValue.groundF;
            syncGroundR = moveValue.groundR;

            syncHeightAngle = moveValue.heightAngle;
            syncLrAngle = moveValue.lrAngle;

            syncDeadState = deadState;

            syncFinish = gameFinish;
            syncGameStart = gameStart;

            syncDrift = moveValue.drift;
            syncReplay = replayAct;

            stream.Serialize(ref syncSteer);
            stream.Serialize(ref syncHandle);
            stream.Serialize(ref syncPedalSpeed);
            stream.Serialize(ref syncRealSpeed);

            stream.Serialize(ref syncGroundF);
            stream.Serialize(ref syncGroundR);

            stream.Serialize(ref syncHeightAngle);
            stream.Serialize(ref syncLrAngle);

            stream.Serialize(ref syncDeadState);

            stream.Serialize(ref syncFinish);
            stream.Serialize(ref syncGameStart);

            stream.Serialize(ref syncGroundF);
            stream.Serialize(ref syncGroundR);

            stream.Serialize(ref syncDrift);
            stream.Serialize(ref syncReplay);
        }
        else
        {
            stream.Serialize(ref syncSteer);
            stream.Serialize(ref syncHandle);
            stream.Serialize(ref syncPedalSpeed);
            stream.Serialize(ref syncRealSpeed);

            stream.Serialize(ref syncGroundF);
            stream.Serialize(ref syncGroundR);

            stream.Serialize(ref syncHeightAngle);
            stream.Serialize(ref syncLrAngle);

            stream.Serialize(ref syncDeadState);

            stream.Serialize(ref syncFinish);
            stream.Serialize(ref syncGameStart);

            stream.Serialize(ref syncGroundF);
            stream.Serialize(ref syncGroundR);

            stream.Serialize(ref syncDrift);
            stream.Serialize(ref syncReplay);

            moveValue.steer = syncSteer;
            moveValue.handle = syncHandle;
            moveValue.pedalSpeed = syncPedalSpeed;
            moveValue.realSpeed = syncRealSpeed;

            moveValue.groundF = syncGroundF;
            moveValue.groundR = syncGroundR;

            moveValue.heightAngle = syncHeightAngle;
            moveValue.lrAngle = syncLrAngle;

            deadState = syncDeadState;

            gameFinish = syncFinish;
            gameStart = syncGameStart;

            moveValue.drift = syncDrift;
            replayAct = syncReplay;
        }
    }
#endif // Unity6 Migration: end of removed OnSerializeNetworkView block

    #endregion

    #region �ؽ��� ����

    public void ChangeTexture()
    {
        foreach(Renderer pos in cycleTexture)
        {
            pos.material.SetTexture("_MainTex", (Texture)Resources.Load("Texture/Cycles_Challenge/" + (MyNumber + 1).ToString()));
        }
        SetArrowColor();
    }

    public void Alpha(bool on)
    {
        int i = 0;
        if (on) i = 1;
        foreach (Renderer pos in cycleTexture)
        {
            pos.material.SetColor("_Color", new Color(0.588f, 0.588f, 0.588f, i));
        }
    }

    #endregion

    #region Replay

    float replayTime;
    int replayCount;

    void Replay_Record()
    {
        replayTime += Time.fixedDeltaTime;
        if (replayTime > 0.01f)
        {
            replay.pos[replayCount] = transform.position;
            replay.rot[replayCount] = transform.rotation;
            replay.realSpeed[replayCount] = moveValue.realSpeed;
            replay.pedalSpeed[replayCount] = moveValue.pedalSpeed;
            replay.steer[replayCount] = moveValue.steer;
            replay.deadState[replayCount] = deadState;
            replayCount++;
            if (replayCount >= replay.pos.Length)
            {
                replay.record = false;
                replayCount = 0;
                //print("Record Complete");
            }
            replayTime = 0;
        }
    }

    void Replay_Play()
    {
        replayTime += Time.fixedDeltaTime;
        if (replayTime > 0.01f)
        {
            transform.position = replay.pos[replayCount];
            transform.rotation = replay.rot[replayCount];
            moveValue.realSpeed = replay.realSpeed[replayCount];
            moveValue.pedalSpeed = replay.pedalSpeed[replayCount];
            moveValue.steer = replay.steer[replayCount];
            deadState = replay.deadState[replayCount];
            replayCount++;
            if (replayCount >= replay.pos.Length)
            {
                replay.play = false;
                replayCount = 0;
                //print("Play Complete");
            }
            replayTime = 0;
        }
    }
    #endregion

    #region �̴ϸ� �� �ڽ� ǥ�� ȭ��ǥ
    public void SetArrowColor()
    {
        //minimapArrow.arrow.renderer.material.SetColor("_Color", minimapArrow.color[color]);
        // Unity6 Migration: .renderer shorthand removed; use GetComponent<Renderer>()
        if (User)
            minimapArrow.arrow.GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Texture/Cycles_Challenge/pos_me"));
        else
            minimapArrow.arrow.GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Texture/Cycles_Challenge/pos_" + (MyNumber + 1).ToString()));
    }

    //float arrowAlphatime = 0;
    //int arrowAlpha = 0;
    int arrowSize = 0;
    float arrowSizeTime = 0;
    public void ArrowPosition(int r)
    {
        if (User)
        {
            if (!gameFinish)
                minimapArrow.arrow.position = new Vector3(transform.position.x, 0, transform.position.z);
            else
                minimapArrow.arrow.position = new Vector3(minimapArrow.finalPos.x, 0, minimapArrow.finalPos.z);
            //arrowAlphatime += Time.deltaTime;
            arrowSizeTime += Time.deltaTime;
            //if (arrowAlphatime > 0.2f)
            //{
            //    arrowAlphatime = 0;
            //    if (arrowAlpha == 0)
            //    {
            //        minimapArrow.arrow.renderer.material.SetColor("_Color", new Color(1, 1, 1, 0));
            //        arrowAlpha = 1;
            //    }
            //    else
            //    {
            //        minimapArrow.arrow.renderer.material.SetColor("_Color", new Color(1, 1, 1, 1));
            //        arrowAlpha = 0;
            //    }
            //}

            if (arrowSizeTime > 0.2f)
            {
                arrowSizeTime = 0;
                if (arrowSize == 0)
                {
                    minimapArrow.arrow.localScale = minimapArrow.arrow.localScale * 1.5f;
                    arrowSize = 1;
                }
                else
                {
                    minimapArrow.arrow.localScale = minimapArrow.arrow.localScale / 1.5f;
                    arrowSize = 0;
                }
            }
        }
        else
        {
            if (!gameFinish)
                minimapArrow.arrow.position = new Vector3(transform.position.x, -r - 2, transform.position.z);
            else
                minimapArrow.arrow.position = new Vector3(minimapArrow.finalPos.x, -r - 2, minimapArrow.finalPos.z);
        }
        if (!gameFinish)
            minimapArrow.arrow.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        else
            minimapArrow.arrow.eulerAngles = new Vector3(0, minimapArrow.finalRot.y, 0);
    }
    #endregion

    #region Trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Finish")
        {
            //if (other.GetComponent<FinishLine>())
            //{
            //    other.GetComponent<FinishLine>().count++;
            //}
            if (GameData.GameState == 1)
            {
                gameFinish = true;
                bRecordTime = false;
            }
            //FinishLine obj = other.GetComponent<FinishLine>();
            //obj.count ++;
            minimapArrow.finalPos = other.transform.position;
            minimapArrow.finalRot = transform.eulerAngles;
            //GameData.FinishPlayer++;
        }
    }

   

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Gravel")
        {
            environment = Environment.Gravel;
        }
        else if (other.tag == "Water")
        {
            environment = Environment.Water;
        }
        else if (other.tag == "Mud")
        {
            environment = Environment.Mud;
        }
    }
    #endregion
}

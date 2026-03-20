using UnityEngine;
using System.Collections;

public class Cycle_Impact : MonoBehaviour {

    Cycle_Control _control;
    private Rigidbody _rb; // Unity6 Migration: cache Rigidbody

    private GameObject ragdoll;
    private Transform MyTransform;
    private Collider[] MyCollider;
    private Transform raystart;

    private float capsule_radius = 0.2f;
    private float capsule_distance = 0.1f;

    private int deadState = 0;

	// Use this for initialization
	void Start () {
	    _control = GetComponent<Cycle_Control>();
        _rb = GetComponent<Rigidbody>(); // Unity6 Migration
        MyTransform = transform;
        MyCollider = GetComponentsInChildren<Collider>();
        ragdoll = (GameObject)Resources.Load("Prefeb/DeadMan " + _control.MyNumber);
        raystart = _control.raypoint;
	}

    //private Vector3 deadPos;
    //private Quaternion deadRot;

    private float time;
    private float colorTime;
    private float stopTime;
    private bool colorSet;
    private Vector3 pastPos;

    // 스폰/리스폰 직후 폴리곤 지형 WheelCollider 불안정으로 인한 오탐 방지
    // cycle_Impact 활성화 후 CRASH_GRACE_SEC 초 동안 충돌 감지 차단
    private float _crashGraceTimer = 0f;
    private const float CRASH_GRACE_SEC = 15.0f; // 리스폰 후 waypoint 고도차로 낙하 시간 확보

    void Update()
    {
        switch (_control.deadState)
        {
            case 0:
                // Respawn 완료(2→0 전환) 시 유예 타이머 리셋
                if (deadState != 0)
                    _crashGraceTimer = 0f;
                deadState = _control.deadState;
                if (_control.User)
                {
                    CycleCam camera = FindObjectOfType(typeof(CycleCam)) as CycleCam;
                    camera.SetTarget(_control.cameraTarget, 1);
                }
                if (_control.cycle_Impact)
                {
                    // 스폰/리스폰 직후 유예 기간 카운트
                    _crashGraceTimer += Time.deltaTime;
                    if (_crashGraceTimer >= CRASH_GRACE_SEC)
                    {
                        if (GameData.BMXServer)
                        {
                           if (!_control.impotal)
                                FindHitCondition();
                        }
                        else
                            FindHitCondition();
                    }
                    else
                    {
                        // 유예 중 로그 (최초 1회)
                        if (_crashGraceTimer <= Time.deltaTime + 0.01f)
                            Debug.Log("[Impact] 충돌 유예 시작 (" + CRASH_GRACE_SEC + "초)");
                    }
                    if (!_control.gameFinish)
                    {
                        if (stopTime == 0)
                        {
                           pastPos = transform.position;
                        }
                        stopTime += Time.deltaTime;
                        if (stopTime > 5)
                        {
							if (_control.User)
							{
								if ((CBikeSerial.GetNewButton(0) && CBikeSerial.GetNewButton(2)) || (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)))
							    {
									if (Vector2.Distance(new Vector2(pastPos.x, pastPos.z), new Vector2(transform.position.x, transform.position.z)) < 1)
									{
										_control.deadState = 2;
									}
									stopTime = 0;
								}
							}
							else
							{
								if (Vector2.Distance(new Vector2(pastPos.x, pastPos.z), new Vector2(transform.position.x, transform.position.z)) < 1)
								{
									_control.deadState = 2;
								}
								stopTime = 0;
							}
					}
					
					//if (_control.moveValue.realSpeed < 5 && _control.checkRespawn)
					//{
                        //    stopTime += Time.fixedDeltaTime;
                        //    if (stopTime > 10)
                        //    {
                        //        _control.deadState = 2;
                        //        stopTime = 0;
                        //    }
                        //}
                    }
                }
                _control.Alpha(true);
                //if (deadState != 0)
                //{
                gameObject.layer = 0;
                foreach (Collider collider in MyCollider)
                {
                    //if (collider.tag != "Wheel")
                    collider.enabled = true;
                }
                    
                    //deadState = _control.deadState;
                //}
                break;
            case 1:
                _crashGraceTimer = 0f; // Crash → 다음 스폰 시 유예 재시작
                if (deadState != 1)
                {
                    Crash();
                    deadState = _control.deadState;
                }
                if (_control.cycle_Impact)
                {
                    time += Time.deltaTime;
                    if (time > 3.0f)
                    {
                        _control.deadState = 2;
                        time = 0.0f;
                    }
                }
                break;
            case 2:
                if (deadState != 2)
                {
                    Respawn();
                    deadState = _control.deadState;
                    //rigidbody.isKinematic = false;
                }

                colorTime += Time.deltaTime;
                if (colorTime > 0.2f)
                {
                    if (colorSet) colorSet = false;
                    else colorSet = true;
                    _control.Alpha(colorSet);
                    colorTime = 0;
                    _rb.isKinematic = false; // Unity6 Migration
                }
                if (_control.cycle_Impact)
                {
                    time += Time.deltaTime;
                    if (time > 3.0f)
                    {
                        _control.deadState = 0;
                        time = 0.0f;
                        colorTime = 0;
                    }
                }
                break;
        }
    }

    bool bIn = true;
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Falldown" && _control.cycle_Impact)
        {
            if (_control.isMine)
            {
                Debug.Log("[Impact] Falldown 트리거: " + other.name);
                _control.deadState = 1;
                AudioSource.PlayClipAtPoint(AudioCtr.snd_fall[Random.Range(0, 2)], transform.position);
            }
        }

        if (other.tag == "RoadOut")
        {
            if (_control.isMine && _control.cycle_Impact)
            {
                bIn = !bIn;
                Debug.Log("[Impact] RoadOut 트리거: " + other.name + " pos=" + transform.position.ToString("F1"));
                _control.deadState = 2;
                Respawn();
                deadState = _control.deadState;
                if (_control.User)
                    AudioSource.PlayClipAtPoint(AudioCtr.snd_loadout_alarm[Random.Range(0, 3)], transform.position);
            }
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.transform.tag == "Falldown" && _control.cycle_Impact)
        {
            if (_control.isMine)
            {
                _control.deadState = 1;
                AudioSource.PlayClipAtPoint(AudioCtr.snd_fall[Random.Range(0, 2)], transform.position);
            }
        }
    }

	// Update is called once per frame
	void FindHitCondition () {
        float spd  = _control.moveValue.realSpeed;
        float lr   = _control.moveValue.lrAngle;
        float ht   = _control.moveValue.heightAngle;

        // ── 앞방향 CapsuleCast ──────────────────────────────
        RaycastHit[] hits;
        Vector3 rayend = raystart.position + raystart.TransformDirection(Vector3.up);
        Vector3 direction = MyTransform.TransformDirection(Vector3.forward);
        hits = Physics.CapsuleCastAll(raystart.position, rayend, capsule_radius, direction, capsule_distance);
        foreach (RaycastHit t in hits)
        {
            if (!t.transform.IsChildOf(MyTransform))
            {
                Debug.DrawLine(raystart.position, t.point, Color.red);
                if (t.transform.tag == "Player")
                {
                    float value = _control.cycle_AI ? 45f : 40f;
                    if (Mathf.Abs(lr) > value + _control.rank * 1.5f)
                    {
                        Debug.Log(string.Format("[Impact] 앞충돌-Player 크래시: hit={0} spd={1:F1} lr={2:F1}", t.transform.name, spd, lr));
                        _control.deadState = 1;
                    }
                }
                else
                {
                    if (spd > 45)
                    {
                        // 지형 메시(TerrainCollider 또는 법선이 위를 향하는 경사면)는 크래시 제외
                        if (t.collider is TerrainCollider) continue;
                        if (t.normal != Vector3.zero && t.normal.y > 0.5f) continue;
                        Debug.Log(string.Format("[Impact] 앞충돌-장애물 크래시: hit={0}[{1}] spd={2:F1}", t.transform.name, t.transform.tag, spd));
                        _control.deadState = 1;
                    }
                }
            }
        }

        // ── 기울기 초과 ─────────────────────────────────────
        if (Mathf.Abs(ht) > 80 || Mathf.Abs(lr) > 50)
        {
            Debug.Log(string.Format("[Impact] 기울기 크래시: lr={0:F1}° ht={1:F1}° spd={2:F1}", lr, ht, spd));
            _control.deadState = 1;
        }

        // ── 뒷방향 CapsuleCast ──────────────────────────────
        RaycastHit[] hits_R;
        Vector3 raystart_R = MyTransform.position + MyTransform.TransformDirection(Vector3.up * 0.6f);
        Vector3 rayend_R = raystart_R + MyTransform.TransformDirection(Vector3.up);
        Vector3 direction_R = MyTransform.TransformDirection(-Vector3.forward);
        hits_R = Physics.CapsuleCastAll(raystart_R, rayend_R, capsule_radius, direction_R, 0.4f);
        foreach (RaycastHit t in hits_R)
        {
            if (!t.transform.IsChildOf(MyTransform))
            {
                Debug.DrawLine(raystart_R, t.point, Color.red);
                if (t.transform.tag == "Player")
                {
                    float value = _control.cycle_AI ? 45f : 40f;
                    if (Mathf.Abs(lr) > value + _control.rank * 1.5f)
                    {
                        Debug.Log(string.Format("[Impact] 뒤충돌-Player 크래시: hit={0} spd={1:F1} lr={2:F1}", t.transform.name, spd, lr));
                        _control.deadState = 1;
                    }
                }
                else
                {
                    if (spd > 45)
                    {
                        Debug.Log(string.Format("[Impact] 뒤충돌-장애물 크래시: hit={0}[{1}] spd={2:F1}", t.transform.name, t.transform.tag, spd));
                        _control.deadState = 1;
                    }
                }
            }
        }

        // ── Up Crash: 이동 중(realSpeed >= 5)에만 검사 ─────
        if (spd >= 5f)
        {
            RaycastHit[] hits_U;
            Vector3 raystart_U = MyTransform.position + MyTransform.TransformDirection(-Vector3.forward * 0.5f) + MyTransform.TransformDirection(Vector3.up * 2.0f);
            Vector3 rayend_U = MyTransform.position + MyTransform.TransformDirection(Vector3.forward * 0.5f) + MyTransform.TransformDirection(Vector3.up * 2.0f);
            Vector3 direction_U = MyTransform.TransformDirection(Vector3.up);
            hits_U = Physics.CapsuleCastAll(raystart_U, rayend_U, 0.05f, direction_U, 0.07f);
            foreach (RaycastHit t in hits_U)
            {
                if (!t.transform.IsChildOf(MyTransform))
                {
                    // TerrainCollider는 지형이므로 Up Crash 대상 제외
                    // (터널·구조물 MeshCollider만 감지)
                    if (t.collider is TerrainCollider) continue;
                    Debug.DrawLine(raystart_U, t.point, Color.red);
                    Debug.Log(string.Format("[Impact] Up Crash: hit={0}[{1}] spd={2:F1}", t.transform.name, t.transform.tag, spd));
                    _control.deadState = 1;
                }
            }
        }
	}

    void Crash()
    {
        _rb.isKinematic = true; // Unity6 Migration
        gameObject.layer = 2;
        foreach (Collider collider in MyCollider)
        {   
            //if (collider.tag != "Wheel")
                collider.enabled = false;
        }
        _control.Alpha(false);
        GameObject deadman = Instantiate(ragdoll, MyTransform.position, MyTransform.rotation) as GameObject;
        RagdollAct ragdollAct = deadman.GetComponent<RagdollAct>();
        if (_control.User)
        {
            _control.cameraTarget = ragdollAct.cameraPoint;
            CycleCam camera = FindObjectOfType(typeof(CycleCam)) as CycleCam;
            camera.SetTargetDead(_control.cameraTarget);
        }
        //MyTransform.position = MyTransform.position + Vector3.up * 100.0f;
        //MyTransform.rotation = Quaternion.EulerAngles(0, MyTransform.eulerAngles.y, 0);
        ragdollAct.Act(_control.moveValue.realSpeed);

        CBikeSerial.m_nCrash = 1;
    }

    void Respawn()
    {
        _rb.isKinematic = true; // Unity6 Migration
        gameObject.layer = 2;
        if (_control.isMine)
        {
            RaycastHit hit;
            if (GameData.inGame) _control.findWay = false;// ��� ���� �� ���� ...
            Transform respawnPos = _control._waypoint.allways[_control.CP.currentStep];
            if (Physics.Raycast(respawnPos.position + new Vector3(Random.Range(-0.5f, 0.5f), 5.0f, Random.Range(-0.5f, 0.5f)), -Vector3.up, out hit))
            {
                Vector3 hitP = hit.point + Vector3.up * 0.1f;
                MyTransform.position = hitP;
                MyTransform.rotation = respawnPos.rotation;
            }
            else
            {
                MyTransform.position = respawnPos.position + Vector3.up;
                MyTransform.rotation = respawnPos.rotation;
            }
        }
        if (_control.User)
        {
            _control.cameraTarget = _control.camera[1];
            CycleCam camera = FindObjectOfType(typeof(CycleCam)) as CycleCam;
            camera.SetTarget(_control.cameraTarget, 1);
        }
        //Collider[] MyCollider = GetComponentsInChildren<Collider>();
        //foreach (Collider collider in MyCollider) collider.isTrigger = false;
        foreach (Collider collider in MyCollider)
        {
            collider.enabled = true;
        }
    }
}

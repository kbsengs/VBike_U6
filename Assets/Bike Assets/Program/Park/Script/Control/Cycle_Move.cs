using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cycle_Move : MoveModule {

    private Cycle_Control _control;
    private float fDelayTime = 0.0f;

    // ══════════════════════════════════════════════════════════
    // 물리 단계 (physicsPhase)
    //   0 = 스폰 직후 대기 (3초, kinematic, 키 입력 차단)
    //   1 = 물리 안정화   (최대 2초, 접지 대기, 키 입력 차단)
    //   2 = 정상 주행     (플레이어 입력 유효)
    // ══════════════════════════════════════════════════════════
    private int physicsPhase = 0;
    private float groundedTimer = 0f;
    private const float SETTLE_TIMEOUT = 2.0f;  // Phase 1 최대 대기 시간(초)
    private float phase2StabTimer = 0f;           // Phase 2 진입 직후 안정화 시간
    private const float PHASE2_STAB_TIME = 0.5f; // Phase 2 초기 angularVelocity 억제 시간(초)

    // ══════════════════════════════════════════════════════════
    // 속도 단계 변수 (_pedalLevel)
    //   - 위 화살표(↑) 한 번 = +1단계, 아래 화살표(↓) 한 번 = -1단계
    //   - 범위: -10(후진 최대) ~ 0(정지) ~ +10(전진 최대)
    //   - 실제 속도: pedalSpeed = _pedalLevel × PEDAL_STEP
    //   - 1단계 = PEDAL_STEP(13) ≈ 10 km/h
    //   - 최대(10단계) = 130 ≈ 100 km/h
    // ══════════════════════════════════════════════════════════
    [HideInInspector] public int _pedalLevel = 0;   // 외부(BikeDebugOverlay)에서 읽기용
    [HideInInspector] public int physicsPhasePublic = 0; // 디버그 오버레이용
    private const int   PEDAL_MAX  = 20;             // 테스트 최대 단계 = 20 km/h
    private const float PEDAL_STEP = 13f;            // 내부 단위 (사용 안 함, 호환 유지)

    private int _prevDeadState = 0; // Respawn 감지용
    // Phase 2 진입 후 플레이어가 Up 키를 누를 때까지 kinematic 유지
    // (스폰 지점 폴리곤 메시 대삼각형 PhysX 불안정 완전 차단)
    private bool _waitingForPedal = true;
    // 충돌 무시한 MeshCollider 목록 (복구 시 동일 목록 사용)
    private System.Collections.Generic.List<Collider> _ignoredMeshColliders
        = new System.Collections.Generic.List<Collider>();

	void Start () {
        _control = GetComponent<Cycle_Control>();
        Init(_control.moveValue.centerOfMass, _control.moveValue.frontTire, _control.moveValue.rearTire);
        FrictionValue_F(forwardFriction, _control.moveValue.forwardFriction.extremumSlip, _control.moveValue.forwardFriction.extremumValue
            , _control.moveValue.forwardFriction.asymptoteSlip, _control.moveValue.forwardFriction.asymptoteValue, _control.moveValue.forwardFriction.stiffness);
        FrictionValue_S(sideFriction, _control.moveValue.sideFriction.extremumSlip, _control.moveValue.sideFriction.extremumValue
           , _control.moveValue.sideFriction.asymptoteSlip, _control.moveValue.sideFriction.asymptoteValue, _control.moveValue.sideFriction.stiffness);
        SpringSet(suspension, _control.moveValue.suspension.spring, _control.moveValue.suspension.damper, _control.moveValue.suspension.position);
        FrictionSetting();

        // ── 시작 시 속도 초기화 ──────────────────────────────
        fDelayTime  = 0.0f;
        physicsPhase = 0;
        _pedalLevel  = 0;       // ← 속도 단계 0
        pedalSpeed   = 0f;      // ← 물리 속도 0
        // ─────────────────────────────────────────────────────

        // Phase 0/1 동안 충돌 감지 비활성
        // (스폰 직후 Up Crash 등 crash detection이 즉시 발동하는 버그 방지)
        // Phase 2 안정화 완료 시점에 다시 true로 복구
        _control.cycle_Impact = false;

        _rb.maxDepenetrationVelocity = 1.0f; // PhysX 관통 폭발력 제한

        Debug.Log("[Cycle_Move] Start() TEST_MODE=" + GameData.TEST_MODE
                  + " pedalLevel=" + _pedalLevel + " pedalSpeed=" + pedalSpeed);
	}

    // ══════════════════════════════════════════════════════════
    // 키 입력: Phase 2 에서만 허용
    //   Phase 0·1 에서 허용하면 대기 중 _pedalLevel 이 최대치가 되어
    //   Phase 2 진입 즉시 최고속도로 출발하는 버그 발생
    // ══════════════════════════════════════════════════════════
    void Update()
    {
        if (!GameData.TEST_MODE || physicsPhase < 2) return; // Phase 0·1 차단

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            _pedalLevel = System.Math.Min(_pedalLevel + 1, PEDAL_MAX);
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            _pedalLevel = System.Math.Max(_pedalLevel - 1, -PEDAL_MAX);
    }

    // ══════════════════════════════════════════════════════════
    // ApplyKeyboard: Phase 2 에서 매 프레임 호출 (TEST_MODE=true 전용)
    //   _pedalLevel → pedalSpeed 변환 + 핸들 처리
    // ══════════════════════════════════════════════════════════
    void ApplyKeyboard()
    {
        pedalSpeed = _pedalLevel * PEDAL_STEP; // ← 여기서 속도 결정

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            handle = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            handle = -1f;
        else
            handle = 0f;

        steer = maxValue.MaxSteer * handle;

        // _pedalLevel == 0 이면 정지·브레이크
        if (_pedalLevel == 0)
        {
            pedalSpeed = 0f;
            if (wheelSetting.wheels != null)
                foreach (WheelCollider pos in wheelSetting.wheels)
                    pos.brakeTorque = 500f;
            return;
        }

        if ((groundR || groundF) && Input.GetKeyDown(KeyCode.Q))
            _rb.AddForce(transform.up * 250, ForceMode.Acceleration);

        drift = Input.GetKey(KeyCode.Space) || CBikeSerial.GetDrift();
    }

	void FixedUpdate () {
        SyncData_Receive();

        if (!_control.cycle_Move)
            return;

        // ══════════════════════════════════════════════════════
        // Respawn 감지: deadState 가 0 이 아닌 상태에서 0 으로 돌아오면
        //              physicsPhase 를 0 으로 리셋해 Phase 0 부터 재시작
        // ══════════════════════════════════════════════════════
        if (_control.deadState == 0 && _prevDeadState != 0)
        {
            physicsPhase = 0;
            fDelayTime   = 0f;
            _pedalLevel  = 0;
            pedalSpeed   = 0f;
            groundedTimer = 0f;
            phase2StabTimer = 0f;
            _waitingForPedal = true;
            _rb.isKinematic = true;
            _control.cycle_Impact = false; // Respawn 후 Phase 0 재시작 시 충돌 감지 재비활성
            Debug.Log("[Cycle_Move] Respawn → Phase 0 리셋");
        }
        _prevDeadState = _control.deadState;

        // ══════════════════════════════════════════════════════
        // Phase 0: 스폰 직후 대기 (3 초, kinematic)
        //   - SnapToGround 로 지면 위에 바이크 배치
        //   - motorTorque·pedalSpeed = 0 으로 강제
        //   - 3 초 후 isKinematic = false → Phase 1 이동
        // ══════════════════════════════════════════════════════
        if (physicsPhase == 0)
        {
            fDelayTime += Time.deltaTime;
            _rb.isKinematic = true;
            SnapToGround();
            // 웨이포인트 rotation의 z값(좌우 기울기)을 0으로 보정
            // 경사면 waypoint에 스폰 시 Phase 1 진입 시 큰 lrAngle 방지
            Vector3 e0 = transform.eulerAngles;
            if (Mathf.Abs(e0.z) > 0.5f && e0.z < 359.5f)
                transform.eulerAngles = new Vector3(e0.x, e0.y, 0f);
            if (wheelSetting.wheels != null)
                foreach (WheelCollider pos in wheelSetting.wheels)
                { pos.motorTorque = 0f; pos.brakeTorque = 500f; }

            if (fDelayTime >= 3.0f)
            {
                _rb.isKinematic = false;
                _rb.linearVelocity  = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                pedalSpeed  = 0f;
                _pedalLevel = 0;
                physicsPhase = 1;
                physicsPhasePublic = 1;
                groundedTimer = 0f;
                Debug.Log("[Cycle_Move] Phase 0 → 1");
            }
            return;
        }

        // ══════════════════════════════════════════════════════
        // Phase 1: 안전 정착 대기 (최대 2 초, kinematic 유지)
        //   - Phase 0 와 동일하게 kinematic + SnapToGround 유지
        //   - 근본 원인: WheelCollider 스프링이 폴리곤 벽을 지면으로 오인하여
        //     폭발적 힘을 생성 → kinematic 유지로 완전 차단
        //   - 접지(groundF & groundR) 또는 타임아웃 확인 후
        //     Phase 2 진입 직전에만 isKinematic = false
        // ══════════════════════════════════════════════════════
        if (physicsPhase == 1)
        {
            // kinematic 유지 + 매 프레임 SnapToGround (Phase 0 와 동일)
            _rb.isKinematic = true;
            SnapToGround();
            // 좌우 기울기 수직화 (lrAngle 조건 통과 보장)
            Vector3 e1 = transform.eulerAngles;
            if (Mathf.Abs(e1.z) > 0.5f && e1.z < 359.5f)
                transform.eulerAngles = new Vector3(e1.x, e1.y, 0f);

            if (wheelSetting.wheels != null)
                foreach (WheelCollider pos in wheelSetting.wheels)
                { pos.motorTorque = 0f; pos.brakeTorque = 10000f; }

            PhysicsValue(); // groundF, groundR, lrAngle 업데이트
            groundedTimer += Time.fixedDeltaTime;

            // 접지 또는 타임아웃 → Phase 2 진입
            bool groundReady = groundF && groundR && groundedTimer >= 0.3f && Mathf.Abs(lrAngle) < 35f;
            bool timeout     = groundedTimer >= SETTLE_TIMEOUT;
            if (groundReady || timeout)
            {
                // Phase 2 진입 직전: 최종 위치·자세 확정 후 kinematic 해제
                SnapToGround();
                Vector3 e = transform.eulerAngles;
                transform.eulerAngles = new Vector3(e.x, e.y, 0f);

                _rb.isKinematic = false;   // 안전 위치에서만 물리 활성화
                _rb.linearVelocity  = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                steer       = 0f;
                handle      = 0f;
                _pedalLevel = 0;
                pedalSpeed  = 0f;

                phase2StabTimer = 0f;
                _waitingForPedal = true;
                physicsPhase = 2;
                physicsPhasePublic = 2;
                LogTerrainAtPhase2();
                Debug.Log("[Cycle_Move] Phase 1 → 2 (kinematic 해제) lrAngle=" + lrAngle.ToString("F1")
                          + " groundedTimer=" + groundedTimer.ToString("F2")
                          + " reason=" + (groundReady ? "groundReady" : "timeout"));
            }

            SyncData_Send_Self();
            SyncData_Send();
            return;
        }

        // ══════════════════════════════════════════════════════
        // Phase 2: 정상 주행
        //   TEST_MODE = true  → ApplyKeyboard() : _pedalLevel × 13 = pedalSpeed
        //   TEST_MODE = false → Serial()        : 하드웨어(CBikeSerial) 입력
        //
        //   _waitingForPedal = true (초기값):
        //     kinematic 유지 + SnapToGround. 플레이어가 Up 키(↑)를 눌러
        //     _pedalLevel > 0 이 될 때까지 폴리곤 스폰 지점 PhysX 불안정 완전 차단.
        //
        //   _waitingForPedal = false (플레이어 Up 입력 후):
        //     kinematic 해제 → PHASE2_STAB_TIME(0.5초) 안정화 → cycle_Impact 활성
        // ══════════════════════════════════════════════════════

        // ── 플레이어 Up 입력 대기 (kinematic 유지) ─────────
        if (_waitingForPedal)
        {
            _rb.isKinematic = true;
            SnapToGround();
            Vector3 ep = transform.eulerAngles;
            if (Mathf.Abs(ep.z) > 0.5f && ep.z < 359.5f)
                transform.eulerAngles = new Vector3(ep.x, ep.y, 0f);
            if (wheelSetting.wheels != null)
                foreach (WheelCollider pos in wheelSetting.wheels)
                { pos.motorTorque = 0f; pos.brakeTorque = 10000f; }
            PhysicsValue();
            SyncData_Send_Self();
            SyncData_Send();

            // TEST_MODE: Up 키 입력 시 kinematic 해제
            if (GameData.TEST_MODE && _pedalLevel != 0)
            {
                SnapToGround(); // 최종 위치 확정
                Vector3 ef = transform.eulerAngles;
                transform.eulerAngles = new Vector3(ef.x, ef.y, 0f);

                // WheelCollider 일시 비활성 — 폴리곤 경계 PhysX 불안정 접촉 차단
                if (wheelSetting.wheels != null)
                    foreach (WheelCollider wc in wheelSetting.wheels)
                        wc.enabled = false;

                // 반경 5m 내 모든 MeshCollider 충돌 일시 무시 — 폴리곤/지형 접촉력 원천 차단
                IgnoreNearbyMeshColliders(true);

                _rb.isKinematic = false;
                _rb.linearVelocity  = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                phase2StabTimer = 0f;
                _waitingForPedal = false;
                Debug.Log("[Cycle_Move] 페달 입력 → kinematic 해제 (pedalLevel=" + _pedalLevel + ")");

                // 3초 후 WheelCollider + Polygon 충돌 복구
                StartCoroutine(ReenableWheelColliders(3.0f));
            }
            return;
        }

        phase2StabTimer += Time.fixedDeltaTime;
        if (phase2StabTimer < PHASE2_STAB_TIME)
        {
            if (!_rb.isKinematic)
            {
                _rb.angularVelocity = Vector3.zero;
                Vector3 vStab = _rb.linearVelocity; vStab.x = 0f; vStab.z = 0f;
                _rb.linearVelocity = vStab;
            }
            if (wheelSetting.wheels != null)
                foreach (WheelCollider pos in wheelSetting.wheels)
                { pos.motorTorque = 0f; pos.brakeTorque = 10000f; }
            PhysicsValue();
            SyncData_Send_Self();
            SyncData_Send();
            return;
        }

        // Phase 2 안정화 완료 → 충돌 감지 활성 (Phase 0 Start()에서 비활성화했던 것 복구)
        if (!_control.cycle_Impact)
        {
            _control.cycle_Impact = true;
            Debug.Log("[Cycle_Move] Phase 2 stab 완료 → cycle_Impact 활성");
        }

        if (GameData.TEST_MODE)
            ApplyKeyboard();
        else
            Serial();

        CheckJump();
        PhysicsValue();

        if (_control.gameFinish)
        {
            pedalSpeed  = 0;
            _pedalLevel = 0;
            _control.cycle_Move = false;
            _control.cycle_AI   = true;
        }

        if (GameData.TEST_MODE)
        {
            // 비정상 상승 속도 클램프 (suspension 폭발 → lrAngle > 50 → Crash 루프 방지)
            Vector3 vel = _rb.linearVelocity;
            if (vel.y > 3f) { vel.y = 3f; _rb.linearVelocity = vel; }

            // ── 수평 속도 폭발 클램프 ─────────────────────────────
            {
                Vector3 hv = _rb.linearVelocity;
                float hSpeedSq = hv.x * hv.x + hv.z * hv.z;
                if (hSpeedSq > 100f) // 10 m/s 초과
                {
                    hv.x = 0f; hv.z = 0f;
                    _rb.linearVelocity = hv;
                }
            }

            // ── 각속도 폭발 클램프 ────────────────────────────────
            // 폴리곤 대삼각형 접촉 → 비정상 토크 → lrAngle 누적 → 크래시 루프 방지
            if (_rb.angularVelocity.sqrMagnitude > 4f) // > 2 rad/s
                _rb.angularVelocity = Vector3.zero;

            // ── z축 기울기 교정 (lrAngle 누적 방지) ─────────────────
            // 폴리곤 접촉으로 인한 비정상 기울기가 5° 이상 누적되면 즉시 교정
            {
                Vector3 eu = transform.eulerAngles;
                float zAngle = eu.z > 180f ? eu.z - 360f : eu.z;
                if (Mathf.Abs(zAngle) > 5f)
                    transform.eulerAngles = new Vector3(eu.x, eu.y, 0f);
            }

            // WheelCollider motorTorque 우회:
            //   pedalSpeed=0 으로 Move() 호출 → 조향·마찰·angularDamping 만 처리
            float savedPedal = pedalSpeed;
            pedalSpeed = 0f;
            Move(true);
            pedalSpeed = savedPedal;

            // Move() 내부에서 pedalSpeed=0 이면 brakeTorque × 2 가 설정되므로 명시 재설정
            if (wheelSetting.wheels != null)
            {
                float bt = (_pedalLevel != 0) ? 0f : 500f;
                foreach (WheelCollider pos in wheelSetting.wheels)
                { pos.motorTorque = 0f; pos.brakeTorque = bt; }
            }

            // ── 정지 상태 강제 (pedalLevel == 0) ──────────────
            //   장애물·경사면이 바이크를 밀어도 수평 속도 0 유지 (테스트 목적)
            if (_pedalLevel == 0 && !_rb.isKinematic)
            {
                Vector3 v = _rb.linearVelocity;
                v.x = 0f; v.z = 0f;
                _rb.linearVelocity = v;
            }

            // ── 전진/후진: 수평 속도 직접 설정 ──────────────────
            //   위↑ 1번 = +1 km/h, 단계 × (1/3.6) m/s
            //   AddForce 방식은 폴리곤 노이즈 속도와 보정력이 충돌하여
            //   좌우 진동을 유발하므로, 속도를 직접 forward 방향으로 강제
            //   lrAngle ≥ 45° 이면 미적용 (넘어진 상태 폭주 방지)
            if (_pedalLevel != 0 && Mathf.Abs(lrAngle) < 45f)
            {
                float targetSpeed = _pedalLevel * (1.0f / 3.6f); // 단계 × 1 km/h → m/s
                Vector3 fwd = transform.forward; fwd.y = 0f;
                if (fwd.sqrMagnitude > 0.01f) fwd.Normalize();
                Vector3 driveVel = _rb.linearVelocity;
                driveVel.x = fwd.x * targetSpeed;
                driveVel.z = fwd.z * targetSpeed;
                _rb.linearVelocity = driveVel;
                // realSpeed 교정: PhysX 노이즈가 realSpeed를 오염시켜 spd>45 크래시 오탐 발생
                // 의도 속도(pedalLevel × 1 km/h)로 덮어써서 장애물 충돌 조건 오탐 차단
                realSpeed = Mathf.Abs(_pedalLevel) * 1.0f;
                _control.moveValue.realSpeed = realSpeed;
            }
            else if (_pedalLevel == 0 && !_rb.isKinematic)
            {
                realSpeed = 0f;
                _control.moveValue.realSpeed = 0f;
            }
        }
        else
        {
            Move(true);
            if (pedalSpeed == 0f && wheelSetting.wheels != null)
                foreach (WheelCollider pos in wheelSetting.wheels)
                    pos.brakeTorque = 500f;
        }

        SyncData_Send_Self();
        SyncData_Send();
	}

    // Phase 1→2 전환 시 지형 정보를 콘솔에 출력 (원인 분석용)
    private void LogTerrainAtPhase2()
    {
        Vector3 pos = transform.position;

        // 지면 Raycast
        RaycastHit hit;
        string surfaceInfo = "지면 없음";
        if (Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, out hit, 30f))
        {
            surfaceInfo = string.Format("이름={0} 태그={1} 법선={2} 거리={3:F2}m",
                hit.collider.name, hit.collider.tag,
                hit.normal.ToString("F2"), hit.distance);
        }

        // 주변 2m 콜라이더
        Collider[] nearby = Physics.OverlapSphere(pos, 2f);
        var sb = new System.Text.StringBuilder();
        foreach (var c in nearby)
        {
            if (c.transform.IsChildOf(transform)) continue;
            sb.Append(c.name).Append("[").Append(c.tag).Append("] ");
        }
        string nearbyInfo = sb.Length > 0 ? sb.ToString() : "없음";

        Debug.Log(string.Format(
            "[Cycle_Move] Phase2 진입 지형 분석\n" +
            "  위치={0}\n" +
            "  lrAngle={1:F1}° heightAngle={2:F1}°\n" +
            "  groundF={3} groundR={4}\n" +
            "  지면: {5}\n" +
            "  주변 콜라이더: {6}",
            pos.ToString("F1"),
            lrAngle, heightAngle,
            groundF, groundR,
            surfaceInfo, nearbyInfo));
    }

    private void SnapToGround()
    {
        float heightOffset = CalcGroundOffset();
        Vector3 rayOrigin = transform.position + Vector3.up * 3f;
        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, 50f);
        float bestY = float.MinValue;
        // 현재 위치보다 0.5m 이상 위의 히트는 폴리곤 벽 상단으로 간주하여 무시
        // "가장 높은 히트" 선택 시 벽 상단에 스냅되어 하늘로 솟구치는 버그 방지
        float maxGroundY = transform.position.y + 0.5f;
        foreach (var hit in hits)
        {
            if (hit.transform.IsChildOf(transform)) continue;
            if (hit.point.y > maxGroundY) continue;
            if (hit.point.y > bestY) bestY = hit.point.y;
        }
        if (bestY > float.MinValue)
        {
            Vector3 pos = transform.position;
            pos.y = bestY + heightOffset;
            transform.position = pos;
        }
    }

    private float CalcGroundOffset()
    {
        if (wheelSetting.wheels == null || wheelSetting.wheels.Length == 0)
            return 1.0f;
        float maxOffset = float.MinValue;
        foreach (var wc in wheelSetting.wheels)
        {
            if (wc == null) continue;
            Vector3 wcInRootLocal = transform.InverseTransformPoint(
                wc.transform.TransformPoint(wc.center));
            float needed = wc.radius - wcInRootLocal.y + 0.1f;
            if (needed > maxOffset) maxOffset = needed;
        }
        return maxOffset > float.MinValue ? maxOffset : 1.0f;
    }

    private float jumpTime;
    private bool bLanding;

    void CheckJump()
    {
        if (!groundF && !groundR)
        {
            jumpTime += Time.fixedDeltaTime;
            bool hi = false;
            RaycastHit hit;
            bool b = Physics.Raycast(transform.position, new Vector3(0, -1, 0), out hit, 10);
            if (b && hit.distance > 0.5f) hi = true;
            float t = 0.4f;
            if (hit.distance < 1) t = t * hit.distance;
            if (hi && jumpTime > t)
            {
                if (!bLanding)
                    CBikeSerial.m_nJump = 1;
                bLanding = true;
            }
        }
        else
        {
            jumpTime = 0;
            if (groundF && groundR && bLanding)
            {
                bLanding = false;
                CBikeSerial.m_nJump = 3;
            }
        }
    }

    private int breakold = 0;
    private bool handleAct = false;
    private float jTime = 0;

    void Serial()
    {
        int b1 = 0, b2 = 0;
        if (_control.deadState > 0 && _control.User)
            CBikeSerial.m_nJump = 0;

        CBikeSerial.FrameBike(Time.fixedDeltaTime, heightAngle, maxValue.MaxSteer, (int)(_control.environment), _rb.linearVelocity.magnitude);
        b1 = CBikeSerial.b1;
        b2 = CBikeSerial.b2;

        if (breakold != (b1 + b2) && (b1 + b2) > 4)
        {
            breakold = (b1 + b2);
            AudioCtr.Play(AudioCtr.snd_break[0], transform.position);
        }
        steer = CBikeSerial.m_fSteer;

        if (steer == 0)
            steer = Input.GetAxis("Horizontal") * 35;

        if (steer == 0)
        {
            if (CBikeSerial.jBtn == "B")
            {
                if (CBikeSerial.GetNewSwitch1(1))
                {
                    if (steer > 0) jTime = 0;
                    jTime += CBikeSerial.jSpeed * Time.deltaTime;
                    steer = -35 * jTime;
                    if (steer < -35) steer = -35;
                }
                else if (CBikeSerial.GetNewSwitch1(0))
                {
                    if (steer < 0) jTime = 0;
                    jTime += CBikeSerial.jSpeed * Time.deltaTime;
                    steer = 35 * jTime;
                    if (steer > 35) steer = 35;
                }
                else
                {
                    jTime = 0;
                    steer = Mathf.MoveTowards(steer, 0, Time.deltaTime);
                }
            }
            else if (CBikeSerial.jBtn == "b")
            {
                if (CBikeSerial.GetNewSwitch2(1))
                {
                    if (steer > 0) jTime = 0;
                    jTime += CBikeSerial.jSpeed * Time.deltaTime;
                    steer = -35 * jTime;
                    if (steer < -35) steer = -35;
                }
                else if (CBikeSerial.GetNewSwitch2(0))
                {
                    if (steer < 0) jTime = 0;
                    jTime += CBikeSerial.jSpeed * Time.deltaTime;
                    steer = 35 * jTime;
                    if (steer > 35) steer = 35;
                }
                else
                {
                    jTime = 0;
                    steer = Mathf.MoveTowards(steer, 0, Time.deltaTime);
                }
            }
        }

        pedalSpeed = 130 * Input.GetAxis("Vertical");
        if (pedalSpeed == 0)
		{
            if (GameData.autoMode == "1")
            {
                if (CBikeSerial.m_fPedalSpeed == 0)
                    pedalSpeed = 12.5f * int.Parse(GameData.autoModeSpeed);
                else
                    pedalSpeed = CBikeSerial.m_fPedalSpeed;
            }
			else pedalSpeed = CBikeSerial.m_fPedalSpeed;
        }

        if ((b1 != 0 && b2 == 0) || (b1 == 0 && b2 != 0) || Input.GetKey(KeyCode.Space))
            drift = true;
        else
            drift = false;

        if (!drift)
            resistance = CBikeSerial.m_fBrakeTorque;

        float Khandle = Input.GetAxis("Horizontal");
        if (Khandle == 0)
        {
            if (handleAct)
            {
                if (drift) handle = -CBikeSerial.GetHandle2() * 0.075f;
                else       handle = -CBikeSerial.GetHandle2() * 0.04f;
            }
            else
            {
                if (handle != CBikeSerial.GetHandle2()) handleAct = true;
            }
        }
        else
            handle = Khandle;

        if (realSpeed < 2)
        {
            if (CBikeSerial.GetNewButton(2) || Input.GetKeyDown(KeyCode.Q))
                _control.deadState = 2;
        }
    }

    void SyncData_Send()
    {
        _control.moveValue.realSpeed   = realSpeed;
        _control.moveValue.heightAngle = heightAngle;
        _control.moveValue.lrAngle     = lrAngle;
        _control.moveValue.groundF     = groundF;
        _control.moveValue.groundR     = groundR;
    }

    void SyncData_Send_Self()
    {
        _control.moveValue.pedalSpeed = pedalSpeed * (80.0f / 150.0f);
        _control.moveValue.steer  = steer;
        _control.moveValue.handle = handle;
        _control.moveValue.drift  = drift;
    }

    void SyncData_Receive()
    {
        resistance = _control.moveValue.resistance;
    }

    // WheelCollider + Polygon 충돌 복구 코루틴
    private IEnumerator ReenableWheelColliders(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!_rb.isKinematic)
        {
            _rb.linearVelocity  = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        if (wheelSetting.wheels != null)
            foreach (WheelCollider wc in wheelSetting.wheels)
                if (wc != null) wc.enabled = true;
        IgnoreNearbyMeshColliders(false);
        Debug.Log("[Cycle_Move] WheelCollider + MeshCollider 충돌 복구 완료 (" + delay + "초 후)");
    }

    // 반경 5m 내 모든 MeshCollider(Polygon·지형·도로 불문)와 바디 콜라이더 간 충돌 설정
    // Tracking02_Terrain01, road333 등 Polygon 컴포넌트 없는 지형도 포함
    private void IgnoreNearbyMeshColliders(bool ignore)
    {
        Collider[] bikeColliders = GetComponentsInChildren<Collider>(true);
        if (ignore)
        {
            _ignoredMeshColliders.Clear();
            Collider[] nearby = Physics.OverlapSphere(transform.position, 5f);
            foreach (Collider nc in nearby)
            {
                if (!(nc is MeshCollider)) continue;
                if (nc.transform.IsChildOf(transform)) continue;
                _ignoredMeshColliders.Add(nc);
                foreach (Collider bc in bikeColliders)
                {
                    if (bc is WheelCollider) continue;
                    Physics.IgnoreCollision(bc, nc, true);
                }
            }
            Debug.Log("[Cycle_Move] 인근 MeshCollider 충돌 무시: " + _ignoredMeshColliders.Count + "개");
        }
        else
        {
            foreach (Collider nc in _ignoredMeshColliders)
            {
                if (nc == null) continue;
                foreach (Collider bc in bikeColliders)
                {
                    if (bc is WheelCollider) continue;
                    Physics.IgnoreCollision(bc, nc, false);
                }
            }
            Debug.Log("[Cycle_Move] 인근 MeshCollider 충돌 복구: " + _ignoredMeshColliders.Count + "개");
            _ignoredMeshColliders.Clear();
        }
    }
}

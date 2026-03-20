using UnityEngine;
using System.Collections;

// ══════════════════════════════════════════════════════════════════
// Cycle_Move — Raycast 기반 자전거 이동 (WheelCollider 제거)
//
// 변경 이유:
//   Unity 6 PhysX 5.x 에서 WheelCollider + MeshCollider(대삼각형) 조합이
//   수백 m/s 폭발력을 발생시켜 스폰 직후 즉시 Crash되는 문제 확인.
//   WheelCollider를 완전히 제거하고 Raycast 2개(앞/뒷바퀴)로 대체.
//
// 이동 원리:
//   1. 시작 후 START_DELAY(1초) kinematic 유지 → 지면 스냅
//   2. kinematic 해제 → Raycast 접지 감지
//   3. 전진/후진: _rb.linearVelocity 직접 설정 (forward * targetSpeed)
//   4. 조향: transform.Rotate (Y축)
//   5. 기울기 안정화: Z축 eulerAngles 보정
// ══════════════════════════════════════════════════════════════════
public class Cycle_Move : MoveModule
{
    private Cycle_Control _control;

    [HideInInspector] public int _pedalLevel      = 0;  // 디버그 오버레이용
    [HideInInspector] public int physicsPhasePublic = 0; // 디버그 오버레이용

    private const int   PEDAL_MAX    = 20;    // 최대 단계 (±20)
    private const float KMH_PER_STEP = 10f;   // 단계당 속도 (km/h)
    private const float START_DELAY  = 1.0f;  // 스폰 후 kinematic 대기 시간(초)

    private float _startTimer    = 0f;
    private bool  _ready         = false;
    private int   _prevDeadState = 0;

    // 점프 감지
    private float _jumpTime  = 0f;
    private bool  _bLanding  = false;

    // 수직 이동 (수동 중력)
    private float   _verticalVel  = 0f;
    private float   _groundY      = 0f;
    private float _groundOffset = 0.33f; // WheelCollider 기반 자동 계산 (Start에서 갱신)
    private Vector3 _prevPos;
    private RaycastHit _fHit, _rHit;

    // 하드웨어 Serial 내부 상태
    private int   _breakold   = 0;
    private bool  _handleAct  = false;
    private float _jTime      = 0f;

    // ── Start ─────────────────────────────────────────────────────
    void Start()
    {
        _control = GetComponent<Cycle_Control>();
        Init(_control.moveValue.centerOfMass,
             _control.moveValue.frontTire,
             _control.moveValue.rearTire);

        // WheelCollider 완전 비활성 — Raycast 이동으로 교체
        if (wheelSetting.wheels != null)
            foreach (WheelCollider wc in wheelSetting.wheels)
                if (wc != null) wc.enabled = false;

        // _groundOffset 자동 계산: 뒷바퀴 WheelCollider 위치·반경 기반
        // 공식: pivot.y - groundY = WheelCollider.radius - (wcWorldY - pivot.y)
        //      → 바퀴 바닥(wcWorldY - radius)이 지면에 닿을 때 pivot이 지면에서 떨어진 높이
        if (wheelSetting.rear != null)
        {
            WheelCollider rearWC = wheelSetting.rear;
            Vector3 wcWorld = rearWC.transform.TransformPoint(rearWC.center);
            float pivotToWheelCenterY = wcWorld.y - transform.position.y;
            _groundOffset = rearWC.radius - pivotToWheelCenterY;
            Debug.Log(string.Format("[Cycle_Move] _groundOffset 자동계산: radius={0:F3} wcWorldY={1:F3} pivotY={2:F3} pivotToWheelCenterY={3:F3} → _groundOffset={4:F3}",
                rearWC.radius, wcWorld.y, transform.position.y, pivotToWheelCenterY, _groundOffset));
        }
        else
        {
            Debug.LogWarning("[Cycle_Move] wheelSetting.rear==null — _groundOffset 기본값 0.33f 사용");
        }

        // Rigidbody 완전 kinematic — 차체 Collider와 MeshCollider 충돌 폭발력 원천 차단
        // 이동·중력·지면 스냅을 코드가 직접 제어
        _rb.isKinematic = true;
        _rb.useGravity   = false;

        _control.cycle_Impact = false;
        _pedalLevel = 0;
        pedalSpeed  = 0f;
        handle      = 0f;
        steer       = 0f;

        Debug.Log("[Cycle_Move] Start — Kinematic Raycast 이동 모드 TEST_MODE=" + GameData.TEST_MODE);
    }

    // ── Update: 키 입력 (준비 완료 후만) ─────────────────────────
    void Update()
    {
        if (!GameData.TEST_MODE || !_ready) return;

        if      (Input.GetKeyDown(KeyCode.UpArrow)   || Input.GetKeyDown(KeyCode.W))
            _pedalLevel = Mathf.Min(_pedalLevel + 1,  PEDAL_MAX);
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            _pedalLevel = Mathf.Max(_pedalLevel - 1, -PEDAL_MAX);
    }

    // ── FixedUpdate ───────────────────────────────────────────────
    void FixedUpdate()
    {
        SyncData_Receive();
        if (!_control.cycle_Move) return;

        // Respawn 감지
        if (_control.deadState == 0 && _prevDeadState != 0)
            DoRespawn();
        _prevDeadState = _control.deadState;

        // Crash(1) / Respawn대기(2) 중 이동 완전 중단
        // Cycle_Impact가 위치를 제어하는 동안 Cycle_Move가 바이크를 움직이지 않도록
        if (_control.deadState != 0)
        {
            SyncData_Send_Self();
            SyncData_Send();
            return;
        }

        // ── 시작 대기 (START_DELAY 초 kinematic) ─────────────────
        if (!_ready)
        {
            _startTimer += Time.fixedDeltaTime;
            _rb.isKinematic = true;
            SnapToGround();

            // z축 기울기 초기화
            Vector3 e0 = transform.eulerAngles;
            transform.eulerAngles = new Vector3(e0.x, e0.y, 0f);

            if (_startTimer >= START_DELAY)
            {
                // kinematic 유지 — 물리 충돌 없이 코드로 직접 이동
                _ready              = true;
                physicsPhasePublic  = 2;
                _control.cycle_Impact = true;
                _prevPos = transform.position;
                Debug.Log("[Cycle_Move] 준비 완료 — 주행 가능");
            }
            SyncData_Send_Self();
            SyncData_Send();
            return;
        }

        // ── 주행 ─────────────────────────────────────────────────
        UpdateAngles();

        if (GameData.TEST_MODE) ApplyKeyboard();
        else                    Serial();

        CheckJump();
        RaycastGround();
        ApplyMovement();
        StabilizeLean();
        UpdateSpeed();

        if (_control.gameFinish)
        {
            pedalSpeed  = 0f;
            _pedalLevel = 0;
            _control.cycle_Move = false;
            _control.cycle_AI   = true;
        }

        SyncData_Send_Self();
        SyncData_Send();
    }

    // ── 각도 계산 ─────────────────────────────────────────────────
    void UpdateAngles()
    {
        heightAngle = SlopeAngleCheck(transform.eulerAngles.x);
        lrAngle     = SlopeAngleCheck(transform.eulerAngles.z);
    }

    // ── TEST_MODE 키보드 입력 ─────────────────────────────────────
    void ApplyKeyboard()
    {
        // pedalSpeed는 내부 단위 (km/h 환산 시 _pedalLevel * KMH_PER_STEP)
        pedalSpeed = _pedalLevel * 13f;

        if      (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) handle =  1f;
        else if (Input.GetKey(KeyCode.LeftArrow)  || Input.GetKey(KeyCode.A)) handle = -1f;
        else                                                                   handle =  0f;

        steer = maxValue.MaxSteer * handle;
        drift = Input.GetKey(KeyCode.Space) || CBikeSerial.GetDrift();

        // 점프
        if ((groundF || groundR) && Input.GetKeyDown(KeyCode.Q))
            _rb.AddForce(transform.up * 250f, ForceMode.Acceleration);
    }

    // ── Raycast 접지 감지 ─────────────────────────────────────────
    void RaycastGround()
    {
        float rayLen    = 4.0f;  // 위 2m + 아래 2m (급경사 오르막 감지)
        float upOffset  = 2.0f;  // 레이 시작을 pivot 2m 위에서 → 경사면에 파묻히지 않음
        float fwdOffset = 0.5f;

        // 수평 forward 사용 — 바이크 pitch 각도와 무관하게 정확한 앞뒤 위치 감지
        Vector3 hFwd = transform.forward;
        hFwd.y = 0f;
        if (hFwd.sqrMagnitude > 0.01f) hFwd.Normalize();
        else hFwd = Vector3.forward;

        Vector3 frontOrigin = transform.position + hFwd * fwdOffset + Vector3.up * upOffset;
        Vector3 rearOrigin  = transform.position - hFwd * fwdOffset + Vector3.up * upOffset;

        // upOffset=2m 이므로 레이가 위에서 아래로 내려오면서 바이크 자신의 Collider를 먼저 맞힐 수 있음
        // RaycastAll + 자기 transform 제외 + 거리 순 정렬로 가장 가까운 지형 hit 선택
        groundF = GroundRaycast(frontOrigin, rayLen, out _fHit);
        groundR = GroundRaycast(rearOrigin,  rayLen, out _rHit);

        // 지면 Y 평균 계산
        if      (groundF && groundR) _groundY = (_fHit.point.y + _rHit.point.y) * 0.5f;
        else if (groundF)            _groundY = _fHit.point.y;
        else if (groundR)            _groundY = _rHit.point.y;

        Debug.DrawRay(frontOrigin, Vector3.down * rayLen, groundF ? Color.green : Color.red);
        Debug.DrawRay(rearOrigin,  Vector3.down * rayLen, groundR ? Color.green : Color.red);
    }

    // 자기 자신 제외 + 거리 순으로 가장 가까운 지면 hit 반환
    bool GroundRaycast(Vector3 origin, float maxDist, out RaycastHit result)
    {
        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, maxDist,
                                               ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var h in hits)
        {
            if (h.transform.IsChildOf(transform)) continue;
            result = h;
            return true;
        }
        result = default;
        return false;
    }

    // ── 이동 (Kinematic — transform 직접 제어) ────────────────────
    void ApplyMovement()
    {
        bool grounded = groundF || groundR;

        // 목표 수평 속도 (m/s)
        float targetMS = GameData.TEST_MODE
            ? _pedalLevel * (KMH_PER_STEP / 3.6f)
            : pedalSpeed / 3.6f;

        Vector3 pos = transform.position;

        // ── 수직: 지면 스냅 or 중력 ──────────────────────────────
        if (grounded)
        {
            _verticalVel = 0f;

            // ── 경사(Pitch) 계산: 앞뒤 지형 높이 차로 바이크 기울기 결정 ──────
            // 앞바퀴가 지면 아래에 파묻히는 이유: 바이크 몸체가 경사와 무관하게 수평이면
            // 앞 0.5m 지점에서 지형이 올라와도 바이크 앞부분이 그대로 수평 유지 → 지형에 박힘
            float slopeRad = 0f;
            if (groundF && groundR)
            {
                float wheelbase  = 0.5f * 2f; // fwdOffset × 2 = 1.0m
                float heightDiff = _fHit.point.y - _rHit.point.y; // + = 오르막
                slopeRad = Mathf.Atan2(heightDiff, wheelbase);
                float slopeDeg = Mathf.Clamp(slopeRad * Mathf.Rad2Deg, -60f, 60f);
                // Unity X 음수 = 앞이 올라감 (오르막), 양수 = 앞이 내려감 (내리막)
                Vector3 euler = transform.eulerAngles;
                transform.eulerAngles = new Vector3(-slopeDeg, euler.y, euler.z);
            }

            // 직접 스냅 — 바퀴가 어디서든 지면에 닿도록 (경사 보정 포함)
            pos.y = _groundY + _groundOffset * Mathf.Cos(slopeRad);
        }
        else
        {
            _verticalVel -= 9.8f * Time.fixedDeltaTime; // 중력
            pos.y += _verticalVel * Time.fixedDeltaTime;

            // 코스 이탈 안전망: Y가 -30 이하로 떨어지면 자동 리스폰
            if (pos.y < -30f)
            {
                Debug.Log("[Cycle_Move] 코스 이탈 감지 (Y=" + pos.y.ToString("F1") + ") → 자동 리스폰");
                _control.deadState = 2;
                return;
            }
        }

        // ── 겹침 탈출: 정지 중에도 주변 벽 겹침 감지 후 밀어냄 ───────
        PushOutFromWalls(ref pos);

        // ── 수평: forward 방향 이동 ───────────────────────────────
        if (Mathf.Abs(targetMS) > 0.01f)
        {
            Vector3 fwd = transform.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude > 0.01f) fwd.Normalize();

            float moveStep = targetMS * Time.fixedDeltaTime;
            float moveSign = Mathf.Sign(moveStep);
            Vector3 moveVec = fwd * moveStep;

            // 전방 SphereCast — 수직 벽 감지 및 슬라이드
            moveVec = ObstacleCheck(pos, fwd, moveStep, moveSign, moveVec);

            pos += moveVec;

            // 조향
            float dir = targetMS >= 0f ? 1f : -1f;
            transform.Rotate(Vector3.up, steer * 1.5f * dir * Time.fixedDeltaTime);
        }

        transform.position = pos;
    }

    // ── 자세 안정화 ───────────────────────────────────────────────
    // FreezeRotationX|Z 로 물리적 쓰러짐 차단.
    // lrAngle 갱신만 담당 (Cycle_Impact Crash 조건용)
    void StabilizeLean()
    {
        // UpdateAngles()와 동일한 계산으로 일관성 유지
        lrAngle = SlopeAngleCheck(transform.eulerAngles.z);
    }

    // ── 속도 계산 ─────────────────────────────────────────────────
    private float _debugTimer = 0f;
    void UpdateSpeed()
    {
        // kinematic이므로 위치 변화량으로 속도 계산
        Vector3 delta = transform.position - _prevPos;
        delta.y = 0f;
        realSpeed = delta.magnitude / Time.fixedDeltaTime * 3.6f; // m/s → km/h
        _prevPos  = transform.position;

        // 2초마다 상태 출력 (정상 동작 확인 후 제거)
        _debugTimer += Time.fixedDeltaTime;
        if (_debugTimer >= 2f)
        {
            _debugTimer = 0f;
            Debug.Log(string.Format(
                "[Cycle_Move] spd={0:F1}km/h pedal={1} groundF={2} groundR={3} lr={4:F1}° pos={5}",
                realSpeed, _pedalLevel, groundF, groundR, lrAngle,
                transform.position.ToString("F1")));
        }

        // 타이어 회전 애니메이션
        if (tireSetting.front != null)
            tireSetting.front.Rotate(Vector3.right * realSpeed * Time.fixedDeltaTime * 50f);
        if (tireSetting.rear != null)
            tireSetting.rear.Rotate(Vector3.right * realSpeed * Time.fixedDeltaTime * 50f);
    }

    // ── 장애물 벽 감지 (SphereCast) ──────────────────────────────
    // 반환값: 장애물 처리 후 실제 이동벡터
    static bool IsWallHit(RaycastHit h, Transform self)
    {
        if (h.transform.IsChildOf(self)) return false;
        if (h.collider is TerrainCollider)   return false;
        if (Mathf.Abs(h.normal.y) >= 0.5f)   return false; // 경사면 제외
        return true;
    }

    Vector3 ObstacleCheck(Vector3 pos, Vector3 fwd, float moveStep, float moveSign, Vector3 moveVec)
    {
        const float SPHERE_R  = 0.4f;
        const float OVERLAP_T = 0.5f; // 이 거리 이하 = 겹침으로 판단
        float castDist = Mathf.Abs(moveStep) + SPHERE_R + 0.1f;

        RaycastHit obsHit;
        if (!Physics.SphereCast(pos + Vector3.up * 0.6f, SPHERE_R,
                fwd * moveSign, out obsHit, castDist,
                ~0, QueryTriggerInteraction.Ignore))
            return moveVec;

        if (!IsWallHit(obsHit, transform)) return moveVec;

        Vector3 wallN = new Vector3(obsHit.normal.x, 0f, obsHit.normal.z);
        if (wallN.sqrMagnitude < 0.01f) return moveVec;
        wallN.Normalize();

        if (obsHit.distance < OVERLAP_T)
        {
            // 이미 겹침 — 구 반경 + 여유만큼 밀어내어 완전 탈출
            // Beech_04(반경 0.73m) 같은 큰 나무도 탈출 가능하도록 충분한 거리 확보
            float pushDist = SPHERE_R - obsHit.distance + 0.15f;
            // ApplyMovement()에서 pos에 직접 적용하므로 여기서는 moveVec만 조정
            // 실제 pos 조작은 PushOutFromWalls에서 이미 처리됨
            return Vector3.zero;
        }
        else
        {
            // 접근 중 — 안전거리까지 전진 + 벽 접선 슬라이드
            float toWall = Mathf.Max(0f, obsHit.distance - SPHERE_R);
            Vector3 slide = moveVec - Vector3.Dot(moveVec, wallN) * wallN;
            return fwd * toWall * moveSign + slide;
        }
    }

    // ── 겹침 탈출 (정지 중에도 매 프레임 실행) ───────────────────
    // OverlapSphere로 현재 위치 주변의 벽 콜라이더를 찾아 밀어냄
    void PushOutFromWalls(ref Vector3 pos)
    {
        const float SPHERE_R = 0.5f; // 탐지 반경 (나무 0.73m 감안하여 넉넉히)
        Vector3 sphereCenter = pos + Vector3.up * 0.6f;

        Collider[] cols = Physics.OverlapSphere(sphereCenter, SPHERE_R,
                              ~0, QueryTriggerInteraction.Ignore);
        foreach (var col in cols)
        {
            if (col.transform.IsChildOf(transform)) continue;
            if (col is TerrainCollider) continue;

            // 겹친 콜라이더의 가장 가까운 점 → 바이크 방향으로 밀어냄
            Vector3 closest = col.ClosestPoint(sphereCenter);
            Vector3 pushDir = sphereCenter - closest;
            pushDir.y = 0f;
            if (pushDir.sqrMagnitude < 0.0001f) continue;

            // 지면 법선 필터 (경사면 제외)
            float overlap = SPHERE_R - pushDir.magnitude;
            if (overlap <= 0f) continue;

            pushDir.Normalize();
            pos += pushDir * (overlap + 0.05f);
        }
    }

    // ── 점프 감지 ─────────────────────────────────────────────────
    void CheckJump()
    {
        if (!groundF && !groundR)
        {
            _jumpTime += Time.fixedDeltaTime;
            RaycastHit hit;
            bool b = Physics.Raycast(transform.position, Vector3.down, out hit, 10f);
            float t = 0.4f;
            if (b && hit.distance < 1f) t *= hit.distance;
            if (b && hit.distance > 0.5f && _jumpTime > t)
            {
                if (!_bLanding) CBikeSerial.m_nJump = 1;
                _bLanding = true;
            }
        }
        else
        {
            _jumpTime = 0f;
            if (groundF && groundR && _bLanding)
            {
                _bLanding = false;
                CBikeSerial.m_nJump = 3;
            }
        }
    }

    // ── 지면 스냅 (스폰/리스폰 시) ───────────────────────────────
    void SnapToGround()
    {
        // 200m: 리스폰 위치가 지면보다 100m+ 높을 수 있음 (waypoint 고도 차이)
        RaycastHit[] hits = Physics.RaycastAll(
            transform.position + Vector3.up * 3f, Vector3.down, 200f);

        float bestY = float.MinValue;
        float maxY  = transform.position.y + 0.5f; // 폴리곤 벽 상단 무시

        foreach (var hit in hits)
        {
            if (hit.transform.IsChildOf(transform)) continue;
            if (hit.point.y > maxY)  continue;
            if (hit.point.y > bestY) bestY = hit.point.y;
        }

        if (bestY > float.MinValue)
        {
            Vector3 pos = transform.position;
            pos.y = bestY + _groundOffset;
            transform.position = pos;
            Debug.Log("[Cycle_Move] SnapToGround: bestY=" + bestY.ToString("F2") + " → posY=" + pos.y.ToString("F2"));
        }
    }

    // ── Respawn 재시작 ────────────────────────────────────────────
    void DoRespawn()
    {
        _startTimer  = 0f;
        _ready       = false;
        _pedalLevel  = 0;
        pedalSpeed   = 0f;
        handle       = 0f;
        steer        = 0f;
        _rb.isKinematic = true;
        _rb.useGravity  = false;
        _verticalVel    = 0f;
        _prevPos        = transform.position; // 리스폰 위치로 초기화 → realSpeed 급등 방지
        realSpeed       = 0f;
        _control.cycle_Impact = false;
        physicsPhasePublic    = 0;
        Debug.Log("[Cycle_Move] Respawn → 재시작");
    }

    // ── SyncData ──────────────────────────────────────────────────
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

    // ── 하드웨어 Serial 입력 ──────────────────────────────────────
    void Serial()
    {
        int b1 = 0, b2 = 0;
        if (_control.deadState > 0 && _control.User)
            CBikeSerial.m_nJump = 0;

        CBikeSerial.FrameBike(Time.fixedDeltaTime, heightAngle,
                              maxValue.MaxSteer, (int)(_control.environment),
                              _rb.linearVelocity.magnitude);
        b1 = CBikeSerial.b1;
        b2 = CBikeSerial.b2;

        if (_breakold != (b1 + b2) && (b1 + b2) > 4)
        {
            _breakold = (b1 + b2);
            AudioCtr.Play(AudioCtr.snd_break[0], transform.position);
        }

        steer = CBikeSerial.m_fSteer;
        if (steer == 0) steer = Input.GetAxis("Horizontal") * 35f;

        if (steer == 0)
        {
            if (CBikeSerial.jBtn == "B")
            {
                if (CBikeSerial.GetNewSwitch1(1))
                {
                    if (steer > 0) _jTime = 0f;
                    _jTime += CBikeSerial.jSpeed * Time.deltaTime;
                    steer   = -35f * _jTime;
                    if (steer < -35f) steer = -35f;
                }
                else if (CBikeSerial.GetNewSwitch1(0))
                {
                    if (steer < 0) _jTime = 0f;
                    _jTime += CBikeSerial.jSpeed * Time.deltaTime;
                    steer   = 35f * _jTime;
                    if (steer > 35f) steer = 35f;
                }
                else
                {
                    _jTime = 0f;
                    steer  = Mathf.MoveTowards(steer, 0f, Time.deltaTime);
                }
            }
            else if (CBikeSerial.jBtn == "b")
            {
                if (CBikeSerial.GetNewSwitch2(1))
                {
                    if (steer > 0) _jTime = 0f;
                    _jTime += CBikeSerial.jSpeed * Time.deltaTime;
                    steer   = -35f * _jTime;
                    if (steer < -35f) steer = -35f;
                }
                else if (CBikeSerial.GetNewSwitch2(0))
                {
                    if (steer < 0) _jTime = 0f;
                    _jTime += CBikeSerial.jSpeed * Time.deltaTime;
                    steer   = 35f * _jTime;
                    if (steer > 35f) steer = 35f;
                }
                else
                {
                    _jTime = 0f;
                    steer  = Mathf.MoveTowards(steer, 0f, Time.deltaTime);
                }
            }
        }

        pedalSpeed = 130f * Input.GetAxis("Vertical");
        if (pedalSpeed == 0f)
        {
            if (GameData.autoMode == "1")
            {
                if (CBikeSerial.m_fPedalSpeed == 0f)
                    pedalSpeed = 12.5f * int.Parse(GameData.autoModeSpeed);
                else
                    pedalSpeed = CBikeSerial.m_fPedalSpeed;
            }
            else pedalSpeed = CBikeSerial.m_fPedalSpeed;
        }

        drift = (b1 != 0 && b2 == 0) || (b1 == 0 && b2 != 0) || Input.GetKey(KeyCode.Space);
        if (!drift) resistance = CBikeSerial.m_fBrakeTorque;

        float kHandle = Input.GetAxis("Horizontal");
        if (kHandle == 0)
        {
            if (_handleAct)
            {
                handle = drift
                    ? -CBikeSerial.GetHandle2() * 0.075f
                    :  -CBikeSerial.GetHandle2() * 0.04f;
            }
            else
            {
                if (handle != CBikeSerial.GetHandle2()) _handleAct = true;
            }
        }
        else handle = kHandle;

        if (realSpeed < 2f)
        {
            if (CBikeSerial.GetNewButton(2) || Input.GetKeyDown(KeyCode.Q))
                _control.deadState = 2;
        }
    }
}

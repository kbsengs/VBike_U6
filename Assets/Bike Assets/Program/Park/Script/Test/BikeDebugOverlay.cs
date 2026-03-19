using UnityEngine;

/// <summary>
/// 바이크 디버그 오버레이
/// 사용법: Demo03 씬의 빈 GameObject에 이 컴포넌트 추가
/// </summary>
public class BikeDebugOverlay : MonoBehaviour
{
    private Cycle_Control _ctrl;
    private Cycle_Move    _move;
    private Rigidbody     _rb;

    private float  _scanTimer = 0f;
    private string _surfaceName   = "-";
    private string _surfaceNormal = "-";
    private string _surfaceTag    = "-";
    private float  _surfaceDist   = -1f;
    private string _nearbyInfo    = "-";

    void Start()
    {
        Debug.Log("[BikeDebugOverlay] Start() — 화면 오버레이 활성화됨");
    }

    void Update()
    {
        if (_ctrl == null)
        {
            _ctrl = FindObjectOfType<Cycle_Control>();
            if (_ctrl != null)
            {
                _move = _ctrl.GetComponent<Cycle_Move>();
                _rb   = _ctrl.GetComponent<Rigidbody>();
                Debug.Log("[BikeDebugOverlay] 바이크 발견: " + _ctrl.name);
            }
            return;
        }

        _scanTimer += Time.deltaTime;
        if (_scanTimer >= 2.0f)
        {
            _scanTimer = 0f;
            ScanTerrain();
            LogToConsole();
        }
    }

    void ScanTerrain()
    {
        if (_ctrl == null) return;
        Vector3 pos = _ctrl.transform.position;

        RaycastHit hit;
        if (Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, out hit, 30f))
        {
            _surfaceName   = hit.collider.name;
            _surfaceNormal = hit.normal.ToString("F2");
            _surfaceTag    = hit.collider.tag;
            _surfaceDist   = hit.distance;
        }
        else
        {
            _surfaceName = "없음"; _surfaceNormal = "-"; _surfaceTag = "-"; _surfaceDist = -1f;
        }

        Collider[] nearby = Physics.OverlapSphere(pos, 2f);
        var sb = new System.Text.StringBuilder();
        foreach (var c in nearby)
        {
            if (c.transform.IsChildOf(_ctrl.transform)) continue;
            sb.Append(c.name).Append("[").Append(c.tag).Append("] ");
        }
        _nearbyInfo = sb.Length > 0 ? sb.ToString() : "없음";
    }

    void LogToConsole()
    {
        if (_ctrl == null) return;
        var mv = _ctrl.moveValue;
        int lv = (_move != null) ? _move._pedalLevel        : -1;
        int ph = (_move != null) ? _move.physicsPhasePublic : -1;
        Vector3 raw = (_rb != null) ? _rb.linearVelocity : Vector3.zero;

        Debug.Log(string.Format(
            "[BikeDebug] phase={0} speed={1:F1}km/h pedal={2} rawVel={3}\n" +
            "  lr={4:F1}° height={5:F1}° groundF={6} groundR={7} dead={8} impact={9}\n" +
            "  지면={10}[{11}] 법선={12} 거리={13:F2}m\n" +
            "  주변={14}",
            ph, mv.realSpeed, lv, raw.ToString("F2"),
            mv.lrAngle, mv.heightAngle, mv.groundF, mv.groundR,
            _ctrl.deadState, _ctrl.cycle_Impact,
            _surfaceName, _surfaceTag, _surfaceNormal, _surfaceDist,
            _nearbyInfo));
    }

    void OnGUI()
    {
        // 반투명 검정 배경
        GUI.color = new Color(0, 0, 0, 0.75f);
        GUI.DrawTexture(new Rect(5, 5, 455, 345), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float x = 12f;
        float y = 10f;
        float w = 440f;
        float h = 18f;

        if (_ctrl == null)
        {
            GUI.Label(new Rect(x, y, w, h), "BikeDebugOverlay: 바이크 탐색 중...");
            return;
        }

        var mv = _ctrl.moveValue;
        int lv = (_move != null) ? _move._pedalLevel        : -1;
        int ph = (_move != null) ? _move.physicsPhasePublic : -1;
        Vector3 raw = (_rb != null) ? _rb.linearVelocity : Vector3.zero;

        // 헤더
        GUI.color = Color.cyan;
        GUI.Label(new Rect(x, y, w, h), "=== 바이크 디버그 오버레이==="); y += h + 2;

        GUI.color = Color.white;
        GUI.Label(new Rect(x, y, w, h), string.Format(
            "Phase: {0}   deadState: {1}   cycle_Impact: {2}",
            PhaseLabel(ph), _ctrl.deadState, _ctrl.cycle_Impact)); y += h + 2;

        // 속도 — 정지상태인데 속도 있으면 빨간색
        GUI.color = (lv == 0 && mv.realSpeed > 0.5f) ? Color.red : Color.white;
        GUI.Label(new Rect(x, y, w, h), string.Format(
            "속도: {0:F1} km/h   페달단계: {1} / 20   (↑+1 ↓-1)",
            mv.realSpeed, lv)); y += h + 2;

        GUI.color = Color.white;
        GUI.Label(new Rect(x, y, w, h), string.Format(
            "rawVelocity: {0}", raw.ToString("F2"))); y += h + 2;

        // 기울기
        GUI.color = (Mathf.Abs(mv.lrAngle) > 40f) ? Color.red :
                    (Mathf.Abs(mv.lrAngle) > 25f) ? Color.yellow : Color.white;
        GUI.Label(new Rect(x, y, w, h), string.Format(
            "좌우기울기 lrAngle:     {0:F1}°   (50° 초과→추락)", mv.lrAngle)); y += h + 2;

        GUI.color = Color.white;
        GUI.Label(new Rect(x, y, w, h), string.Format(
            "앞뒤기울기 heightAngle: {0:F1}°", mv.heightAngle)); y += h + 2;

        GUI.Label(new Rect(x, y, w, h), string.Format(
            "앞바퀴 접지: {0}   뒷바퀴 접지: {1}", mv.groundF, mv.groundR)); y += h + 2;

        // 지형
        GUI.color = Color.yellow;
        GUI.Label(new Rect(x, y, w, h), "── 지형 ──────────────────────────────"); y += h + 2;
        GUI.color = Color.white;
        GUI.Label(new Rect(x, y, w, h), string.Format(
            "오브젝트: {0}   태그: {1}", _surfaceName, _surfaceTag)); y += h + 2;
        GUI.Label(new Rect(x, y, w, h), string.Format(
            "법선: {0}   거리: {1:F2}m", _surfaceNormal, _surfaceDist)); y += h + 2;

        // 주변
        GUI.color = Color.yellow;
        GUI.Label(new Rect(x, y, w, h), "── 주변 콜라이더 (반경 2m) ─────────────"); y += h + 2;
        GUI.color = Color.white;
        GUI.Label(new Rect(x, y, w, h * 2), _nearbyInfo); y += h * 2 + 2;

        // 조작 안내
        GUI.color = Color.gray;
        GUI.Label(new Rect(x, y, w, h), "[↑/W] +1km/h  [↓/S] -1km/h  [←→] 조향  [Q] 점프");
    }

    static string PhaseLabel(int ph)
    {
        switch (ph)
        {
            case 0: return "0-대기(3초)";
            case 1: return "1-접지대기";
            case 2: return "2-주행가능";
            default: return ph.ToString();
        }
    }
}

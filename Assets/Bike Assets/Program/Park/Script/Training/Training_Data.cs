using UnityEngine;
using System.Collections;

public class Training_Data : MonoBehaviour {

    public Cycle_Control MyCharacter;
    public Training_GUI _GUI;
    public float remainTime;

    public void CreatBike()
    {
        // Unity6 Migration: _Waypoint가 없으면 Demo03 평탄 지점에서 스폰
        Vector3 spawnPos = new Vector3(201f, 10f, 163f);
        Quaternion spawnRot = Quaternion.identity;
        GameObject waypointObj = GameObject.Find("_Waypoint");
        if (waypointObj != null)
        {
            WaypointDefine wd = waypointObj.GetComponent<WaypointDefine>();
            if (wd != null && wd.allways != null && wd.allways.Length > 0)
            {
                // 훈련 모드는 항상 waypoint[0](출발선)에서 시작
                // Random.Range 사용 시 경사면에 스폰되어 Phase 1 불안정 발생
                int pos = 0;
                spawnPos = wd.allways[pos].position;
                spawnRot = wd.allways[pos].rotation;
            }
        }
        else
        {
            Debug.LogWarning("Training_Data.CreatBike: '_Waypoint' not found in scene. Spawning at default position.");
        }
        GameObject obj = Instantiate((GameObject)Resources.Load("Prefeb/Cycle " + GameData.number), spawnPos, spawnRot) as GameObject;
        if (obj == null)
        {
            Debug.LogError("Training_Data.CreatBike: Failed to load 'Prefeb/Cycle " + GameData.number + "'. Check Resources folder.");
            return;
        }
        MyCharacter = obj.GetComponent<Cycle_Control>();
        MyCharacter.wayName = "_Waypoint";
        MyCharacter.cycle_Move = true;
        MyCharacter.cycle_Impact = true;
        MyCharacter.User = true;
        MyCharacter.gameStart = true;
        MyCharacter.minimapArrow.arrow.localScale = GameData.ArrowSize_MTB[0];
        CycleCam cam = FindObjectOfType(typeof(CycleCam)) as CycleCam;
        if (cam != null) cam.SetTarget(MyCharacter.cameraTarget, 1);

        // 훈련 모드 MinimapCam 초기화
        // 바이크 스폰 위치를 중심으로 위에서 아래를 내려다보는 위치로 설정
        SetupMinimapCam(spawnPos);

        Destroy(GameObject.Find("_Loading"));
    }

    void SetupMinimapCam(Vector3 trackCenter)
    {
        GameObject minimapCamObj = GameObject.Find("MinimapCam");
        if (minimapCamObj == null)
        {
            Debug.LogWarning("[Training_Data] MinimapCam GameObject를 찾을 수 없습니다.");
            return;
        }
        Camera minimapCam = minimapCamObj.GetComponent<Camera>();
        if (minimapCam == null) return;

        // 현재 씬에 이미 설정된 위치가 적절하면 그대로 사용;
        // 카메라가 원점(0,0,0) 근처에 있을 경우만 스폰 위치 기반으로 재설정
        Vector3 curPos = minimapCamObj.transform.position;
        bool isAtOrigin = curPos.magnitude < 5f;
        if (isAtOrigin)
        {
            // 스폰 위치 위 200m, 직하 방향으로 설정
            minimapCamObj.transform.position = new Vector3(trackCenter.x, 200f, trackCenter.z);
            minimapCamObj.transform.eulerAngles = new Vector3(90f, 0f, 0f);
            minimapCam.orthographic = true;
            minimapCam.orthographicSize = 300f;
            Debug.Log("[Training_Data] MinimapCam 위치 재설정: " + minimapCamObj.transform.position);
        }
        else
        {
            Debug.Log("[Training_Data] MinimapCam 기존 위치 유지: " + curPos
                      + " orthoSize=" + minimapCam.orthographicSize);
        }
    }

    public void CreatGUI()
    {
        GameObject obj = Instantiate((GameObject)Resources.Load("_GameGUI")) as GameObject;
        _GUI = obj.GetComponent<Training_GUI>();
        obj.name = "_GUI";
    }
}

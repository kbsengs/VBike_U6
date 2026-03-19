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
        Destroy(GameObject.Find("_Loading"));
    }

    public void CreatGUI()
    {
        GameObject obj = Instantiate((GameObject)Resources.Load("_GameGUI")) as GameObject;
        _GUI = obj.GetComponent<Training_GUI>();
        obj.name = "_GUI";
    }
}

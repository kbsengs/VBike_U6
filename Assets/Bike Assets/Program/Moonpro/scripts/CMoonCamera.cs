using UnityEngine;
using System.Collections;

public class CMoonCamera : MonoBehaviour 
{
    RankData rankData;
    bool ok = false;

    bool change = false;
    int m_nFirst, m_nNewFirst;
    float time1, time2;

    float starttime = 0;
    float savez;

    GameObject m_oCamera;

    int dir = 0;
    float savepoint;

    BMX_Server_Data _Data;
    
    float changetime = 1.0f, changetime2 = 4.0f;
    float nexttime;

    public Vector3 m_vStartPos;
    public Vector3 m_vIntroPos;
    public Vector3 m_vViewPos;
    int camerastate;

    public Vector3 m_vFinishPos;
    Vector3 m_vGoalPos1, m_vGoalPos2;

    bool bLast = true;

    public void Init()
    {
        bLast = true;
        ok = false;
        starttime = 0;
        nexttime = 10;
        camerastate = 0;
        m_vGoalPos1 = Vector3.zero;
        dir = 0;
        m_nNewFirst = 5;
    }

//    void OnGUI()
//    {
//        GUI.color = Color.black;
//        string str;
//        str = camerastate.ToString();
//        GUI.Label(new Rect(10, 800, 100, 20), str);
//        str = starttime.ToString("f1");
//        GUI.Label(new Rect(10, 840, 100, 20), str);
//        GUI.color = Color.white;
//    }

    public void InitGame()
    {
        m_oCamera = GameObject.Find("Eye");
        if (m_oCamera == null) return;

        switch (GameData.BMXMap)
        {
            case 1:
                m_vIntroPos = new Vector3(-50, 0, 50);
                m_vViewPos = new Vector3(-20, 5, 0);
                break;
            case 2:
            case 3:
                m_vIntroPos = new Vector3(50, 0, 50);
                m_vViewPos = new Vector3(0, 4, 20);
                break;
        }
        //if (GameObject.Find(GameData.bmxStart[GameData.BMXMap]))
        m_vStartPos = GameObject.Find(GameData.bmxStart[GameData.BMXMap]).transform.position;

        m_vFinishPos = GameObject.Find("_Finish" + GameData.BMXMap.ToString()).transform.position;

        _Data = GameObject.Find("_Manager").GetComponent<BMX_Server_Data>();
        if (_Data == null) return;

        GameObject o = GameObject.Find(GameData.bmxWay[GameData.BMXMap]);
        if (o == null) return;

        rankData = o.GetComponent<RankData>();
        ok = true;
    }

    void Update()
    {
        if (!ok)
        {
            return;
        }

        if (!rankData.initComplete)
        {
            CameraIntro();
            return;
        }
        else CameraGame();
    }

    void CameraIntro()
    {
        switch(_Data._ServerState) 
        {
            case 0: CameraIntro1(); break;
            case 1: CameraIntro1(); break;
            case 2: CameraIntro2(); break;
        }
    }

    void CameraIntro1()
    {
        m_oCamera.transform.position = new Vector3(m_vStartPos.x + m_vIntroPos.x, m_vStartPos.y + m_vIntroPos.y, m_vStartPos.z + m_vIntroPos.z);
        m_oCamera.transform.LookAt(m_vStartPos);
    }

    void CameraIntro2()
    {
        float t = GameData.SERVER_READY_TIME - 1;

        Vector3 end = m_vViewPos;
        if (_Data._ServerTime > 1)
        {
            float time = (_Data._ServerTime - 1) / t;

            end = (1 - time) * m_vViewPos + m_vIntroPos * time;

            m_oCamera.transform.position = new Vector3(m_vStartPos.x + end.x, m_vStartPos.y + end.y, m_vStartPos.z + end.z);
            m_oCamera.transform.LookAt(m_vStartPos);
        }
        else
        {
            m_oCamera.transform.position = new Vector3(m_vStartPos.x + end.x, m_vStartPos.y + end.y, m_vStartPos.z + end.z);
            m_oCamera.transform.LookAt(m_vStartPos);
        }
    }

    void CameraIntro3()
    {
        Vector3 end = m_vViewPos;
        m_oCamera.transform.position = new Vector3(m_vStartPos.x + end.x, m_vStartPos.y + end.y, m_vStartPos.z + end.z);
        m_oCamera.transform.LookAt(m_vStartPos);
    }

    void CameraGame()
    {
        starttime += Time.deltaTime;
        CameraNormal();
    }

    Vector3 AllCyclePos()
    {
        Vector3 v = Vector3.zero;

        for (int i = 0; i < rankData.cycles.Length; i++)
        {
            v += rankData.ranklist[i].transform.position;
        }
        v /= (float)(rankData.cycles.Length);

        return v;
    }

    void CameraNormal()
    {
        Vector3 v;
        Vector3 v1, v2;

        if (starttime < changetime)
        {
            v = m_vStartPos * (changetime - starttime) + starttime * rankData.cycles[m_nNewFirst].transform.position;
            v = v / changetime;
            m_nFirst = m_nNewFirst;
            v1 = v;
            v2 = v;
        }
        else
        {
            // 1등 바라봄
            if (!change)
            {
                if (m_nFirst != rankData.ranklist[0].MyNumber)
                {
                    change = true;
                    time1 = 0;
                    m_nNewFirst = rankData.ranklist[0].MyNumber;
                }
            }

            switch (GameData.BMXMap)
            {
                case 1: if (rankData.ranklist[0].transform.position.z > m_vFinishPos.z - 8) bLast = false; break;
                case 2:
                    if (rankData.ranklist[0].transform.position.x > 524 && rankData.ranklist[0].transform.position.z > 300) bLast = false; 
                    break;
                case 3: if (rankData.ranklist[0].transform.position.x > m_vFinishPos.x - 15) bLast = false; break;
            }
            if (starttime > nexttime && camerastate == 0 && bLast)
            {
                camerastate = 1;
                time2 = 0;
            }

            // 1등 변경
            if (change)
            {
                time1 += Time.deltaTime;

                v = rankData.cycles[m_nFirst].transform.position * (changetime - time1) + time1 * rankData.cycles[m_nNewFirst].transform.position;
                v = v / changetime;

                if (time1 > changetime)
                {
                    change = false;
                    nexttime = starttime + 4.0f;
                    m_nFirst = m_nNewFirst;
                }
            }
            // 1등 보기
            else
            {
                v = rankData.ranklist[0].transform.position;
                m_nFirst = rankData.ranklist[0].MyNumber;
            }
            float gg = 0.9f;
            //v = v * gg + AllCyclePos() * (1 - gg);

            if (camerastate == 0)
            {
                v1 = v;
                v2 = v;
            }
            // 1등에서 평균으로 바라봄
            else if (camerastate == 1)
            {
                v2 = v;

                time2 += Time.deltaTime;
                v = v2 * (changetime2 - time2) + time2 * (v2 * 0.6f + AllCyclePos() * 0.4f);
                v = v / changetime2;

                if (time2 > changetime2)
                {
                    camerastate = 2;
                    time2 = 0;
                }
                v1 = v;
            }
            // 평균에서 1등으로 바라봄
            else 
            {
                v2 = v;

                time2 += Time.deltaTime;

                v = (v2 * 0.6f + AllCyclePos() * 0.4f) * (changetime2 - time2) + time2 * v2;
                v = v / changetime2;

                if (time2 > changetime2)
                {
                    camerastate = 0;
                    nexttime = starttime + 5.0f;
                }
                v1 = v;
            }
        }

        // 처음시작시 중간에서 z축으로 변경
        float tt = starttime;
        if (starttime > 5.0f) tt = 5.0f;
        Vector3 last;

        //float nz;
        last.y = m_vViewPos.y;
        if (GameData.BMXMap > 1)
        {
            last.x = tt * 2.0f;
            last.z = m_vViewPos.z - tt * m_vViewPos.z / 5.0f;
            /*
            // 트랙코너에서 10미터 전까지만 이동
            // 지형에 파묻히는거 피함
            float gap = 0.0f;
            switch (GameData.BMXMap)
            {
                case 2: gap = 5.0f; break;
                case 3: gap = 0.0f; break;
            }

            int n = rankData.rankstep[m_nNewFirst] + 1;            
            if (n >= rankData._RankPointDefine.rankpoint.Length) n = 0;
            if (dir == 0)
            {
                if (v2.z > rankData._WaypointDefine.allways[rankData._RankPointDefine.rankpoint[n]].position.z - gap)
                {
                    dir = 2;
                    savepoint = v2.z;
                }
            }
            else if (dir == 1)
            {
                if (v2.z < rankData._WaypointDefine.allways[rankData._RankPointDefine.rankpoint[n]].position.z + gap)
                {
                    dir = 3;
                    savepoint = v2.z;
                }
            }
            else if (dir == 2)
            {
                if (v2.z < savepoint) dir = 1;
            }
            else
            {
                if (v2.z > savepoint) dir = 0;
            }

            if (dir > 1) nz = savez;
            else
            {
                savez = v2.z + last.z;
                nz = savez;
            }
            */
            //savez = v2.z + last.z;

            if (GameData.BMXMap == 2)
            {
                if (!bLast)
                {
                    float zz = v1.z - 350;
                    zz *= 0.05f;
                    if (zz > 5.0f) zz = 5.0f;
                    else if (zz < 0) zz = 0.0f;

                    last.x = 10 - zz;
                    last.z = -zz;
                }
                else if (starttime > 10)
                {
                    float zz = v1.z - 300;
                    zz *= 0.2f;
                    if (v1.z < 300) last.z = -zz;
                }
            }
            else
            {
                float zz = 240 - v1.z;
                zz *= 0.15f;
                if (v1.z < 240) last.z = zz;
            }

            if (rankData.ranklist[0].gameFinish && m_vGoalPos1.z == 0 && !change) 
            {
                m_vGoalPos1 = v1;
                m_vGoalPos2 = v2 + last;
            }
        }
        else
        {
            last.z = tt * 2.0f;

            if(v1.x > 340 && starttime > 30) last.x = m_vViewPos.x - tt * m_vViewPos.x / 5.0f - (v1.x - 340) * 0.5f;
            else last.x = m_vViewPos.x - tt * m_vViewPos.x / 5.0f;

            if (rankData.ranklist[0].gameFinish && m_vGoalPos1.z == 0 && !change)
            {
                m_vGoalPos1 = v1;
                m_vGoalPos2 = v2 + last;
            }
        }
        v2 = v2 + last;

        if (rankData.ranklist[0].gameFinish && !change)
        {
            m_oCamera.transform.position = m_vGoalPos2;
            m_oCamera.transform.LookAt(m_vGoalPos1);
        }
        else
        {
            m_oCamera.transform.position = v2;
            m_oCamera.transform.LookAt(v1);
        }
    }
}

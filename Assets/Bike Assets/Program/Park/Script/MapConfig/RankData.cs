using UnityEngine;
using System.Collections;

public class RankData : MonoBehaviour {

    public Cycle_Control[] cycles;
    private int[] rank;
    public Cycle_Control[] ranklist;
    public int[] rankstep;
    public int[] beforestep;
    public float[] distance;
    public int[] loopCount;
    [System.NonSerialized]
    public RankPointDefine _RankPointDefine;
    [System.NonSerialized]
    public WaypointDefine _WaypointDefine;

    public bool initComplete = false;
    Vector3 m_vFinishPos;

	public void Init () {
        _RankPointDefine = GetComponent<RankPointDefine>();
        _WaypointDefine = GetComponent<WaypointDefine>();

		rank = null;
		ranklist = null;
		rankstep = null;
		beforestep = null;
		distance = null;
		loopCount = null;

        rank = new int[cycles.Length];
        ranklist = new Cycle_Control[cycles.Length];
        rankstep = new int[cycles.Length];
        beforestep = new int[cycles.Length];
        distance = new float[cycles.Length]; 
        loopCount = new int[cycles.Length];
        initComplete = true;

        if (GameData.GameState == 0)
            m_vFinishPos = GameObject.Find("_Finish" + GameData.BMXMap.ToString()).transform.position;
	}

	public void ClearData()
	{
		rank = new int[cycles.Length];
		ranklist = new Cycle_Control[cycles.Length];
		rankstep = new int[cycles.Length];
		beforestep = new int[cycles.Length];
		distance = new float[cycles.Length]; 
		loopCount = new int[cycles.Length];
	}

    void FixedUpdate()
    {
        if (initComplete)
        {
            FindRankStep();
        }
    }
	
    void RankNext(int i)
    {
        beforestep[i] = rankstep[i];
        rankstep[i]++;
        if (rankstep[i] >= _RankPointDefine.rankpoint.Length)
        {
            rankstep[i] = 0;
            loopCount[i]++;
        }
    }

	void FindRankStep () 
    {
        if (GameData.GameState == 1)
        {
            for (int i = 0; i < cycles.Length; i++)
            {
                float angle = Angle_Check(cycles[i].transform, _WaypointDefine.allways[_RankPointDefine.rankpoint[rankstep[i]]]);
               
                if (distance[i] < 50)// || (angle > -90 && angle < 90))
                //if (angle > -70 && angle < 70)
                {
                    beforestep[i] = rankstep[i];
                    rankstep[i]++;
                    if (rankstep[i] >= _RankPointDefine.rankpoint.Length)
                    {
                        rankstep[i] = 0;
                        loopCount[i]++;
                    }
                }
                if (beforestep[i] != rankstep[i])
                {
                    float angleBefore = Angle_Check(cycles[i].transform, _WaypointDefine.allways[_RankPointDefine.rankpoint[beforestep[i]]]);
                    if (angleBefore < -70 && angleBefore > 70)
                    {
                        rankstep[i]--;
                        if (rankstep[i] < 0) rankstep[i] = _RankPointDefine.rankpoint.Length - 1;
                        beforestep[i] = rankstep[i];
                        if (beforestep[i] < 0) beforestep[i] = _RankPointDefine.rankpoint.Length - 1;
                    }
                }
                Vector2 a = new Vector2(cycles[i].transform.position.x, cycles[i].transform.position.z);
                Vector2 b = new Vector2(_WaypointDefine.allways[_RankPointDefine.rankpoint[rankstep[i]]].position.x, _WaypointDefine.allways[_RankPointDefine.rankpoint[rankstep[i]]].position.z);
                distance[i] = Vector2.Distance(b, a);
            }
        }


        if (GameData.GameState == 0)
        {
            for (int i = 0; i < cycles.Length; i++)
            {
                int n = rankstep[i]+1;
                if (n >= _RankPointDefine.rankpoint.Length) n = 0;
                Vector3 next = _WaypointDefine.allways[_RankPointDefine.rankpoint[n]].position - _WaypointDefine.allways[_RankPointDefine.rankpoint[rankstep[i]]].position;
            
                float gap = 0.7f;
                if (_WaypointDefine.allways[_RankPointDefine.rankpoint[n]].forward.z > gap)
                {
                    if (cycles[i].transform.position.z > _WaypointDefine.allways[_RankPointDefine.rankpoint[n]].position.z) RankNext(i);
                }
                else if (_WaypointDefine.allways[_RankPointDefine.rankpoint[n]].forward.z < -gap)
                {
                    if (cycles[i].transform.position.z < _WaypointDefine.allways[_RankPointDefine.rankpoint[n]].position.z) RankNext(i);
                }
                else if (_WaypointDefine.allways[_RankPointDefine.rankpoint[n]].forward.x > gap)
                {
                    if (cycles[i].transform.position.x > _WaypointDefine.allways[_RankPointDefine.rankpoint[n]].position.x) RankNext(i);
                }
                else 
                {
                    if (cycles[i].transform.position.x < _WaypointDefine.allways[_RankPointDefine.rankpoint[n]].position.x) RankNext(i);
                }
                n = rankstep[i]+1;
                if (n >= _RankPointDefine.rankpoint.Length) n = 0;

                Vector2 a = new Vector2(cycles[i].transform.position.x, cycles[i].transform.position.z);
                Vector2 b = new Vector2(_WaypointDefine.allways[_RankPointDefine.rankpoint[n]].position.x, _WaypointDefine.allways[_RankPointDefine.rankpoint[n]].position.z);
                distance[i] = Vector2.Distance(a, b);

                int goal = _RankPointDefine.rankpoint.Length - 2;
                if (GameData.BMXMap == 3) goal--;

                if (rankstep[i] >= goal - 1 && !cycles[i].gameFinish)
                {
                    float cyclegap = 1.05f;
                    bool ok = false;
                    //Vector3 p = cycles[i].transform.position;
                    if (GameData.BMXMap == 1)
                    {                        
                        if (cycles[i].transform.position.x > m_vFinishPos.x - cyclegap)
                        {
                            ok = true;
                            //cycles[i].transform.position = new Vector3(m_vFinishPos.x - cyclegap, p.y, p.z);
                        }
                    }
                    else
                    {
                        if (cycles[i].transform.position.z < m_vFinishPos.z + cyclegap)
                        {
                            ok = true;
                            //cycles[i].transform.position = new Vector3(p.x, p.y, m_vFinishPos.z + cyclegap);
                        }
                    }
                    if(ok)
                    {
                            if (cycles[i].rank < 4 && GameData.isServer) // Unity6 Migration: Network.peerType removed
                            {
                                BMX_Server_Data data = FindObjectOfType(typeof(BMX_Server_Data)) as BMX_Server_Data;
                                data.ReplaySave(cycles[i].rank - 1);
                            }
                            cycles[i].gameFinish = true;
                            cycles[i].bRecordTime = false;
                            AudioCtr.Play(AudioCtr.snd_flash);
                    }
                }
            }
        }

        /*
        for (int rank_A = 0; rank_A < cycles.Length; rank_A++)
        {
            int startRank = 1;
            for (int rank_B = 0; rank_B < cycles.Length; rank_B++)
            {
                if (loopCount[rank_A] < loopCount[rank_B])
                {
                    startRank++;
                }
                else if (loopCount[rank_A] == loopCount[rank_B]) 
                {
                    if (rankstep[rank_A] < rankstep[rank_B])
                    {
                        startRank++;
                    }
                    else if (rankstep[rank_A] == rankstep[rank_B])
                    {
                        //int target = _RankPointDefine.rankpoint[rankstep[rank_A]];

                        //Vector2 fromA = new Vector2(cycles[rank_A].transform.position.x, cycles[rank_A].transform.position.y);
                        //Vector2 fromB = new Vector2(cycles[rank_B].transform.position.x, cycles[rank_B].transform.position.y);

                        //Vector2 to = new Vector2(_WaypointDefine.allways[target].position.x, _WaypointDefine.allways[target].position.z);

                        //float disA = Vector2.Distance(to, fromA);
                        //float disB = Vector2.Distance(to, fromB);

                        if (distance[rank_A] > distance[rank_B])
                            startRank++;
                        else if (distance[rank_A] == distance[rank_B] && rank_A > rank_B) startRank++;
                    }
                }
            }
            if (!cycles[rank_A].gameFinish)
            {
                rank[rank_A] = startRank;
                cycles[rank_A].rank = startRank;
                ranklist[rank[rank_A] - 1] = cycles[rank_A];
            }
        }
        */
        for (int rank_A = 0; rank_A < cycles.Length; rank_A++)
        {
            int startRank = 1;
            for (int rank_B = 0; rank_B < cycles.Length; rank_B++)
            {
                if (rank_A == rank_B) continue;
                if (cycles[rank_A].gameFinish && !cycles[rank_B].gameFinish) continue;
                else if (cycles[rank_A].gameFinish && cycles[rank_B].gameFinish)
                {
                    if (cycles[rank_A].rank > cycles[rank_B].rank) startRank++;
                    continue;
                }
                else if (!cycles[rank_A].gameFinish && cycles[rank_B].gameFinish)
                {
                    startRank++;
                    continue;
                }

                if (loopCount[rank_A] < loopCount[rank_B])
                {
                    startRank++;
                }
                else if (loopCount[rank_A] == loopCount[rank_B])
                {
                    if (rankstep[rank_A] < rankstep[rank_B])
                    {
                        startRank++;
                    }
                    else if (rankstep[rank_A] == rankstep[rank_B])
                    {
                        if (distance[rank_A] > distance[rank_B]) startRank++;
                        else if (distance[rank_A] == distance[rank_B] && rank_A > rank_B) startRank++;
                    }
                }
            }
            rank[rank_A] = startRank;
            cycles[rank_A].rank = startRank;
            if (!cycles[rank_A].gameStart)
                cycles[rank_A].rank = cycles.Length;
            ranklist[rank[rank_A] - 1] = cycles[rank_A];
        }
    }

    float Angle_Check(Transform target, Transform myTransform)
    {
        //Vector2 target_V2 = new Vector2(target.position.x, target.position.z);
        //Vector2 my_V2 = new Vector2(myTransform.position.x, myTransform.position.z);
        Vector3 my_Dir = myTransform.TransformDirection(Vector3.forward);
        Vector3 target_Dir = target.position - myTransform.position;
        target_Dir = target_Dir.normalized;

        float x1 = my_Dir.x;
        float y1 = my_Dir.z;

        float x2 = target_Dir.x;
        float y2 = target_Dir.z;

        float angle = Mathf.Atan2(x2 * y1 - y2 * x1, x2 * x1 + y1 * y2) * Mathf.Rad2Deg;
        return angle;
    }
}

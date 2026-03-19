using UnityEngine;
using System.Collections;

public class InGameGUI : MonoBehaviour
{
    public Transform ranking;//��ŷ ���� GUI�� �����ϴ� ������Ʈ
    public Transform total_time; //��ü �ð�
    public Transform km;
    public Transform time;
    public Transform game_over;
    public Transform _321go;
    public Transform count_down;
    public Transform check_point;
    public Transform single_rank;
    public Transform multi_rank;
    public Transform minimap;
    public Transform flag;
    public Transform mud;
    public Transform water;
    public Transform count5;
    public Transform text_wait;
    public Transform text_readywait;
    public Transform remain_time;

    private bool count5_start;
    private float count5Time;
    private int count5Value;

    private bool result;

    #region ȿ��
    private bool mudAct = false;
    private bool waterAct = false;

    public Animation[] mudAni;
    public Animation[] waterAni;

    private float effectTime;
    #endregion

    #region ī��Ʈ �ٿ�
    private Texture[] countdown = new Texture[4];
    private bool startCount = false;
    private float countTime = 0;
    private Texture[] count = new Texture[11];
    private Renderer plane007;
    #endregion

    #region üũ����Ʈ
    private bool checkpoint_Act = false;
    private float checkpointTime = 0;
    #endregion

    #region ��ŷ ����
    private Texture[] rankTexture = new Texture[GameData.MAX_PLAYER]; //������ ������
    private Transform[] rank = new Transform[GameData.MAX_PLAYER]; //������ ������
    private float[] rankPos = new float[GameData.MAX_PLAYER]; //0 ~ 11 ������ 1 ~ 12�� ��ġ�� ��Ÿ��
    public int[] currentRank = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }; //���� ���
    private Renderer myRank; //���� ���� ���� �׸�
    private int myCurrentRank; //���� ���� ��ŷ
    #endregion

    #region �Ÿ� �� Į�θ�
    private Texture[] s_num = new Texture[10]; //Į�θ� �Ÿ� ��ü �ð� �ؽ�
    private Renderer[] calorie = new Renderer[4]; //Į�θ�
    private Renderer[] distance = new Renderer[4]; //�Ÿ�
    //[System.NonSerialized]
    public float myDistance;
    //[System.NonSerialized]
    public float myCalorie;
    public float myTotalTime;
    #endregion

    #region �ð� ����
    private Renderer[] totaltime = new Renderer[6];
    private Renderer[] remainTime = new Renderer[2];
    private Texture[] rank_num = new Texture[10];
    #endregion

    #region �ӵ�
    private Renderer[] speed = new Renderer[2];
    #endregion

    #region �̱� ���
    private Renderer[] single_distance = new Renderer[4];
    private Renderer[] single_calorie = new Renderer[4];
    private Renderer[] single_time = new Renderer[6];
    private Renderer single_number;
    private Renderer single_ranking;
    private Texture[] num = new Texture[10];
    private Texture[] m = new Texture[10];
    private Texture[] finalRank = new Texture[GameData.MAX_PLAYER];
    private bool result_single = false;
    private float resultTime = 0;
    #endregion

    #region ��Ƽ ���
    private Renderer[] multi_1st_Time = new Renderer[6];
    private Renderer[] multi_2nd_Time = new Renderer[6];
    private Renderer[] multi_3rd_Time = new Renderer[6];
    private Renderer multi_1st;
    private Renderer multi_2nd;
    private Renderer multi_3rd;
    private bool result_multi = false;
    #endregion

    void Awake()
    {
        #region ī��Ʈ �ٿ�
        for (int i = 0; i < 4; i++)
        {
            countdown[i] = (Texture)Resources.Load("Texture/game_gui/countdown_" + i);
        }
        for (int i = 0; i < 11; i++)
        {
            count[i] = (Texture)Resources.Load("Texture/game_gui/count_" + i);
        }
        plane007 = count_down.Find("Plane007").GetComponent<Renderer>();
        #endregion
        #region ��ŷ ���� GUI ����
        for (int i = 0; i < rank.Length; i++)
        {
            rank[i] = ranking.Find("rank_" + (i + 1).ToString());
            rank[i].GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Texture/game_gui/p" + (i + 1).ToString()));
            rankPos[i] = rank[i].position.y;
            rankTexture[i] = (Texture)Resources.Load("Texture/game_gui/rank_" + (i + 1).ToString() + "_0");
        }
        myRank = ranking.Find("position").GetComponent<Renderer>();
        #endregion
        #region Į�θ� �Ÿ� ���� GUI ����
        for (int i = 0; i < 4; i++)
        {
            distance[i] = ranking.Find("m_" + (4 - i).ToString()).GetComponent<Renderer>();
            calorie[i] = ranking.Find("c_" + (4 - i).ToString()).GetComponent<Renderer>();
        }
        for (int i = 0; i < 10; i++)
        {
            s_num[i] = (Texture)Resources.Load("Texture/game_gui/s_num_" + i);
        }
        #endregion
        #region ��ü �ð��� ���� GUI ����
        for (int i = 0; i < 6; i++)
        {
            totaltime[i] = total_time.Find("my_time_" + (6 - i).ToString()).GetComponent<Renderer>();
        }
        #endregion
        #region ���� �ð� ���� GUI ����
        for (int i = 0; i < 2; i++)
        {
            remainTime[i] = time.Find("time_" + (2 - i).ToString()).GetComponent<Renderer>();
        }
        for (int i = 0; i < 10; i++)
        {
            rank_num[i] = (Texture)Resources.Load("Texture/game_gui/rank_num" + i);
        }
        #endregion
        #region ���ǵ� ���� GUI ����
        for (int i = 0; i < 2; i++)
        {
            speed[i] = km.Find("k_" + (2 - i).ToString()).GetComponent<Renderer>();
        }
        #endregion
        #region �̱� ��� ���� GUI ����
        for (int i = 0; i < GameData.MAX_PLAYER; i++)
        {
            finalRank[i] = (Texture)Resources.Load("Texture/game_gui/" + (i + 1).ToString());
        }
        for (int i = 0; i < 10; i++)
        {
            num[i] = (Texture)Resources.Load("Texture/game_gui/num_" + i);
            m[i] = (Texture)Resources.Load("Texture/game_gui/m_" + (i + 1).ToString());
        }
        for (int i = 0; i < 4; i++)
        {
            single_distance[i] = single_rank.Find("rank2_t" + (4 - i).ToString()).GetComponent<Renderer>();
            single_calorie[i] = single_rank.Find("rank3_t" + (4 - i).ToString()).GetComponent<Renderer>();
        }
        for (int i = 0; i < 6; i++)
        {
            single_time[i] = single_rank.Find("rank1_t" + (6 - i).ToString()).GetComponent<Renderer>();
        }
        single_number = single_rank.Find("ranking_1").GetComponent<Renderer>();
        single_ranking = single_rank.Find("Plane004").GetComponent<Renderer>();
        #endregion
        #region ��Ƽ ��� ���� GUI ����
        for (int i = 0; i < 6; i++)
        {
            multi_1st_Time[i] = multi_rank.Find("rank1_t" + (6 - i).ToString()).GetComponent<Renderer>();
            multi_2nd_Time[i] = multi_rank.Find("rank2_t" + (6 - i).ToString()).GetComponent<Renderer>();
            multi_3rd_Time[i] = multi_rank.Find("rank3_t" + (6 - i).ToString()).GetComponent<Renderer>();
        }
        multi_1st = multi_rank.Find("ranking_1").GetComponent<Renderer>();
        multi_2nd = multi_rank.Find("ranking_2").GetComponent<Renderer>();
        multi_3rd = multi_rank.Find("ranking_3").GetComponent<Renderer>();
        #endregion
        #region
        mudAni = mud.GetComponentsInChildren<Animation>();
        waterAni = water.GetComponentsInChildren<Animation>();
        #endregion
    }

    void Start()
    {
        #region ó�� ���۽� ���� ���·� ���۵Ǿ�� �� ������Ʈ��
        game_over.gameObject.SetActive(false);
        _321go.gameObject.SetActive(false);
        time.gameObject.SetActive(false);
        //ranking.gameObject.SetActive(false);
        count_down.gameObject.SetActive(false);
        check_point.gameObject.SetActive(false);
        single_rank.gameObject.SetActive(false);
        multi_rank.gameObject.SetActive(false);
        flag.gameObject.SetActive(false);
        //minimap.gameObject.SetActive(false);
        count5.gameObject.SetActive(false);
        text_readywait.gameObject.SetActive(false);
        text_wait.gameObject.SetActive(false);
        remain_time.gameObject.SetActive(false);

        minimap.GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Minimap"));
        #endregion
    }

    void Update()
    {
        #region ī��Ʈ�ٿ� ����
        if (startCount)
        {
            countTime += Time.deltaTime;
            if (countTime > _321go.GetComponent<Animation>().clip.length * 2)
            {
                startCount = false;
                countTime = 0;
                _321go.GetComponent<Animation>().Stop();
                _321go.gameObject.SetActive(false);
            }
        }
        #endregion

        #region üũ����Ʈ ����
        if (checkpoint_Act)
        {
            checkpointTime += Time.deltaTime;
            if (checkpointTime > check_point.GetComponent<Animation>().clip.length)
            {
                checkpoint_Act = false;
                checkpointTime = 0;
                check_point.GetComponent<Animation>().Stop();
                check_point.gameObject.SetActive(false);
            }
        }
        #endregion

        #region ���â ����
        if (result_single)
        {
            resultTime += Time.deltaTime;
            if (resultTime > 5)
            {
                result_single = false;
                resultTime = 0;
                single_rank.GetComponent<Animation>().clip = single_rank.GetComponent<Animation>().GetClip("close");
                single_rank.GetComponent<Animation>().Stop();
                single_rank.GetComponent<Animation>().Play();
            }
        }

        if (result_multi)
        {
            resultTime += Time.deltaTime;
            if (resultTime > 5)
            {
                result_multi = false;
                resultTime = 0;
                multi_rank.GetComponent<Animation>().clip = multi_rank.GetComponent<Animation>().GetClip("close");
                multi_rank.GetComponent<Animation>().Stop();
                multi_rank.GetComponent<Animation>().Play();
            }
        }
        #endregion

        #region ȿ�� ����
        if (mudAct || waterAct)
        {
            effectTime += Time.deltaTime;
            if (effectTime > 3.0f)
            {
                if (mudAct)
                {
                    foreach (Animation pos in mudAni)
                    {
                        pos.Stop();
                    }
                    mudAct = false;
                }
                else if (waterAct)
                {
                    foreach (Animation pos in waterAni)
                    {
                        pos.Stop();
                    }
                    waterAct = false;
                }
                effectTime = 0;
            }
        }
        #endregion

        if (count5_start)
        {
            count5Time += Time.deltaTime;
            if (count5Time >= 1.0f)
            {
                count5.GetComponent<Animation>().Stop();
                count5Value++;
                if (count5Value > 4)
                {
                    count5.gameObject.SetActive(false);
                    count5_start = false;
                }
                if (count5_start)
                {
                    count5.Find("Plane008").GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Texture/Mtb_multi/c_" + (5 - count5Value).ToString()));
                    count5.GetComponent<Animation>().Play();
                    count5Time = 0;
                }
            }
        }

        if (result)
        {
            minimap.gameObject.SetActive(false);
            ranking.gameObject.SetActive(false);
            total_time.gameObject.SetActive(false);
            km.gameObject.SetActive(false);
            count_down.gameObject.SetActive(false);
        }
    }

    public void Count5()
    {
        if (!count5_start)
        {
            count5.gameObject.SetActive(true);
            count5_start = true;
            count5Time = 0;
            count5.Find("Plane008").GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Texture/Mtb_multi/c_" + (5 - count5Value).ToString()));
            count5.GetComponent<Animation>().Play();
        }
    }

    public void RankPos(int r) //��ŷ ��ġ ǥ��
    {
        for (int i = 0; i < rank.Length; i++)
        {
            float damp = 0.5f;
            Vector3 pos = rank[i].transform.position;
            pos = new Vector3(pos.x, Mathf.MoveTowards(pos.y, rankPos[currentRank[i]], damp * Time.deltaTime), pos.z);
            //pos = new Vector3(pos.x, rankPos[currentRank[i]], pos.z);
            rank[i].transform.position = pos;
        }
        if (myCurrentRank != r)
        {
            myRank.material.SetTexture("_MainTex", rankTexture[r]);
            myCurrentRank = r;
        }
    }

    public void Distance(float realSpeed) //�Ÿ� ���
    {
        myDistance += (Mathf.Abs(realSpeed) / 3.6f) * Time.deltaTime;
        int[] d_n = new int[4];
        d_n[0] = (int)(myDistance % 10);
        d_n[1] = (int)((myDistance / 10) % 10);
        d_n[2] = (int)((myDistance / 100) % 10);
        d_n[3] = (int)((myDistance / 1000) % 10);

        for (int i = 0; i < 4; i++)
        {
            distance[i].material.SetTexture("_MainTex", s_num[d_n[i]]);
        }
    }

    public void Calorie(float pedalSpeed) //Į�θ� ���
    {
        myCalorie += ((0.28f * Mathf.Abs(pedalSpeed)) * 70.0f / 3600.0f) * Time.deltaTime;
        int[] c_n = new int[4];
        c_n[0] = (int)((myCalorie * 10) % 10);
        c_n[1] = (int)((myCalorie / 1) % 10);
        c_n[2] = (int)((myCalorie / 10) % 10);
        c_n[3] = (int)((myCalorie / 100) % 10);

        for (int i = 0; i < 4; i++)
        {
            calorie[i].material.SetTexture("_MainTex", s_num[c_n[i]]);
        }
    }

    public void TotalTime(float t) //��ü �ð� ǥ��
    {
        remain_time.gameObject.SetActive(false);
		myTotalTime += t;
        int minute = (int)(myTotalTime / 60.0f);
        int second = (int)(myTotalTime - minute * 60.0f);
        int mSecond = (int)(((myTotalTime - minute * 60.0f) * 100.0f) % 100);

        int[] t_n = new int [6];

        t_n[0] = (int)(mSecond % 10);
        t_n[1] = (int)((mSecond / 10) % 10);
        t_n[2] = (int)(second % 10);
        t_n[3] = (int)((second / 10) % 10);
        t_n[4] = (int)(minute % 10);
        t_n[5] = (int)((minute / 10) % 10);

        for (int i = 0; i < 6; i++ )
        {
            totaltime[i].material.SetTexture("_MainTex", s_num[t_n[i]]);
        }
    }



    public void RemainTime(float t) //���� �ð� ǥ��
    {
        int[] t_n = new int[2];
        t_n[0] = (int)(t % 10);
        t_n[1] = (int)((t / 10) % 10);

        for (int i = 0; i < 2; i++)
        {
            remainTime[i].material.SetTexture("_MainTex", s_num[t_n[i]]);
        }
    }

    public void Speed(float s) //���� ���ǵ�ǥ��
    {
        int[] s_n = new int[2];
        s_n[0] = (int)(s % 10);
        s_n[1] = (int)((s / 10) % 10);
        for (int i = 0; i < 2; i++)
        {
            speed[i].material.SetTexture("_MainTex", s_num[s_n[i]]);
        }
    }

    public void GameOver(bool act) //���� ����
    {
        if (act)
        {
            game_over.gameObject.SetActive(true);
            game_over.GetComponent<Animation>().Stop();
            game_over.GetComponent<Animation>().Play();
        }
        else
        {
            game_over.GetComponent<Animation>().Stop();
            game_over.gameObject.SetActive(false);
        }
    }

    public void StartCountdown(int c) //���� ī��Ʈ �ٿ�
    {
        _321go.gameObject.SetActive(true);
        _321go.GetComponent<Renderer>().material.SetTexture("_MainTex", countdown[c]);
        _321go.GetComponent<Animation>().Stop();
        _321go.GetComponent<Animation>().Play();
        startCount = true;
        countTime = 0;
    }

    public void EndCountdown(int c) //���� ī��Ʈ �ٿ�
    {
        if (c != 0)
        {
            count_down.gameObject.SetActive(true);
            plane007.material.SetTexture("_MainTex", count[c]);
            count_down.GetComponent<Animation>().Stop();
            count_down.GetComponent<Animation>().Play();
        }
        else
        {   
            plane007.material.SetTexture("_MainTex", count[c]);
            count_down.GetComponent<Animation>().Stop();
            count_down.gameObject.SetActive(false);
            GameOver(true);
        }
    }

    public void CheckPoint() //üũ ����Ʈ
    {
        check_point.gameObject.SetActive(true);
        check_point.GetComponent<Animation>().Stop();
        check_point.GetComponent<Animation>().Play();
        checkpoint_Act = true;
        checkpointTime = 0;
    }

    public void SingleResult(int r, int n, float t, float d, float c) // ���â �̱�
    {
		if( r >0 )
			r -= 1; // must rank -1
        result = true;
        single_rank.gameObject.SetActive(true);
        single_rank.GetComponent<Animation>().clip = single_rank.GetComponent<Animation>().GetClip("open");
        single_rank.GetComponent<Animation>().Play();

        flag.gameObject.SetActive(true);
        flag.GetComponent<Animation>().Play();

        single_number.material.SetTexture("_MainTex", m[n]);
        single_ranking.material.SetTexture("_MainTex", finalRank[r]);

        int[] d_n = new int[4];
        d_n[0] = (int)(d % 10);
        d_n[1] = (int)((d / 10) % 10);
        d_n[2] = (int)((d / 100) % 10);
        d_n[3] = (int)((d / 1000) % 10);

        for (int i = 0; i < 4; i++)
        {
            single_distance[i].material.SetTexture("_MainTex", num[d_n[i]]);
        }

        int[] c_n = new int[4];
        c_n[0] = (int)((c * 10) % 10);
        c_n[1] = (int)((c / 1) % 10);
        c_n[2] = (int)((c / 10) % 10);
        c_n[3] = (int)((c / 100) % 10);

        for (int i = 0; i < 4; i++)
        {
            single_calorie[i].material.SetTexture("_MainTex", num[c_n[i]]);
        }

        int minute = (int)(t / 60.0f);
        int second = (int)(t - minute * 60.0f);
        int mSecond = (int)(((t - minute * 60.0f) * 100.0f) % 100);

        int[] t_n = new int[6];

        t_n[0] = (int)(mSecond % 10);
        t_n[1] = (int)((mSecond / 10) % 10);
        t_n[2] = (int)(second % 10);
        t_n[3] = (int)((second / 10) % 10);
        t_n[4] = (int)(minute % 10);
        t_n[5] = (int)((minute / 10) % 10);

        for (int i = 0; i < 6; i++)
        {
            single_time[i].material.SetTexture("_MainTex", s_num[t_n[i]]);
        }

        //result_single = true;
        //resultTime = 0;
    }

    public void MultiResult(int n1, int n2, int n3, float t1, float t2, float t3)
    {
        multi_rank.gameObject.SetActive(true);
        multi_rank.GetComponent<Animation>().clip = multi_rank.GetComponent<Animation>().GetClip("open");
        multi_rank.GetComponent<Animation>().Stop();
        multi_rank.GetComponent<Animation>().Play();

        flag.gameObject.SetActive(true);
        flag.GetComponent<Animation>().Play();

        multi_1st.material.SetTexture("_MainTex", m[n1]);
        multi_2nd.material.SetTexture("_MainTex", m[n2]);
        multi_3rd.material.SetTexture("_MainTex", m[n3]);

        int minute1 = (int)(t1 / 60.0f);
        int second1 = (int)(t1 - minute1 * 60.0f);
        int mSecond1 = (int)(((t1 - minute1 * 60.0f) * 100.0f) % 100);

        int[] t_n1 = new int[6];

        t_n1[0] = (int)(mSecond1 % 10);
        t_n1[1] = (int)((mSecond1 / 10) % 10);
        t_n1[2] = (int)(second1 % 10);
        t_n1[3] = (int)((second1 / 10) % 10);
        t_n1[4] = (int)(minute1 % 10);
        t_n1[5] = (int)((minute1 / 10) % 10);

        for (int i = 0; i < 6; i++)
        {
            multi_1st_Time[i].material.SetTexture("_MainTex", s_num[t_n1[i]]);
        }

        int minute2 = (int)(t2 / 60.0f);
        int second2 = (int)(t2 - minute2 * 60.0f);
        int mSecond2 = (int)(((t2 - minute2 * 60.0f) * 100.0f) % 100);

        int[] t_n2 = new int[6];

        t_n2[0] = (int)(mSecond2 % 10);
        t_n2[1] = (int)((mSecond2 / 10) % 10);
        t_n2[2] = (int)(second2 % 10);
        t_n2[3] = (int)((second2 / 10) % 10);
        t_n2[4] = (int)(minute2 % 10);
        t_n2[5] = (int)((minute2 / 10) % 10);

        for (int i = 0; i < 6; i++)
        {
            multi_2nd_Time[i].material.SetTexture("_MainTex", s_num[t_n2[i]]);
        }

        int minute3 = (int)(t3 / 60.0f);
        int second3 = (int)(t3 - minute3 * 60.0f);
        int mSecond3 = (int)(((t3 - minute3 * 60.0f) * 100.0f) % 100);

        int[] t_n3 = new int[6];

        t_n3[0] = (int)(mSecond3 % 10);
        t_n3[1] = (int)((mSecond3 / 10) % 10);
        t_n3[2] = (int)(second3 % 10);
        t_n3[3] = (int)((second3 / 10) % 10);
        t_n3[4] = (int)(minute3 % 10);
        t_n3[5] = (int)((minute3 / 10) % 10);

        for (int i = 0; i < 6; i++)
        {
            multi_3rd_Time[i].material.SetTexture("_MainTex", s_num[t_n3[i]]);
        }

        result_multi = true;
        resultTime = 0;
    }

    public void Minimap(int i)
    {
        GameObject obj = GameObject.Find("MinimapCam");
        
        if (i == 0)
        {
            obj.GetComponent<Camera>().orthographicSize = 500;
            GameObject.Find("Minimap").GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Minimap/map_5_full"));
        }
        else if (i == 3)
        {
            obj.transform.position = new Vector3(70, 100, 330);
            obj.transform.eulerAngles = new Vector3(90, -90, 0);
            obj.GetComponent<Camera>().orthographicSize = 145;
            GameObject.Find("Minimap").GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Minimap/map5_03"));
        }
        else if (i == 1)
        {
            obj.transform.position = new Vector3(309, 100, 210);
            obj.transform.eulerAngles = new Vector3(90, 180, 0);
            obj.GetComponent<Camera>().orthographicSize = 100;
            GameObject.Find("Minimap").GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Minimap/map5_01"));
        }
        else if (i == 2)
        {
            obj.transform.position = new Vector3(485, 100, 376);
            obj.transform.eulerAngles = new Vector3(90, -90, 0);
            obj.GetComponent<Camera>().orthographicSize = 130;
            GameObject.Find("Minimap").GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Minimap/map5_02"));
        }
    }

    public void MudEffect()
    {
        if (!mudAct)
        {
            foreach (Animation pos in mudAni)
            {
                pos.Play();
            }
            mudAct = true;
        }
    }

    public void WaterEffect()
    {
        if (!waterAct)
        {
            foreach (Animation pos in waterAni)
            {
                pos.Play();
            }
            waterAct = true;
        }
    }

    public void WaitMentShow(int state)
    {
        if (state == 0)
        {
            text_wait.gameObject.SetActive(false);
            text_readywait.gameObject.SetActive(false);
        }
        else if (state == 1)
        {
            text_wait.gameObject.SetActive(true);
            text_wait.GetComponent<Animation>().Play();
            text_readywait.gameObject.SetActive(false);
        }
        else if (state == 2)
        {
            text_wait.gameObject.SetActive(false);
            text_readywait.gameObject.SetActive(true);
            text_readywait.Find("Plane011").GetComponent<Animation>().Play();
        }
    }

    public void ShowServerTime(float t)
    {   
        remain_time.gameObject.SetActive(true);

        int minute = (int)(t / 60.0f);
        int second = (int)(t - minute * 60.0f);
        int mSecond = (int)(((t - minute * 60.0f) * 100.0f) % 100);

        int[] t_n = new int[6];

        t_n[0] = (int)(mSecond % 10);
        t_n[1] = (int)((mSecond / 10) % 10);
        t_n[2] = (int)(second % 10);
        t_n[3] = (int)((second / 10) % 10);
        t_n[4] = (int)(minute % 10);
        t_n[5] = (int)((minute / 10) % 10);

        for (int i = 0; i < 6; i++)
        {
            totaltime[i].material.SetTexture("_MainTex", s_num[t_n[i]]);
        }
    }

    public void TraningShow()
    {
        ranking.Find("rank_back").GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Texture/Training/rank_0_0"));
        foreach (Transform pos in rank)
        {
            pos.gameObject.SetActive(false);
        }
        myRank.gameObject.SetActive(false);
        //remain_time.gameObject.SetActive(true);
    }

    public void Training_RemainTime(float t)
    {
        remain_time.gameObject.SetActive(true);

        //int minute = (int)(t / 60.0f);
        //int second = (int)(t - minute * 60.0f);
        //int t_m10 = (int)((minute / 10) % 10);
        //int t_m1 = (int)((minute / 1) % 10);
        //int t_s10 = (int)((second / 10) % 10);
        //int t_s1 = (int)((second / 1) % 10);

        //remain_time.Find("r_time_0").GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Texture/Training/b_num_" + (t_m10).ToString()));
        //remain_time.Find("r_time_1").GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Texture/Training/b_num_" + (t_m1).ToString()));
        //remain_time.Find("r_time_2").GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Texture/Training/b_num_" + (t_s10).ToString()));
        //remain_time.Find("r_time_3").GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)Resources.Load("Texture/Training/b_num_" + (t_s1).ToString()));
        int minute = (int)(t / 60.0f);
        int second = (int)(t - minute * 60.0f);
        int mSecond = (int)(((t - minute * 60.0f) * 100.0f) % 100);

        int[] t_n = new int[6];

        t_n[0] = (int)(mSecond % 10);
        t_n[1] = (int)((mSecond / 10) % 10);
        t_n[2] = (int)(second % 10);
        t_n[3] = (int)((second / 10) % 10);
        t_n[4] = (int)(minute % 10);
        t_n[5] = (int)((minute / 10) % 10);

        for (int i = 0; i < 6; i++)
        {
            totaltime[i].material.SetTexture("_MainTex", s_num[t_n[i]]);
        }
    }
}

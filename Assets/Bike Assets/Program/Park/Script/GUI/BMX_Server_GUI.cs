using UnityEngine;
using UnityEngine.UI; // Unity6 Migration: RawImage -> RawImage
using System.Collections;

public class BMX_Server_GUI : MonoBehaviour {

    public Transform rank_sever;
    public Transform bmx_server_rsult;
    public Transform minimap;
    public Transform start_sign;
    public Transform result_shot;
    public Transform flash;

    public Transform[] ranks = new Transform[GameData.MAX_PLAYER];

    public Transform[] resultRank = new Transform[3];
    public Transform[] resultTime = new Transform[3];

    bool startSignStart;
    float startSignTime;

	// Use this for initialization
	void Awake () {
        for (int i = 0; i < ranks.Length; i++)
        {
            ranks[i] = rank_sever.Find("rank_" + (i + 1).ToString());
        }

        for (int i = 0; i < 3; i++)
        {
            resultRank[i] = bmx_server_rsult.Find("ranking" + (i + 1));
            resultTime[i] = bmx_server_rsult.Find("rank" + (i + 1) + "_time");
        }
	}

    void Start()
    {
        bmx_server_rsult.gameObject.SetActive(false);
        start_sign.gameObject.SetActive(false);
        result_shot.gameObject.SetActive(false);
        flash.gameObject.SetActive(false);
    }

    void Update()
    {
        #region ī��Ʈ�ٿ� ����
        if (startSignStart)
        {
            startSignTime += Time.deltaTime;
            if (startSignTime > start_sign.GetComponent<Animation>().clip.length * 2)
            {
                startSignStart = false;
                startSignTime = 0;
                start_sign.GetComponent<Animation>().Stop();
                start_sign.gameObject.SetActive(false);
            }
        }
        #endregion
    }

    public void ShowRank(int num, int rank)
    {
        num = num + 1;
        ranks[rank].GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/m_" + num);
    }

    public void ShowResult(int r1n, int r2n, int r3n, float r1t, float r2t, float r3t)
    {
        rank_sever.gameObject.SetActive(false);
        minimap.gameObject.SetActive(false);
        bmx_server_rsult.gameObject.SetActive(true);

        resultRank[0].GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/m_" + (r1n + 1).ToString());
        resultRank[1].GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/m_" + (r2n + 1).ToString());
        resultRank[2].GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/m_" + (r3n + 1).ToString());

        int minute = (int)(r1t / 60.0f);
        int second = (int)(r1t - minute * 60.0f);
        int t_min_10 = (int)((minute / 10) % 10);
        int t_min_1 = (int)((minute / 1) % 10);
        int t_sec_10 = (int)((second / 10) % 10);
        int t_sec_1 = (int)((second / 1) % 10);
        int t_10 = (int)((r1t * 10) % 10);
        int t_1 = (int)((r1t * 100) % 10);

        resultTime[0].Find("num_0").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + (t_min_10).ToString());
        resultTime[0].Find("num_1").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + (t_min_1).ToString());
        resultTime[0].Find("num_2").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + (t_sec_10).ToString());
        resultTime[0].Find("num_3").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + (t_sec_1).ToString());
        resultTime[0].Find("num_4").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + (t_10).ToString());
        resultTime[0].Find("num_5").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + (t_1).ToString());

        minute = (int)(r2t / 60.0f);
        second = (int)(r2t - minute * 60.0f);
        t_min_10 = (int)((minute / 10) % 10);
        t_min_1 = (int)((minute / 1) % 10);
        t_sec_10 = (int)((second / 10) % 10);
        t_sec_1 = (int)((second / 1) % 10);
        t_10 = (int)((r2t * 10) % 10);
        t_1 = (int)((r2t * 100) % 10);

        resultTime[1].Find("num_0").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + (t_min_10).ToString());
        resultTime[1].Find("num_1").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + (t_min_1).ToString());
        resultTime[1].Find("num_2").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + (t_sec_10).ToString());
        resultTime[1].Find("num_3").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + (t_sec_1).ToString());
        resultTime[1].Find("num_4").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + (t_10).ToString());
        resultTime[1].Find("num_5").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + (t_1).ToString());

        minute = (int)(r3t / 60.0f);
        second = (int)(r3t - minute * 60.0f);
        t_min_10 = (int)((minute / 10) % 10);
        t_min_1 = (int)((minute / 1) % 10);
        t_sec_10 = (int)((second / 10) % 10);
        t_sec_1 = (int)((second / 1) % 10);
        t_10 = (int)((r3t * 10) % 10);
        t_1 = (int)((r3t * 100) % 10);

        resultTime[2].Find("num_0").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + (t_min_10).ToString());
        resultTime[2].Find("num_1").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + (t_min_1).ToString());
        resultTime[2].Find("num_2").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + (t_sec_10).ToString());
        resultTime[2].Find("num_3").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + (t_sec_1).ToString());
        resultTime[2].Find("num_4").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + (t_10).ToString());
        resultTime[2].Find("num_5").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + (t_1).ToString());
    }

    public void StartSign(int i)
    {
        start_sign.gameObject.SetActive(true);
        start_sign.GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/countdown_" + i);
        start_sign.GetComponent<Animation>().Stop();
        start_sign.GetComponent<Animation>().Play();
        startSignStart = true;
        startSignTime = 0;
    }

    public void ResultShot(bool act, Texture target)
    {
        if (act)
        {
            result_shot.gameObject.SetActive(act);
            result_shot.GetComponent<RawImage>().texture = target;
            flash.gameObject.SetActive(act);
            flash.GetComponent<Animation>().Play();
        }
        else
        {
            result_shot.gameObject.SetActive(act);
            flash.gameObject.SetActive(act);
        }
    }
}

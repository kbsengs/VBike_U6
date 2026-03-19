using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video; // Unity6 Migration: MovieTexture -> VideoPlayer
using System.Collections;

public class BMX_Champ : MonoBehaviour
{

    public Transform computer;
    public Transform player;
    public Transform countdown;
    public Transform map_select;
    public Transform time;
    public Transform movie;
	
	public Transform star_f;
	public Transform star_h;

    #region AI ���� ǥ��
    public Transform[] computers = new Transform[12];
    public Transform[] players = new Transform[12];
    #endregion

    #region ī��Ʈ�ٿ�
    private Animation[] countdownAni;
    private int countdownValue;
    private bool countStart;
    private float countdownTime;
    #endregion

    #region ��
    private Transform[] track = new Transform[3];
    private Transform[] track_big = new Transform[3];
    #endregion
	
	private Transform[] stars_f = new Transform[60];
	private Transform[] stars_h = new Transform[60];

    public VideoClip[] video = new VideoClip[3]; // Unity6 Migration: MovieTexture -> VideoClip
    private VideoPlayer[] videoPlayers; // runtime VideoPlayer components
    // Use this for initialization
	void Awake () {
        #region Track
        for (int i = 0; i < track.Length; i++)
        {
            track[i] = map_select.Find("track_" + (i + 1).ToString());
            track_big[i] = map_select.Find("track_" + (i + 1).ToString() + "_big");
        }
        #endregion

        for (int i = 0; i < computers.Length; i++)
        {
            computers[i] = computer.Find("computer" + (i + 1).ToString());
            players[i] = player.Find("player" + (i + 1).ToString());
        }
		
		for( int i = 0; i < 12; i++)
		{
			for( int j = 0 ; j < 5; j++)
			{
				stars_f[i * 5 + j] = star_f.Find("star" + (i+1).ToString() + "_" + (j+1).ToString() );				
				stars_f[i * 5 + j].gameObject.SetActive(false);
				stars_h[i * 5 + j] = star_h.Find("star" + (i+1).ToString() + "_" + (j+1).ToString() );
				stars_h[i * 5 + j].gameObject.SetActive(false);
			}
		}
		
		//r_f.gameObject.SetActive(false);
		//star_h.gameObject.SetActive(false);
		
        countdownAni = countdown.GetComponentsInChildren<Animation>();
        countdown.gameObject.SetActive(false);
	}

    private int alphaDir = 1;
    private float alpha = 0;

	// Update is called once per frame
	void Update () {
        if (countStart)
        {
            countdownTime += Time.deltaTime;
            if (countdownTime >= 1.0f)
            {
                foreach (Animation pos in countdownAni)
                {
                    pos.Stop();
                }
                countdownValue++;
                if (countdownValue > 4)
                {
                    countdown.gameObject.SetActive(false);
                    countStart = false;
                }
                if (countStart)
                {
                    countdownAni[0].GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/Mtb_multi/c_" + (5 - countdownValue).ToString());
                    foreach (Animation pos in countdownAni)
                    {
                        pos.Play();
                    }
                    countdownTime = 0;
                }
            }
        }

        alpha += alphaDir * Time.deltaTime;
        if (alpha > 1) alphaDir = -1;
        else if (alpha < 0) alphaDir = 1;

        foreach (Transform pos in player)
        {
            pos.GetComponent<RawImage>().color = new Color(0.588f, 0.588f, 0.588f, alpha);
        }
	}

    public void ShowPlayer(int count, int num, bool ready)
    {
        if (num != -1 && ready)
        {
            computers[count].position = new Vector3(0, 0, -4);
            players[count].position = new Vector3(0, 0, 4);
        }
        else
        {
            computers[count].position = new Vector3(0, 0, 4);
            players[count].position = new Vector3(0, 0, -4);
        }
    }

    public void WaitTime(float t)
    {
        if (t < 0) t = 0.0f;
        int _10 = (int)((t / 10) % 10);
        int _1 = (int)(t % 10);
        int _01 = (int)((t * 10) % 10);
        int _001 = (int)((t * 100) % 10);

        time.Find("time_0").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/server_view/t_num" + _10.ToString());
        time.Find("time_1").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/server_view/t_num" + _1.ToString());
        time.Find("time_2").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/server_view/t_num" + _01.ToString());
        time.Find("time_3").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/server_view/t_num" + _001.ToString());
    }

    public void Countdown()
    {
        if (!countStart)
        {
            countStart = true;
            countdownValue = 0;
            countdownTime = 0;
            countdown.gameObject.SetActive(true);
            countdownAni[0].GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/Mtb_multi/c_" + (5 - countdownValue).ToString());
            foreach (Animation pos in countdownAni)
            {
                pos.Play();
            }
        }
    }

    public void TrackSelect(int map)
    {
        for (int i = 0; i < track.Length; i++)
        {
            if (map == i)
            {
                track[i].position = new Vector3(0, 0, 3);
                track_big[i].position = new Vector3(0, 0, 2);
            }
            else
            {
                track[i].position = new Vector3(0, 0, -3);
                track_big[i].position = new Vector3(0, 0, -2);
            }
        }
        // Unity6 Migration: MovieTexture.Play() replaced by VideoPlayer
        // TODO: Assign VideoPlayer component to 'movie' GameObject and call Play(video[map])
        // movie.GetComponent<VideoPlayer>().clip = video[map];
        // movie.GetComponent<VideoPlayer>().Play();
    }
	
	public void Show_Star()
	{
        for (int i = 0; i < GameData.MAX_PLAYER; i++)
        {
            GameData.RANK_POINT[GameData.BMX_FutureRank[i]-1] = 15 - (int)(i * 1.5f);

            //int v = ((10 - (i + 1)) / 2);
            //int k = (GameData.BMX_FutureRank[i] - 1) * 5;
            //Debug.Log("k = " + k + "   v = " + v);
            

            //for( int j = 0 ; j < v; j++)
            //    stars_f[ k + j ].gameObject.SetActive(true);
            //float t = Random.Range(0, 2);
            //if (t < 1.0f) stars_h[k + v].gameObject.SetActive(true);
        }

        for (int i = 0; i < GameData.MAX_PLAYER; i++)
        {  
            float gap = 3.0f;
            int v = (int)(GameData.RANK_POINT[i] / gap);
            float f = (GameData.RANK_POINT[i] / gap) - v;
            if (v > 5)
            {
                v = 5;
                f = 0;
            }
            for (int j = 0; j < v; j++)
                stars_f[i * 5 + j].gameObject.SetActive(true);
            if (f > 0.5f) stars_h[i * 5 + v].gameObject.SetActive(true);
        }
	}
}

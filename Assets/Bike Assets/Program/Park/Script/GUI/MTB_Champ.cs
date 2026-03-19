using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video; // Unity6 Migration: MovieTexture -> VideoPlayer
using System.Collections;

public class MTB_Champ : MonoBehaviour {

    public Transform arrow;
    public Transform map_select;

    public Transform start;
    public Transform time;

    public Transform client;
    public Transform server;

    public Transform player;
    public Transform totalplayer;

    public Transform countdown;

    public Transform movie;

    public Transform Info3;
    public Transform Info2;

    #region ī��Ʈ�ٿ�
    private Animation[] countdownAni;
    private int countdownValue;
    private bool countStart;
    private float countdownTime;
    #endregion

    #region ��
    private Transform[] arrow_On = new Transform[2];
    private Transform[] arrow_Off = new Transform[2];

    private Transform[] track = new Transform[3];
    private Transform[] track_big = new Transform[3];
    #endregion
    bool mapChange;
    float mapTime;

    public VideoClip[] video = new VideoClip[3]; // Unity6 Migration: MovieTexture -> VideoClip

    public AnimationClip selectArrow;

    void Awake()
    {
        #region Arrow
        arrow_On[0] = arrow.Find("arrow_l");
        arrow_On[1] = arrow.Find("arrow_r");

        arrow_On[0].gameObject.SetActive(false);
        arrow_On[1].gameObject.SetActive(false);

        arrow_Off[0] = arrow.Find("arrow_l_0");
        arrow_Off[1] = arrow.Find("arrow_r_0");
        #endregion
        #region Captain Player Com
        
        #endregion
        #region Track
        for (int i = 0; i < track.Length; i++)
        {
            track[i] = map_select.Find("track_" + (i + 1).ToString());
            track_big[i] = map_select.Find("track_" + (i + 1).ToString() + "_big");
        }
        #endregion

        countdownAni = countdown.GetComponentsInChildren<Animation>();
        countdown.gameObject.SetActive(false);

        if (GameData._3D)
        {
            RawImage[] allGUI = GetComponentsInChildren<RawImage>();
            foreach (RawImage pos in allGUI)
            {
                // Unity6 Migration: GUITexture.pixelInset -> RectTransform
                RectTransform rt = pos.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x / 2, rt.anchoredPosition.y);
                rt.sizeDelta = new Vector2(rt.sizeDelta.x / 2, rt.sizeDelta.y);
            }
        }
    }

    void Update()
    {
        if (mapChange)
        {
            mapTime += Time.deltaTime;
            if (mapTime > 0.2f)
            {
                arrow_On[0].gameObject.SetActive(false);
                arrow_On[1].gameObject.SetActive(false);
                mapChange = false;
            }
        }

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
    }

    public void Server_Client(bool isServer)
    {
        if (isServer)
        {
            server.position = new Vector3(0, 0, 2);
            client.position = new Vector3(0, 0, -2);
        }
        else
        {
            server.position = new Vector3(0, 0, -2);
            client.position = new Vector3(0, 0, 2);
        }
    }

    public void ShowPlayerNumber(int num)
    {
        player.GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/m_" + (num +1).ToString());
    }

    public void TotalPlayer(int num)
    {
        int _10 = (int)((num / 10) % 10);
        int _1 = (int)(num % 10);
        totalplayer.Find("10").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + _10.ToString());
        totalplayer.Find("1").GetComponent<RawImage>().texture = (Texture)Resources.Load("Texture/game_gui/num_" + _1.ToString());
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
        // TODO: Assign VideoPlayer component to 'movie' GameObject and call Play(video[2-map])
        // movie.GetComponent<VideoPlayer>().clip = video[2 - map];
        // movie.GetComponent<VideoPlayer>().Play();
    }

    public void TrackArrow(int rl)
    {
        mapTime = 0;
        arrow_On[rl].gameObject.SetActive(true);
        mapChange = true;
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

    public void ImServer(bool who)
    {
        if (who)
        {
            Info2.gameObject.SetActive(true);
            Info3.gameObject.SetActive(true);
        }
        else
        {
            Info2.gameObject.SetActive(false);
            Info3.gameObject.SetActive(false);
        }
    }
}

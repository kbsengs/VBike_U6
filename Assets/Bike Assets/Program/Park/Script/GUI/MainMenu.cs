using UnityEngine;
using UnityEngine.UI; // Unity6 Migration: RawImage -> RawImage
using System.Collections;

public class MainMenu : MonoBehaviour {

    public Transform bmx;
    public Transform mtb;
    public Transform training;
    public Transform champ;
    public AnimationClip click_ani;
	public Transform credit;
	public Transform insertcoin;

    #region Training
    //private Transform training_track;
    private Transform[] training_On = new Transform[3];
    private Transform[] training_Off = new Transform[3];
    #endregion

    #region MTB
    private Transform[] mtb_On = new Transform[3];
    private Transform[] mtb_Off = new Transform[3];
    private Transform[] mtb_name = new Transform[3];
    #endregion

    #region BMX
    private Transform[] bmx_On = new Transform[3];
    private Transform[] bmx_Off = new Transform[3];
    private Transform[] bmx_name = new Transform[3];
    #endregion

    #region ChapionShip
    private Transform[] champ_btn = new Transform[2];
    #endregion


	#region Credit
    private Transform[] credit000 = new Transform[6];
	private Texture[] creditTexture = new Texture[10];
    #endregion

    public enum SelectGame
    {
        Training = 0, MTB = 1, BMX = 2, CampionShip = 3
    }
    public SelectGame selectGame = SelectGame.Training;

    private float time;
    public bool click;
    private Transform animationTarget;

    void Awake()
    {
        //training_track = training.Find("training_track_1");

        #region Training
        for (int i = 0; i < training_On.Length; i++)
        {
            training_On[i] = training.Find("training_track_" + (i + 1).ToString());
            training_Off[i] = training.Find("training_track_" + (i + 1).ToString() + "_0");
        }
        #endregion

        #region MTB BMX
        for (int i = 0; i < 3; i++)
        {
            mtb_On[i] = mtb.Find("mtb_track_" +(i + 1).ToString());
            mtb_Off[i] = mtb.Find("mtb_track_" + (i + 1).ToString() + "_0");
            mtb_name[i] = mtb.Find("track_" + (i + 1).ToString());

            bmx_On[i] = bmx.Find("bmx_track_" + (i + 1).ToString());
            bmx_Off[i] = bmx.Find("bmx_track_" + (i + 1).ToString() + "_0");
            bmx_name[i] = bmx.Find("track_" + (i + 1).ToString());
        }
        #endregion

        #region ChapionShip
        champ_btn[0] = champ.Find("champ_mtb");
        champ_btn[1] = champ.Find("champ_bmx");
        #endregion

		#region credit
		for (int i = 0; i < 10; i++)
        {
			if (i < 6 && credit != null)
                credit000[i] = credit.Find("credit" + i.ToString());

            creditTexture[i] = (Texture)Resources.Load("Texture/game_gui/num_" + i);
        }
		#endregion
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

	// Use this for initialization
	void Start ()
    {
        #region 처음 시작시 첫번째로 선택
//        Select_Game(0);
        #endregion
    }
	bool ready = false;

	// Update is called once per frame
	void Update () {

        switch (selectGame)
        {
            case SelectGame.Training:
                break;
            case SelectGame.BMX:
                break;
            case SelectGame.MTB:
                break;
            case SelectGame.CampionShip:
                break;
        }
        if (click)
        {
            time += Time.deltaTime;
            if (time > click_ani.length)
            {
                click = false;
                time = 0;
                animationTarget.GetComponent<Animation>().Stop();
            }
        }

        // Unity6 Migration: credit000 may be null if Main_menu prefab lacks credit child objects
        bool creditReady = credit != null && credit000[0] != null;

        if (GameData.FREE_MODE) {
            if (insertcoin != null) insertcoin.gameObject.SetActive(false);
            if (creditReady) {
                for (int i = 0; i < 6; i++) {
                    credit000[i].GetComponent<RawImage>().enabled = false;
                }
            }
        } else {
            if (GameData.NOW_CREDIT > 0) {
                int value = (int)(GameData.NOW_CREDIT / GameData.ONEGAMECOIN);
                int[] d_n = new int[3];
                d_n[0] = (int)(value % 10);
                d_n[1] = (int)((value / 10) % 10);
                if (value < 100 && d_n[1] == 0) d_n[1] = -1;
                d_n[2] = (int)((value / 100) % 10);
                if (value < 1000 && d_n[2] == 0) d_n[2] = -1;

                if (creditReady) {
                    for (int i = 0; i < 6; i++) {
                        credit000[i].GetComponent<RawImage>().enabled = true;
                        if (i < 3) {
                            if (d_n[i] >= 0)
                                credit000[i].GetComponent<RawImage>().texture = creditTexture[d_n[i]];
                            else
                                credit000[i].GetComponent<RawImage>().enabled = false;
                        }
                    }
                    credit000[3].GetComponent<RawImage>().texture = creditTexture[GameData.NOW_CREDIT % GameData.ONEGAMECOIN];
                    credit000[4].GetComponent<RawImage>().texture = creditTexture[GameData.ONEGAMECOIN];
                }
            } else {
                if (creditReady) {
                    for (int i = 0; i < 6; i++) {
                        credit000[i].GetComponent<RawImage>().enabled = false;
                    }
                }
            }
            if (GameData.NOW_CREDIT < GameData.ONEGAMECOIN) {
                if (!ready && insertcoin != null) {
                    insertcoin.gameObject.SetActive(true);
                    insertcoin.GetComponent<Animation>().Play();
                }
            } else {
                ready = true;
                if (insertcoin != null) insertcoin.gameObject.SetActive(false);
            }
        }
	}

    public void Select_Game(int g)
    {
        Debug.Log("bb");
        if (g == 0)
        {
            bmx.gameObject.SetActive(false);
            mtb.gameObject.SetActive(false);
            training.gameObject.SetActive(true);
            champ.gameObject.SetActive(false);
            selectGame = SelectGame.Training;
            Training_MapSelect(0);
        }
        else if (g == 1)
        {
            bmx.gameObject.SetActive(false);
            mtb.gameObject.SetActive(true);
            training.gameObject.SetActive(false);
            champ.gameObject.SetActive(false);
            selectGame = SelectGame.MTB;
            MTB_MapSelect(0);
        }
        else if (g == 2)
        {
            bmx.gameObject.SetActive(true);
            mtb.gameObject.SetActive(false);
            training.gameObject.SetActive(false);
            champ.gameObject.SetActive(false);
            selectGame = SelectGame.BMX;
            BMX_MapSelect(0);
        }
        else if (g == 3)
        {
            bmx.gameObject.SetActive(false);
            mtb.gameObject.SetActive(false);
            training.gameObject.SetActive(false);
            champ.gameObject.SetActive(true);
            selectGame = SelectGame.CampionShip;
            Champ_MapSelect(0);
        }
    }

    public void Training_MapSelect(int m)
    {
        for (int i = 0; i < training_On.Length; i++)
        {
            if (i == m)
            {
                training_On[i].gameObject.SetActive(true);
                training_Off[i].gameObject.SetActive(false);
                animationTarget = training_On[i];
                training_On[i].position = new Vector3(0, 0, 15);
            }
            else
            {
                training_On[i].gameObject.SetActive(false);
                training_Off[i].gameObject.SetActive(true);
                training_On[i].position = new Vector3(0, 0, 5);
                training_Off[i].position = new Vector3(0, 0, 5);
            }
        }
    }

    public void MTB_MapSelect(int m)
    {
        for (int i = 0; i < 3; i++)
        {
            if (i == m)
            {
                mtb_On[i].gameObject.SetActive(true);
                mtb_Off[i].gameObject.SetActive(false);
                mtb_name[i].gameObject.SetActive(true);
                animationTarget = mtb_On[i];
                mtb_On[i].position = new Vector3(0, 0, 15);
            }
            else
            {
                mtb_On[i].gameObject.SetActive(false);
                mtb_Off[i].gameObject.SetActive(true);
                mtb_name[i].gameObject.SetActive(false);
                mtb_On[i].position = new Vector3(0, 0, 5);
                mtb_Off[i].position = new Vector3(0, 0, 5);
            }
        }
        if (m == 2)
            mtb_Off[1].position = new Vector3(0, 0, 10);
        else
            mtb_Off[1].position = new Vector3(0, 0, 5);
    }

    public void BMX_MapSelect(int m)
    {
        for (int i = 0; i < 3; i++)
        {
            if (i == m)
            {
                bmx_On[i].gameObject.SetActive(true);
                bmx_Off[i].gameObject.SetActive(false);
                bmx_name[i].gameObject.SetActive(true);
                bmx_On[i].position = new Vector3(0, 0, 15);
                animationTarget = bmx_On[i];
            }
            else
            {
                bmx_On[i].gameObject.SetActive(false);
                bmx_Off[i].gameObject.SetActive(true);
                bmx_name[i].gameObject.SetActive(false);
                bmx_On[i].position = new Vector3(0, 0, 5);
                bmx_Off[i].position = new Vector3(0, 0, 10);
            }
        }
        if (m == 0)
            bmx_Off[2].position = new Vector3(0, 0, 5);
        else
            bmx_Off[2].position = new Vector3(0, 0, 10);
    }

    public void Champ_MapSelect(int g)
    {
        if (g == 0)
        {
            champ_btn[0].gameObject.SetActive(true);
            champ_btn[1].gameObject.SetActive(false);
            animationTarget = champ_btn[0];
        }
        else if (g == 1)
        {
            champ_btn[0].gameObject.SetActive(false);
            champ_btn[1].gameObject.SetActive(true);
            animationTarget = champ_btn[1];
        }
    }

    public void ClickAction()
    {
        if (!animationTarget.GetComponent<Animation>())
        {
            animationTarget.gameObject.AddComponent<Animation>();
            animationTarget.GetComponent<Animation>().playAutomatically = false;
            animationTarget.GetComponent<Animation>().AddClip(click_ani, "select");
            animationTarget.GetComponent<Animation>().clip = click_ani;
        }
        animationTarget.GetComponent<Animation>().Play();
        click = true;
        time = 0;
    }
}

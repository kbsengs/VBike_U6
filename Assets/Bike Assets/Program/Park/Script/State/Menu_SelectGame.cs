using UnityEngine;
using System.Collections;

public class Menu_SelectGame : GameState {

    #region Members
	RFID	pRFID;
    MainMenu menu;
    int select_L = 0;
    int select_R = 0;
    bool keyAct = false;
    float delayTime;

    public enum GameKind
    {
        Training = 0, MTB = 1, BMX = 2, ChampionShip = 3
    }
    public GameKind gameKind = GameKind.Training;
    #endregion

    #region OnActivate
    public override void OnActivate()
    {
        StartCoroutine(Activate());
    }
    #endregion

    //float demoTime;
    //bool end;
    #region OnUpdate
    public override void OnUpdate()
    {   
        if ( GameData.NOW_CREDIT < GameData.ONEGAMECOIN)
        {
            AudioListener.volume = 0.0f;
            return;
        }
        else
        {
            AudioListener.volume = 1.0f;
        }

        if (GameData.ISCONFIG)
            return;

        #region ���� �޴� ����
     
        LeftKeySetting( (GameData.USE_NETWORK) ? 3 : 2);

        #endregion
        #region ������ �޴� ����
        switch (gameKind)
        {
            case GameKind.Training:
                RightKeySetting(2);
                //AudioSource.PlayClipAtPoint( AudioCtr.snd_bt_cancle[2], Vector3.zero );
                break;
            case GameKind.MTB:
                RightKeySetting(2);
                break;
            case GameKind.BMX:
                RightKeySetting(2);
                break;
            case GameKind.ChampionShip:
                RightKeySetting((GameData.USE_SERVER) ? 1 : 0);
                break;
        }
        #endregion
        #region �ش� ���Ӽ���
        SelectKeySetting();
        #endregion
        if (!keyAct)
        {
            delayTime += Time.deltaTime;
            if (delayTime > menu.click_ani.length)
            {
				StartCoroutine( pRFID.IsStart());
				//if( menu.GetComponent<RFID>
                delayTime = -20000;
                GameData.NOW_CREDIT-= GameData.ONEGAMECOIN;
                MoveNextGame();
            }
        }

        //demoTime += Time.deltaTime;
        //if (Input.GetKeyDown(KeyCode.Space) || CBikeSerial.GetNewButton(1))
        //{
        //    demoTime = 0;
        //}
        //if (demoTime >= 20 && !end)
        //{
        //    end = true;
        //    demoTime = 0;
        //    StartCoroutine(Demo_Mode());
        //}
    }
    #endregion

    #region OnDeactivate
    public override void OnDeactivate()
    {
        AudioCtr.Stop(AudioCtr.snd_bgm[0]);
        if (menu) Destroy(menu.gameObject);
        DestroyImmediate(this);
    }
    #endregion

    IEnumerator Activate()
    {
        AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Menu");
        yield return async;
        GameObject obj = Instantiate((GameObject)Resources.Load("Main_menu")) as GameObject;
		if (GameData.USE_SERVER) 
		{
			select_L = 3;
			Debug.Log("aa");
		}
        menu = obj.GetComponent<MainMenu>();		
		pRFID = obj.GetComponent<RFID>();
		
        obj.name = "_MainMenu";
		GameSelect(select_L);
        keyAct = true;
        GameMng.m_StartUpdate = true;
		
		CBikeSerial.InitIntro();
		CBikeSerial.InitGame();
		
		AudioCtr.Play( AudioCtr.snd_bgm[0], AudioCtr.BGM_VALUME, true );

        // Unity6 Migration: Network.peerType / Network.Disconnect removed (Phase 4: re-implement via UDP)
        // if (Network.peerType != NetworkPeerType.Disconnected) Network.Disconnect();

		if (GameData.USE_SERVER) 
						NetworkStart ();
        //randomTime = Random.Range(5, 30);
        //StartCoroutine(Test());
    }

    float randomTime = 0;

    IEnumerator Test()
    {
        yield return new WaitForSeconds(randomTime);
        StartCoroutine(BMXClientStart());
    }

    void GameSelect(int s)
    {
        switch (s)
        {
            case 0:
                gameKind = GameKind.Training;
                break;
            case 1:
                gameKind = GameKind.MTB;
                break;
            case 2:
                gameKind = GameKind.BMX;
                break;
            case 3:
                gameKind = GameKind.ChampionShip;
                break;
        }
        menu.Select_Game(select_L);
        select_R = 0;
    }

    void LeftKeySetting(int i)
    {
        if (!keyAct) return;
        if (Input.GetKeyDown(KeyCode.A) || CBikeSerial.GetNewButton(0))
        {
			AudioCtr.Play(AudioCtr.snd_bt_move[2]);
            select_L++;
            if (select_L > i) select_L = 0;
            GameSelect(select_L);
        }
    }

	void NetworkStart()
	{
		select_R = 1;
		switch (gameKind)
		{
			case GameKind.Training:
				menu.Training_MapSelect(select_R);
				break;
			case GameKind.MTB:
				menu.MTB_MapSelect(select_R);
				break;
			case GameKind.BMX:
				menu.BMX_MapSelect(select_R);
				break;
			case GameKind.ChampionShip:
				menu.Champ_MapSelect(select_R);
				break;
		}
	}
    void RightKeySetting(int i)
    {
        if (!keyAct) return;
        if (Input.GetKeyDown(KeyCode.D) || CBikeSerial.GetNewButton(2))
        {
			AudioCtr.Play(AudioCtr.snd_bt_select[2]);
            select_R++;
            if (select_R > i) select_R = 0;
            switch (gameKind)
            {
                case GameKind.Training:
                    menu.Training_MapSelect(select_R);
                    break;
                case GameKind.MTB:
                    menu.MTB_MapSelect(select_R);
                    break;
                case GameKind.BMX:
                    menu.BMX_MapSelect(select_R);
                    break;
                case GameKind.ChampionShip:
                    menu.Champ_MapSelect(select_R);
                    break;
            }
        }
    }


    bool btnDelay;
    float btnDelayTime;

    void SelectKeySetting()
    {
        if (!keyAct) return;

        
        if (Input.GetKeyDown(KeyCode.Space) || CBikeSerial.GetNewButton(1))
        {
            AudioCtr.Play(AudioCtr.snd_bt_start[0]);                
            keyAct = false;
            menu.ClickAction();
        }
    }

    void MoveNextGame()
    {	
        switch (gameKind)
        {
            case GameKind.Training:
                GameData.TraningMap = select_R;
                StartCoroutine(TrainingMode());
                break;
            case GameKind.MTB:
                GameData.MTBMap = select_R;
                StartCoroutine(MTBSingle());
                break;
            case GameKind.BMX:
                GameData.BMXMap = select_R + 1;
                StartCoroutine(BMXSingle());
                break;
            case GameKind.ChampionShip:
                if (select_R == 0)
                {
                    StartCoroutine(MTBLobbyConnect());
                }
                else if (select_R == 1)
                {
                    StartCoroutine(BMXClientStart());
                }
                break;
        }
    }

    //void BMXClientStart()
    //{
    //    gameObject.AddComponent<BMX_Data>();
    //    gameObject.AddComponent<BMX_Wait>();
    //    gameObject.AddComponent<BMX_InGame>();

    //    StateControl.gameMng.SetState(typeof(BMX_Wait));
    //}

    IEnumerator Demo_Mode()
    {
        yield return new WaitForSeconds(1);
        gameObject.AddComponent<DemoMode>();
        GameObject obj = Instantiate((GameObject)Resources.Load("Prefeb/Loading_MTB")) as GameObject;
        obj.name = "_Loading";
        StateControl.gameMng.SetState(typeof(DemoMode));
    }
    IEnumerator TrainingMode()
    {
        yield return new WaitForSeconds(1);
        gameObject.AddComponent<Training_Data>();
        gameObject.AddComponent<Training_InGame>();
        GameObject obj = Instantiate((GameObject)Resources.Load("Prefeb/Loading_MTB")) as GameObject;
        obj.name = "_Loading";
        StateControl.gameMng.SetState(typeof(Training_InGame));
    }

    IEnumerator BMXClientStart()
    {
        yield return new WaitForSeconds(1);
        gameObject.AddComponent<BMX_Client_Data>();
        gameObject.AddComponent<BMX_Client_Wait>();

        //GameObject obj = Instantiate((GameObject)Resources.Load("Prefeb/Loading_BMX")) as GameObject;
        //obj.name = "_Loading";
        GameData.GameState = 0;
        StateControl.gameMng.SetState(typeof(BMX_Client_Wait));
        //gameObject.AddComponent<BMX_Multi_Data>();
        //gameObject.AddComponent<BMX_Multi_Wait>();

        //GameData.GameState = 0;
        //StateControl.gameMng.SetState(typeof(BMX_Multi_Wait));
    }

    IEnumerator MTBLobbyConnect()
    {   
        //gameObject.AddComponent<UDPConnection>();
        yield return new WaitForSeconds(1);
        //Network.Connect(GameData.SERVER_IP, 52000, "MTB");
        gameObject.AddComponent<MTB_Client_Data>();
        gameObject.AddComponent<MTB_Client_Wait>();
        //GameObject obj = Instantiate((GameObject)Resources.Load("Prefeb/Loading_MTB")) as GameObject;
        //obj.name = "_Loading";
        GameData.GameState = 1;
        StateControl.gameMng.SetState(typeof(MTB_Client_Wait));
    }

    IEnumerator MTBSingle()
    {
        yield return new WaitForSeconds(1);
        gameObject.AddComponent<MTB_S_Data>();
        gameObject.AddComponent<MTB_S_Wait>();
        GameObject obj = Instantiate((GameObject)Resources.Load("Prefeb/Loading_MTB")) as GameObject;
        obj.name = "_Loading";
        GameData.GameState = 1;
        StateControl.gameMng.SetState(typeof(MTB_S_Wait));
    }

    IEnumerator BMXSingle()
    {
        yield return new WaitForSeconds(1);
        gameObject.AddComponent<BMX_S_Data>();
        gameObject.AddComponent<BMX_S_Wait>();
        GameObject obj = Instantiate((GameObject)Resources.Load("Prefeb/Loading_BMX")) as GameObject;
        obj.name = "_Loading";
        GameData.GameState = 0;
        StateControl.gameMng.SetState(typeof(BMX_S_Wait));
    }
}

using UnityEngine;
using System.Collections;

public class BMX_Server_InGame : GameState {

    #region Members
    BMX_Server_Data _Data;

    //bool nFind_FinishPlayer = false;
    #endregion
	
    #region OnActivate
    public override void OnActivate()
    {   
        //nFind_FinishPlayer = false;
        _Data = GetComponent<BMX_Server_Data>();
        _Data.AIStartPosition();
        GameObject obj = Instantiate((GameObject)Resources.Load("BMX_Server_InGame")) as GameObject;
        obj.name = "_GameGUI";
        _Data._GameGUI = obj.GetComponent<BMX_Server_GUI>();
        GameMng.m_StartUpdate = true;

//        StartCoroutine(_Data.SendWebData(1));
        StartCoroutine(StartSign());
		
		AudioCtr.Play( AudioCtr.snd_bgm[ GameData.BMXMap ], AudioCtr.BGM_VALUME, true );
		AudioListener.volume = 1.0f;
    }
    #endregion
    //public override void OnFixedUpdate()
    //{
    //    _Data.SaveScreenShot();
    //}

    float limitTime = 180.0f;

    #region OnUpdate
    public override void OnUpdate()
    {
        switch (_Data._ServerState)
        {
            case 2:
                _Data.SyncServerTime();
                if (_Data._ServerTime <= 0)
                {
                    GameObject.Find("start" + GameData.BMXMap + "/start_plane1").GetComponent<Animation>().Play();
                    _Data.FindPlayingPlayer();
                    //_Data._RankData.Init();
                    _Data._ServerState = 3;
                    for (int i = 0; i < _Data.AI.Length; i++)
                    {
                        if (_Data.AI[i].gameStart)
                        {
                            _Data.AI[i].Rb.isKinematic = false;
                            _Data.AI[i].cycle_Impact = true;
                            _Data.AI[i].cycle_AI = true;
                            _Data.AI[i].checkRespawn = true;
                        }
                    }
                }
                break;
            case 3:
                //_Data.FindPlayingPlayer();
                limitTime -= Time.deltaTime;
                if ( _Data.FindFinishPlayer() > 1 || limitTime <= 0)
                {
                    _Data._ServerState = 4;
                    _Data._ServerTime = GameData.SERVER_FINISH_TIME;
                    _Data.SendState(_Data._ServerState, _Data._ServerTime);
                }
                for (int i = 0; i < _Data._RankData.ranklist.Length; i++)
                {
                    _Data._GameGUI.ShowRank(_Data._RankData.ranklist[i].MyNumber, i);
                }
                break;
            case 4:
                _Data.SyncServerTime();
                if (_Data._ServerTime <= -2.0f)
                {
                    if (!replayAct)
                    {
                        StartCoroutine(ShowReplay());
                        replayAct = true;
                    }
                }
                for (int i = 0; i < _Data._RankData.ranklist.Length; i++)
                {
                    _Data._GameGUI.ShowRank(_Data._RankData.ranklist[i].MyNumber, i);
                }
                break;
            case 5:
				Debug.Log(" next State Result ");
				gameObject.AddComponent<BMX_Server_Result>();
				StateControl.gameMng.SetState(typeof(BMX_Server_Result));
                break;
        }
    }
    #endregion

    #region OnDeactivate
    public override void OnDeactivate()
    {
        AudioCtr.Stop(AudioCtr.snd_bgm[GameData.BMXMap]);
        DestroyImmediate(this);
    }
    #endregion

    IEnumerator StartSign()
    {
        yield return new WaitForSeconds(2);
        //_Data._GameGUI.StartSign(0);
		AudioCtr.Play( AudioCtr.snd_count[10] );
        yield return new WaitForSeconds(1);
        //_Data._GameGUI.StartSign(1);
		AudioCtr.Play( AudioCtr.snd_count[10] );
        yield return new WaitForSeconds(1);
        //_Data._GameGUI.StartSign(2);
		AudioCtr.Play( AudioCtr.snd_count[10] );
        yield return new WaitForSeconds(1);
        //_Data._GameGUI.StartSign(3);
		AudioCtr.Play( AudioCtr.snd_count[11] );
    }

    bool replayAct = false;

    IEnumerator ShowReplay()
    {
        _Data._RankData.initComplete = false;

        if (GameData.BMXMap == 1)
        {
            GameObject.Find("Eye").transform.position = GameObject.Find("FinishCam1").transform.position;
            GameObject.Find("Eye").transform.LookAt(GameObject.Find("_Finish1/FinishLine").transform);
        }
        else if (GameData.BMXMap == 2)
        {
            GameObject.Find("Eye").transform.position = GameObject.Find("FinishCam2").transform.position;
            GameObject.Find("Eye").transform.LookAt(GameObject.Find("_Finish2/FinishLine").transform);
        }
        else if (GameData.BMXMap == 3)
        {
            GameObject.Find("Eye").transform.position = GameObject.Find("FinishCam3").transform.position;
            GameObject.Find("Eye").transform.LookAt(GameObject.Find("_Finish3/FinishLine").transform);
        }

        _Data.SendState(5, GameData.SERVER_FINISH_TIME);
        _Data.SendResult();

        _Data.ReplayShow(0);
        _Data._GameGUI.ResultShot(true, (Texture)Resources.Load("Texture/server_view/1pass"));
        AudioCtr.Play(AudioCtr.snd_flash);
        print("1��");
        yield return new WaitForSeconds(2);
        _Data.ReplayShow(1);
        _Data._GameGUI.ResultShot(true, (Texture)Resources.Load("Texture/server_view/2pass"));
        AudioCtr.Play(AudioCtr.snd_flash);
        print("2��");
        yield return new WaitForSeconds(2);
        _Data._GameGUI.ResultShot(true, (Texture)Resources.Load("Texture/server_view/3pass"));
        _Data.ReplayShow(2);
        AudioCtr.Play(AudioCtr.snd_flash);
        print("3��");
        yield return new WaitForSeconds(2);
        _Data._GameGUI.ResultShot(false, (Texture)Resources.Load("Texture/server_view/3pass"));
        _Data._ServerState = 5;
        _Data._ServerTime = GameData.SERVER_FINISH_TIME;

//        StartCoroutine(_Data.SendWebData(2));
    }
}

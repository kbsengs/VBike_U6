using UnityEngine;
//using System;
using System.Collections;
using System.Runtime.InteropServices;
	
public class BMX_Server_Wait : GameState {

    #region Members
    BMX_Server_Data _Data;	
	bool startCountDown = false;
    #endregion
	
	[DllImport("ZoLock")]
	private static extern int CheckLock(string app);
    [DllImport("ZoLock")]
	private static extern int GetLockData(int i);
	
    #region OnActivate
	float ftime = 0;
	bool start = false;

	bool key = false;

    int nBGMID = 0;
    public override void OnActivate()
    {
        StartCoroutine(Activate());  
		key = false;
		//_Data._GUI.Show_Star();
    }
    #endregion


    #region OnUpdate
    public override void OnUpdate()
    {
		ftime += Time.deltaTime;
		if(!start && ftime > 0.2f)
		{
			//_Data._GUI.Show_Star();
			start = true;
		}

        if (start && ftime > 2.0f)
        {
            ftime = 0.0f;
//            StartCoroutine(_Data.SendWebData(4));
        }

		int readyPlayer = 0;

        for(int i = 0; i < _Data._PlayerInfo.Length; i++)
        {
            _Data._GUI.ShowPlayer(i, _Data._PlayerInfo[i].number, _Data._PlayerInfo[i].ready);
			if (_Data._PlayerInfo[i].ready) readyPlayer ++;
        }
        _Data._GUI.TrackSelect(GameData.BMXMap - 1);
		//Debug.Log ("ready player = " + readyPlayer);
        if (_Data._ServerState == 0)
        {
			if (GameData.noTime == 0)
			{
            	_Data.SyncServerTime();
            	_Data._GUI.WaitTime(_Data._ServerTime);
			}
            else 
			{
				_Data._GUI.WaitTime(0);
			}
			if( _Data._ServerTime <= 5 && !startCountDown )
			{
				startCountDown = true;
                StartPointControl sp = GameObject.Find(GameData.bmxStart[GameData.BMXMap]).GetComponent<StartPointControl>();
                sp.RandomPos();
// Unity6: networkView.RPC("RPC_SendStartpointPosition", RPCMode.All, sp.name,
//                  sp.num[0], sp.num[1], sp.num[2], sp.num[3], sp.num[4],
//                  sp.num[5], sp.num[6], sp.num[7], sp.num[8], sp.num[9]);
				StartCoroutine(Countdown());
			}
			if (_Data._ServerTime <= 0 || ((Input.GetKeyDown(KeyCode.KeypadEnter)|| Input.GetKeyDown(KeyCode.Space)) && !key && readyPlayer > 0))
            {
//                StartCoroutine(_Data.SendWebData(5));
				key = true;
                StartPointControl sp = GameObject.Find(GameData.bmxStart[GameData.BMXMap]).GetComponent<StartPointControl>();
                sp.RandomPos();
// Unity6: networkView.RPC("RPC_SendStartpointPosition", RPCMode.All, sp.name,
//                  sp.num[0], sp.num[1], sp.num[2], sp.num[3], sp.num[4],
//                  sp.num[5], sp.num[6], sp.num[7], sp.num[8], sp.num[9]);
                _Data._GUI.WaitTime(0);
                _Data._GUI.Countdown();
                _Data._ServerState = 1;
                _Data._ServerTime = GameData.SERVER_READY_TIME;
                _Data.SendState(_Data._ServerState, _Data._ServerTime);
            }
        }
        else if (_Data._ServerState == 1)
        {
            _Data.SyncServerTime();
            if (_Data._ServerTime <= 0)
            {
//                StartCoroutine(_Data.SendWebData(5));

				AudioCtr.Play( AudioCtr.snd_count[3] );
                _Data._ServerState = 2;
                _Data._ServerTime = GameData.SERVER_READY_TIME;
                _Data.SendState(_Data._ServerState, _Data._ServerTime);
                gameObject.AddComponent<BMX_Server_InGame>();
                StateControl.gameMng.SetState(typeof(BMX_Server_InGame));
            }
        }
    }
    #endregion

    #region OnDeactivate
    public override void OnDeactivate()
    {
        AudioCtr.Stop(AudioCtr.snd_bgm[nBGMID]);
        DestroyServerGUI();
        DestroyImmediate(this);
    }
    #endregion

    IEnumerator Activate()
    {   

        CMoonCamera c = GameObject.Find("_Manager/Moon").GetComponent<CMoonCamera>();
        c.Init();

        startCountDown = false;
        _Data = GetComponent<BMX_Server_Data>();

        for (int i = 0; i < GameData.BMX_FutureRank.Length; i++)
        {
            int temp = (int)Random.Range(1, 11);
            for (int j = 0; j < i; j++)
            {
                if (GameData.BMX_FutureRank[j] == temp)
                {
                    temp = 0;
                }
            }
            if (temp != 0)
            {
                GameData.BMX_FutureRank[i] = temp;
            }
            else
            {
                i--;
            }
        }

//        StartCoroutine(_Data.SendWebData(0));
//        StartCoroutine(_Data.SendWebData(3));


        if (!_Data._The_server_has_been_made)
        {
            //AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Map5");
            yield return StartCoroutine(LoadBundle.DownLoadBundle("Map5")); //async;
// Unity6: Network.incomingPassword = "BMX";
// Unity6: Network.InitializeServer(GameData.MAX_PLAYER, 53000, false);
            _Data._The_server_has_been_made = true;
            Object obj = Instantiate((GameObject)Resources.Load("Prefeb/_Startpoint1"));
            obj.name = "_Startpoint1";
            obj = Instantiate((GameObject)Resources.Load("Prefeb/_Startpoint2"));
            obj.name = "_Startpoint2";
            obj = Instantiate((GameObject)Resources.Load("Prefeb/_Startpoint3"));
            obj.name = "_Startpoint3";

            StartPointControl[] sp = FindObjectsOfType(typeof(StartPointControl)) as StartPointControl[];
            foreach (StartPointControl pos in sp)
            {
                pos.Init();
            }
            CreatServerGUI();
            _Data.CreatAI();
            GameMng.m_StartUpdate = true;
            _Data._ServerTime = GameData.SERVER_WAIT_TIME;

            //CMoonCamera c = GameObject.Find("_Manager/Moon").GetComponent<CMoonCamera>();
            c.InitGame();
            print("ok");
        }
        else
        {
            StartPointControl[] sp = FindObjectsOfType(typeof(StartPointControl)) as StartPointControl[];
            foreach (StartPointControl pos in sp)
            {
                pos.RandomPos();
            }
            CreatServerGUI();
            _Data.AIPositionZero();
            GameMng.m_StartUpdate = true;
            GameObject.Find("start1/start_plane1").GetComponent<Animation>().clip.SampleAnimation(GameObject.Find("start1/start_plane1"), 0);
            GameObject.Find("start2/start_plane1").GetComponent<Animation>().clip.SampleAnimation(GameObject.Find("start2/start_plane1"), 0);
            GameObject.Find("start3/start_plane1").GetComponent<Animation>().clip.SampleAnimation(GameObject.Find("start3/start_plane1"), 0);

            c.InitGame();
        }
		_Data.cycles = new Cycle_Control[GameData.MAX_PLAYER];
        //print("wait activate");

        nBGMID = Random.Range(4, 6);
        AudioCtr.Play(AudioCtr.snd_bgm[nBGMID], AudioCtr.BGM_VALUME, true);
        Debug.Log("�������");
        start = false;
        ftime = 0;

//		RankData[] rankdata = FindObjectsOfType (typeof(RankData)) as RankData[];
//		foreach (RankData r in rankdata)
//						r.Init ();
		//if( CheckLock("TazoBike") == 0 )
        //{		
	        
        //}
    }

    void CreatServerGUI()
    {
        GameObject obj = Instantiate((GameObject)Resources.Load("Server_view")) as GameObject;
        _Data._GUI = obj.GetComponent<BMX_Champ>();
    }

    void DestroyServerGUI()
    {
        Destroy(_Data._GUI.gameObject);
    }
	
	IEnumerator Countdown()
    {
        AudioCtr.Play( AudioCtr.snd_count[2] );
        yield return new WaitForSeconds(1);
        AudioCtr.Play( AudioCtr.snd_count[2] );
        yield return new WaitForSeconds(1);
        AudioCtr.Play( AudioCtr.snd_count[2] );
        yield return new WaitForSeconds(1);
        AudioCtr.Play( AudioCtr.snd_count[2] );
		yield return new WaitForSeconds(1);
        AudioCtr.Play( AudioCtr.snd_count[2] );		
        yield return new WaitForSeconds(1);
        AudioCtr.Play( AudioCtr.snd_count[2] );
        yield return new WaitForSeconds(1);
        AudioCtr.Play( AudioCtr.snd_count[2] );
        yield return new WaitForSeconds(1);
        AudioCtr.Play( AudioCtr.snd_count[2] );
		yield return new WaitForSeconds(1);
        AudioCtr.Play( AudioCtr.snd_count[2] );
		yield return new WaitForSeconds(1);
        AudioCtr.Play( AudioCtr.snd_count[2] );
		yield return new WaitForSeconds(1); 		
	}
}

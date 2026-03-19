using UnityEngine;
using System.Collections;

public class MTB_Client_InGame : GameState
{

    #region Members
    MTB_Client_Data _Data;

    bool creatUser = false;
    bool creatAI = false;
    bool connect = false;

    int ready;
    bool positionReady = false;

    bool startCountdown;
    int TickTime = 0;
    #endregion

    #region OnActivate
    public override void OnActivate()
    {
        StartCoroutine(Activate());
    }
    #endregion

    #region OnUpdate
    public override void OnUpdate()
    {
        if (_Data._ServerState == 2)
        {
            if (connect)
            {
                if (!creatUser)
                {
                    _Data.CreatBike();
// Unity6: if (Network.isServer)
                    {
                        switch (GameData.MTBMap)
                        {
                            case 0:
                                Mission.NetLoadMission("Prefeb_Map1/rollingStone");
                                break;
                            case 1:
                                Mission.NetLoadMission("Prefeb_Map2/Rolling Barrel1");
                                Mission.NetLoadMission("Prefeb_Map2/Rolling Barrel2");
                                break;
                            case 2:
                                Mission.NetLoadMission("Prefeb_Map3/Rolling car");
                                Mission.NetLoadMission("Prefeb_Map3/Rolling tire01");
                                Mission.NetLoadMission("Prefeb_Map3/Rolling tire02");
                                break;
                        }
                    }
                    AudioCtr.Play(AudioCtr.snd_bgm[GameData.MTBMap + 1], AudioCtr.BGM_VALUME, true);
                    creatUser = true;
                }
// Unity6: if (Network.isServer && !creatAI)
                {
                    Cycle_Control[] all = FindObjectsOfType(typeof(Cycle_Control)) as Cycle_Control[];
                    //foreach (Cycle_Control pos in all)
                    //{
                    //    _Data.players[pos.MyNumber] = pos.MyNumber;
                    //}
                    if (all.Length == _Data._ReadyPlayer + 1)
                    {
                        _Data.CreatAI();
                        creatAI = true;
                        _Data._ServerState = 3;
                        _Data._ServerTime = GameData.SERVER_READY_TIME;
// Unity6: networkView.RPC("SendSyncState", RPCMode.Others, _Data._ServerState, _Data._ServerTime);
                    }
                }
            }
            else
            {
                if (positionReady && ready == _Data._ReadyPlayer)
                {
                    StartPointControl sp = GameObject.Find("_Startpoint").GetComponent<StartPointControl>();
// Unity6: networkView.RPC("RPC_SendStartpointPosition", RPCMode.All, sp.name,
//                      sp.num[0], sp.num[1], sp.num[2], sp.num[3], sp.num[4],
//                      sp.num[5], sp.num[6], sp.num[7], sp.num[8], sp.num[9]);
                }
            }
        }
        else if (_Data._ServerState == 3)
        {
            _Data.Synctime();
            if (_Data._ServerTime <= 3 && !startCountdown)
            {
                startCountdown = true;
                StartCoroutine(Countdown());
            }
            if (_Data._ServerTime <= 0)
            {
                //GameObject.Find("start" + GameData.BMXMap + "/start_plane1").GetComponent<Animation>().Play();
                _Data.RankDataSetting();
                _Data._ServerState = 4;
// Unity6: if (Network.isServer)
                {
                    foreach (Cycle_Control pos in _Data.ai)
                    {
                        pos.Rb.isKinematic = false;
                        pos.cycle_Impact = true;
                        pos.gameStart = true;
                        pos.cycle_AI = true;
                    }
                }
                _Data.MyCharacter.Rb.isKinematic = false;
                _Data.MyCharacter.cycle_Move = true;
                _Data.MyCharacter.gameStart = true;
                _Data.MyCharacter.cycle_Impact = true;
            }
        }
        else if (_Data._ServerState == 4)
        {
            if (_Data.MyCharacter.gameFinish)
            {
                _Data._ServerState = 6;
            }

            if (_Data.FindFinishPlayer() > 2)
            {
                _Data._ServerState = 5;
                _Data._ServerTime = GameData.SERVER_FINISH_TIME;
            }
            if (!_Data.MyCharacter.gameFinish)
                _Data._GameGUI.TotalTime(Time.deltaTime);
            _Data._GameGUI.Calorie(_Data.MyCharacter.moveValue.pedalSpeed);
            _Data._GameGUI.Distance(_Data.MyCharacter.moveValue.realSpeed);
            _Data._GameGUI.Speed(_Data.MyCharacter.moveValue.realSpeed);
            RankGUI();
        }
        else if (_Data._ServerState == 5)
        {
            if (_Data.MyCharacter.gameFinish)
            {
                _Data._ServerState = 6;
            }

            if (TickTime != (int)_Data._ServerTime)
            {
                //Debug.Log(TickTime);
                TickTime = (int)_Data._ServerTime;
                if (TickTime <= 10)
                {
                    //Debug.Log("Count Down");
                    if (TickTime < 0) TickTime = 0;
                    _Data._GameGUI.EndCountdown(TickTime);
                }
            }

            _Data.Synctime();

            if (_Data._ServerTime <= -2.0f)
            {
                _Data._ServerState = 6;
            }
            if (!_Data.MyCharacter.gameFinish)
                _Data._GameGUI.TotalTime(Time.deltaTime);
            _Data._GameGUI.Calorie(_Data.MyCharacter.moveValue.pedalSpeed);
            _Data._GameGUI.Distance(_Data.MyCharacter.moveValue.realSpeed);
            _Data._GameGUI.Speed(_Data.MyCharacter.moveValue.realSpeed);
            RankGUI();
        }
        else if (_Data._ServerState == 6)
        {
            _Data._ServerTime = GameData.SERVER_FINISH_TIME;
            _Data._GameGUI.GameOver(false);
            Debug.Log(" next State Result ");
            gameObject.AddComponent<MTB_Client_Result>();
            StateControl.gameMng.SetState(typeof(MTB_Client_Result));

        }
        if (_Data._GUI)
        {
            if (_Data.MyCharacter.environment == Cycle_Control.Environment.Mud)
            {
                _Data._GameGUI.MudEffect();
            }
            else if (_Data.MyCharacter.environment == Cycle_Control.Environment.Water)
            {
                _Data._GameGUI.WaterEffect();
            }
        }
    }
    #endregion

    #region OnDeactivate
    public override void OnDeactivate()
    {
        AudioCtr.Stop(AudioCtr.snd_bgm[GameData.MTBMap + 1]);
        DestroyImmediate(this);
    }
    #endregion

    IEnumerator Activate()
    {
        //AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(GameData.map[GameData.MTBMap]);
        yield return StartCoroutine(LoadBundle.DownLoadBundle(GameData.map[GameData.MTBMap]));//async;

        TickTime = 0;
        _Data = GetComponent<MTB_Client_Data>();

        _Data.InGameState = true;
        GameObject obj = Instantiate((GameObject)Resources.Load("Game")) as GameObject;
        _Data._GameGUI = obj.GetComponent<InGameGUI>();

        Object startpoint = Instantiate((GameObject)Resources.Load("Prefeb/_Startpoint_M_" + GameData.MTBMap));
        startpoint.name = "_Startpoint";

// Unity6: if (Network.isServer)
        {
            StartPointControl[] sp = FindObjectsOfType(typeof(StartPointControl)) as StartPointControl[];
            foreach (StartPointControl pos in sp)
            {
                pos.Init();
                pos.RandomPos();
            }
            positionReady = true;
// Unity6: //Network.InitializeServer(GameData.MAX_PLAYER, 55000, false);
        }
        // else { } // Unity6: networkView.RPC("SendReady") removed
        GameMng.m_StartUpdate = true;
		AudioListener.volume = 1.0f;
    }

    IEnumerator Countdown()
    {
        _Data._GameGUI.StartCountdown(0);
        AudioCtr.Play(AudioCtr.snd_count[10]);
        yield return new WaitForSeconds(1);
        _Data._GameGUI.StartCountdown(1);
        AudioCtr.Play(AudioCtr.snd_count[10]);
        yield return new WaitForSeconds(1);
        _Data._GameGUI.StartCountdown(2);
        AudioCtr.Play(AudioCtr.snd_count[10]);
        yield return new WaitForSeconds(1);
        _Data._GameGUI.StartCountdown(3);
        AudioCtr.Play(AudioCtr.snd_count[11]);
    }

    // [RPC] removed Unity6 Migration
    void SendReady()
    {
        ready++;
    }

    // [RPC] removed Unity6 Migration
    void RPC_SendStartpointPosition(string name, int _0, int _1, int _2, int _3, int _4, int _5, int _6, int _7, int _8, int _9)
    {
        if (!connect)
        {
// Unity6: if (Network.isClient)
            {
                StartPointControl sp = FindObjectOfType(typeof(StartPointControl)) as StartPointControl;
                sp.Init();
                sp.num[0] = _0;
                sp.num[1] = _1;
                sp.num[2] = _2;
                sp.num[3] = _3;
                sp.num[4] = _4;
                sp.num[5] = _5;
                sp.num[6] = _6;
                sp.num[7] = _7;
                sp.num[8] = _8;
                sp.num[9] = _9;
                sp.SetPos();
            }
            connect = true;
        }
    }

    void RankGUI()
    {
        for (int i = 0; i < _Data.cycles.Length; i++)
        {
            _Data._GameGUI.currentRank[i] = _Data.cycles[i].rank - 1;
        }
        int count = 0;
        for (int i = 0; i < _Data.cycles.Length; i++)
        {
            if (_Data._GameGUI.currentRank[_Data.MyCharacter.MyNumber] == _Data._GameGUI.currentRank[i])
                count++;
        }
        if (count < 2)
            _Data._GameGUI.RankPos(_Data.MyCharacter.rank - 1);
    }
}

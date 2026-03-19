using UnityEngine;
using System.Collections;

public class BMX_Client_Wait : GameState {

    #region Members
    BMX_Client_Data _Data;
    int nBGMID = 0;
    #endregion

    #region OnActivate
    public override void OnActivate()
    {
        _Data = GetComponent<BMX_Client_Data>();
        StartCoroutine(Activate());
    }
    #endregion

    #region OnUpdate
    public override void OnUpdate()
    {
        _Data.SyncServerTime();
        if (_Data._ServerState == 0)
        {
            if (_Data._User)
            {
                _Data._GUI.Speed(_Data._User.moveValue.realSpeed);
                if (_Data._ServerTime >= 0)
                {
                    _Data._GUI.WaitMentShow(2);
                    _Data._GUI.ShowServerTime(_Data._ServerTime);
                    //if (_Data._ServerTime < 5)
                    //{
                    //    _Data._User.cycle_Impact = false;
                    //}
                }
                else
                {
                    _Data._GUI.WaitMentShow(1);
                }
            }
            //if (Input.GetKeyDown(KeyCode.Q) || CBikeSerial.GetNewButton(1))
            //{
// Unity6: //    Network.Disconnect();
            //    gameObject.AddComponent<Menu_SelectGame>();
            //    StateControl.gameMng.SetState(typeof(Menu_SelectGame));
            //    DestroyImmediate(_Data);
            //}
        }
        if (_Data._ServerState == 1)
        {
            _Data._GUI.WaitMentShow(0);
            _Data._User.Rb.isKinematic = true;
            _Data._User.cycle_AI = false;
            _Data._User.cycle_Move = false;
            _Data._User.cycle_Impact = false;
            //_Data._User.deadState = 0;
            _Data._GUI.Speed(0);
            _Data._GUI.ShowServerTime(0);
        }
        else if (_Data._ServerState == 2)
        {
            gameObject.AddComponent<BMX_Client_InGame>();
            StateControl.gameMng.SetState(typeof(BMX_Client_InGame));
        }
    }
    #endregion

    #region OnDeactivate
    public override void OnDeactivate()
    {
        AudioCtr.Stop(AudioCtr.snd_bgm[nBGMID]);
        DestroyImmediate(this);
    }
    #endregion

    IEnumerator Activate()
    {
        //AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Map5");
        yield return StartCoroutine(LoadBundle.DownLoadBundle("Map5"));//async;

        GameObject obj = Instantiate((GameObject)Resources.Load("Game")) as GameObject;
        _Data._GUI = obj.GetComponent<InGameGUI>();

        Object sp = Instantiate((GameObject)Resources.Load("Prefeb/_Startpoint1"));
        sp.name = "_Startpoint1";
        sp = Instantiate((GameObject)Resources.Load("Prefeb/_Startpoint2"));
        sp.name = "_Startpoint2";
        sp = Instantiate((GameObject)Resources.Load("Prefeb/_Startpoint3"));
        sp.name = "_Startpoint3";

        StartPointControl[] startpos = FindObjectsOfType(typeof(StartPointControl)) as StartPointControl[];
        foreach (StartPointControl pos in startpos)
        {
            pos.Init();
            pos.RandomPos();
        }

// Unity6: Network.Connect(GameData.SERVER_IP, 53000, "BMX");

        GameMng.m_StartUpdate = true;

        nBGMID = Random.Range(4, 6);
        AudioCtr.Play(AudioCtr.snd_bgm[ nBGMID ], AudioCtr.BGM_VALUME, true);
    }
}

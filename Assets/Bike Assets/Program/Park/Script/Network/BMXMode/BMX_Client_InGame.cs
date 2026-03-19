using UnityEngine;
using System.Collections;

public class BMX_Client_InGame : GameState {

    #region Members
    BMX_Client_Data _Data;
    bool startCountdown;
	int TickTime = 0;
    #endregion

    #region OnActivate
    public override void OnActivate()
    {
		TickTime = 0;
        _Data = GetComponent<BMX_Client_Data>();
        _Data.UserStartPosition();
        GameMng.m_StartUpdate = true;
		AudioListener.volume = 1.0f;
		
		AudioCtr.Play( AudioCtr.snd_bgm[ GameData.BMXMap ], AudioCtr.BGM_VALUME, true );
    }
    #endregion

    #region OnUpdate
    public override void OnUpdate()
    {   
        switch (_Data._ServerState)
        {
            case 2:
                _Data._GUI.TotalTime(0);
                _Data._GUI.Speed(0);
                _Data.SyncServerTime();
                if (_Data._ServerTime <= 3 && !startCountdown)
                {
                    startCountdown = true;
                    StartCoroutine(Countdown());
                }
                if (_Data._ServerTime <= 0)
                {
                    GameObject.Find("start" + GameData.BMXMap + "/start_plane1").GetComponent<Animation>().Play();
                    _Data.FindCycles();
                    _Data._ServerState = 3;
                    _Data._User.Rb.isKinematic = false;
                    _Data._User.cycle_Impact = true;
                    _Data._User.cycle_Move = true;
                    _Data._User.cycle_AI = false;
                    _Data._User.checkRespawn = true;
                }
                break;
            case 3:
                if (_Data._User.gameFinish)
                {
                    _Data._ServerState = 5;
                    _Data._User.cycle_AI = true;
                    _Data._User.cycle_Move = false;
                }
				if( !_Data._User.gameFinish )
					_Data._GUI.TotalTime(  Time.deltaTime );
                _Data._GUI.Calorie(_Data._User.moveValue.pedalSpeed);
                _Data._GUI.Distance(_Data._User.moveValue.realSpeed);
                _Data._GUI.Speed(_Data._User.moveValue.realSpeed);
                RankGUI();
                break;
            case 4:
                
                if (_Data._User.gameFinish)
                {
                    _Data._ServerState = 5;
                    _Data._User.cycle_AI = true;
                    _Data._User.cycle_Move = false;
                }
			
				if( TickTime != (int)_Data._ServerTime )
				{
					//Debug.Log(TickTime);
					TickTime = (int)_Data._ServerTime;
					if( TickTime <= 10 )
					{
						//Debug.Log("Count Down");
						if( TickTime < 0 ) TickTime = 0;
						_Data._GUI.EndCountdown( TickTime );
					}
				}
				
				_Data.SyncServerTime();
			
				if( !_Data._User.gameFinish )
					_Data._GUI.TotalTime(  Time.deltaTime );
                _Data._GUI.Calorie(_Data._User.moveValue.pedalSpeed);
                _Data._GUI.Distance(_Data._User.moveValue.realSpeed);
                _Data._GUI.Speed(_Data._User.moveValue.realSpeed);
                RankGUI();
                break;
            case 5:
                _Data._User.cycle_AI = true;
                _Data._User.cycle_Move = false;
			
				_Data._GUI.GameOver(false);
				Debug.Log(" next State Result ");
				gameObject.AddComponent<BMX_Client_Result>();
				StateControl.gameMng.SetState(typeof(BMX_Client_Result));
				
                break;
        }
        if (_Data._User.environment == Cycle_Control.Environment.Mud)
        {
            _Data._GUI.MudEffect();
        }
        else if (_Data._User.environment == Cycle_Control.Environment.Water)
        {
            _Data._GUI.WaterEffect();
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

    void RankGUI()
    {
        for (int i = 0; i < _Data.cycles.Length; i++)
        {
            _Data._GUI.currentRank[i] = _Data.cycles[i].rank - 1;
        }
        int count = 0;
        for (int i = 0; i < _Data.cycles.Length; i++)
        {
            if (_Data._GUI.currentRank[_Data._User.MyNumber] == _Data._GUI.currentRank[i])
                count++;
        }
        if (count < 2)
            _Data._GUI.RankPos(_Data._User.rank - 1);
    }

    IEnumerator Countdown()
    {
        _Data._GUI.StartCountdown(0);
		AudioCtr.Play( AudioCtr.snd_count[10] );
        yield return new WaitForSeconds(1);
        _Data._GUI.StartCountdown(1);
		AudioCtr.Play( AudioCtr.snd_count[10] );
        yield return new WaitForSeconds(1);
        _Data._GUI.StartCountdown(2);
		AudioCtr.Play( AudioCtr.snd_count[10] );
        yield return new WaitForSeconds(1);
        _Data._GUI.StartCountdown(3);
		AudioCtr.Play( AudioCtr.snd_count[11] );
    }
}

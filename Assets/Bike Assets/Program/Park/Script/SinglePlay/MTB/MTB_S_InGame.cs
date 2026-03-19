using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Unity6 Migration

public class MTB_S_InGame : GameState {

    #region Members
    MTB_S_Data data;

    bool rankStart = false;
	
	float fDelayTime;
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
        if (rankStart)
        {
            if (data.MyCharacter.gameFinish)
            {
                Debug.Log(" next State Result ");
                gameObject.AddComponent<MTB_S_Result>();
                StateControl.gameMng.SetState(typeof(MTB_S_Result));
            }

            if (data.FindFinishPlayer() > 2 || data.MyCharacter.gameFinish)
			{
				if( TickTime != (int)fDelayTime )
				{
					//Debug.Log(TickTime);
					TickTime = (int)fDelayTime;
					if( TickTime <= 10 )
					{
						//Debug.Log("Count Down");
						if( TickTime < 0 ) TickTime = 0;
						data.gui.EndCountdown( TickTime );
					}
				}
				fDelayTime -= Time.deltaTime;
				if( fDelayTime <= -2.0f )
				{
					data.gui.GameOver(false);
					Debug.Log(" next State MTB_S_Result ");
					gameObject.AddComponent<MTB_S_Result>();
        			StateControl.gameMng.SetState(typeof(MTB_S_Result));
				}
			}
			if( !data.MyCharacter.gameFinish )
				data.gui.TotalTime(  Time.deltaTime );	
            data.gui.Distance(data.MyCharacter.moveValue.realSpeed);
            data.gui.Speed(data.MyCharacter.moveValue.realSpeed);
            data.gui.Calorie(data.MyCharacter.moveValue.pedalSpeed);
            RankGUI();
            if (data.MyCharacter.environment == Cycle_Control.Environment.Mud)
            {
                data.gui.MudEffect();
            }
            else if (data.MyCharacter.environment == Cycle_Control.Environment.Water)
            {
                data.gui.WaterEffect();
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
        AsyncOperation async = SceneManager.LoadSceneAsync(GameData.map[GameData.MTBMap]); // Unity6 Migration
        yield return async;

        Object obj = Instantiate((GameObject)Resources.Load("Prefeb/_Startpoint_M_" + GameData.MTBMap));
        obj.name = "_Startpoint";

        StartPointControl[] sp = FindObjectsOfType(typeof(StartPointControl)) as StartPointControl[];
        foreach (StartPointControl pos in sp)
        {
            pos.Init();
            pos.RandomPos();
        }

		switch( GameData.MTBMap )
		{
		case 0:
			Mission.LoadMission("Prefeb_Map1/rollingStone");
			break;
		case 1:
			Mission.LoadMission("Prefeb_Map2/Rolling Barrel1");
			Mission.LoadMission("Prefeb_Map2/Rolling Barrel2");
			break;
		case 2:
			Mission.LoadMission("Prefeb_Map3/Rolling car");
			Mission.LoadMission("Prefeb_Map3/Rolling tire01");
			Mission.LoadMission("Prefeb_Map3/Rolling tire02");				
			break;
		}

		TickTime = 0;
		fDelayTime = 10.0f;
        data = FindObjectOfType(typeof(MTB_S_Data)) as MTB_S_Data;
		
		data.remainTime = GameData.remainTime[GameData.MTBMap];
		
        data.CreatGUI();
        data.CreatBike();
        StartCoroutine(Ready());
        GameMng.m_StartUpdate = true;
		
		AudioCtr.Play( AudioCtr.snd_bgm[ GameData.MTBMap+1 ], AudioCtr.BGM_VALUME, true );		
		AudioListener.volume = 1.0f;
    }

    IEnumerator Ready()
    {
        yield return new WaitForSeconds(1);
        data.gui.StartCountdown(0);
		AudioCtr.Play( AudioCtr.snd_count[10] );
        yield return new WaitForSeconds(1);
        data.gui.StartCountdown(1);
		AudioCtr.Play( AudioCtr.snd_count[10] );
        yield return new WaitForSeconds(1);
        data.gui.StartCountdown(2);
		AudioCtr.Play( AudioCtr.snd_count[10] );
        yield return new WaitForSeconds(1);
        data.gui.StartCountdown(3);
		AudioCtr.Play( AudioCtr.snd_count[11] );
		
        //GameObject.Find("start" + GameData.BMXMap + "/start_plane1").animation.Play();
        Cycle_Control[] allcycles = FindObjectsOfType(typeof(Cycle_Control)) as Cycle_Control[];
        for (int i = 0; i < allcycles.Length; i++)
        {
            data.cycles[allcycles[i].MyNumber] = allcycles[i];
        }

        data.rankData = GameObject.Find("_Waypoint").GetComponent<RankData>();
        data.rankData.cycles = data.cycles;
        data.rankData.Init();
        rankStart = true;

        foreach (Cycle_Control pos in data.ai)
        {
            pos.cycle_AI = true;
            pos.gameStart = true;
            pos.cycle_Impact = true;
            pos.Rb.isKinematic = false; // Unity6 Migration
        }
        data.MyCharacter.cycle_Move = true;
        data.MyCharacter.gameStart = true;
        data.MyCharacter.cycle_Impact = true;
        data.MyCharacter.Rb.isKinematic = false; // Unity6 Migration
    }

    
    void RankGUI()
    {
        for (int i = 0; i < data.cycles.Length; i++)
        {
            data.gui.currentRank[i] = data.cycles[i].rank - 1;
        }
        int count = 0;
        for (int i = 0; i < data.cycles.Length; i++)
        {
            if (data.gui.currentRank[0] == data.gui.currentRank[i])
                count++;
        }
        if (count < 2)
            data.gui.RankPos(data.MyCharacter.rank - 1);
    }
}

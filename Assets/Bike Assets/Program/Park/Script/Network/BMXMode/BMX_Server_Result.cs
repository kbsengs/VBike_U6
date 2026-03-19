using UnityEngine;
using System.Collections;

public class BMX_Server_Result : GameState {

	 #region Members
    BMX_Server_Data data;
    #endregion

	float fTime = 0.0f;
	
    #region OnActivate
    public override void OnActivate()
    {
    	fTime = GameData.SERVER_FINISH_TIME;
    	data = GetComponent<BMX_Server_Data>();
        if (data._RankData.ranklist[0].fPlayTime >= data._RankData.ranklist[1].fPlayTime)
            data._RankData.ranklist[1].fPlayTime = data._RankData.ranklist[0].fPlayTime + Random.value * 0.03f + 0.01f;
        if (data._RankData.ranklist[1].fPlayTime >= data._RankData.ranklist[2].fPlayTime)
            data._RankData.ranklist[2].fPlayTime = data._RankData.ranklist[1].fPlayTime + Random.value * 0.03f + 0.01f;
        data._GameGUI.ShowResult(data._RankData.ranklist[0].MyNumber, data._RankData.ranklist[1].MyNumber, data._RankData.ranklist[2].MyNumber,
		                         data._RankData.ranklist[0].fPlayTime, data._RankData.ranklist[1].fPlayTime, data._RankData.ranklist[2].fPlayTime);
        GameMng.m_StartUpdate = true;

        AudioCtr.Play(AudioCtr.snd_bgm[6], AudioCtr.BGM_VALUME, false);
    }
    #endregion

    #region OnDeactivate
    public override void OnDeactivate()
    {
        AudioCtr.Stop(AudioCtr.snd_bgm[6]);
    	fTime = 0;
//		data._RankData.ClearData ();
        //DestroyImmediate(data);
        Destroy(GameObject.Find("_GameGUI"));
        DestroyImmediate(this);
    }
    #endregion
    
    #region OnUpdate
    public override void OnUpdate()
    {
		data.SyncServerTime();
        if (data._ServerTime <= 0)
        {
            data._ServerState = 0;
            data._ServerTime = GameData.SERVER_WAIT_TIME;
            GameData.BMXMap++;
            if (GameData.BMXMap > 3) GameData.BMXMap = 1;
            data.EndState(data._ServerState, data._ServerTime);
        }
    }
    #endregion
}

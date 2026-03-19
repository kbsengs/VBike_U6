using UnityEngine;
using System.Collections;

public class BMX_Client_Result : GameState {

	 #region Members
    BMX_Client_Data data;
    #endregion

	
    #region OnActivate
    public override void OnActivate()
    {	
    	data = GetComponent<BMX_Client_Data>();
        data._User.cycle_Move = false;
        data._User.cycle_AI = true;
        GameMng.m_StartUpdate = true;        
		
		data._ServerTime = GameData.SERVER_FINISH_TIME;
		data._GUI.SingleResult( data._User.rank, data._User.MyNumber, data._GUI.myTotalTime , data._GUI.myDistance, data._GUI.myCalorie );
    }
    #endregion

    #region OnDeactivate
    public override void OnDeactivate()
    {
        //DestroyImmediate(data);
        DestroyImmediate(this);
    }
    #endregion
    
    #region OnUpdate
    public override void OnUpdate()
    {
        //data.SyncServerTime();
		
        //if( data._ServerTime <= -10.0f )
        //{
// Unity6: //    Network.Disconnect(200);
        //    Debug.Log("Next State Menu_SelectGame");
        //    gameObject.AddComponent<Menu_SelectGame>();
        //    StateControl.gameMng.SetState(typeof(Menu_SelectGame));
        //}
		
    }
    #endregion

}

using UnityEngine;
using System.Collections;

public class BMX_S_Result : GameState {

	 #region Members
    BMX_S_Data data;
    #endregion

	float fTime = 0.0f;
	
    #region OnActivate
    public override void OnActivate()
    {
    	fTime = GameData.SERVER_FINISH_TIME;
    	data = GetComponent<BMX_S_Data>();
        //data = FindObjectOfType(typeof(BMX_S_Data)) as BMX_S_Data;
        data.MyCharacter.cycle_Move = false;
        data.MyCharacter.cycle_AI = true;
        GameMng.m_StartUpdate = true;
        //data.gui.SingleResult(1,2,123.345f,3.56f,32.23f );
		data.gui.SingleResult( data.MyCharacter.rank, data.MyCharacter.MyNumber, data.gui.myTotalTime , data.gui.myDistance, data.gui.myCalorie );
    }
    #endregion

    #region OnDeactivate
    public override void OnDeactivate()
    {
    	fTime = 0;
		DestroyImmediate(data);
        DestroyImmediate(this);
    }
    #endregion
    
    #region OnUpdate
    public override void OnUpdate()
    {
		
    	fTime -= Time.deltaTime;
		
    	if( fTime <= 0 )
		{
			Debug.Log("Next State Menu_SelectGame");
			gameObject.AddComponent<Menu_SelectGame>();
			StateControl.gameMng.SetState(typeof(Menu_SelectGame));
		}
		
    }
    #endregion

}

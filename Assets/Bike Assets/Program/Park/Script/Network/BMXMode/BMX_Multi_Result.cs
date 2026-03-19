using UnityEngine;
using System.Collections;

public class BMX_Multi_Result : GameState
{

    #region Members
    BMX_Multi_Data data;
    #endregion

    float fTime = 0.0f;

    #region OnActivate
    public override void OnActivate()
    {
        fTime = GameData.SERVER_FINISH_TIME;
        data = GetComponent<BMX_Multi_Data>();
        data.MyCharacter.cycle_Move = false;
        data.MyCharacter.cycle_AI = true;
        GameMng.m_StartUpdate = true;
        data._GameGUI.SingleResult(data.MyCharacter.rank, data.MyCharacter.MyNumber, data._GameGUI.myTotalTime, data._GameGUI.myDistance, data._GameGUI.myCalorie);
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

        data.Synctime();

        if (data._ServerTime <= 0)
        {
            Debug.Log("Next State Menu_SelectGame");
            gameObject.AddComponent<Menu_SelectGame>();
            StateControl.gameMng.SetState(typeof(Menu_SelectGame));
        }

    }
    #endregion

}

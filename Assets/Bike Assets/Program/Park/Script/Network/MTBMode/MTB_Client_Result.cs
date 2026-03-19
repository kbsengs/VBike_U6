using UnityEngine;
using System.Collections;

public class MTB_Client_Result : GameState
{

    #region Members
    MTB_Client_Data data;
    int player;
    bool finish;
    #endregion

    float fTime = 0.0f;

    #region OnActivate
    public override void OnActivate()
    {
        fTime = GameData.SERVER_FINISH_TIME;
        data = GetComponent<MTB_Client_Data>();
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
// Unity6: if (Network.isServer)
            {
                if (player >= data._ReadyPlayer)
                {
                    finish = true;
// Unity6: networkView.RPC("SendAllFinish", RPCMode.All);
                }
            }
// Unity6: else if (Network.isClient)
            {
                finish = true;
// Unity6: networkView.RPC("SendFinish", RPCMode.Server);
            }
        }

    }
    #endregion
    // [RPC] removed Unity6 Migration
    void SendFinish()
    {
        player++;
    }

    // [RPC] removed Unity6 Migration
    void SendAllFinish()
    {
        finish = true;
        Debug.Log("Next State Menu_SelectGame");
        gameObject.AddComponent<Menu_SelectGame>();
        StateControl.gameMng.SetState(typeof(Menu_SelectGame));
    }
}

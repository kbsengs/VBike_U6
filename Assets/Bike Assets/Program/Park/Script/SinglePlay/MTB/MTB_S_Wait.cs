using UnityEngine;
using System.Collections;

public class MTB_S_Wait : GameState {

    #region Members
    MTB_S_Data data;
    #endregion

    #region OnActivate
    public override void OnActivate()
    {
        data = FindObjectOfType(typeof(MTB_S_Data)) as MTB_S_Data;
        GameMng.m_StartUpdate = true;
    }
    #endregion

    #region OnUpdate
    public override void OnUpdate()
    {
        gameObject.AddComponent<MTB_S_InGame>();
        StateControl.gameMng.SetState(typeof(MTB_S_InGame));
    }
    #endregion

    #region OnDeactivate
    public override void OnDeactivate()
    {
        DestroyImmediate(this);
    }
    #endregion
}

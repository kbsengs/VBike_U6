using UnityEngine;
using System.Collections;

public class BMX_S_Wait : GameState {

    #region Members
    BMX_S_Data data;
    #endregion

    #region OnActivate
    public override void OnActivate()
    {
        data = FindObjectOfType(typeof(BMX_S_Data)) as BMX_S_Data;
        
        GameMng.m_StartUpdate = true;
    }
    #endregion

    #region OnUpdate
    public override void OnUpdate()
    {
        gameObject.AddComponent<BMX_S_InGame>();
        StateControl.gameMng.SetState(typeof(BMX_S_InGame));
    }
    #endregion

    #region OnDeactivate
    public override void OnDeactivate()
    {
        DestroyImmediate(this);
    }
    #endregion
   
}

using UnityEngine;
using System.Collections;

public class StateDemo : GameState
{
    #region Members
    #endregion

    #region OnActivate
    public override void OnActivate()
    {
        GameMng.m_StartUpdate = true;    
    }
    #endregion

    #region OnUpdate
    public override void OnUpdate()
    {

    }
    #endregion

    #region OnDeactivate
    public override void OnDeactivate()
    {
        
    }
    #endregion
}
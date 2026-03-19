using UnityEngine;
using System.Collections;

public class Config : GameState {

    #region Members
    public enum SetSerial
    {
        False = 0, True = 1
    }
    public static SetSerial _Serial = SetSerial.True;
    public enum Set3D
    {
        False = 1, True = 2
    }
    public static Set3D _3D = Set3D.False;
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
        GameMng.m_StartUpdate = true;
    }
    #endregion
}

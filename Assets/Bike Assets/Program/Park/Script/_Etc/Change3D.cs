using UnityEngine;
using System.Collections;

public class Change3D : GameState {

    #region Members

    #endregion

    #region Functions

    public override void OnActivate()
    {
        gameObject.AddComponent<Menu_SelectGame>();
        StateControl.gameMng.SetState(typeof(Menu_SelectGame));
    }

    public override void OnDeactivate()
    {
        DestroyImmediate(this);
    }

    public override void OnUpdate()
    {

    }

    #endregion
}

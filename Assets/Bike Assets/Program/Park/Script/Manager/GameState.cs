using UnityEngine;
using System.Collections;

public abstract class GameState : MonoBehaviour {

    #region Members

    #endregion

    #region Functions

    public abstract void OnActivate();

    public abstract void OnDeactivate();

    public abstract void OnUpdate();

    public virtual void OnFixedUpdate() { }

    #endregion
}

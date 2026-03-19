using UnityEngine;
using System.Collections;

public class GameMng : MonoBehaviour
{
    #region Members

    public string m_NowStateName; //현재 스테이트 이름
    
    private GameState m_State; // 현재 스테이트
    
    public static bool m_StartUpdate = false; // 스크립트 업데이트 함수 실행

    #endregion 

    #region UnityFunctions

    void Update()
    {
        if (m_State != null && m_StartUpdate)
        {
            m_State.OnUpdate(); //해당 스테이트 계속 실행
        }
    }

    void FixedUpdate()
    {
        if (m_State != null && m_StartUpdate)
        {
            m_State.OnFixedUpdate(); //해당 스테이트 계속 실행
        }
    }

    #endregion

    #region SetStateFunction

    public void SetState(System.Type newStateType) //스테이트 이동 함수
    {

//        if (!GameData.m_bLock) return;

        m_StartUpdate = false;
        if (m_State != null)
        {   
            m_State.OnDeactivate();
        }

        m_State = GetComponentInChildren(newStateType) as GameState;
        if (StateControl.m_State == null || StateControl.m_State.ToString() != newStateType.ToString())
        {
            StateControl.m_State = newStateType;
            m_NowStateName = newStateType.ToString();

            if (m_State != null)
            {
                m_State.OnActivate();
            }
        }
    }
    #endregion
}

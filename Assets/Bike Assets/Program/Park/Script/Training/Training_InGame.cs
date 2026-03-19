using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Unity6 Migration

public class Training_InGame : GameState {

    #region Members
    Training_Data data;
    bool countdown = false;
    int nBGMID = 0;
    #endregion

    #region OnActivate
    public override void OnActivate()
    {
        GameMng.m_StartUpdate = false; // Unity6 Migration: prevent OnUpdate running before Activate() completes
        data = FindObjectOfType(typeof(Training_Data)) as Training_Data;
        StartCoroutine(Activate());
    }
    #endregion

    private bool result;
    private float resultTime = 10.0f;
    #region OnUpdate
    public override void OnUpdate()
    {
        if (countdown)
            data.remainTime -= Time.deltaTime;
        //if (data.remainTime <= 10)
        //{
        //    if (!countdown)
        //        StartCoroutine(Countdown());
        //    countdown = true;
        //}

        if (data.remainTime <= 0)
        {
            data.remainTime = 0;
        }
        //data._GUI.Training_RemainTime(data.remainTime);
        //if (data.remainTime == 0)
        if (!result)
        {
            if (data.remainTime == 0 || (Input.GetKeyDown(KeyCode.Alpha1) && GameData.FREE_MODE) || (CBikeSerial.GetNewButton(1) && GameData.FREE_MODE))
            {
                result = true;
                if (data._GUI != null) data._GUI.ShowResult();
            }
            if (data._GUI != null && data.MyCharacter != null)
            {
                data._GUI.speed = data.MyCharacter.moveValue.realSpeed;
                data._GUI.calorie = data.MyCharacter.moveValue.pedalSpeed;
                data._GUI.distance = data.MyCharacter.moveValue.realSpeed;
                data._GUI.remainTime = data.remainTime;
            }
        }
        else
        {
            resultTime -= Time.deltaTime;
            resultTime = Mathf.Clamp(resultTime, 0.0f, 10.0f);
            if (resultTime == 0 || (Input.GetKeyDown(KeyCode.Alpha1) && GameData.FREE_MODE) || (CBikeSerial.GetNewButton(1) && GameData.FREE_MODE))
            {
                gameObject.AddComponent<Menu_SelectGame>();
                StateControl.gameMng.SetState(typeof(Menu_SelectGame));
            }
        }
    }
    #endregion

    #region OnDeactivate
    public override void OnDeactivate()
    {
        AudioCtr.Stop(AudioCtr.snd_bgm[nBGMID ]);
        DestroyImmediate(data);
        DestroyImmediate(this);
    }
    #endregion

    IEnumerator Activate()
    {
        //data.remainTime = GameData.remainTime[GameData.MTBMap];
        string targetMap = "Demo02";
        if (GameData.TraningMap == 1)
            targetMap = "Demo03";
        else if (GameData.TraningMap == 2)
            targetMap = "Tracking03";
        AsyncOperation async = SceneManager.LoadSceneAsync(targetMap); // Unity6 Migration
        yield return async;
        data.CreatGUI();
        data.CreatBike();
        data.remainTime = GameData.TRAINING_TIME * 60.0f;
        //data._GUI.TraningShow();
        GameMng.m_StartUpdate = true;
		
        if (GameData.TraningMap == 0)
		    Mission.LoadMission("Prefeb_Map1/AllSeagull");
		AudioListener.volume = 1.0f;
        AudioCtr.Play(AudioCtr.snd_bgm[nBGMID], AudioCtr.BGM_VALUME, true);
        if (!countdown)
            StartCoroutine(Countdown());
        countdown = true;
    }

    IEnumerator Countdown()
    {
        //data._GUI.EndCountdown(9);
        //yield return new WaitForSeconds(1);
        //data._GUI.EndCountdown(8);
        //yield return new WaitForSeconds(1);
        //data._GUI.EndCountdown(7);
        //yield return new WaitForSeconds(1);
        //data._GUI.EndCountdown(6);
        //yield return new WaitForSeconds(1);
        //data._GUI.EndCountdown(5);
        //yield return new WaitForSeconds(1);
        //data._GUI.EndCountdown(4);
        //yield return new WaitForSeconds(1);
        //data._GUI.EndCountdown(3);
        //yield return new WaitForSeconds(1);
        //data._GUI.EndCountdown(2);
        //yield return new WaitForSeconds(1);
        //data._GUI.EndCountdown(1);
        yield return new WaitForSeconds(10);
    }
}

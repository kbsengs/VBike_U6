using UnityEngine;
using System.Collections;

public class DemoMode : GameState {

    float demoTime;
    bool end;
    #region OnActivate
    public override void OnActivate()
    {
        
    }
    #endregion

    #region OnUpdate
    public override void OnUpdate()
    {
        demoTime += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space) || CBikeSerial.GetNewButton(1))
        {
            demoTime = 20;
        }
        if (demoTime >= 20 && !end)
        {
            end = true;
            demoTime = 0;
            gameObject.AddComponent<Menu_SelectGame>();
            StateControl.gameMng.SetState(typeof(Menu_SelectGame));
        }
    }
    #endregion

    #region OnDeactivate
    public override void OnDeactivate()
    {
        AudioCtr.Stop(AudioCtr.snd_bgm[0]);
        DestroyImmediate(this);
    }
    #endregion


    IEnumerator Activate()
    {
        GameData.demo = (GameData.demo + 1) % 2;
        yield return StartCoroutine(LoadBundle.DownLoadBundle(GameData.map[3]));//async;

        GameMng.m_StartUpdate = true;
        AudioCtr.Play(AudioCtr.snd_bgm[0], AudioCtr.BGM_VALUME, true);	
    }
}

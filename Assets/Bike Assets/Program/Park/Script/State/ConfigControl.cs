using UnityEngine;
using System.Collections;

public class ConfigControl : GameState {

    int state = -2;

    #region OnActivate
    public override void OnActivate()
    {
        StartCoroutine(Activate());
    }
    #endregion

    IEnumerator Activate()
    {
        AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Menu");
        yield return async;
        GameMng.m_StartUpdate = true;
    }

    #region OnUpdate
    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.A) || CBikeSerial.GetNewButton(0))
        {
            state++;
            if (state > 3) state = -2;
        }

        if (state == -2)
        {
            if (Input.GetKeyDown(KeyCode.D) || CBikeSerial.GetNewButton(2))
            {
                if (GameData.FREE_MODE)
                {
                    GameData.FREE_MODE = false;
                }
                else
                {
                    GameData.FREE_MODE = true;
                }
            }
        }
        else if (state == -1)
        {
            if (Input.GetKeyDown(KeyCode.D) || CBikeSerial.GetNewButton(2))
            {
                if (GameData.USE_RFID)
                {
                    GameData.USE_RFID = false;
                    PlayerPrefs.SetInt("USE_RFID", 0);
                }
                else
                {
                    GameData.USE_RFID = true;
                    PlayerPrefs.SetInt("USE_RFID", 1);
                }
                
            }
        }
        else if (state == 0)
        {
            if (Input.GetKeyDown(KeyCode.D) || CBikeSerial.GetNewButton(2))
            {
                if (GameData._3D)
                {
                    GameData._3D = false;
                    PlayerPrefs.SetInt("3D", 0);
                }
                else
                {
                    GameData._3D = true;
                    PlayerPrefs.SetInt("3D", 1);
                }
            }
        }
        else if (state == 1)
        {
            if (Input.GetKeyDown(KeyCode.D) || CBikeSerial.GetNewButton(2))
            {
                PlayerPrefs.SetInt("TotalCoin", 0);
            }
        }
        else if (state == 2)
        {
            if (Input.GetKeyDown(KeyCode.D) || CBikeSerial.GetNewButton(2))
            {
                float value = PlayerPrefs.GetFloat("Speed1");
                value += 1.0f;
                if (value > 100) value = 20.0f;
                PlayerPrefs.SetFloat("Speed1", value);   
            }
        }
        else if (state == 3)
        {
            if (Input.GetKeyDown(KeyCode.D) || CBikeSerial.GetNewButton(2))
            {
                float value = PlayerPrefs.GetFloat("Speed2");
                value += 1.0f;
                if (value > 100) value = 20.0f;
                PlayerPrefs.SetFloat("Speed2", value);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) || CBikeSerial.GetNewButton(1))
        {
            gameObject.AddComponent<Change3D>();
            StateControl.gameMng.SetState(typeof(Change3D));
        }
    }
    #endregion

    #region OnDeactivate
    public override void OnDeactivate()
    {
        DestroyImmediate(this);
    }
    #endregion

    void OnGUI()
    {
        if (state == -2)
        {
            GUILayout.Label("�׽�Ʈ ��� - " + GameData.FREE_MODE);
            GUILayout.Label("������ ��ư�� ������ TEST ��ȯ�� �˴ϴ�.");
        }
        else if (state == -1)
        {
            GUILayout.Label("RFID CARD USE - " + GameData.USE_RFID);
            GUILayout.Label("������ ��ư�� ������ TRUE �Ǵ� FALSE ��ȯ�� �˴ϴ�.");
        }
        else if (state == 0)
        {
            GUILayout.Label("3D ��� - " + GameData._3D);
            GUILayout.Label("������ ��ư�� ������ 2D, 3D ��ȯ�� �˴ϴ�.");
        }
        else if (state == 1)
        {
            GUILayout.Label("�� ���� ���� ��� - " + PlayerPrefs.GetInt("TotalCoin"));
            GUILayout.Label("������ ��ư�� ������ ������ �ʱ�ȭ �˴ϴ�.");
        }
        else if (state == 2)
        {
            GUILayout.Label("Speed1 - " + PlayerPrefs.GetFloat("Speed1"));
            GUILayout.Label("������ ��ư�� ������ 5�� ���� �����ϴ�. �ּ� 20 �ִ� 50");
        }
        else if (state == 3)
        {
            GUILayout.Label("Speed2 - " + PlayerPrefs.GetFloat("Speed2"));
            GUILayout.Label("������ ��ư�� ������ 5�� ���� �����ϴ�. �ּ� 20 �ִ� 50");
        }
        GUILayout.Label("��� ��ư�� ������ ������ ����Ǹ� �ʱ�ȭ������ ���ư��ϴ�.");
        GUILayout.Label("���� ��ư�� ������ ���� �޴��� �ٲ�ϴ�. (3D �޴�, �� ���� ���� �޴�)");
    }
}

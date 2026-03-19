using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Unity6 마이그레이션 테스트용 부트스트랩.
/// Menu UI(GUITexture 제거로 블랙) 를 우회하고 Training 모드를 바로 시작한다.
/// 테스트 완료 후 이 스크립트를 비활성화하면 정상 메뉴 흐름으로 복귀.
/// </summary>
public class TestBootstrap : MonoBehaviour
{
    [Header("테스트 설정")]
    public bool skipMenu = true;
    public int trainingMap = 1; // 0=Demo02, 1=Demo03, 2=Tracking03

    void Awake()
    {
        // Awake는 모든 Start()보다 먼저 실행되므로
        // StateControl.Start()가 일반 메뉴 초기화를 건너뛰도록 미리 플래그 설정
        if (!skipMenu) return;
        GameData.TEST_MODE = true;
        GameData.FREE_MODE = true;
        GameData.NOW_CREDIT = GameData.ONEGAMECOIN;
        GameData.TraningMap = trainingMap;
    }

    void Start()
    {
        if (!skipMenu) return;
        StartCoroutine(StartTraining());
    }

    IEnumerator StartTraining()
    {
        // StateControl이 초기화될 때까지 1프레임 대기
        yield return null;

        // StateControl이 없으면 생성
        if (StateControl.gameMng == null)
        {
            Debug.LogWarning("TestBootstrap: StateControl not found. Add StateControl GameObject to this scene.");
            yield break;
        }

        // Training 모드 직접 시작
        string targetMap = "Demo02";
        if (trainingMap == 1) targetMap = "Demo03";
        else if (trainingMap == 2) targetMap = "Tracking03";

        Debug.Log("TestBootstrap: Loading " + targetMap + " directly...");

        StateControl.gameMng.gameObject.AddComponent<Training_Data>();
        StateControl.gameMng.gameObject.AddComponent<Training_InGame>();
        GameObject loading = Instantiate((GameObject)Resources.Load("Prefeb/Loading_MTB"));
        if (loading != null) loading.name = "_Loading";
        StateControl.gameMng.SetState(typeof(Training_InGame));
    }
}

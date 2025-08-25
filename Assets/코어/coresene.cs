using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 전환 기능을 사용하기 위해 이 줄을 추가합니다.

/// <summary>
/// CoreScene의 UI와 흐름을 관리합니다.
/// 게임 모드를 선택하고 다음 씬으로 직접 전환하는 역할을 합니다.
/// </summary>
public class CoreSceneManager : MonoBehaviour
{
    [Header("UI 연결")]
    [Tooltip("전투 씬으로 전환하는 버튼입니다.")]
    public Button startBattleButton;

    void Start()
    {
        if (startBattleButton != null)
        {
            startBattleButton.onClick.AddListener(StartBattleMode);
        }
        else
        {
            Debug.LogWarning("[CoreSceneManager] 전투 시작 버튼이 연결되지 않았습니다.");
        }
    }

    /// <summary>
    /// 전투 모드를 시작하고, BattleScene으로 직접 전환합니다.
    /// </summary>
    public void StartBattleMode()
    {
        // 1. GlobalManager가 있는지 확인하고, 게임 모드를 설정합니다.
        if (GlobalManager.instance != null)
        {
            Debug.Log("전투 모드 시작을 준비합니다.");
        }
        else
        {
            Debug.LogError("[CoreSceneManager] GlobalManager를 찾을 수 없습니다! CoreScene에 GlobalManager가 있는지 확인해주세요.");
            return;
        }

        // 2. SceneManager를 이용해 "BattleScene"을 직접 로드합니다.
        Debug.Log("BattleScene으로 직접 전환합니다.");
        SceneManager.LoadScene("BattleScene");
    }
}
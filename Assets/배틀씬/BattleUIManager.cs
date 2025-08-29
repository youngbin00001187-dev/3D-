using UnityEngine;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager instance;

    [Header("UI 요소 연결")]
    [Tooltip("카드 선택을 확정하는 버튼입니다.")]
    public Button proceedButton;
    [Tooltip("플레이어의 손패 카드들이 표시되는 패널입니다.")]
    public GameObject handPanel;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
        // 스페이스바를 눌렀는지 확인합니다.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // [수정] 1순위: '진행' 버튼이 활성화되어 있는지 먼저 확인합니다.
            if (proceedButton != null && proceedButton.gameObject.activeInHierarchy)
            {
                // 활성화되어 있다면, 버튼 클릭 함수를 직접 호출합니다.
                Debug.Log("[BattleUIManager] 스페이스바 입력 감지. '진행' 버튼을 클릭합니다.");
                OnProceedButtonClicked();
            }
            // 2순위: '진행' 버튼이 비활성화 상태이고, 카드 선택 단계일 때 멀리건을 시도합니다.
            else if (GameManager.Instance != null &&
                GameManager.Instance.currentPhase == GameManager.BattlePhase.PlayerTurn_CardSelection)
            {
                if (CardManager.instance != null)
                {
                    Debug.Log("[BattleUIManager] 스페이스바 입력 감지. 멀리건을 시도합니다.");
                    CardManager.instance.PerformMulligan();
                }
            }
        }
    }
    void Start()
    {
        if (proceedButton != null)
        {
            proceedButton.onClick.AddListener(OnProceedButtonClicked);
            // 시작 시 버튼 숨기기
            proceedButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 진행 버튼이 클릭되면 이벤트를 발생시키고, UI를 정리합니다.
    /// </summary>
    private void OnProceedButtonClicked()
    {
        // 핸드 패널을 감추는 로직 추가
        if (handPanel != null)
        {
            handPanel.SetActive(false);
            Debug.Log("[BattleUIManager] 핸드 패널을 비활성화합니다.");
        }

        // 진행 버튼도 숨기기
        if (proceedButton != null)
        {
            proceedButton.gameObject.SetActive(false);
        }

        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.RaiseActionPhaseStart();
        }
    }

    public void ShowHandPanel()
    {
        if (handPanel != null)
        {
            handPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 진행 버튼을 표시합니다.
    /// </summary>
    public void ShowProceedButton()
    {
        if (proceedButton != null)
        {
            proceedButton.gameObject.SetActive(true);
            Debug.Log("[BattleUIManager] 진행 버튼을 표시합니다.");
        }
    }

    /// <summary>
    /// 진행 버튼을 숨깁니다.
    /// </summary>
    public void HideProceedButton()
    {
        if (proceedButton != null)
        {
            proceedButton.gameObject.SetActive(false);
            Debug.Log("[BattleUIManager] 진행 버튼을 숨깁니다.");
        }
    }
}
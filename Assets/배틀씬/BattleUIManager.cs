using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager instance;

    [Header("UI 요소 연결")]
    [Tooltip("카드 선택을 확정하는 버튼입니다.")]
    public Button proceedButton;
    [Tooltip("플레이어의 손패 카드들이 표시되는 패널입니다.")]
    public GameObject handPanel;

    [Header("덱 UI 연결")]
    [Tooltip("덱의 남은 양을 시각적으로 보여줄 이미지입니다.")]
    public Image deckImage;
    [Tooltip("덱의 남은 장 수를 표시할 텍스트입니다.")]
    public TextMeshProUGUI deckCountText;
    [Tooltip("덱 이미지의 최소 높이 비율입니다. (예: 0.2 = 20%)")]
    [Range(0f, 1f)]
    public float deckImageMinHeightRatio = 0.2f;

    private int _initialDeckSize = 1;
    private float _initialDeckImageHeight;
    private bool _isDeckUIInitialized = false;

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

    private void OnEnable()
    {
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.OnCardManagerReady += HandleCardManagerReady;
        }
    }

    private void OnDisable()
    {
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.OnCardManagerReady -= HandleCardManagerReady;
        }
    }

    void Start()
    {
        if (proceedButton != null)
        {
            proceedButton.onClick.AddListener(OnProceedButtonClicked);
            proceedButton.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (proceedButton != null && proceedButton.gameObject.activeInHierarchy)
            {
                OnProceedButtonClicked();
            }
            else if (GameManager.Instance != null &&
                GameManager.Instance.currentPhase == GameManager.BattlePhase.PlayerTurn_CardSelection)
            {
                if (CardManager.instance != null)
                {
                    CardManager.instance.PerformMulligan();
                }
            }
        }

        UpdateDeckUI();
    }

    private void HandleCardManagerReady()
    {
        if (_isDeckUIInitialized) return;
        StartCoroutine(InitializeDeckUICoroutine());
    }

    private IEnumerator InitializeDeckUICoroutine()
    {
        yield return new WaitForEndOfFrame();

        if (CardManager.instance != null)
        {
            _initialDeckSize = CardManager.instance.GetTotalCardsCount();
            if (_initialDeckSize == 0) _initialDeckSize = 1;
        }

        if (deckImage != null)
        {
            _initialDeckImageHeight = deckImage.rectTransform.sizeDelta.y;
        }

        _isDeckUIInitialized = true;
        Debug.Log($"[BattleUIManager] 덱 UI 초기화 완료. 시작 덱 크기: {_initialDeckSize}, 이미지 높이: {_initialDeckImageHeight}");
    }

    private void UpdateDeckUI()
    {
        if (!_isDeckUIInitialized || CardManager.instance == null || deckCountText == null || deckImage == null)
        {
            return;
        }

        int currentDeckCount = CardManager.instance.DeckCount;
        deckCountText.text = currentDeckCount.ToString();

        float currentRatio = (float)currentDeckCount / _initialDeckSize;
        float targetHeight = Mathf.Lerp(_initialDeckImageHeight * deckImageMinHeightRatio, _initialDeckImageHeight, currentRatio);
        deckImage.rectTransform.sizeDelta = new Vector2(deckImage.rectTransform.sizeDelta.x, targetHeight);
    }

    private void OnProceedButtonClicked()
    {
        if (handPanel != null)
        {
            handPanel.SetActive(false);
        }
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

    public void ShowProceedButton()
    {
        if (proceedButton != null)
        {
            proceedButton.gameObject.SetActive(true);
        }
    }

    public void HideProceedButton()
    {
        if (proceedButton != null)
        {
            proceedButton.gameObject.SetActive(false);
        }
    }
}
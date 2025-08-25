using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class CardManager : MonoBehaviour
{
    public static CardManager instance;

    [Header("UI 연결")]
    public GameObject cardUiPrefab;
    public Transform handTransform;
    public Transform actionPanelTransform;
    public Button proceedButton;

    [Header("참조 연결")]
    public PlayerController playerController;
    public LayerMask tileLayerMask;

    // --- 카드 덱 관련 변수들 ---
    private List<CardDataSO> drawPile = new List<CardDataSO>();
    private List<CardDataSO> hand = new List<CardDataSO>();
    private List<CardDataSO> discardPile = new List<CardDataSO>();
    private List<GameObject> handUiObjects = new List<GameObject>();
    private List<CardDataSO> actionCards = new List<CardDataSO>();
    private List<GameObject> actionCardUiObjects = new List<GameObject>();

    // --- 전투 규칙 관련 변수들 ---
    private int handSize;
    private int mulligansLeft;
    private int playerMaxActionsPerTurn;
    private bool isInitialized = false;

    // --- 타겟팅 및 하이라이트 관련 변수들 ---
    private CardDataSO selectedCardForTargeting;
    private List<Tile3D> _targetingHighlightTiles = new List<Tile3D>();
    private List<Tile3D> _hoverHighlightTiles = new List<Tile3D>();
    private List<Tile3D> _aoePreviewTiles = new List<Tile3D>();
    private Tile3D _lastHoveredTile;

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }
    private void Start()
    {
        // '진행' 버튼이 눌렸을 때 OnProceedButtonClicked 함수가 호출되도록 등록합니다.
        if (proceedButton != null)
        {
            proceedButton.onClick.AddListener(OnProceedButtonClicked);
        }
    }
    void Update()
    {
        if (GameManager.instance == null || playerController == null) return;

        // 타겟팅 모드일 때(액션 패널에 카드가 있을 때)만 타일 호버를 감지합니다.
        // 이 기능은 모든 페이즈에서 작동합니다.
        if (selectedCardForTargeting != null)
        {
            HandleTileHoverForAoePreview();
        }

        // '액션 실행' 페이즈일 때만 타일 클릭 입력을 받습니다.
        if (GameManager.instance.currentPhase == GameManager.BattlePhase.ActionExecution)
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleTileClick();
            }
        }
    }

    private void HandleTileHoverForAoePreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Tile3D currentHoveredTile = null;

        // 디버그: 레이캐스트가 제대로 작동하는지 확인
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 0.1f);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, tileLayerMask))
        {
            currentHoveredTile = hit.collider.GetComponent<Tile3D>();

            // 디버그: 어떤 오브젝트에 히트했는지 확인
            Debug.Log($"<color=cyan>[AOE Preview] 레이캐스트 히트: {hit.collider.name}, Tile3D 컴포넌트: {(currentHoveredTile != null ? "있음" : "없음")}</color>");
        }
        else
        {
            // 디버그: 레이캐스트가 아무것도 히트하지 않을 때
            Debug.Log($"<color=orange>[AOE Preview] 레이캐스트 히트 없음. LayerMask: {tileLayerMask.value}</color>");
        }

        // 마우스가 다른 타일로 움직였다면
        if (currentHoveredTile != _lastHoveredTile)
        {
            Debug.Log($"<color=yellow>[AOE Preview] 타일 변경: {(_lastHoveredTile?.name ?? "null")} -> {(currentHoveredTile?.name ?? "null")}</color>");

            // 1. 기존에 있던 AOE 미리보기 하이라이트를 지웁니다.
            ClearAoePreviewHighlight();

            // 2. 새로 호버한 타일이 '조준 가능 범위'(_targetingHighlightTiles) 안에 있다면,
            if (currentHoveredTile != null && _targetingHighlightTiles.Contains(currentHoveredTile))
            {
                Debug.Log($"<color=green>[AOE Preview] 유효한 타겟 타일에 호버: {currentHoveredTile.name}</color>");
                // 3. 그 타일을 중심으로 AOE 미리보기를 덧그립니다.
                ShowAoePreviewHighlight(currentHoveredTile);
            }
            else if (currentHoveredTile != null)
            {
                Debug.Log($"<color=red>[AOE Preview] 유효하지 않은 타겟 타일: {currentHoveredTile.name}. 타겟팅 하이라이트 타일 수: {_targetingHighlightTiles.Count}</color>");

                // 디버그: 타겟팅 하이라이트 타일들의 이름을 출력
                for (int i = 0; i < _targetingHighlightTiles.Count; i++)
                {
                    Debug.Log($"  - 타겟팅 하이라이트 타일[{i}]: {_targetingHighlightTiles[i]?.name ?? "null"}");
                }
            }
            _lastHoveredTile = currentHoveredTile;
        }
    }
    public void OnProceedButtonClicked()
    {
       
        {
            // ▼▼▼ 이 부분이 수정되었습니다 ▼▼▼
            // 직접 코루틴을 시작하는 대신, '액션 페이즈 시작' 이벤트를 방송합니다.
            if (BattleEventManager.instance != null)
            {
                BattleEventManager.instance.RaiseActionPhaseStart();
            }
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            // 실제 액션 실행은 이 이벤트를 구독한 다른 함수가 담당하게 됩니다.
            // StartCoroutine(ActionPhaseCoroutine()); // 이 줄은 이제 필요 없습니다.
        }
    }
    private void HandleTileClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, tileLayerMask))
        {
            Tile3D clickedTile = hit.collider.GetComponent<Tile3D>();

            if (clickedTile != null && _targetingHighlightTiles.Contains(clickedTile))
            {
                Debug.Log($"<color=yellow>유효한 타일({clickedTile.name}) 클릭! 액션을 실행합니다.</color>");
                // TODO: TurnManager에게 액션 전달 및 사용한 카드 제거 로직
                UpdateTargetingMode();
            }
        }
    }

    private void OnEnable()
    {
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.OnBattleStart += OnBattleStarted;
        }
    }

    private void OnDisable()
    {
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.OnBattleStart -= OnBattleStarted;
        }
    }

    public void Initialize(List<CardDataSO> playerDeck, int handSize, int mulligansPerTurn, int maxActions)
    {
        this.handSize = handSize;
        this.mulligansLeft = mulligansPerTurn;
        this.playerMaxActionsPerTurn = maxActions;
        if (playerDeck == null || playerDeck.Count == 0) return;
        drawPile = new List<CardDataSO>(playerDeck);
        drawPile = drawPile.OrderBy(a => System.Guid.NewGuid()).ToList();
        isInitialized = true;
        if (proceedButton != null) proceedButton.gameObject.SetActive(false);
    }

    private void OnBattleStarted()
    {
        if (!isInitialized)
        {
            StartCoroutine(WaitForInitializationAndDraw());
            return;
        }
        DrawNewHand();
    }

    private IEnumerator WaitForInitializationAndDraw()
    {
        float waitTime = 0f;
        while (!isInitialized && waitTime < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }
        if (isInitialized)
        {
            DrawNewHand();
        }
    }

    public void DrawNewHand()
    {
        for (int i = 0; i < handSize; i++)
        {
            DrawCard();
        }
    }

    private void DrawCard()
    {
        if (drawPile.Count == 0)
        {
            if (discardPile.Count > 0)
            {
                drawPile.AddRange(discardPile);
                discardPile.Clear();
                drawPile = drawPile.OrderBy(a => System.Guid.NewGuid()).ToList();
            }
            else return;
        }
        CardDataSO drawnCard = drawPile[0];
        drawPile.RemoveAt(0);
        hand.Add(drawnCard);
        CreateCardUI(drawnCard);
    }

    private void CreateCardUI(CardDataSO cardData)
    {
        if (cardUiPrefab == null || handTransform == null) return;
        GameObject newCardUI = Instantiate(cardUiPrefab, handTransform);
        newCardUI.GetComponent<CardUI>().Setup(cardData, this);
        handUiObjects.Add(newCardUI);
    }

    public void OnCardClicked(GameObject cardUIObject, CardDataSO cardData)
    {
        if (cardUIObject.transform.parent == handTransform)
        {
            SelectCardFromHand(cardUIObject, cardData);
        }
        else if (cardUIObject.transform.parent == actionPanelTransform)
        {
            ReturnCardToHand(cardUIObject, cardData);
        }
    }

    private void SelectCardFromHand(GameObject cardUIObject, CardDataSO cardData)
    {
        if (actionCards.Count >= playerMaxActionsPerTurn) return;
        hand.Remove(cardData);
        actionCards.Add(cardData);
        handUiObjects.Remove(cardUIObject);
        actionCardUiObjects.Add(cardUIObject);
        StartCoroutine(AnimateCardMovement(cardUIObject, actionPanelTransform));
        UpdateTargetingMode();
    }

    private void ReturnCardToHand(GameObject cardUIObject, CardDataSO cardData)
    {
        actionCards.Remove(cardData);
        hand.Add(cardData);
        actionCardUiObjects.Remove(cardUIObject);
        handUiObjects.Add(cardUIObject);
        StartCoroutine(AnimateCardMovement(cardUIObject, handTransform));
        UpdateTargetingMode();
    }

    private IEnumerator AnimateCardMovement(GameObject cardUIObject, Transform targetParent)
    {
        cardUIObject.transform.SetParent(targetParent, true);
        yield return null;
    }

    public void CheckProceedButtonState()
    {
        if (proceedButton == null) return;
        bool shouldBeActive = (actionCards.Count >= playerMaxActionsPerTurn);
        proceedButton.gameObject.SetActive(shouldBeActive);
    }

    // --- 하이라이트 로직 ---

    public void HandleCardHoverEnter(CardDataSO cardData)
    {
        if (HighlightManager.instance == null || playerController == null) return;

        Debug.Log($"<color=magenta>[Card Hover] 카드 호버 시작: {cardData.name}</color>");

        ClearTargetingHighlight();

        List<GameObject> tiles = GetTargetableTiles(cardData, playerController);
        Debug.Log($"<color=magenta>[Card Hover] 타겟 가능한 타일 수: {tiles.Count}</color>");

        _hoverHighlightTiles = tiles.Select(t => t.GetComponent<Tile3D>()).ToList();
        HighlightManager.instance.AddHighlight(_hoverHighlightTiles, HighlightManager.HighlightType.PlayerPreview);
    }

    public void HandleCardHoverExit()
    {
        if (HighlightManager.instance == null) return;

        Debug.Log($"<color=magenta>[Card Hover] 카드 호버 종료</color>");

        ClearHoverHighlight();

        ShowTargetingHighlight();
    }

    private void UpdateTargetingMode()
    {
        Debug.Log($"<color=blue>[Targeting] 타겟팅 모드 업데이트. 액션 카드 수: {actionCards.Count}</color>");

        ClearTargetingHighlight();

        if (actionCards.Any())
        {
            selectedCardForTargeting = actionCards[0];
            Debug.Log($"<color=blue>[Targeting] 타겟팅용 카드 설정: {selectedCardForTargeting.name}</color>");
            ShowTargetingHighlight();
        }
        else
        {
            selectedCardForTargeting = null;
            Debug.Log($"<color=blue>[Targeting] 타겟팅용 카드 없음</color>");
        }
        CheckProceedButtonState();
    }

    private void ShowTargetingHighlight()
    {
        if (selectedCardForTargeting != null)
        {
            Debug.Log($"<color=blue>[Targeting] 타겟팅 하이라이트 표시: {selectedCardForTargeting.name}</color>");

            List<GameObject> tiles = GetTargetableTiles(selectedCardForTargeting, playerController);
            Debug.Log($"<color=blue>[Targeting] 타겟 가능한 타일 수: {tiles.Count}</color>");

            _targetingHighlightTiles = tiles.Select(t => t.GetComponent<Tile3D>()).Where(t => t != null).ToList();
            Debug.Log($"<color=blue>[Targeting] Tile3D 컴포넌트가 있는 타일 수: {_targetingHighlightTiles.Count}</color>");

            if (_targetingHighlightTiles.Count > 0)
            {
                HighlightManager.instance.AddHighlight(_targetingHighlightTiles, HighlightManager.HighlightType.PlayerPreview);

                // 디버그: 타겟팅 하이라이트 타일들의 이름 출력
                for (int i = 0; i < _targetingHighlightTiles.Count; i++)
                {
                    Debug.Log($"  - 타겟팅 타일[{i}]: {_targetingHighlightTiles[i].name}");
                }
            }
        }
    }

    private void ClearTargetingHighlight()
    {
        if (_targetingHighlightTiles.Count > 0)
        {
            Debug.Log($"<color=blue>[Targeting] 타겟팅 하이라이트 제거: {_targetingHighlightTiles.Count}개 타일</color>");
            HighlightManager.instance.RemoveHighlight(_targetingHighlightTiles, HighlightManager.HighlightType.PlayerPreview);
            _targetingHighlightTiles.Clear();
        }
    }

    private void ClearHoverHighlight()
    {
        if (_hoverHighlightTiles.Count > 0)
        {
            Debug.Log($"<color=magenta>[Card Hover] 호버 하이라이트 제거: {_hoverHighlightTiles.Count}개 타일</color>");
            HighlightManager.instance.RemoveHighlight(_hoverHighlightTiles, HighlightManager.HighlightType.PlayerPreview);
            _hoverHighlightTiles.Clear();
        }
    }

    private void ShowAoePreviewHighlight(Tile3D targetTile)
    {
        if (selectedCardForTargeting == null)
        {
            Debug.LogWarning("[AOE Preview] selectedCardForTargeting이 null입니다!");
            return;
        }

        if (selectedCardForTargeting.actionSequence == null || selectedCardForTargeting.actionSequence.Count == 0)
        {
            Debug.LogWarning($"[AOE Preview] {selectedCardForTargeting.name}의 actionSequence가 null이거나 비어있습니다!");
            return;
        }

        GameAction mainAction = selectedCardForTargeting.actionSequence[0];
        Debug.Log($"<color=green>[AOE Preview] AOE 미리보기 표시 중. 타겟 타일: {targetTile.name}, 액션: {mainAction.GetType().Name}</color>");

        List<GameObject> aoeTiles = mainAction.GetActionImpactTiles(playerController, targetTile.gameObject);
        Debug.Log($"<color=green>[AOE Preview] AOE 영향 타일 수: {aoeTiles.Count}</color>");

        _aoePreviewTiles = aoeTiles.Select(t => t.GetComponent<Tile3D>()).Where(t => t != null).ToList();
        Debug.Log($"<color=green>[AOE Preview] Tile3D 컴포넌트가 있는 AOE 타일 수: {_aoePreviewTiles.Count}</color>");

        if (_aoePreviewTiles.Count > 0)
        {
            // 디버그: AOE 타일과 타겟팅 타일이 겹치는지 확인
            foreach (var aoe in _aoePreviewTiles)
            {
                bool isAlsoTargeting = _targetingHighlightTiles.Contains(aoe);
                Debug.Log($"<color=cyan>[AOE Preview] {aoe.name} - 타겟팅 타일이기도 함: {isAlsoTargeting}</color>");
            }

            // HighlightManager 상태 체크
            if (HighlightManager.instance == null)
            {
                Debug.LogError("[AOE Preview] HighlightManager.instance가 null입니다!");
                return;
            }

            Debug.Log($"<color=green>[AOE Preview] HighlightManager에 하이라이트 추가 요청중... 타입: PlayerTarget</color>");

            HighlightManager.instance.AddHighlight(_aoePreviewTiles, HighlightManager.HighlightType.PlayerTarget);

            Debug.Log($"<color=green>[AOE Preview] HighlightManager.AddHighlight 호출 완료</color>");

            // 디버그: 직접 색상 변경해서 테스트
            Debug.Log($"<color=red>[TEST] 직접 색상 변경 테스트 시작</color>");
            foreach (var tile in _aoePreviewTiles)
            {
                if (tile.MyMaterial != null)
                {
                    tile.MyMaterial.color = Color.red; // 빨간색으로 강제 변경
                    Debug.Log($"<color=red>[TEST] {tile.name}을 빨간색으로 강제 변경</color>");
                }
            }
        }
    }

    private void ClearAoePreviewHighlight()
    {
        if (_aoePreviewTiles.Count > 0)
        {
            Debug.Log($"<color=green>[AOE Preview] AOE 미리보기 제거: {_aoePreviewTiles.Count}개 타일</color>");

            if (HighlightManager.instance == null)
            {
                Debug.LogError("[AOE Preview] HighlightManager.instance가 null입니다!");
                return;
            }

            Debug.Log($"<color=green>[AOE Preview] HighlightManager에 하이라이트 제거 요청중...</color>");
            HighlightManager.instance.RemoveHighlight(_aoePreviewTiles, HighlightManager.HighlightType.PlayerTarget);

            // AOE 제거 후 원래 PlayerPreview 하이라이트 복원
            HighlightManager.instance.AddHighlight(_aoePreviewTiles, HighlightManager.HighlightType.PlayerPreview);

            Debug.Log($"<color=green>[AOE Preview] HighlightManager.RemoveHighlight 호출 완료</color>");

            _aoePreviewTiles.Clear();
        }
    }

    private List<GameObject> GetTargetableTiles(CardDataSO cardData, UnitController user)
    {
        if (cardData.actionSequence == null || cardData.actionSequence.Count == 0)
        {
            Debug.LogWarning($"[GetTargetableTiles] {cardData.name}의 actionSequence가 null이거나 비어있습니다!");
            return new List<GameObject>();
        }

        GameAction mainAction = cardData.actionSequence[0];
        List<GameObject> targetableTiles = mainAction.GetTargetableTiles(user);
        Debug.Log($"[GetTargetableTiles] {cardData.name}의 타겟 가능한 타일 수: {targetableTiles.Count}");

        return targetableTiles;
    }

    // HighlightManager에서 호출할 메서드
    public bool IsTargeting()
    {
        return selectedCardForTargeting != null;
    }
}
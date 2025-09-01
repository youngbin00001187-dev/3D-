using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using DG.Tweening;

/// <summary>
/// 플레이어의 카드 덱, 핸드, 버린 카드 더미를 관리하고,
/// 카드 사용 로직 및 UI 상호작용을 처리합니다.
/// </summary>
public class CardManager : MonoBehaviour
{
    public static CardManager instance;

    [Header("UI 연결")]
    public Transform handPanelTransform;
    public GameObject cardUIPrefab;
    public Transform actionPanelTransform;

    [HideInInspector] public PlayerController playerController;

    private List<CardDataSO> _playerDeck;
    private List<CardDataSO> _discardPile;
    private List<CardDataSO> _currentHand;

    private int _handSize;
    private int _mulligansPerTurn;
    private int _maxActionsPerTurn;
    private int _remainingMulligans;

    // 현재 타겟팅 중인 카드
    private CardDataSO _targetingCard;
    private GameObject _targetingCardUIObject;
    private Tile3D _lastHoveredTileForPreview;

    private HighlightManager _highlightManager;
    private List<Tile3D> _currentPreviewTiles = new List<Tile3D>();
    private bool _isPreviewSuppressedByHover = false;
    private Dictionary<GameObject, CardDataSO> _selectedCards = new Dictionary<GameObject, CardDataSO>();

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    private void OnEnable()
    {
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.OnAllUnitsPlaced += OnAllUnitsPlacedHandler;
        }
    }

    private void OnDisable()
    {
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.OnAllUnitsPlaced -= OnAllUnitsPlacedHandler;
        }
    }

    private void OnAllUnitsPlacedHandler()
    {
        if (BattleInitializer.instance != null)
        {
            Initialize(
              BattleInitializer.instance.playerDeck,
              BattleInitializer.instance.handSize,
              BattleInitializer.instance.mulligansPerTurn,
              BattleInitializer.instance.playerMaxActionsPerTurn
            );
        }
    }

    void Update()
    {
        // 상태 1: 핸드의 다른 카드를 호버하고 있을 때 (_isPreviewSuppressedByHover == true)
        // 이 경우, Update 메서드는 아무것도 하지 않고 즉시 종료하여 HandleCardHoverEnter가 그린 미리보기를 보존합니다.
        if (_isPreviewSuppressedByHover)
        {
            return;
        }

        // 상태 2: 타겟팅 중인 카드가 없을 때
        if (_targetingCard == null)
        {
            // 남아있을 수 있는 모든 관련 미리보기를 깨끗하게 지웁니다.
            if (_highlightManager != null)
            {
                _highlightManager.ClearAllHighlightsOfType(HighlightManager.HighlightType.PlayerPreview);
                _highlightManager.ClearAllHighlightsOfType(HighlightManager.HighlightType.PlayerTarget);
            }
            _lastHoveredTileForPreview = null;
            return;
        }

        // --- 상태 3: 타겟팅 카드가 있고, 다른 카드를 호버하고 있지 않을 때 ---

        // 3-1. 항상 기본 사거리(PlayerPreview)를 표시합니다.
        UpdateTargetingAura(_targetingCard);

        // 3-2. 실시간 AOE 미리보기(PlayerTarget) 로직을 처리합니다.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Tile3D currentHoveredTile = null;

        if (Physics.Raycast(ray, out hit, 200f))
        {
            currentHoveredTile = hit.collider.GetComponent<Tile3D>();
        }

        if (currentHoveredTile != _lastHoveredTileForPreview)
        {
            // 이전에 표시했던 PlayerTarget 하이라이트는 무조건 지웁니다.
            _highlightManager.ClearAllHighlightsOfType(HighlightManager.HighlightType.PlayerTarget);

            // 마우스가 유효한 타일 위에 있다면 PlayerTarget 하이라이트를 "추가"합니다.
            if (currentHoveredTile != null && _currentPreviewTiles.Contains(currentHoveredTile))
            {
                GameAction firstAction = _targetingCard.actionSequence[0];
                var impactTiles = firstAction.GetActionImpactTiles(playerController, currentHoveredTile.gameObject)
                                             .Select(t => t.GetComponent<Tile3D>())
                                             .Where(t => t != null).ToList();
                _highlightManager.AddHighlight(impactTiles, HighlightManager.HighlightType.PlayerTarget);
            }
            _lastHoveredTileForPreview = currentHoveredTile;
        }

        // 3-3. 클릭 처리는 액션 페이즈일 때만 작동합니다.
        if (GameManager.Instance != null && GameManager.Instance.currentPhase == GameManager.BattlePhase.ActionPhase)
        {
            HandleMouseClick();
        }
    }

    public void Initialize(List<CardDataSO> deck, int handSize, int mulligans, int maxActions)
    {
        _handSize = handSize;
        _mulligansPerTurn = mulligans;
        _maxActionsPerTurn = maxActions;
        _highlightManager = HighlightManager.instance;
        _playerDeck = new List<CardDataSO>(deck.Where(card => card != null).ToList());
        _discardPile = new List<CardDataSO>();
        _currentHand = new List<CardDataSO>();
        _selectedCards.Clear();
        ClearTargetingCard();
        ShuffleDeck();

        // [신규] 모든 초기화가 끝났음을 이벤트 매니저에 알립니다.
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.RaiseCardManagerReady();
        }
    }

    #region 드로우 및 멀리건

    public void DrawHand()
    {
        // UI 정리
        foreach (Transform child in handPanelTransform) { Destroy(child.gameObject); }
        foreach (Transform child in actionPanelTransform) { Destroy(child.gameObject); }

        // 현재 핸드와 선택된 카드들을 버린 카드 더미로 이동
        _discardPile.AddRange(_currentHand);
        _discardPile.AddRange(_selectedCards.Values);
        _currentHand.Clear();
        _selectedCards.Clear();

        ClearTargetingCard();
        CheckAndUpdateProceedButton();
        _remainingMulligans = _mulligansPerTurn;

        // 새로운 핸드 뽑기
        DrawCards(_handSize);

        if (BattleUIManager.instance != null) { BattleUIManager.instance.ShowHandPanel(); }
    }

    public void PerformMulligan()
    {
        if (GameManager.Instance.currentPhase != GameManager.BattlePhase.PlayerTurn_CardSelection)
        {
            Debug.LogWarning("[Mulligan] 카드 선택 단계에서만 멀리건을 할 수 있습니다.");
            return;
        }
        if (_remainingMulligans <= 0)
        {
            Debug.LogWarning("[Mulligan] 남은 멀리건 횟수가 없습니다.");
            return;
        }

        _remainingMulligans--;
        Debug.Log($"<color=orange>[Mulligan] 멀리건을 실행합니다. 남은 횟수: {_remainingMulligans}</color>");

        // [수정] 핸드에 카드가 0장이어도 상관없이 진행합니다.
        // 핸드에 남아있는 카드를 모두 버립니다.
        _discardPile.AddRange(_currentHand);
        Debug.Log($"[Mulligan] 핸드에 있던 {_currentHand.Count}장의 카드를 버렸습니다.");
        _currentHand.Clear();

        // 핸드 패널의 UI를 모두 제거합니다.
        foreach (Transform cardUI in handPanelTransform)
        {
            Destroy(cardUI.gameObject);
        }

        // 무조건 핸드 사이즈만큼 새로 드로우합니다.
        DrawCards(_handSize);
    }

    private void DrawCards(int amount)
    {
        Debug.Log($"[Draw] {amount}장의 카드를 새로 뽑습니다.");
        int actualDrawnCount = 0;

        for (int i = 0; i < amount; i++)
        {
            // 덱이 비어있으면 버린 카드 더미를 섞어서 덱으로 만들기
            if (_playerDeck.Count == 0)
            {
                ReshuffleDiscardPile();
            }

            // 그래도 덱이 비어있다면 (모든 카드를 다 뽑은 상황) 중단
            if (_playerDeck.Count == 0)
            {
                Debug.LogWarning($"[Draw] 덱과 버린 더미에 더 이상 뽑을 카드가 없습니다. {actualDrawnCount}/{amount}장만 뽑았습니다.");
                break;
            }

            CardDataSO drawnCard = _playerDeck[0];
            _playerDeck.RemoveAt(0);
            _currentHand.Add(drawnCard);
            InstantiateCardUI(drawnCard);
            actualDrawnCount++;
        }

        Debug.Log($"[Draw] 실제로 {actualDrawnCount}장의 카드를 뽑았습니다. 현재 핸드: {_currentHand.Count}장");
    }

    #endregion

    #region 기존 카드 관리 함수

    private void UpdateTargetingAura(CardDataSO cardData)
    {
        if (_highlightManager == null || playerController == null || cardData == null) return;
        _highlightManager.ClearAllHighlightsOfType(HighlightManager.HighlightType.PlayerPreview);
        if (cardData.intentPredictionRange.Count > 0)
        {
            List<GameObject> targetableTiles = new List<GameObject>();
            Vector3Int playerPos = playerController.GetGridPosition();
            foreach (var vector in cardData.intentPredictionRange)
            {
                GameObject tile = GridManager3D.instance.GetTileAtPosition(playerPos + vector);
                if (tile != null) targetableTiles.Add(tile);
            }
            _currentPreviewTiles = targetableTiles.Select(t => t.GetComponent<Tile3D>()).Where(t => t != null).ToList();
            _highlightManager.AddHighlight(_currentPreviewTiles, HighlightManager.HighlightType.PlayerPreview);
        }
    }

    private void HandleMouseClick()
    {
        if (_targetingCard == null) return;
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 200f))
            {
                Tile3D clickedTile = hit.collider.GetComponent<Tile3D>();
                if (clickedTile != null)
                {
                    StartCoroutine(CommitTargetCoroutine(clickedTile.gameObject));
                }
            }
        }
    }

    public void OnCardClicked(GameObject cardObject, CardDataSO cardData)
    {
        if (GameManager.Instance.currentPhase == GameManager.BattlePhase.PlayerTurn_CardSelection)
        {
            if (cardObject.transform.parent == handPanelTransform)
            {
                if (actionPanelTransform.childCount >= _maxActionsPerTurn) return;
                MoveCardToActionPanel(cardObject, cardData);
            }
            else if (cardObject.transform.parent == actionPanelTransform)
            {
                MoveCardToHandPanel(cardObject, cardData);
            }
        }
        else if (GameManager.Instance.currentPhase == GameManager.BattlePhase.ActionPhase)
        {
            if (cardObject.transform.parent == actionPanelTransform && cardObject != _targetingCardUIObject)
            {
                Debug.Log($"[CardManager] 행동 순서 변경: '{cardData.cardName}' 카드를 우선 타겟팅합니다.");
                cardObject.transform.SetAsFirstSibling();
                UpdateTargetingCard();
            }
        }
    }

    private void UpdateTargetingCard()
    {
        ClearTargetingCard();
        if (actionPanelTransform.childCount > 0)
        {
            Transform firstCardTransform = actionPanelTransform.GetChild(0);
            if (firstCardTransform != null)
            {
                CardUI firstCardUI = firstCardTransform.GetComponent<CardUI>();
                if (firstCardUI != null)
                {
                    _targetingCard = firstCardUI.GetCardData();
                    _targetingCardUIObject = firstCardUI.gameObject;
                    Debug.Log($"[CardManager] '{_targetingCard.cardName}' 카드가 자동으로 타겟팅 카드로 설정됨.");
                }
            }
        }
    }

    private void ClearTargetingCard()
    {
        if (_targetingCard != null)
        {
            if (_highlightManager != null)
                _highlightManager.ClearAllHighlightsOfType(HighlightManager.HighlightType.PlayerPreview);
            _targetingCard = null;
            _targetingCardUIObject = null;
        }
    }

    public void HandleCardHoverEnter(CardDataSO cardData)
    {
        if (cardData == null) return;
        _isPreviewSuppressedByHover = true;
        UpdateTargetingAura(cardData);
    }

    public void HandleCardHoverExit()
    {
        _isPreviewSuppressedByHover = false;
    }

    private IEnumerator CommitTargetCoroutine(GameObject targetTile)
    {
        if (_targetingCard == null || _targetingCardUIObject == null) yield break;
        Tile3D clickedTile3D = targetTile.GetComponent<Tile3D>();
        if (clickedTile3D == null || !_currentPreviewTiles.Contains(clickedTile3D)) yield break;

        // [추가] ClearTargetingCard()가 호출되기 전에 카드 데이터를 미리 저장합니다.
        CardDataSO cardToUse = _targetingCard;

        QueuedAction newAction = new QueuedAction
        {
            User = playerController,
            SourceCard = cardToUse, // 저장해둔 데이터를 사용합니다.
            TargetTile = targetTile
        };

        if (ActionTurnManager.Instance.IsProcessingQueue)
        {
            if (playerController.HasBreaksLeft())
            {
                playerController.UseBreak();
                ActionTurnManager.Instance.AddActionToInterruptQueue(newAction);

                // [추가] 사용한 카드를 버린 카드 더미에 추가합니다.
                _discardPile.Add(cardToUse);

                GameObject cardToDestroy = _targetingCardUIObject;
                _selectedCards.Remove(cardToDestroy);
                ClearTargetingCard();
                Destroy(cardToDestroy);
                yield return null;
                UpdateTargetingCard();
            }
            else
            {
                Debug.LogWarning("[BREAK] 큐 처리 중이지만 남은 브레이크 횟수가 없습니다!");
                yield break;
            }
        }
        else
        {
            ActionTurnManager.Instance.AddActionToNormalQueue(newAction);

            // [추가] 사용한 카드를 버린 카드 더미에 추가합니다.
            _discardPile.Add(cardToUse);

            GameObject cardToDestroy = _targetingCardUIObject;
            _selectedCards.Remove(cardToDestroy);
            ClearTargetingCard();
            Destroy(cardToDestroy);
            yield return null;
            UpdateTargetingCard();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NotifyPlayerActionSubmitted();
            }
        }
    }

    public int GetActionCardCount()
    {
        return actionPanelTransform.childCount;
    }

    private void CheckAndUpdateProceedButton()
    {
        bool shouldShow = actionPanelTransform.childCount >= _maxActionsPerTurn;
        if (BattleUIManager.instance != null)
        {
            if (shouldShow) BattleUIManager.instance.ShowProceedButton();
            else BattleUIManager.instance.HideProceedButton();
        }
    }

    private void MoveCardToActionPanel(GameObject cardObject, CardDataSO cardData)
    {
        cardObject.transform.SetParent(actionPanelTransform, false);
        _currentHand.Remove(cardData);
        _selectedCards.Add(cardObject, cardData);
        CheckAndUpdateProceedButton();
        UpdateTargetingCard();
    }

    private void MoveCardToHandPanel(GameObject cardObject, CardDataSO cardData)
    {
        cardObject.transform.SetParent(handPanelTransform, false);
        _currentHand.Add(cardData);
        _selectedCards.Remove(cardObject);
        CheckAndUpdateProceedButton();
        UpdateTargetingCard();
    }

    private void ShuffleDeck()
    {
        for (int i = 0; i < _playerDeck.Count; i++)
        {
            CardDataSO temp = _playerDeck[i];
            int randomIndex = Random.Range(i, _playerDeck.Count);
            _playerDeck[i] = _playerDeck[randomIndex];
            _playerDeck[randomIndex] = temp;
        }
        Debug.Log($"[Shuffle] 덱을 섞었습니다. 현재 덱: {_playerDeck.Count}장");
    }

    private void ReshuffleDiscardPile()
    {
        if (_discardPile.Count > 0)
        {
            Debug.Log($"[CardManager] 덱이 비어있어 버린 카드 더미 {_discardPile.Count}장을 덱으로 섞습니다.");
            _playerDeck.AddRange(_discardPile);
            _discardPile.Clear();
            ShuffleDeck();
        }
        else
        {
            Debug.Log("[CardManager] 버린 카드 더미도 비어있습니다.");
        }
    }

    private void InstantiateCardUI(CardDataSO data)
    {
        if (cardUIPrefab != null && handPanelTransform != null)
        {
            GameObject newCardUI = Instantiate(cardUIPrefab, handPanelTransform);
            CardUI cardComponent = newCardUI.GetComponent<CardUI>();
            if (cardComponent != null)
            {
                cardComponent.Setup(data, this);
            }
        }
    }

    public void OnActionPhaseStart()
    {
        UpdateTargetingCard();
    }

    #endregion

    #region 디버그 및 상태 확인용 함수

    public void LogCurrentState()
    {
        Debug.Log($"[CardManager State] 덱: {_playerDeck.Count}장, 핸드: {_currentHand.Count}장, 버린 더미: {_discardPile.Count}장, 액션: {_selectedCards.Count}장");
    }

    public int GetTotalCardsCount()
    {
        return _playerDeck.Count + _currentHand.Count + _discardPile.Count + _selectedCards.Count;
    }

    #endregion
}
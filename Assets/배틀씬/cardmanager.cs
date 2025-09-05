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
    #region 변수 및 인스턴스

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

    private CardDataSO _targetingCard;
    private GameObject _targetingCardUIObject;
    private Tile3D _lastHoveredTileForPreview;

    private HighlightManager _highlightManager;
    private List<Tile3D> _currentPreviewTiles = new List<Tile3D>();
    private bool _isPreviewSuppressedByHover = false;
    private Dictionary<GameObject, CardDataSO> _selectedCards = new Dictionary<GameObject, CardDataSO>();

    [Header("애니메이션 관련")]
    [Tooltip("애니메이션 중인 카드가 임시로 소속될 최상위 캔버스 RectTransform")]
    public RectTransform mainCanvasTransform;
    public Transform discardPileTransform;
    [Tooltip("카드 이동의 시작점이 될 덱(Deck)의 Transform")]
    public Transform deckDrawTransform;
    [Tooltip("액션/핸드 패널의 공간을 미리 차지할 투명한 자리 표시자 UI 프리팹")]
    public GameObject placeholderPrefab;
    [Tooltip("카드가 목표지점까지 날아가는 시간")]
    public float cardMoveDuration = 0.3f;
    [Tooltip("카드 드로우 시 각 카드의 애니메이션 사이 딜레이")]
    public float cardDrawDelay = 0.1f;
    [Tooltip("레이캐스트를 실행할 카메라를 지정합니다.")]
    public Camera raycastCamera;

    #endregion

    #region Unity 생명주기 및 초기화

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
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.RaiseCardManagerReady();
        }
    }

    #endregion

    #region 메인 업데이트 루프

    void Update()
    {
        if (_isPreviewSuppressedByHover) return;
        if (_targetingCard == null)
        {
            if (_highlightManager != null)
            {
                _highlightManager.ClearAllHighlightsOfType(HighlightManager.HighlightType.PlayerPreview);
                _highlightManager.ClearAllHighlightsOfType(HighlightManager.HighlightType.PlayerTarget);
            }
            _lastHoveredTileForPreview = null;
            return;
        }

        UpdateTargetingAura(_targetingCard);

        if (raycastCamera == null) return;
        Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Tile3D currentHoveredTile = null;
        if (Physics.Raycast(ray, out hit, 200f))
        {
            currentHoveredTile = hit.collider.GetComponent<Tile3D>();
        }

        if (currentHoveredTile != _lastHoveredTileForPreview)
        {
            _highlightManager.ClearAllHighlightsOfType(HighlightManager.HighlightType.PlayerTarget);
            if (currentHoveredTile != null && _currentPreviewTiles.Contains(currentHoveredTile))
            {
                GameAction impactAction = _targetingCard.actionSequence.LastOrDefault(a => a is AttackAction);
                if (impactAction == null && _targetingCard.actionSequence.Count > 0)
                {
                    impactAction = _targetingCard.actionSequence[0];
                }
                if (impactAction != null)
                {
                    var impactTiles = impactAction.GetActionImpactTiles(playerController, currentHoveredTile.gameObject)
                                                   .Select(t => t.GetComponent<Tile3D>())
                                                   .Where(t => t != null).ToList();
                    _highlightManager.AddHighlight(impactTiles, HighlightManager.HighlightType.PlayerTarget);
                }
            }
            _lastHoveredTileForPreview = currentHoveredTile;
        }
        if (GameManager.Instance != null && GameManager.Instance.currentPhase == GameManager.BattlePhase.ActionPhase)
        {
            HandleMouseClick();
        }
    }

    #endregion

    #region 공통 카드 애니메이션

    /// <summary>
    /// 카드를 지정된 목적지로 애니메이션과 함께 이동시킵니다.
    /// </summary>
    private IEnumerator MoveCardWithPlaceholder(GameObject cardObject, Transform targetParent, System.Action onComplete = null)
    {
        GameObject placeholder = Instantiate(placeholderPrefab, targetParent);
        placeholder.transform.SetAsLastSibling();

        yield return new WaitForEndOfFrame();
        Vector3 targetPosition = placeholder.transform.position;

        cardObject.transform.SetParent(mainCanvasTransform, true);

        cardObject.transform.DOMove(targetPosition, cardMoveDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                cardObject.transform.SetParent(targetParent, false);
                cardObject.transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
                Destroy(placeholder);
                onComplete?.Invoke();
            });
    }

    /// <summary>
    /// 여러 카드를 동시에 지정된 위치로 날려보냅니다 (멀리건 등에 사용).
    /// </summary>
    private IEnumerator MoveMultipleCardsToPosition(List<GameObject> cards, Transform targetTransform, bool destroyAfter = false)
    {
        if (cards.Count == 0) yield break;

        Sequence moveSequence = DOTween.Sequence();

        foreach (GameObject card in cards)
        {
            card.transform.SetParent(mainCanvasTransform, true);
            moveSequence.Join(card.transform.DOMove(targetTransform.position, cardMoveDuration).SetEase(Ease.InQuad));

            if (destroyAfter)
            {
                moveSequence.Join(card.transform.DOScale(Vector3.zero, cardMoveDuration));
            }
        }

        yield return moveSequence.WaitForCompletion();

        if (destroyAfter)
        {
            foreach (GameObject card in cards)
            {
                Destroy(card);
            }
        }
    }

    /// <summary>
    /// 덱에서 핸드로 카드를 뽑는 애니메이션을 실행합니다.
    /// </summary>
    private IEnumerator DrawSingleCardAnimated(CardDataSO cardData)
    {
        GameObject placeholder = Instantiate(placeholderPrefab, handPanelTransform);

        yield return new WaitForEndOfFrame();
        Vector3 targetPosition = placeholder.transform.position;

        GameObject newCardUI = Instantiate(cardUIPrefab, deckDrawTransform.position, Quaternion.identity, mainCanvasTransform);
        CardUI cardComponent = newCardUI.GetComponent<CardUI>();
        if (cardComponent != null)
        {
            cardComponent.Setup(cardData, this);
        }

        newCardUI.transform.DOMove(targetPosition, cardMoveDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                newCardUI.transform.SetParent(handPanelTransform, false);
                newCardUI.transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
                Destroy(placeholder);
            });
    }

    #endregion

    #region 카드 드로우 및 멀리건

    public void DrawHand()
    {
        foreach (Transform child in handPanelTransform) { Destroy(child.gameObject); }
        foreach (Transform child in actionPanelTransform) { Destroy(child.gameObject); }

        _discardPile.AddRange(_currentHand);
        _discardPile.AddRange(_selectedCards.Values);
        _currentHand.Clear();
        _selectedCards.Clear();

        ClearTargetingCard();
        CheckAndUpdateProceedButton();
        _remainingMulligans = _mulligansPerTurn;

        StartCoroutine(DrawCardsAnimated(_handSize));

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

        StartCoroutine(PerformMulliganAnimated());
    }

    private IEnumerator PerformMulliganAnimated()
    {
        _remainingMulligans--;
        Debug.Log($"<color=orange>[Mulligan] 멀리건을 실행합니다. 남은 횟수: {_remainingMulligans}</color>");

        _discardPile.AddRange(_currentHand);
        _currentHand.Clear();

        List<GameObject> cardsToDiscard = new List<GameObject>();
        foreach (Transform cardTransform in handPanelTransform)
        {
            cardsToDiscard.Add(cardTransform.gameObject);
        }

        yield return StartCoroutine(MoveMultipleCardsToPosition(cardsToDiscard, discardPileTransform, destroyAfter: true));
        yield return StartCoroutine(DrawCardsAnimated(_handSize));
    }

    private IEnumerator DrawCardsAnimated(int amount)
    {
        Debug.Log($"[Draw] {amount}장의 카드를 새로 뽑습니다.");

        for (int i = 0; i < amount; i++)
        {
            if (_playerDeck.Count == 0)
            {
                ReshuffleDiscardPile();
            }
            if (_playerDeck.Count == 0)
            {
                Debug.LogWarning($"[Draw] 덱과 버린 더미에 더 이상 뽑을 카드가 없습니다.");
                break;
            }

            CardDataSO drawnCard = _playerDeck[0];
            _playerDeck.RemoveAt(0);
            _currentHand.Add(drawnCard);

            yield return StartCoroutine(DrawSingleCardAnimated(drawnCard));
            yield return new WaitForSeconds(cardDrawDelay);
        }
    }

    #endregion

    #region 카드 상호작용 및 타겟팅

    public void OnCardClicked(GameObject cardObject, CardDataSO cardData)
    {
        if (GameManager.Instance.currentPhase == GameManager.BattlePhase.PlayerTurn_CardSelection)
        {
            if (cardObject.transform.parent == handPanelTransform)
            {
                if (actionPanelTransform.childCount < _maxActionsPerTurn)
                {
                    StartCoroutine(MoveCardToActionPanelAnimated(cardObject, cardData));
                }
            }
            else if (cardObject.transform.parent == actionPanelTransform)
            {
                StartCoroutine(MoveCardToHandPanelAnimated(cardObject, cardData));
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

    private void HandleMouseClick()
    {
        if (_targetingCard == null) return;
        if (Input.GetMouseButtonDown(0))
        {
            if (raycastCamera == null) return;
            Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
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

    private IEnumerator CommitTargetCoroutine(GameObject targetTile)
    {
        if (_targetingCard == null || _targetingCardUIObject == null) yield break;
        Tile3D clickedTile3D = targetTile.GetComponent<Tile3D>();
        if (clickedTile3D == null || !_currentPreviewTiles.Contains(clickedTile3D)) yield break;

        CardDataSO cardToUse = _targetingCard;
        QueuedAction newAction = new QueuedAction { User = playerController, SourceCard = cardToUse, TargetTile = targetTile };

        if (ActionTurnManager.Instance.IsProcessingQueue)
        {
            if (playerController.HasBreaksLeft())
            {
                playerController.UseBreak();
                ActionTurnManager.Instance.AddActionToInterruptQueue(newAction);
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

    #endregion

    #region 카드 이동 및 상태 관리

    private IEnumerator MoveCardToActionPanelAnimated(GameObject cardObject, CardDataSO cardData)
    {
        _currentHand.Remove(cardData);
        _selectedCards.Add(cardObject, cardData);

        yield return StartCoroutine(MoveCardWithPlaceholder(cardObject, actionPanelTransform, () => {
            CheckAndUpdateProceedButton();
            UpdateTargetingCard();
        }));
    }

    private IEnumerator MoveCardToHandPanelAnimated(GameObject cardObject, CardDataSO cardData)
    {
        _selectedCards.Remove(cardObject);
        _currentHand.Add(cardData);

        yield return StartCoroutine(MoveCardWithPlaceholder(cardObject, handPanelTransform, () => {
            CheckAndUpdateProceedButton();
            UpdateTargetingCard();
        }));
    }

    private void CheckAndUpdateProceedButton()
    {
        bool shouldShow = _selectedCards.Count >= _maxActionsPerTurn;
        if (BattleUIManager.instance != null)
        {
            if (shouldShow) BattleUIManager.instance.ShowProceedButton();
            else BattleUIManager.instance.HideProceedButton();
        }
    }

    public void OnActionPhaseStart()
    {
        UpdateTargetingCard();
    }

    #endregion

    #region 덱 및 버린 카드 관리

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

    #endregion

    #region 디버그 및 상태 확인용

    public int GetActionCardCount()
    {
        return actionPanelTransform.childCount;
    }

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
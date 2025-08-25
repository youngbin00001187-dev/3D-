using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class CardManager : MonoBehaviour
{
    public static CardManager instance;

    [Header("UI ����")]
    public GameObject cardUiPrefab;
    public Transform handTransform;
    public Transform actionPanelTransform;
    public Button proceedButton;

    [Header("���� ����")]
    public PlayerController playerController;
    public LayerMask tileLayerMask;

    // --- ī�� �� ���� ������ ---
    private List<CardDataSO> drawPile = new List<CardDataSO>();
    private List<CardDataSO> hand = new List<CardDataSO>();
    private List<CardDataSO> discardPile = new List<CardDataSO>();
    private List<GameObject> handUiObjects = new List<GameObject>();
    private List<CardDataSO> actionCards = new List<CardDataSO>();
    private List<GameObject> actionCardUiObjects = new List<GameObject>();

    // --- ���� ��Ģ ���� ������ ---
    private int handSize;
    private int mulligansLeft;
    private int playerMaxActionsPerTurn;
    private bool isInitialized = false;

    // --- Ÿ���� �� ���̶���Ʈ ���� ������ ---
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
        // '����' ��ư�� ������ �� OnProceedButtonClicked �Լ��� ȣ��ǵ��� ����մϴ�.
        if (proceedButton != null)
        {
            proceedButton.onClick.AddListener(OnProceedButtonClicked);
        }
    }
    void Update()
    {
        if (GameManager.instance == null || playerController == null) return;

        // Ÿ���� ����� ��(�׼� �гο� ī�尡 ���� ��)�� Ÿ�� ȣ���� �����մϴ�.
        // �� ����� ��� ������� �۵��մϴ�.
        if (selectedCardForTargeting != null)
        {
            HandleTileHoverForAoePreview();
        }

        // '�׼� ����' �������� ���� Ÿ�� Ŭ�� �Է��� �޽��ϴ�.
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

        // �����: ����ĳ��Ʈ�� ����� �۵��ϴ��� Ȯ��
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 0.1f);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, tileLayerMask))
        {
            currentHoveredTile = hit.collider.GetComponent<Tile3D>();

            // �����: � ������Ʈ�� ��Ʈ�ߴ��� Ȯ��
            Debug.Log($"<color=cyan>[AOE Preview] ����ĳ��Ʈ ��Ʈ: {hit.collider.name}, Tile3D ������Ʈ: {(currentHoveredTile != null ? "����" : "����")}</color>");
        }
        else
        {
            // �����: ����ĳ��Ʈ�� �ƹ��͵� ��Ʈ���� ���� ��
            Debug.Log($"<color=orange>[AOE Preview] ����ĳ��Ʈ ��Ʈ ����. LayerMask: {tileLayerMask.value}</color>");
        }

        // ���콺�� �ٸ� Ÿ�Ϸ� �������ٸ�
        if (currentHoveredTile != _lastHoveredTile)
        {
            Debug.Log($"<color=yellow>[AOE Preview] Ÿ�� ����: {(_lastHoveredTile?.name ?? "null")} -> {(currentHoveredTile?.name ?? "null")}</color>");

            // 1. ������ �ִ� AOE �̸����� ���̶���Ʈ�� ����ϴ�.
            ClearAoePreviewHighlight();

            // 2. ���� ȣ���� Ÿ���� '���� ���� ����'(_targetingHighlightTiles) �ȿ� �ִٸ�,
            if (currentHoveredTile != null && _targetingHighlightTiles.Contains(currentHoveredTile))
            {
                Debug.Log($"<color=green>[AOE Preview] ��ȿ�� Ÿ�� Ÿ�Ͽ� ȣ��: {currentHoveredTile.name}</color>");
                // 3. �� Ÿ���� �߽����� AOE �̸����⸦ ���׸��ϴ�.
                ShowAoePreviewHighlight(currentHoveredTile);
            }
            else if (currentHoveredTile != null)
            {
                Debug.Log($"<color=red>[AOE Preview] ��ȿ���� ���� Ÿ�� Ÿ��: {currentHoveredTile.name}. Ÿ���� ���̶���Ʈ Ÿ�� ��: {_targetingHighlightTiles.Count}</color>");

                // �����: Ÿ���� ���̶���Ʈ Ÿ�ϵ��� �̸��� ���
                for (int i = 0; i < _targetingHighlightTiles.Count; i++)
                {
                    Debug.Log($"  - Ÿ���� ���̶���Ʈ Ÿ��[{i}]: {_targetingHighlightTiles[i]?.name ?? "null"}");
                }
            }
            _lastHoveredTile = currentHoveredTile;
        }
    }
    public void OnProceedButtonClicked()
    {
       
        {
            // ���� �� �κ��� �����Ǿ����ϴ� ����
            // ���� �ڷ�ƾ�� �����ϴ� ���, '�׼� ������ ����' �̺�Ʈ�� ����մϴ�.
            if (BattleEventManager.instance != null)
            {
                BattleEventManager.instance.RaiseActionPhaseStart();
            }
            // ����������������������

            // ���� �׼� ������ �� �̺�Ʈ�� ������ �ٸ� �Լ��� ����ϰ� �˴ϴ�.
            // StartCoroutine(ActionPhaseCoroutine()); // �� ���� ���� �ʿ� �����ϴ�.
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
                Debug.Log($"<color=yellow>��ȿ�� Ÿ��({clickedTile.name}) Ŭ��! �׼��� �����մϴ�.</color>");
                // TODO: TurnManager���� �׼� ���� �� ����� ī�� ���� ����
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

    // --- ���̶���Ʈ ���� ---

    public void HandleCardHoverEnter(CardDataSO cardData)
    {
        if (HighlightManager.instance == null || playerController == null) return;

        Debug.Log($"<color=magenta>[Card Hover] ī�� ȣ�� ����: {cardData.name}</color>");

        ClearTargetingHighlight();

        List<GameObject> tiles = GetTargetableTiles(cardData, playerController);
        Debug.Log($"<color=magenta>[Card Hover] Ÿ�� ������ Ÿ�� ��: {tiles.Count}</color>");

        _hoverHighlightTiles = tiles.Select(t => t.GetComponent<Tile3D>()).ToList();
        HighlightManager.instance.AddHighlight(_hoverHighlightTiles, HighlightManager.HighlightType.PlayerPreview);
    }

    public void HandleCardHoverExit()
    {
        if (HighlightManager.instance == null) return;

        Debug.Log($"<color=magenta>[Card Hover] ī�� ȣ�� ����</color>");

        ClearHoverHighlight();

        ShowTargetingHighlight();
    }

    private void UpdateTargetingMode()
    {
        Debug.Log($"<color=blue>[Targeting] Ÿ���� ��� ������Ʈ. �׼� ī�� ��: {actionCards.Count}</color>");

        ClearTargetingHighlight();

        if (actionCards.Any())
        {
            selectedCardForTargeting = actionCards[0];
            Debug.Log($"<color=blue>[Targeting] Ÿ���ÿ� ī�� ����: {selectedCardForTargeting.name}</color>");
            ShowTargetingHighlight();
        }
        else
        {
            selectedCardForTargeting = null;
            Debug.Log($"<color=blue>[Targeting] Ÿ���ÿ� ī�� ����</color>");
        }
        CheckProceedButtonState();
    }

    private void ShowTargetingHighlight()
    {
        if (selectedCardForTargeting != null)
        {
            Debug.Log($"<color=blue>[Targeting] Ÿ���� ���̶���Ʈ ǥ��: {selectedCardForTargeting.name}</color>");

            List<GameObject> tiles = GetTargetableTiles(selectedCardForTargeting, playerController);
            Debug.Log($"<color=blue>[Targeting] Ÿ�� ������ Ÿ�� ��: {tiles.Count}</color>");

            _targetingHighlightTiles = tiles.Select(t => t.GetComponent<Tile3D>()).Where(t => t != null).ToList();
            Debug.Log($"<color=blue>[Targeting] Tile3D ������Ʈ�� �ִ� Ÿ�� ��: {_targetingHighlightTiles.Count}</color>");

            if (_targetingHighlightTiles.Count > 0)
            {
                HighlightManager.instance.AddHighlight(_targetingHighlightTiles, HighlightManager.HighlightType.PlayerPreview);

                // �����: Ÿ���� ���̶���Ʈ Ÿ�ϵ��� �̸� ���
                for (int i = 0; i < _targetingHighlightTiles.Count; i++)
                {
                    Debug.Log($"  - Ÿ���� Ÿ��[{i}]: {_targetingHighlightTiles[i].name}");
                }
            }
        }
    }

    private void ClearTargetingHighlight()
    {
        if (_targetingHighlightTiles.Count > 0)
        {
            Debug.Log($"<color=blue>[Targeting] Ÿ���� ���̶���Ʈ ����: {_targetingHighlightTiles.Count}�� Ÿ��</color>");
            HighlightManager.instance.RemoveHighlight(_targetingHighlightTiles, HighlightManager.HighlightType.PlayerPreview);
            _targetingHighlightTiles.Clear();
        }
    }

    private void ClearHoverHighlight()
    {
        if (_hoverHighlightTiles.Count > 0)
        {
            Debug.Log($"<color=magenta>[Card Hover] ȣ�� ���̶���Ʈ ����: {_hoverHighlightTiles.Count}�� Ÿ��</color>");
            HighlightManager.instance.RemoveHighlight(_hoverHighlightTiles, HighlightManager.HighlightType.PlayerPreview);
            _hoverHighlightTiles.Clear();
        }
    }

    private void ShowAoePreviewHighlight(Tile3D targetTile)
    {
        if (selectedCardForTargeting == null)
        {
            Debug.LogWarning("[AOE Preview] selectedCardForTargeting�� null�Դϴ�!");
            return;
        }

        if (selectedCardForTargeting.actionSequence == null || selectedCardForTargeting.actionSequence.Count == 0)
        {
            Debug.LogWarning($"[AOE Preview] {selectedCardForTargeting.name}�� actionSequence�� null�̰ų� ����ֽ��ϴ�!");
            return;
        }

        GameAction mainAction = selectedCardForTargeting.actionSequence[0];
        Debug.Log($"<color=green>[AOE Preview] AOE �̸����� ǥ�� ��. Ÿ�� Ÿ��: {targetTile.name}, �׼�: {mainAction.GetType().Name}</color>");

        List<GameObject> aoeTiles = mainAction.GetActionImpactTiles(playerController, targetTile.gameObject);
        Debug.Log($"<color=green>[AOE Preview] AOE ���� Ÿ�� ��: {aoeTiles.Count}</color>");

        _aoePreviewTiles = aoeTiles.Select(t => t.GetComponent<Tile3D>()).Where(t => t != null).ToList();
        Debug.Log($"<color=green>[AOE Preview] Tile3D ������Ʈ�� �ִ� AOE Ÿ�� ��: {_aoePreviewTiles.Count}</color>");

        if (_aoePreviewTiles.Count > 0)
        {
            // �����: AOE Ÿ�ϰ� Ÿ���� Ÿ���� ��ġ���� Ȯ��
            foreach (var aoe in _aoePreviewTiles)
            {
                bool isAlsoTargeting = _targetingHighlightTiles.Contains(aoe);
                Debug.Log($"<color=cyan>[AOE Preview] {aoe.name} - Ÿ���� Ÿ���̱⵵ ��: {isAlsoTargeting}</color>");
            }

            // HighlightManager ���� üũ
            if (HighlightManager.instance == null)
            {
                Debug.LogError("[AOE Preview] HighlightManager.instance�� null�Դϴ�!");
                return;
            }

            Debug.Log($"<color=green>[AOE Preview] HighlightManager�� ���̶���Ʈ �߰� ��û��... Ÿ��: PlayerTarget</color>");

            HighlightManager.instance.AddHighlight(_aoePreviewTiles, HighlightManager.HighlightType.PlayerTarget);

            Debug.Log($"<color=green>[AOE Preview] HighlightManager.AddHighlight ȣ�� �Ϸ�</color>");

            // �����: ���� ���� �����ؼ� �׽�Ʈ
            Debug.Log($"<color=red>[TEST] ���� ���� ���� �׽�Ʈ ����</color>");
            foreach (var tile in _aoePreviewTiles)
            {
                if (tile.MyMaterial != null)
                {
                    tile.MyMaterial.color = Color.red; // ���������� ���� ����
                    Debug.Log($"<color=red>[TEST] {tile.name}�� ���������� ���� ����</color>");
                }
            }
        }
    }

    private void ClearAoePreviewHighlight()
    {
        if (_aoePreviewTiles.Count > 0)
        {
            Debug.Log($"<color=green>[AOE Preview] AOE �̸����� ����: {_aoePreviewTiles.Count}�� Ÿ��</color>");

            if (HighlightManager.instance == null)
            {
                Debug.LogError("[AOE Preview] HighlightManager.instance�� null�Դϴ�!");
                return;
            }

            Debug.Log($"<color=green>[AOE Preview] HighlightManager�� ���̶���Ʈ ���� ��û��...</color>");
            HighlightManager.instance.RemoveHighlight(_aoePreviewTiles, HighlightManager.HighlightType.PlayerTarget);

            // AOE ���� �� ���� PlayerPreview ���̶���Ʈ ����
            HighlightManager.instance.AddHighlight(_aoePreviewTiles, HighlightManager.HighlightType.PlayerPreview);

            Debug.Log($"<color=green>[AOE Preview] HighlightManager.RemoveHighlight ȣ�� �Ϸ�</color>");

            _aoePreviewTiles.Clear();
        }
    }

    private List<GameObject> GetTargetableTiles(CardDataSO cardData, UnitController user)
    {
        if (cardData.actionSequence == null || cardData.actionSequence.Count == 0)
        {
            Debug.LogWarning($"[GetTargetableTiles] {cardData.name}�� actionSequence�� null�̰ų� ����ֽ��ϴ�!");
            return new List<GameObject>();
        }

        GameAction mainAction = cardData.actionSequence[0];
        List<GameObject> targetableTiles = mainAction.GetTargetableTiles(user);
        Debug.Log($"[GetTargetableTiles] {cardData.name}�� Ÿ�� ������ Ÿ�� ��: {targetableTiles.Count}");

        return targetableTiles;
    }

    // HighlightManager���� ȣ���� �޼���
    public bool IsTargeting()
    {
        return selectedCardForTargeting != null;
    }
}
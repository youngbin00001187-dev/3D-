using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ������ ��ü���� �帧(�غ� -> ī�弱�� -> �׼�)�� �����ϴ� �߾� �Ŵ����Դϴ�.
/// BattleEventManager�κ��� ��ȣ�� �޾�, �÷��̾�� ���� ���� �ְ�޴� �׼� ����� �����մϴ�.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum BattlePhase { Setup, PlayerTurn_CardSelection, ActionPhase, CombatEnded }
    public BattlePhase currentPhase;

    [Header("���� ��� ����")]
    public CardManager cardManager;
    public ActionTurnManager actionTurnManager;
    public PlayerController player;

    [Header("UI ���")]
    [Tooltip("Ÿ�� Ŭ���� ���� 3D Plane ������Ʈ�� �����ϼ���.")]
    public GameObject inputBlocker;

    private List<UnitController> _allUnits = new List<UnitController>();
    private List<EnemyController> _allEnemies = new List<EnemyController>();

    private bool _isPlayerActionSubmitted = false;
    private bool _isActionPhaseActive = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetInputBlocker(false);
    }

    void Update()
    {
        if (currentPhase == BattlePhase.ActionPhase)
        {
            UpdateInputBlockerStatus();
        }
    }

    private void UpdateInputBlockerStatus()
    {
        if (actionTurnManager == null || player == null) return;
        bool shouldBlock = actionTurnManager.IsProcessingQueue && !player.HasBreaksLeft();
        SetInputBlocker(shouldBlock);
    }

    private void OnEnable()
    {
        if (BattleEventManager.instance != null)
        {
            // [����] OnAllUnitsPlaced ��� CardManager�� �غ�Ǿ��ٴ� ��ȣ�� ����ϴ�.
            BattleEventManager.instance.OnCardManagerReady += OnCardManagerReadyHandler;
            BattleEventManager.instance.OnActionPhaseStart += OnActionPhaseStartHandler;
        }
    }

    private void OnDisable()
    {
        if (BattleEventManager.instance != null)
        {
            // [����] ���� ������ �����մϴ�.
            BattleEventManager.instance.OnCardManagerReady -= OnCardManagerReadyHandler;
            BattleEventManager.instance.OnActionPhaseStart -= OnActionPhaseStartHandler;
        }
    }

    /// <summary>
    /// [����] CardManager�� �غ� �Ϸ� ��ȣ�� ������ �� ȣ��Ǵ� �ڵ鷯�Դϴ�.
    /// </summary>
    private void OnCardManagerReadyHandler()
    {
        _allUnits = FindObjectsByType<UnitController>(FindObjectsSortMode.None).ToList();
        _allEnemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None).ToList();
        player = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).FirstOrDefault();
        StartPreparePhase();
    }

    private void StartPreparePhase()
    {
        _isActionPhaseActive = false;
        foreach (var unit in _allUnits)
        {
            if (unit != null) unit.ResetActions();
        }
        if (player != null)
        {
            player.ResetBreaks();
        }
        SetCurrentPhase(BattlePhase.Setup);

        foreach (var enemy in _allEnemies)
        {
            if (enemy != null)
            {
                enemy.PlanNextAction();
                enemy.UpdateIntentDisplay();
            }
        }

        SetCurrentPhase(BattlePhase.PlayerTurn_CardSelection);
        cardManager.DrawHand();
        Debug.Log("<color=yellow>=== �غ� ������ �Ϸ�. ī�� ������ �����ϼ���. ===</color>");
    }

    private void OnActionPhaseStartHandler()
    {
        if (_isActionPhaseActive) return;
        _isActionPhaseActive = true;
        SetCurrentPhase(BattlePhase.ActionPhase);
        StartCoroutine(ActionPhaseLoopCoroutine());
    }

    public void NotifyPlayerActionSubmitted()
    {
        _isPlayerActionSubmitted = true;
    }

    private IEnumerator ActionPhaseLoopCoroutine()
    {
        // '�÷��̾� �ൿ ����'
        while (cardManager.GetActionCardCount() > 0)
        {
            Debug.Log("<color=cyan>�÷��̾� ��: �ൿ�� �����ϼ���.</color>");
            _isPlayerActionSubmitted = false;
            yield return new WaitUntil(() => _isPlayerActionSubmitted);

            // 1. ���� �̹� '��ȹ��' �ൿ�� ����(ť�� ���)�մϴ�.
            Debug.Log("<color=red>�� ��: ��� ���� ��ȹ�� �ൿ�� ť�� �߰��մϴ�.</color>");
            foreach (var enemy in _allEnemies.Where(e => e != null && e.HasMoreActionsThisRound()))
            {
                enemy.CommitActionToQueue();
                enemy.DecrementActionsLeft();
            }

            // 2. �׼� ť�� ó���մϴ�.
            Debug.Log("<color=yellow>ť ó�� ����...</color>");
            yield return StartCoroutine(actionTurnManager.ProcessActionQueueCoroutine());
            Debug.Log("<color=yellow>ť ó�� �Ϸ�.</color>");

            if (CheckForCombatEnd()) yield break;

            // 3. ��� �׼� ó���� ���� ��, ���� '����' �ൿ�� ��ȹ�ϰ� ǥ���մϴ�.
            Debug.Log("<color=orange>������ ���� �ൿ�� ��ȹ�ϰ� ǥ���մϴ�...</color>");
            foreach (var enemy in _allEnemies.Where(e => e != null && e.HasMoreActionsThisRound()))
            {
                enemy.PlanNextAction();
                enemy.UpdateIntentDisplay();
            }

            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("<color=red>�÷��̾� �ൿ ����. ���� ���� ���� �����մϴ�.</color>");

        // '���� �� �ൿ ����'
        while (_allEnemies.Any(e => e != null && e.HasMoreActionsThisRound()))
        {
            // �� ������ ���� ���� �ൿ�� �̹� ��ȹ�Ǿ� �ְ� �ǵ��� ǥ�õ� �����Դϴ�.
            // �÷��̾ �ǵ��� �� �ð��� �ݴϴ�.
            yield return new WaitForSeconds(1.0f);

            // 1. ���� �̹� '��ȹ��' �ൿ�� ����(ť�� ���)�մϴ�.
            foreach (var enemy in _allEnemies.Where(e => e != null && e.HasMoreActionsThisRound()))
            {
                enemy.CommitActionToQueue();
                enemy.DecrementActionsLeft();
            }

            // 2. �׼� ť�� ó���մϴ�.
            Debug.Log("<color=yellow>ť ó�� ����...</color>");
            yield return StartCoroutine(actionTurnManager.ProcessActionQueueCoroutine());
            Debug.Log("<color=yellow>ť ó�� �Ϸ�.</color>");

            if (CheckForCombatEnd()) yield break;

            // 3. ��� �׼� ó���� ���� ��, ���� ������ ���� '����' �ൿ�� ��ȹ�ϰ� ǥ���մϴ�.
            Debug.Log("<color=orange>������ ���� �ൿ�� ��ȹ�ϰ� ǥ���մϴ�...</color>");
            foreach (var enemy in _allEnemies.Where(e => e != null && e.HasMoreActionsThisRound()))
            {
                enemy.PlanNextAction();
                enemy.UpdateIntentDisplay();
            }

            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("<color=green>=== ��� �ൿ �Ϸ�. ���带 �����մϴ�. ===</color>");
        EndRound();
    }

    public void EndRound()
    {
        _isActionPhaseActive = false;
        SetInputBlocker(false);
        if (CheckForCombatEnd()) return;
        StartPreparePhase();
    }

    #region ���� ���� �� ��ƿ��Ƽ �Լ�

    public bool CheckForCombatEnd()
    {
        if (!_allEnemies.Any(e => e != null && e.currentHealth > 0)) { OnCombatVictory(); return true; }
        if (player != null && player.currentHealth <= 0) { OnCombatDefeat(); return true; }
        return false;
    }

    private void OnCombatVictory()
    {
        if (currentPhase == BattlePhase.CombatEnded) return;
        Debug.Log("<color=yellow>===== ���� �¸�! =====</color>");
        SetCurrentPhase(BattlePhase.CombatEnded);
        SetInputBlocker(true);
    }

    private void OnCombatDefeat()
    {
        if (currentPhase == BattlePhase.CombatEnded) return;
        Debug.Log("<color=red>===== ���� �й�... =====</color>");
        SetCurrentPhase(BattlePhase.CombatEnded);
        SetInputBlocker(true);
    }

    public void SetCurrentPhase(BattlePhase newPhase)
    {
        currentPhase = newPhase;
        Debug.Log($"[GameManager] ���� ������: {newPhase}");
    }

    private void SetInputBlocker(bool isBlocked)
    {
        if (inputBlocker == null) return;
        inputBlocker.SetActive(isBlocked);
        Debug.Log($"[GameManager] 3D Input Blocker {(isBlocked ? "ON" : "OFF")}");
    }

    #endregion
}
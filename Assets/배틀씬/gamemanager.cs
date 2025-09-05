using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 게임의 전체적인 흐름(준비 -> 카드선택 -> 액션)을 관리하는 중앙 매니저입니다.
/// BattleEventManager로부터 신호를 받아, 플레이어와 적이 턴을 주고받는 액션 페이즈를 제어합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum BattlePhase { Setup, PlayerTurn_CardSelection, ActionPhase, CombatEnded }
    public BattlePhase currentPhase;

    [Header("관리 대상 연결")]
    public CardManager cardManager;
    public ActionTurnManager actionTurnManager;
    public PlayerController player;

    [Header("UI 요소")]
    [Tooltip("타일 클릭을 막는 3D Plane 오브젝트를 연결하세요.")]
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
            // [수정] OnAllUnitsPlaced 대신 CardManager가 준비되었다는 신호를 듣습니다.
            BattleEventManager.instance.OnCardManagerReady += OnCardManagerReadyHandler;
            BattleEventManager.instance.OnActionPhaseStart += OnActionPhaseStartHandler;
        }
    }

    private void OnDisable()
    {
        if (BattleEventManager.instance != null)
        {
            // [수정] 구독 해제도 변경합니다.
            BattleEventManager.instance.OnCardManagerReady -= OnCardManagerReadyHandler;
            BattleEventManager.instance.OnActionPhaseStart -= OnActionPhaseStartHandler;
        }
    }

    /// <summary>
    /// [수정] CardManager가 준비 완료 신호를 보냈을 때 호출되는 핸들러입니다.
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
        Debug.Log("<color=yellow>=== 준비 페이즈 완료. 카드 선택을 시작하세요. ===</color>");
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
        // '플레이어 행동 루프'
        while (cardManager.GetActionCardCount() > 0)
        {
            Debug.Log("<color=cyan>플레이어 턴: 행동을 선택하세요.</color>");
            _isPlayerActionSubmitted = false;
            yield return new WaitUntil(() => _isPlayerActionSubmitted);

            // 1. 적은 이미 '계획된' 행동을 실행(큐에 등록)합니다.
            Debug.Log("<color=red>적 턴: 모든 적이 계획된 행동을 큐에 추가합니다.</color>");
            foreach (var enemy in _allEnemies.Where(e => e != null && e.HasMoreActionsThisRound()))
            {
                enemy.CommitActionToQueue();
                enemy.DecrementActionsLeft();
            }

            // 2. 액션 큐를 처리합니다.
            Debug.Log("<color=yellow>큐 처리 시작...</color>");
            yield return StartCoroutine(actionTurnManager.ProcessActionQueueCoroutine());
            Debug.Log("<color=yellow>큐 처리 완료.</color>");

            if (CheckForCombatEnd()) yield break;

            // 3. 모든 액션 처리가 끝난 후, 적은 '다음' 행동을 계획하고 표시합니다.
            Debug.Log("<color=orange>적들이 다음 행동을 계획하고 표시합니다...</color>");
            foreach (var enemy in _allEnemies.Where(e => e != null && e.HasMoreActionsThisRound()))
            {
                enemy.PlanNextAction();
                enemy.UpdateIntentDisplay();
            }

            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("<color=red>플레이어 행동 종료. 남은 적의 턴을 진행합니다.</color>");

        // '남은 적 행동 루프'
        while (_allEnemies.Any(e => e != null && e.HasMoreActionsThisRound()))
        {
            // 이 시점에 적의 다음 행동은 이미 계획되어 있고 의도도 표시된 상태입니다.
            // 플레이어가 의도를 볼 시간을 줍니다.
            yield return new WaitForSeconds(1.0f);

            // 1. 적은 이미 '계획된' 행동을 실행(큐에 등록)합니다.
            foreach (var enemy in _allEnemies.Where(e => e != null && e.HasMoreActionsThisRound()))
            {
                enemy.CommitActionToQueue();
                enemy.DecrementActionsLeft();
            }

            // 2. 액션 큐를 처리합니다.
            Debug.Log("<color=yellow>큐 처리 시작...</color>");
            yield return StartCoroutine(actionTurnManager.ProcessActionQueueCoroutine());
            Debug.Log("<color=yellow>큐 처리 완료.</color>");

            if (CheckForCombatEnd()) yield break;

            // 3. 모든 액션 처리가 끝난 후, 다음 루프를 위해 '다음' 행동을 계획하고 표시합니다.
            Debug.Log("<color=orange>적들이 다음 행동을 계획하고 표시합니다...</color>");
            foreach (var enemy in _allEnemies.Where(e => e != null && e.HasMoreActionsThisRound()))
            {
                enemy.PlanNextAction();
                enemy.UpdateIntentDisplay();
            }

            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("<color=green>=== 모든 행동 완료. 라운드를 종료합니다. ===</color>");
        EndRound();
    }

    public void EndRound()
    {
        _isActionPhaseActive = false;
        SetInputBlocker(false);
        if (CheckForCombatEnd()) return;
        StartPreparePhase();
    }

    #region 전투 종료 및 유틸리티 함수

    public bool CheckForCombatEnd()
    {
        if (!_allEnemies.Any(e => e != null && e.currentHealth > 0)) { OnCombatVictory(); return true; }
        if (player != null && player.currentHealth <= 0) { OnCombatDefeat(); return true; }
        return false;
    }

    private void OnCombatVictory()
    {
        if (currentPhase == BattlePhase.CombatEnded) return;
        Debug.Log("<color=yellow>===== 전투 승리! =====</color>");
        SetCurrentPhase(BattlePhase.CombatEnded);
        SetInputBlocker(true);
    }

    private void OnCombatDefeat()
    {
        if (currentPhase == BattlePhase.CombatEnded) return;
        Debug.Log("<color=red>===== 전투 패배... =====</color>");
        SetCurrentPhase(BattlePhase.CombatEnded);
        SetInputBlocker(true);
    }

    public void SetCurrentPhase(BattlePhase newPhase)
    {
        currentPhase = newPhase;
        Debug.Log($"[GameManager] 현재 페이즈: {newPhase}");
    }

    private void SetInputBlocker(bool isBlocked)
    {
        if (inputBlocker == null) return;
        inputBlocker.SetActive(isBlocked);
        Debug.Log($"[GameManager] 3D Input Blocker {(isBlocked ? "ON" : "OFF")}");
    }

    #endregion
}
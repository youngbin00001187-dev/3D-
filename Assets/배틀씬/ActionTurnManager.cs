using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 게임의 모든 액션을 관리하는 중앙 큐 매니저입니다.
/// 통상 큐와 인터럽트 큐를 분리하여 우선순위가 높은 행동을 먼저 처리합니다.
/// </summary>
public class ActionTurnManager : MonoBehaviour
{
    public static ActionTurnManager Instance { get; private set; }

    [Header("관리 대상 연결")]
    public GameManager gameManager;

    [Header("액션 실행 딜레이 설정")]
    [Tooltip("액션 턴이 시작되기 전의 대기 시간입니다. (선딜)")]
    [SerializeField] private float normalActionPreDelay = 0f;
    [Tooltip("액션 턴이 종료된 후의 대기 시간입니다. (후딜)")]
    [SerializeField] private float normalActionDelay = 0.5f;
    [Space]
    [Tooltip("인터럽트 턴이 시작되기 전의 대기 시간입니다. (선딜)")]
    [SerializeField] private float interruptActionPreDelay = 0f;
    [Tooltip("인터럽트 턴이 종료된 후의 대기 시간입니다. (후딜)")]
    [SerializeField] private float interruptActionDelay = 0.2f;


    private Queue<QueuedAction> _normalQueue = new Queue<QueuedAction>();
    private Queue<QueuedAction> _interruptQueue = new Queue<QueuedAction>();

    private bool _isProcessingQueue = false;
    public bool IsProcessingQueue => _isProcessingQueue;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void OnEnable()
    {
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.OnActionPhaseStart += OnActionPhaseStartHandler;
        }
    }

    private void OnDisable()
    {
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.OnActionPhaseStart -= OnActionPhaseStartHandler;
        }
    }

    private void OnActionPhaseStartHandler()
    {
        Debug.Log("<color=yellow>[ActionTurnManager] 액션 페이즈 시작 이벤트 수신. 큐 처리 준비 완료.</color>");
    }

    /// <summary>
    /// 인터럽트 큐를 확인하고 처리하는 보조 코루틴입니다.
    /// </summary>
    private IEnumerator ProcessInterruptQueue()
    {
        if (_interruptQueue.Count > 0)
        {
            if (BreakEffectManager.Instance != null) BreakEffectManager.Instance.StartBreakEffect();

            while (_interruptQueue.Count > 0)
            {
                QueuedAction action = _interruptQueue.Dequeue();

                if (interruptActionPreDelay > 0)
                {
                    yield return new WaitForSeconds(interruptActionPreDelay);
                }

                if (action.User != null)
                {
                    Animator animator = action.User.GetComponent<Animator>();
                    Debug.Log($"<color=purple>[인터럽트 액션 실행]</color> 유닛: {action.User.name}, 카드: {action.SourceCard.cardName}");
                    foreach (GameAction gameAction in action.SourceCard.actionSequence)
                    {
                        gameAction.Prepare(action.User, action.TargetTile);
                        yield return StartCoroutine(gameAction.Execute());
                    }
                    if (animator != null) animator.SetInteger("motionID", 0);
                }
                if (_interruptQueue.Count > 0)
                {
                    yield return new WaitForSeconds(interruptActionDelay);
                }
            }

            if (BreakEffectManager.Instance != null)
            {
                StartCoroutine(BreakEffectManager.Instance.StopBreakEffectSequence());
            }
        }
    }

    // ▼▼▼ 이 메서드의 로직이 핵심적으로 변경되었습니다 ▼▼▼
    /// <summary>
    /// 모든 액션 큐를 순차적으로 처리하는 메인 코루틴입니다. (인터럽트 우선)
    /// </summary>
    public IEnumerator ProcessActionQueueCoroutine()
    {
        _isProcessingQueue = true;

        // 두 큐 중 하나라도 액션이 남아있는 동안 계속 루프를 돕니다.
        while (_normalQueue.Count > 0 || _interruptQueue.Count > 0)
        {
            // 매 루프 시작 시, 인터럽트 큐를 최우선으로 확인하고 처리합니다.
            if (_interruptQueue.Count > 0)
            {
                yield return StartCoroutine(ProcessInterruptQueue());
            }

            // 인터럽트 큐가 비어있고, 통상 큐에 액션이 있다면 하나를 처리합니다.
            if (_normalQueue.Count > 0)
            {
                QueuedAction action = _normalQueue.Dequeue();

                if (normalActionPreDelay > 0)
                {
                    yield return new WaitForSeconds(normalActionPreDelay);
                }

                if (action.User != null)
                {
                    Animator animator = action.User.GetComponent<Animator>();
                    Debug.Log($"<color=cyan>[통상 액션 실행]</color> 유닛: {action.User.name}, 카드: {action.SourceCard.cardName}");
                    foreach (GameAction gameAction in action.SourceCard.actionSequence)
                    {
                        gameAction.Prepare(action.User, action.TargetTile);
                        yield return StartCoroutine(gameAction.Execute());
                    }
                    if (animator != null) animator.SetInteger("motionID", 0);
                }

                // 후딜은 통상 액션이 끝난 직후에만 적용됩니다.
                // 다음 행동이 인터럽트일 경우, 후딜 없이 즉시 반응해야 합니다.
                if (_normalQueue.Count > 0 && _interruptQueue.Count == 0)
                {
                    yield return new WaitForSeconds(normalActionDelay);
                }
            }
        }

        _isProcessingQueue = false;
        Debug.Log("<color=magenta>[ActionTurnManager] 모든 액션 큐 처리 완료.</color>");
    }

    public void AddActionToNormalQueue(QueuedAction action)
    {
        _normalQueue.Enqueue(action);
        Debug.Log($"[ActionTurnManager] 통상 큐에 '{action.SourceCard.cardName}' 액션 추가. 현재 큐 크기: {_normalQueue.Count}");
    }

    public void AddActionToInterruptQueue(QueuedAction action)
    {
        _interruptQueue.Enqueue(action);
        Debug.Log($"[ActionTurnManager] 인터럽트 큐에 '{action.SourceCard.cardName}' 액션 추가. 현재 큐 크기: {_interruptQueue.Count}");
    }

    public void SetNormalActionDelay(float delay)
    {
        normalActionDelay = Mathf.Max(0f, delay);
    }
    public void SetInterruptActionDelay(float delay)
    {
        interruptActionDelay = Mathf.Max(0f, delay);
    }
}
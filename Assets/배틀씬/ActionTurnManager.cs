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
    [SerializeField] private float normalActionDelay = 0.5f;
    [SerializeField] private float interruptActionDelay = 0.2f;

    private Queue<QueuedAction> _normalQueue = new Queue<QueuedAction>();
    private Queue<QueuedAction> _interruptQueue = new Queue<QueuedAction>();

    private bool _isProcessingQueue = false;
    public bool IsProcessingQueue => _isProcessingQueue;

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
            if (BreakEffectManager.Instance != null) BreakEffectManager.Instance.StopBreakEffect();
        }
    }

    /// <summary>
    /// 모든 액션 큐를 순차적으로 처리하는 메인 코루틴입니다.
    /// </summary>
    public IEnumerator ProcessActionQueueCoroutine()
    {
        _isProcessingQueue = true;

        // 시작 시 혹시 모를 인터럽트 큐를 먼저 처리
        yield return StartCoroutine(ProcessInterruptQueue());

        // 통상 큐가 빌 때까지 처리
        while (_normalQueue.Count > 0)
        {
            QueuedAction action = _normalQueue.Dequeue();
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

            // 통상 액션이 하나 끝날 때마다, 그 사이에 들어온 인터럽트가 있는지 확인/처리
            yield return StartCoroutine(ProcessInterruptQueue());

            if (_normalQueue.Count > 0)
            {
                yield return new WaitForSeconds(normalActionDelay);
            }
        }

        _isProcessingQueue = false;
        Debug.Log("<color=magenta>[ActionTurnManager] 모든 액션 큐 처리 완료.</color>");
    }

    /// <summary>
    /// 통상 큐에 액션을 추가합니다.
    /// </summary>
    public void AddActionToNormalQueue(QueuedAction action)
    {
        _normalQueue.Enqueue(action);
        Debug.Log($"[ActionTurnManager] 통상 큐에 '{action.SourceCard.cardName}' 액션 추가. 현재 큐 크기: {_normalQueue.Count}");
    }

    /// <summary>
    /// 인터럽트 큐에 액션을 추가합니다.
    /// </summary>
    public void AddActionToInterruptQueue(QueuedAction action)
    {
        _interruptQueue.Enqueue(action);
        Debug.Log($"[ActionTurnManager] 인터럽트 큐에 '{action.SourceCard.cardName}' 액션 추가. 현재 큐 크기: {_interruptQueue.Count}");
    }

    /// <summary>
    /// 런타임에서 딜레이 값을 변경하는 유틸리티 메서드들
    /// </summary>
    public void SetNormalActionDelay(float delay)
    {
        normalActionDelay = Mathf.Max(0f, delay);
    }
    public void SetInterruptActionDelay(float delay)
    {
        interruptActionDelay = Mathf.Max(0f, delay);
    }
}
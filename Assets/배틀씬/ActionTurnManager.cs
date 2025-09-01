using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ������ ��� �׼��� �����ϴ� �߾� ť �Ŵ����Դϴ�.
/// ��� ť�� ���ͷ�Ʈ ť�� �и��Ͽ� �켱������ ���� �ൿ�� ���� ó���մϴ�.
/// </summary>
public class ActionTurnManager : MonoBehaviour
{
    public static ActionTurnManager Instance { get; private set; }

    [Header("���� ��� ����")]
    public GameManager gameManager;

    [Header("�׼� ���� ������ ����")]
    [Tooltip("�׼� ���� ���۵Ǳ� ���� ��� �ð��Դϴ�. (����)")]
    [SerializeField] private float normalActionPreDelay = 0f;
    [Tooltip("�׼� ���� ����� ���� ��� �ð��Դϴ�. (�ĵ�)")]
    [SerializeField] private float normalActionDelay = 0.5f;
    [Space]
    [Tooltip("���ͷ�Ʈ ���� ���۵Ǳ� ���� ��� �ð��Դϴ�. (����)")]
    [SerializeField] private float interruptActionPreDelay = 0f;
    [Tooltip("���ͷ�Ʈ ���� ����� ���� ��� �ð��Դϴ�. (�ĵ�)")]
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
        Debug.Log("<color=yellow>[ActionTurnManager] �׼� ������ ���� �̺�Ʈ ����. ť ó�� �غ� �Ϸ�.</color>");
    }

    /// <summary>
    /// ���ͷ�Ʈ ť�� Ȯ���ϰ� ó���ϴ� ���� �ڷ�ƾ�Դϴ�.
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
                    Debug.Log($"<color=purple>[���ͷ�Ʈ �׼� ����]</color> ����: {action.User.name}, ī��: {action.SourceCard.cardName}");
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

    // ���� �� �޼����� ������ �ٽ������� ����Ǿ����ϴ� ����
    /// <summary>
    /// ��� �׼� ť�� ���������� ó���ϴ� ���� �ڷ�ƾ�Դϴ�. (���ͷ�Ʈ �켱)
    /// </summary>
    public IEnumerator ProcessActionQueueCoroutine()
    {
        _isProcessingQueue = true;

        // �� ť �� �ϳ��� �׼��� �����ִ� ���� ��� ������ ���ϴ�.
        while (_normalQueue.Count > 0 || _interruptQueue.Count > 0)
        {
            // �� ���� ���� ��, ���ͷ�Ʈ ť�� �ֿ켱���� Ȯ���ϰ� ó���մϴ�.
            if (_interruptQueue.Count > 0)
            {
                yield return StartCoroutine(ProcessInterruptQueue());
            }

            // ���ͷ�Ʈ ť�� ����ְ�, ��� ť�� �׼��� �ִٸ� �ϳ��� ó���մϴ�.
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
                    Debug.Log($"<color=cyan>[��� �׼� ����]</color> ����: {action.User.name}, ī��: {action.SourceCard.cardName}");
                    foreach (GameAction gameAction in action.SourceCard.actionSequence)
                    {
                        gameAction.Prepare(action.User, action.TargetTile);
                        yield return StartCoroutine(gameAction.Execute());
                    }
                    if (animator != null) animator.SetInteger("motionID", 0);
                }

                // �ĵ��� ��� �׼��� ���� ���Ŀ��� ����˴ϴ�.
                // ���� �ൿ�� ���ͷ�Ʈ�� ���, �ĵ� ���� ��� �����ؾ� �մϴ�.
                if (_normalQueue.Count > 0 && _interruptQueue.Count == 0)
                {
                    yield return new WaitForSeconds(normalActionDelay);
                }
            }
        }

        _isProcessingQueue = false;
        Debug.Log("<color=magenta>[ActionTurnManager] ��� �׼� ť ó�� �Ϸ�.</color>");
    }

    public void AddActionToNormalQueue(QueuedAction action)
    {
        _normalQueue.Enqueue(action);
        Debug.Log($"[ActionTurnManager] ��� ť�� '{action.SourceCard.cardName}' �׼� �߰�. ���� ť ũ��: {_normalQueue.Count}");
    }

    public void AddActionToInterruptQueue(QueuedAction action)
    {
        _interruptQueue.Enqueue(action);
        Debug.Log($"[ActionTurnManager] ���ͷ�Ʈ ť�� '{action.SourceCard.cardName}' �׼� �߰�. ���� ť ũ��: {_interruptQueue.Count}");
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
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
            if (BreakEffectManager.Instance != null) BreakEffectManager.Instance.StopBreakEffect();
        }
    }

    /// <summary>
    /// ��� �׼� ť�� ���������� ó���ϴ� ���� �ڷ�ƾ�Դϴ�.
    /// </summary>
    public IEnumerator ProcessActionQueueCoroutine()
    {
        _isProcessingQueue = true;

        // ���� �� Ȥ�� �� ���ͷ�Ʈ ť�� ���� ó��
        yield return StartCoroutine(ProcessInterruptQueue());

        // ��� ť�� �� ������ ó��
        while (_normalQueue.Count > 0)
        {
            QueuedAction action = _normalQueue.Dequeue();
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

            // ��� �׼��� �ϳ� ���� ������, �� ���̿� ���� ���ͷ�Ʈ�� �ִ��� Ȯ��/ó��
            yield return StartCoroutine(ProcessInterruptQueue());

            if (_normalQueue.Count > 0)
            {
                yield return new WaitForSeconds(normalActionDelay);
            }
        }

        _isProcessingQueue = false;
        Debug.Log("<color=magenta>[ActionTurnManager] ��� �׼� ť ó�� �Ϸ�.</color>");
    }

    /// <summary>
    /// ��� ť�� �׼��� �߰��մϴ�.
    /// </summary>
    public void AddActionToNormalQueue(QueuedAction action)
    {
        _normalQueue.Enqueue(action);
        Debug.Log($"[ActionTurnManager] ��� ť�� '{action.SourceCard.cardName}' �׼� �߰�. ���� ť ũ��: {_normalQueue.Count}");
    }

    /// <summary>
    /// ���ͷ�Ʈ ť�� �׼��� �߰��մϴ�.
    /// </summary>
    public void AddActionToInterruptQueue(QueuedAction action)
    {
        _interruptQueue.Enqueue(action);
        Debug.Log($"[ActionTurnManager] ���ͷ�Ʈ ť�� '{action.SourceCard.cardName}' �׼� �߰�. ���� ť ũ��: {_interruptQueue.Count}");
    }

    /// <summary>
    /// ��Ÿ�ӿ��� ������ ���� �����ϴ� ��ƿ��Ƽ �޼����
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
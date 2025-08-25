using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// ���� ���� ��ü���� ���� �帧(����, ������)�� �����ϴ� �Ѱ� �Ŵ����Դϴ�.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum BattlePhase
    {
        Setup,              // �¾� ������
        Draw,               // ��ο� ������
        CardSelection,      // ī�� ���� ������
        ActionExecution,    // �׼� ���� ������
        PostProcessing,     // ��ó�� ������
        RoundEnd            // ���� ����
    }
    public BattlePhase currentPhase { get; private set; }

    // Button ������ CardManager�� �����Ǿ����Ƿ� ���⼭ �����մϴ�.

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    // Start() �Լ��� ���� ����ֽ��ϴ�.
    private void Start()
    {
    }

    private void OnEnable()
    {
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.OnBattleStart += OnBattleStarted;
            // '�׼� ������ ����' �̺�Ʈ�� �����մϴ�.
            BattleEventManager.instance.OnActionPhaseStart += OnActionPhaseStarted;
        }
    }

    private void OnDisable()
    {
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.OnBattleStart -= OnBattleStarted;
            BattleEventManager.instance.OnActionPhaseStart -= OnActionPhaseStarted;
        }
    }

    private void OnBattleStarted()
    {
        StartCoroutine(RoundCoroutine());
    }

    private IEnumerator RoundCoroutine()
    {
        // --- 1. �¾� ������ ---
        currentPhase = BattlePhase.Setup;
        Debug.Log("Phase: Setup");
        yield return null;

        // --- 2. ��ο� ������ ---
        currentPhase = BattlePhase.Draw;
        Debug.Log("Phase: Draw");
        // CardManager�� OnBattleStart �Ǵ� OnNewRound �̺�Ʈ�� ��� ������ ī�带 �̽��ϴ�.
        yield return null;

        // --- 3. ī�� ���� ������ ---
        currentPhase = BattlePhase.CardSelection;
        Debug.Log("Phase: CardSelection");
        // �� ���¿����� CardManager�� '����' ��ư�� �����⸦ ��ٸ��ϴ�.
    }

    /// <summary>
    /// '�׼� ������ ����' ��ȣ�� �޾��� �� ȣ��˴ϴ�.
    /// </summary>
    private void OnActionPhaseStarted()
    {
        if (currentPhase == BattlePhase.CardSelection)
        {
            StartCoroutine(ActionPhaseCoroutine());
        }
    }

    private IEnumerator ActionPhaseCoroutine()
    {
        // --- 4. �׼� ���� ������ ---
        currentPhase = BattlePhase.ActionExecution;
        Debug.Log("Phase: ActionExecution");
        // TODO: TurnManager���� �׼� ť ó���� ����
        yield return null;

        // --- 5. ��ó�� ������ ---
        currentPhase = BattlePhase.PostProcessing;
        Debug.Log("Phase: PostProcessing");
        yield return null;

        // --- 6. ���� ���� ---
        currentPhase = BattlePhase.RoundEnd;
        Debug.Log("Phase: RoundEnd");

        StartCoroutine(RoundCoroutine()); // ���� ���带 �����մϴ�.
    }
}
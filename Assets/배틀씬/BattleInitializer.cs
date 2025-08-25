using UnityEngine;
using System.Collections.Generic;

public class BattleInitializer : MonoBehaviour
{
    public static BattleInitializer instance;

    [Header("�׽�Ʈ�� ���� ������")]
    public EncounterSO testEncounter;

    [HideInInspector] public EncounterSO currentEncounter;
    [HideInInspector] public List<CardDataSO> playerDeck;
    [HideInInspector] public int handSize;
    [HideInInspector] public int mulligansPerTurn;
    private int _maxActions; // ���� �ִ� �׼� �� ���� �߰� ����

    void Awake()
    {
        Debug.Log("<color=blue>[BattleInitializer] Awake ȣ���</color>");
        if (instance == null)
        {
            instance = this;
            Debug.Log("<color=blue>[BattleInitializer] �ν��Ͻ� ���� �Ϸ�</color>");
        }
        else
        {
            Destroy(gameObject);
        }

        FetchDataFromGlobalManager();
    }

    void Start()
    {
        Debug.Log("<color=blue>[BattleInitializer] Start ȣ���</color>");

        // �ڡڡ� �߿�: ���� CardManager�� �ʱ�ȭ�մϴ�! �ڡڡ�
        InitializeCardManager();

        // ������ �غ� ��������, �̺�Ʈ ü���� ù ��° ��ȣź�� �߻��մϴ�.
        if (BattleEventManager.instance != null)
        {
            Debug.Log("<color=blue>[BattleInitializer] �׸��� ���� �̺�Ʈ �߻�</color>");
            BattleEventManager.instance.RaiseSetupGrid();
        }
        else
        {
            Debug.LogError("<color=red>[BattleInitializer] BattleEventManager�� ã�� �� �����ϴ�!</color>");
        }
    }

    /// <summary>
    /// CardManager�� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void InitializeCardManager()
    {
        Debug.Log("<color=blue>[BattleInitializer] CardManager �ʱ�ȭ ����</color>");

        if (CardManager.instance == null)
        {
            Debug.LogError("<color=red>[BattleInitializer] CardManager.instance�� ã�� �� �����ϴ�!</color>");
            return;
        }

        if (playerDeck == null || playerDeck.Count == 0)
        {
            Debug.LogError("<color=red>[BattleInitializer] �÷��̾� ���� ����ֽ��ϴ�!</color>");
            return;
        }

        Debug.Log($"<color=blue>[BattleInitializer] CardManager.Initialize ȣ�� - ��: {playerDeck.Count}��, ����: {handSize}, �ָ���: {mulligansPerTurn}, �ִ�׼�: {_maxActions}</color>");
        // ���� ���⿡ _maxActions ������ �����մϴ� ����
        CardManager.instance.Initialize(playerDeck, handSize, mulligansPerTurn, _maxActions);
        Debug.Log("<color=green>[BattleInitializer] CardManager �ʱ�ȭ �Ϸ�!</color>");
    }

    private void FetchDataFromGlobalManager()
    {
        Debug.Log("<color=blue>[BattleInitializer] GlobalManager���� ������ �������� ����</color>");

        if (GlobalManager.instance != null)
        {
            currentEncounter = GlobalManager.instance.nextEncounter;
            playerDeck = GlobalManager.instance.playerBattleDeck;
            handSize = GlobalManager.instance.handSize;
            mulligansPerTurn = GlobalManager.instance.mulligansPerTurn;
            _maxActions = GlobalManager.instance.playerMaxActionsPerTurn; // ���� �� ���� �߰��մϴ� ����

            Debug.Log($"<color=blue>[BattleInitializer] GlobalManager ������ �ε� �Ϸ� - ��: {playerDeck?.Count ?? 0}��</color>");
        }
        else
        {
            Debug.LogWarning("<color=orange>[BattleInitializer] GlobalManager�� ã�� �� ���� �׽�Ʈ�� �����͸� ����մϴ�.</color>");
            currentEncounter = testEncounter;

            // �׽�Ʈ�� �⺻�� ����
            if (playerDeck == null || playerDeck.Count == 0)
            {
                Debug.LogWarning("<color=orange>[BattleInitializer] �׽�Ʈ�� �� ���� ����մϴ�. ���� ī�� �����͸� �������ּ���!</color>");
                playerDeck = new List<CardDataSO>();
            }

            if (handSize == 0) handSize = 5;
            if (mulligansPerTurn == 0) mulligansPerTurn = 1;
            if (_maxActions == 0) _maxActions = 3; // ���� �� ���� �߰��մϴ� ����
        }

        Debug.Log($"<color=green>[BattleInitializer] ���� ������ - ��: {playerDeck?.Count ?? 0}��, ����: {handSize}, �ָ���: {mulligansPerTurn}, �ִ�׼�: {_maxActions}</color>");
    }

    /// <summary>
    /// ����׿�: ���� BattleInitializer ���¸� ����մϴ�.
    /// </summary>
    [ContextMenu("Debug BattleInitializer State")]
    public void DebugInitializerState()
    {
        Debug.Log($"<color=white>=== BattleInitializer ���� ===\n" +
                  $"Current Encounter: {currentEncounter?.name ?? "NULL"}\n" +
                  $"Player Deck: {playerDeck?.Count ?? 0}��\n" +
                  $"Hand Size: {handSize}\n" +
                  $"Mulligans Per Turn: {mulligansPerTurn}\n" +
                  $"Max Actions: {_maxActions}\n" + // ����� �α׿��� �߰�
                  $"CardManager Instance: {CardManager.instance != null}\n" +
                  $"BattleEventManager Instance: {BattleEventManager.instance != null}</color>");
    }
}
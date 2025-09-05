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
    public int playerMaxActionsPerTurn;

    // [�߰�] �÷��̾� ü�� ������ ������ ����
    [HideInInspector] public int playerMaxHealth;
    [HideInInspector] public int playerCurrentHealth;


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

    private void FetchDataFromGlobalManager()
    {
        Debug.Log("<color=blue>[BattleInitializer] GlobalManager���� ������ �������� ����</color>");

        if (GlobalManager.instance != null)
        {
            currentEncounter = GlobalManager.instance.nextEncounter;
            playerDeck = GlobalManager.instance.playerBattleDeck;
            handSize = GlobalManager.instance.handSize;
            mulligansPerTurn = GlobalManager.instance.mulligansPerTurn;
            playerMaxActionsPerTurn = GlobalManager.instance.playerMaxActionsPerTurn;

            // [�߰�] GlobalManager�κ��� �÷��̾� ü�� ������ �����ɴϴ�.
            playerMaxHealth = GlobalManager.instance.playerMaxHealth;
            playerCurrentHealth = GlobalManager.instance.playerCurrentHealth;

            Debug.Log($"<color=blue>[BattleInitializer] GlobalManager ������ �ε� �Ϸ� - ��: {playerDeck?.Count ?? 0}��, ü��: {playerCurrentHealth}/{playerMaxHealth}</color>");
        }
        else
        {
            Debug.LogWarning("<color=orange>[BattleInitializer] GlobalManager�� ã�� �� ���� �׽�Ʈ�� �����͸� ����մϴ�.</color>");
            currentEncounter = testEncounter;
            if (playerDeck == null || playerDeck.Count == 0)
            {
                playerDeck = new List<CardDataSO>();
            }
            if (handSize == 0) handSize = 5;
            if (mulligansPerTurn == 0) mulligansPerTurn = 1;
            if (playerMaxActionsPerTurn == 0) playerMaxActionsPerTurn = 3;

            // [�߰�] �׽�Ʈ�� ü�� ������ ����
            if (playerMaxHealth == 0) playerMaxHealth = 100;
            if (playerCurrentHealth == 0) playerCurrentHealth = playerMaxHealth;
        }
        Debug.Log($"<color=green>[BattleInitializer] ���� ������ - ��: {playerDeck?.Count ?? 0}��, ����: {handSize}, �ָ���: {mulligansPerTurn}, �ִ�׼�: {playerMaxActionsPerTurn}</color>");
    }

    [ContextMenu("Debug BattleInitializer State")]
    public void DebugInitializerState()
    {
        Debug.Log($"<color=white>=== BattleInitializer ���� ===\n" +
                  $"Current Encounter: {currentEncounter?.name ?? "NULL"}\n" +
                  $"Player Deck: {playerDeck?.Count ?? 0}��\n" +
                  $"Hand Size: {handSize}\n" +
                  $"Mulligans Per Turn: {mulligansPerTurn}\n" +
                  $"Max Actions: {playerMaxActionsPerTurn}\n" +
                  $"Player Health: {playerCurrentHealth}/{playerMaxHealth}\n" +
                  $"CardManager Instance: {CardManager.instance != null}\n" +
                  $"BattleEventManager Instance: {BattleEventManager.instance != null}</color>");
    }
}

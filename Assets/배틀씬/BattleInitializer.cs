using UnityEngine;
using System.Collections.Generic;

public class BattleInitializer : MonoBehaviour
{
    public static BattleInitializer instance;

    [Header("테스트용 조우 데이터")]
    public EncounterSO testEncounter;

    [HideInInspector] public EncounterSO currentEncounter;
    [HideInInspector] public List<CardDataSO> playerDeck;
    [HideInInspector] public int handSize;
    [HideInInspector] public int mulligansPerTurn;
    public int playerMaxActionsPerTurn;

    // [추가] 플레이어 체력 정보를 저장할 변수
    [HideInInspector] public int playerMaxHealth;
    [HideInInspector] public int playerCurrentHealth;


    void Awake()
    {
        Debug.Log("<color=blue>[BattleInitializer] Awake 호출됨</color>");
        if (instance == null)
        {
            instance = this;
            Debug.Log("<color=blue>[BattleInitializer] 인스턴스 설정 완료</color>");
        }
        else
        {
            Destroy(gameObject);
        }

        FetchDataFromGlobalManager();
    }

    void Start()
    {
        Debug.Log("<color=blue>[BattleInitializer] Start 호출됨</color>");

        if (BattleEventManager.instance != null)
        {
            Debug.Log("<color=blue>[BattleInitializer] 그리드 설정 이벤트 발생</color>");
            BattleEventManager.instance.RaiseSetupGrid();
        }
        else
        {
            Debug.LogError("<color=red>[BattleInitializer] BattleEventManager를 찾을 수 없습니다!</color>");
        }
    }

    private void FetchDataFromGlobalManager()
    {
        Debug.Log("<color=blue>[BattleInitializer] GlobalManager에서 데이터 가져오기 시작</color>");

        if (GlobalManager.instance != null)
        {
            currentEncounter = GlobalManager.instance.nextEncounter;
            playerDeck = GlobalManager.instance.playerBattleDeck;
            handSize = GlobalManager.instance.handSize;
            mulligansPerTurn = GlobalManager.instance.mulligansPerTurn;
            playerMaxActionsPerTurn = GlobalManager.instance.playerMaxActionsPerTurn;

            // [추가] GlobalManager로부터 플레이어 체력 정보를 가져옵니다.
            playerMaxHealth = GlobalManager.instance.playerMaxHealth;
            playerCurrentHealth = GlobalManager.instance.playerCurrentHealth;

            Debug.Log($"<color=blue>[BattleInitializer] GlobalManager 데이터 로드 완료 - 덱: {playerDeck?.Count ?? 0}장, 체력: {playerCurrentHealth}/{playerMaxHealth}</color>");
        }
        else
        {
            Debug.LogWarning("<color=orange>[BattleInitializer] GlobalManager를 찾을 수 없어 테스트용 데이터를 사용합니다.</color>");
            currentEncounter = testEncounter;
            if (playerDeck == null || playerDeck.Count == 0)
            {
                playerDeck = new List<CardDataSO>();
            }
            if (handSize == 0) handSize = 5;
            if (mulligansPerTurn == 0) mulligansPerTurn = 1;
            if (playerMaxActionsPerTurn == 0) playerMaxActionsPerTurn = 3;

            // [추가] 테스트용 체력 데이터 설정
            if (playerMaxHealth == 0) playerMaxHealth = 100;
            if (playerCurrentHealth == 0) playerCurrentHealth = playerMaxHealth;
        }
        Debug.Log($"<color=green>[BattleInitializer] 최종 데이터 - 덱: {playerDeck?.Count ?? 0}장, 손패: {handSize}, 멀리건: {mulligansPerTurn}, 최대액션: {playerMaxActionsPerTurn}</color>");
    }

    [ContextMenu("Debug BattleInitializer State")]
    public void DebugInitializerState()
    {
        Debug.Log($"<color=white>=== BattleInitializer 상태 ===\n" +
                  $"Current Encounter: {currentEncounter?.name ?? "NULL"}\n" +
                  $"Player Deck: {playerDeck?.Count ?? 0}장\n" +
                  $"Hand Size: {handSize}\n" +
                  $"Mulligans Per Turn: {mulligansPerTurn}\n" +
                  $"Max Actions: {playerMaxActionsPerTurn}\n" +
                  $"Player Health: {playerCurrentHealth}/{playerMaxHealth}\n" +
                  $"CardManager Instance: {CardManager.instance != null}\n" +
                  $"BattleEventManager Instance: {BattleEventManager.instance != null}</color>");
    }
}

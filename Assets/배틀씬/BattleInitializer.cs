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
    private int _maxActions; // ▼▼▼ 최대 액션 수 변수 추가 ▼▼▼

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

        // ★★★ 중요: 먼저 CardManager를 초기화합니다! ★★★
        InitializeCardManager();

        // 데이터 준비가 끝났으니, 이벤트 체인의 첫 번째 신호탄을 발사합니다.
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

    /// <summary>
    /// CardManager를 초기화합니다.
    /// </summary>
    private void InitializeCardManager()
    {
        Debug.Log("<color=blue>[BattleInitializer] CardManager 초기화 시작</color>");

        if (CardManager.instance == null)
        {
            Debug.LogError("<color=red>[BattleInitializer] CardManager.instance를 찾을 수 없습니다!</color>");
            return;
        }

        if (playerDeck == null || playerDeck.Count == 0)
        {
            Debug.LogError("<color=red>[BattleInitializer] 플레이어 덱이 비어있습니다!</color>");
            return;
        }

        Debug.Log($"<color=blue>[BattleInitializer] CardManager.Initialize 호출 - 덱: {playerDeck.Count}장, 손패: {handSize}, 멀리건: {mulligansPerTurn}, 최대액션: {_maxActions}</color>");
        // ▼▼▼ 여기에 _maxActions 변수를 전달합니다 ▼▼▼
        CardManager.instance.Initialize(playerDeck, handSize, mulligansPerTurn, _maxActions);
        Debug.Log("<color=green>[BattleInitializer] CardManager 초기화 완료!</color>");
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
            _maxActions = GlobalManager.instance.playerMaxActionsPerTurn; // ▼▼▼ 이 줄을 추가합니다 ▼▼▼

            Debug.Log($"<color=blue>[BattleInitializer] GlobalManager 데이터 로드 완료 - 덱: {playerDeck?.Count ?? 0}장</color>");
        }
        else
        {
            Debug.LogWarning("<color=orange>[BattleInitializer] GlobalManager를 찾을 수 없어 테스트용 데이터를 사용합니다.</color>");
            currentEncounter = testEncounter;

            // 테스트용 기본값 설정
            if (playerDeck == null || playerDeck.Count == 0)
            {
                Debug.LogWarning("<color=orange>[BattleInitializer] 테스트용 빈 덱을 사용합니다. 실제 카드 데이터를 설정해주세요!</color>");
                playerDeck = new List<CardDataSO>();
            }

            if (handSize == 0) handSize = 5;
            if (mulligansPerTurn == 0) mulligansPerTurn = 1;
            if (_maxActions == 0) _maxActions = 3; // ▼▼▼ 이 줄을 추가합니다 ▼▼▼
        }

        Debug.Log($"<color=green>[BattleInitializer] 최종 데이터 - 덱: {playerDeck?.Count ?? 0}장, 손패: {handSize}, 멀리건: {mulligansPerTurn}, 최대액션: {_maxActions}</color>");
    }

    /// <summary>
    /// 디버그용: 현재 BattleInitializer 상태를 출력합니다.
    /// </summary>
    [ContextMenu("Debug BattleInitializer State")]
    public void DebugInitializerState()
    {
        Debug.Log($"<color=white>=== BattleInitializer 상태 ===\n" +
                  $"Current Encounter: {currentEncounter?.name ?? "NULL"}\n" +
                  $"Player Deck: {playerDeck?.Count ?? 0}장\n" +
                  $"Hand Size: {handSize}\n" +
                  $"Mulligans Per Turn: {mulligansPerTurn}\n" +
                  $"Max Actions: {_maxActions}\n" + // 디버그 로그에도 추가
                  $"CardManager Instance: {CardManager.instance != null}\n" +
                  $"BattleEventManager Instance: {BattleEventManager.instance != null}</color>");
    }
}
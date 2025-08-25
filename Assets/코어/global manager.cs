using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 씬이 변경되어도 파괴되지 않고 게임 전체의 데이터를 관리하는 중앙 관리자입니다.
/// 이 스크립트는 순수한 데이터 '저장소'의 역할만 담당합니다.
/// </summary>
public class GlobalManager : MonoBehaviour
{
    public static GlobalManager instance;

    [Header("플레이어 기본 정보")]
    [Tooltip("플레이어의 최대 체력입니다.")]
    public int playerMaxHealth = 100;
    [Tooltip("플레이어의 현재 체력입니다. 전투가 끝나도 유지됩니다.")]
    public int playerCurrentHealth;

    [Header("플레이어 전투 규칙")]
    [Tooltip("매 턴 시작 시 손에 채워질 카드의 수입니다.")]
    public int handSize = 5;
    [Tooltip("매 턴 주어지는 멀리건(패 다시 뽑기) 횟수입니다.")]
    public int mulligansPerTurn = 1;

    // ▼▼▼ 여기에 최대 액션 수 변수를 추가했습니다 ▼▼▼
    [Tooltip("플레이어가 한 턴에 선택할 수 있는 최대 카드(액션) 수입니다.")]
    public int playerMaxActionsPerTurn = 3;
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    [Header("플레이어 덱 정보")]
    [Tooltip("플레이어가 소유하고 있는 모든 카드 목록입니다.")]
    public List<CardDataSO> playerCardCollection = new List<CardDataSO>();
    [Tooltip("실제 전투에서 사용할 최종 덱입니다.")]
    public List<CardDataSO> playerBattleDeck = new List<CardDataSO>();

    [Header("다음 전투 정보")]
    [Tooltip("다음 전투 씬에서 사용할 Encounter 데이터입니다.")]
    public EncounterSO nextEncounter;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            playerCurrentHealth = playerMaxHealth;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

// 적의 AI 타겟팅 방식을 정의합니다.
public enum E_MoveType
{
    Fixed,       // 고정된 범위 내에서 무작위 타겟
    ChasePlayer  // 플레이어에게 가장 가까운 타겟
}

// [System.Serializable]을 통해 인스펙터 창에서 편집할 수 있습니다.
[System.Serializable]
public class EnemyAction
{
    [Tooltip("인스펙터에서 알아보기 쉽도록 행동의 이름을 적어줍니다.")]
    public string actionName;

    [Tooltip("이 행동이 어떤 방식으로 타겟을 정할지 선택합니다.")]
    public E_MoveType movementType;

    [Tooltip("이 행동이 실제로 사용할 카드(GameAction)입니다.")]
    public CardDataSO referenceCard;
}

// ScriptableObject를 생성하기 위한 메뉴 경로를 설정합니다.
[CreateAssetMenu(fileName = "New Enemy Data", menuName = "3DProject/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("적의 이름입니다.")]
    public string enemyName;
    [Tooltip("전투에 등장할 적의 프리팹입니다.")]
    public GameObject enemyPrefab;
    [Tooltip("적의 최대 체력입니다.")]
    public int maxHealth;

    // ▼▼▼ 여기에 actionsPerTurn 변수를 추가했습니다 ▼▼▼
    [Header("전투 규칙")]
    [Tooltip("이 적이 한 턴에 행동할 수 있는 횟수입니다.")]
    public int actionsPerTurn = 1;
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    [Header("행동 패턴")]
    [Tooltip("이 적이 순서대로 사용할 행동 목록입니다.")]
    public List<EnemyAction> actionPattern;
}
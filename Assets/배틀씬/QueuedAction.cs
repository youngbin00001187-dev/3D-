using UnityEngine;

public struct QueuedAction
{
    public UnitController User;
    public CardDataSO SourceCard;
    public GameObject TargetTile; // 플레이어의 절대 목표 지정에 계속 사용
    public Vector3Int? RelativeVector; // [신규] 적의 상대 목표 지정을 위해 추가
}
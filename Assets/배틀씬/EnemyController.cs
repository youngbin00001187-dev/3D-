using UnityEngine;

/// <summary>
/// UnitController를 상속받는 적 전용 클래스입니다.
/// </summary>
public class EnemyController : UnitController
{
    [Header("적 전용 데이터")]
    [Tooltip("이 적의 모든 데이터를 담고 있는 SO 에셋입니다.")]
    public EnemyDataSO enemyData;

    // 앞으로 적의 AI, 행동 패턴 선택 등의 로직이 이 스크립트에 추가될 것입니다.

    // 현재는 UnitController의 모든 기능을 그대로 물려받습니다.
}
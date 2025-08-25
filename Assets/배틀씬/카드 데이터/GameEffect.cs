using System.Collections;
using UnityEngine;
using System; // 'Action' (콜백)을 사용하기 위해 필요합니다.

/// <summary>
/// GameEffect가 액션의 어느 시점에 발동될지를 정의합니다.
/// </summary>
public enum EffectTiming
{
    OnActionStart,
    OnTargetImpact,
    AfterMove,        // [신규] 이동 애니메이션 완료 후
    OnArrival,
    OnActionEnd
}

/// <summary>
/// 게임에 존재하는 모든 GameEffect 클래스의 종류를 정의합니다.
/// </summary>
public enum EffectType
{
    None,
    Knockback,
    Stun,// ▼▼▼ 여기에 새로운 이펙트 타입 추가 ▼▼▼
    // --- 향후 추가될 효과 예시 ---
    // LeavePoisonMist, 
    // GainArmorBuff,
}

/// <summary>
/// GameAction에 부착될 효과의 모든 데이터를 정의합니다.
/// </summary>
[System.Serializable]
public struct ActionEffect
{
    public EffectType effectType;
    public EffectTiming timing;
    [Tooltip("체크: 이 효과의 애니메이션이 끝날 때까지 액션이 기다립니다. (예: 넉백)\n해제: 효과를 발동만 시키고 즉시 다음 행동으로 넘어갑니다. (예: 독안개)")]
    public bool waitForCompletion;

    public ActionEffect(bool wait = true)
    {
        this.effectType = EffectType.None;
        this.timing = EffectTiming.OnTargetImpact;
        this.waitForCompletion = wait;
    }
}

/// <summary>
/// 모든 게임 내 부가 효과가 상속받는 추상 클래스입니다.
/// </summary>
[System.Serializable]
public abstract class GameEffect
{
    public abstract IEnumerator Apply(UnitController user, GameObject targetTile, Action onComplete);
}
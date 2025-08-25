using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VFX 생성 방식
/// </summary>
public enum E_VfxSpawnType
{
    PerTarget,  // 적중한 대상마다 생성
    AtCaster    // 사용자 위치에서 1회 생성
}

/// <summary>
/// VFX 발동 정보
/// </summary>
[Serializable]
public struct VFXEffect
{
    [Tooltip("VFXManager에 등록된 이펙트 ID")]
    public int vfxId;
    [Tooltip("이 VFX가 발동될 타이밍")]
    public EffectTiming timing;
    [Tooltip("VFX 생성 방식")]
    public E_VfxSpawnType spawnType;
}

/// <summary>
/// 게임 액션 추상 클래스
/// </summary>
[Serializable]
public abstract class GameAction
{
    [Header("액션 부가 효과 (게임 로직)")]
    [Tooltip("액션에 부착될 게임 효과 목록")]
    public List<ActionEffect> attachedEffects = new List<ActionEffect>();

    [Header("액션 시각 효과 (VFX)")]
    [Tooltip("액션 실행 시 발동될 VFX 목록")]
    public List<VFXEffect> vfxEffects = new List<VFXEffect>();

    [NonSerialized] protected UnitController actionUser;
    [NonSerialized] protected GameObject actionTargetTile;

    public virtual void Prepare(UnitController user, GameObject target)
    {
        this.actionUser = user;
        this.actionTargetTile = target;
    }

    public IEnumerator Execute()
    {
        // 스턴 체크 등 필요 시 추가
        yield return InternalExecute();
    }

    protected abstract IEnumerator InternalExecute();
    public abstract List<GameObject> GetTargetableTiles(UnitController user);
    public abstract List<GameObject> GetActionImpactTiles(UnitController user, GameObject targetTile);

    /// <summary>
    /// 특정 타이밍의 게임 효과 실행
    /// </summary>
    protected IEnumerator ExecuteEffectsByTiming(EffectTiming timing)
    {
        if (attachedEffects == null) yield break;
        foreach (var effect in attachedEffects)
        {
            if (effect.timing == timing)
            {
                yield return ExecuteEffectAndWait(effect);
            }
        }
    }

    protected IEnumerator ExecuteEffectAndWait(ActionEffect effect)
    {
        if (effect.effectType == EffectType.None) yield break;

        if (!effect.waitForCompletion)
        {
            actionUser.StartCoroutine(
                EffectManager.Instance.ExecuteEffect(effect.effectType, actionUser, actionTargetTile, null)
            );
            yield break;
        }

        bool isEffectFinished = false;
        Action callback = () => { isEffectFinished = true; };
        actionUser.StartCoroutine(
            EffectManager.Instance.ExecuteEffect(effect.effectType, actionUser, actionTargetTile, callback)
        );
        yield return new WaitUntil(() => isEffectFinished);
    }

    /// <summary>
    /// 특정 타이밍의 VFX 실행
    /// </summary>
    protected void ExecuteVFXByTiming(EffectTiming timing, UnitController primaryTarget = null)
    {
        if (vfxEffects == null || vfxEffects.Count == 0 || VFXManager.Instance == null) return;

        foreach (var vfx in vfxEffects)
        {
            if (vfx.timing != timing) continue;

            Vector3 spawnPos = Vector3.zero;
            if (vfx.spawnType == E_VfxSpawnType.AtCaster)
                spawnPos = actionUser.transform.position;
            else if (vfx.spawnType == E_VfxSpawnType.PerTarget)
                spawnPos = primaryTarget != null ? primaryTarget.transform.position :
                           actionTargetTile != null ? actionTargetTile.transform.position : Vector3.zero;

            VFXManager.Instance.PlayHitEffect(spawnPos, vfx.vfxId);
        }
    }
}

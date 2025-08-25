using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VFX ���� ���
/// </summary>
public enum E_VfxSpawnType
{
    PerTarget,  // ������ ��󸶴� ����
    AtCaster    // ����� ��ġ���� 1ȸ ����
}

/// <summary>
/// VFX �ߵ� ����
/// </summary>
[Serializable]
public struct VFXEffect
{
    [Tooltip("VFXManager�� ��ϵ� ����Ʈ ID")]
    public int vfxId;
    [Tooltip("�� VFX�� �ߵ��� Ÿ�̹�")]
    public EffectTiming timing;
    [Tooltip("VFX ���� ���")]
    public E_VfxSpawnType spawnType;
}

/// <summary>
/// ���� �׼� �߻� Ŭ����
/// </summary>
[Serializable]
public abstract class GameAction
{
    [Header("�׼� �ΰ� ȿ�� (���� ����)")]
    [Tooltip("�׼ǿ� ������ ���� ȿ�� ���")]
    public List<ActionEffect> attachedEffects = new List<ActionEffect>();

    [Header("�׼� �ð� ȿ�� (VFX)")]
    [Tooltip("�׼� ���� �� �ߵ��� VFX ���")]
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
        // ���� üũ �� �ʿ� �� �߰�
        yield return InternalExecute();
    }

    protected abstract IEnumerator InternalExecute();
    public abstract List<GameObject> GetTargetableTiles(UnitController user);
    public abstract List<GameObject> GetActionImpactTiles(UnitController user, GameObject targetTile);

    /// <summary>
    /// Ư�� Ÿ�̹��� ���� ȿ�� ����
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
    /// Ư�� Ÿ�̹��� VFX ����
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

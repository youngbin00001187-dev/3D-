using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// 모든 GameEffect 및 유닛 간 상호작용을 관리하는 중앙 관리자(싱글턴)입니다. (3D 버전)
/// </summary>
public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    private Dictionary<EffectType, GameEffect> _effectPrototypes;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeEffects();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeEffects()
    {
        _effectPrototypes = new Dictionary<EffectType, GameEffect>();
        // KnockbackEffect, StunEffect 등 GameEffect를 상속받는 클래스가 프로젝트에 추가되면
        // 이 부분의 주석을 해제하여 등록해야 합니다.
        // _effectPrototypes.Add(EffectType.Knockback, new KnockbackEffect());
        // _effectPrototypes.Add(EffectType.Stun, new StunEffect());
    }

    /// <summary>
    /// EffectType ID를 받아 해당하는 효과를 실행하고, 그 효과가 끝날 때까지 기다립니다.
    /// </summary>
    public IEnumerator ExecuteEffect(EffectType effectType, UnitController user, GameObject targetTile, Action onComplete)
    {
        if (_effectPrototypes.TryGetValue(effectType, out GameEffect effectToApply))
        {
            yield return effectToApply.Apply(user, targetTile, onComplete);
        }
        else
        {
            Debug.LogWarning($"[EffectManager] '{effectType}'에 해당하는 효과가 등록되어 있지 않습니다!");
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// 두 유닛이 동시에 움직이는 범용 연출 툴입니다. (3D 환경에 맞게 수정됨)
    /// </summary>
    public IEnumerator ExecuteSimultaneousMoveCoroutine(UnitController unitA, UnitController unitB, GameObject destTileA, GameObject destTileB, float moveSpeed)
    {
        // ▼▼▼ GridManager3D를 참조하도록 수정합니다 ▼▼▼
        GridManager3D.instance.UnregisterUnitPosition(unitA, unitA.GetGridPosition());
        GridManager3D.instance.UnregisterUnitPosition(unitB, unitB.GetGridPosition());

        Vector3 destPosA = destTileA.transform.position;
        Vector3 destPosB = destTileB.transform.position;

        Sequence sequence = DOTween.Sequence();
        float duration = Vector3.Distance(unitA.transform.position, destPosA) / moveSpeed;
        sequence.Append(unitA.transform.DOMove(destPosA, duration).SetEase(Ease.OutQuad));
        sequence.Join(unitB.transform.DOMove(destPosB, duration).SetEase(Ease.OutQuad));

        yield return sequence.WaitForCompletion();

        // ▼▼▼ 이동이 끝난 후, UnitController의 MoveToTile 함수를 호출하도록 단순화합니다 ▼▼▼
        unitA.MoveToTile(destTileA);
        unitB.MoveToTile(destTileB);
    }
}
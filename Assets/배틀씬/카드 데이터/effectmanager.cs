using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// ��� GameEffect �� ���� �� ��ȣ�ۿ��� �����ϴ� �߾� ������(�̱���)�Դϴ�. (3D ����)
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
        // KnockbackEffect, StunEffect �� GameEffect�� ��ӹ޴� Ŭ������ ������Ʈ�� �߰��Ǹ�
        // �� �κ��� �ּ��� �����Ͽ� ����ؾ� �մϴ�.
        // _effectPrototypes.Add(EffectType.Knockback, new KnockbackEffect());
        // _effectPrototypes.Add(EffectType.Stun, new StunEffect());
    }

    /// <summary>
    /// EffectType ID�� �޾� �ش��ϴ� ȿ���� �����ϰ�, �� ȿ���� ���� ������ ��ٸ��ϴ�.
    /// </summary>
    public IEnumerator ExecuteEffect(EffectType effectType, UnitController user, GameObject targetTile, Action onComplete)
    {
        if (_effectPrototypes.TryGetValue(effectType, out GameEffect effectToApply))
        {
            yield return effectToApply.Apply(user, targetTile, onComplete);
        }
        else
        {
            Debug.LogWarning($"[EffectManager] '{effectType}'�� �ش��ϴ� ȿ���� ��ϵǾ� ���� �ʽ��ϴ�!");
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// �� ������ ���ÿ� �����̴� ���� ���� ���Դϴ�. (3D ȯ�濡 �°� ������)
    /// </summary>
    public IEnumerator ExecuteSimultaneousMoveCoroutine(UnitController unitA, UnitController unitB, GameObject destTileA, GameObject destTileB, float moveSpeed)
    {
        // ���� GridManager3D�� �����ϵ��� �����մϴ� ����
        GridManager3D.instance.UnregisterUnitPosition(unitA, unitA.GetGridPosition());
        GridManager3D.instance.UnregisterUnitPosition(unitB, unitB.GetGridPosition());

        Vector3 destPosA = destTileA.transform.position;
        Vector3 destPosB = destTileB.transform.position;

        Sequence sequence = DOTween.Sequence();
        float duration = Vector3.Distance(unitA.transform.position, destPosA) / moveSpeed;
        sequence.Append(unitA.transform.DOMove(destPosA, duration).SetEase(Ease.OutQuad));
        sequence.Join(unitB.transform.DOMove(destPosB, duration).SetEase(Ease.OutQuad));

        yield return sequence.WaitForCompletion();

        // ���� �̵��� ���� ��, UnitController�� MoveToTile �Լ��� ȣ���ϵ��� �ܼ�ȭ�մϴ� ����
        unitA.MoveToTile(destTileA);
        unitB.MoveToTile(destTileB);
    }
}
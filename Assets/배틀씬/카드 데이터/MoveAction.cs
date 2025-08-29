using System.Collections;
using System.Collections.Generic;
using System.Linq; // .Any()�� ����ϱ� ���� �߰�
using UnityEngine;

[System.Serializable]
public class MoveAction : GameAction
{
    [Header("Targeting Settings")]
    [Tooltip("�̵� ������ Ÿ�� �����Դϴ�. (Vector3Int ���)")]
    public List<Vector3Int> movementRange = new List<Vector3Int>();

    [Header("Settings")]
    public float moveSpeed = 8f;
    public int motionID = 1;

    public override List<GameObject> GetTargetableTiles(UnitController user)
    {
        List<GameObject> tiles = new List<GameObject>();
        Vector3Int userPos = user.GetGridPosition();
        foreach (var vector in movementRange)
        {
            GameObject tile = GridManager3D.instance.GetTileAtPosition(userPos + vector);
            if (tile != null)
            {
                tiles.Add(tile);
            }
        }
        return tiles;
    }

    public override List<GameObject> GetActionImpactTiles(UnitController user, GameObject targetTile)
    {
        return new List<GameObject> { targetTile };
    }

    // MoveAction.cs

    // MoveAction.cs

    protected override IEnumerator InternalExecute()
    {
        Debug.Log($"<color=green>ACTION: {actionUser.name}��(��) {actionTargetTile.name}(��)�� �̵��� �õ��մϴ�.</color>");

        // 1. ���� Ÿ�� ���, ���� ȿ�� ����
        GameObject startingTile = actionUser.currentTile;
        ExecuteVFXByTiming(EffectTiming.OnActionStart);
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionStart); // �μ� 1�� ���� ȣ��

        // 2. �ִϸ��̼� ���� �� �̵�
        Animator animator = actionUser.GetComponent<Animator>();
        if (animator != null) animator.SetInteger("motionID", this.motionID);

        yield return actionUser.MoveCoroutine(actionTargetTile, moveSpeed);

        // 3. �˹� ȿ������ ���� ��ġ ���� ����
        foreach (var effect in attachedEffects)
        {
            if (EffectManager.Instance.GetEffect(effect.effectType) is KnockbackEffect knockbackEffect)
            {
                knockbackEffect.sourceTileOverride = startingTile;
            }
        }
        // �˹� ȿ�� ���� (�μ� 1�� ���� ȣ��)
        yield return ExecuteEffectsByTiming(EffectTiming.OnTargetImpact);

        // 4. ���� ���� Ȯ��
        Vector3Int targetPos = actionTargetTile.GetComponent<Tile3D>().gridPosition;
        var unitsOnTarget = GridManager3D.instance.GetUnitsAtPosition(targetPos);
        bool isStillOverlapping = unitsOnTarget.Any(u => u != actionUser);

        if (isStillOverlapping)
        {
            // 5-A. �̵� ���� ��: ������ �� ���� ��ġ ����
            Debug.Log("[MoveAction] ���� Ȯ�� ��� ��ΰ� ����, �������մϴ�.");
            UnitController otherUnit = unitsOnTarget.First(u => u != actionUser);
            ExecuteVFXByTiming(EffectTiming.OnTargetImpact, otherUnit);

            yield return actionUser.RecoilCoroutine(actionUser.transform.position);
            actionUser.MoveToTile(startingTile);
        }
        else
        {
            // 5-B. �̵� ���� ��: �̵� �Ϸ� ȿ�� ����
            Debug.Log("[MoveAction] ���� Ȯ�� ��� ��ΰ� ��� �̵��� Ȯ���մϴ�.");
            ExecuteVFXByTiming(EffectTiming.AfterMove);
            yield return ExecuteEffectsByTiming(EffectTiming.AfterMove); // �μ� 1�� ���� ȣ��
        }

        // 6. ���� ���� ȿ�� (�μ� 1�� ���� ȣ��)
        ExecuteVFXByTiming(EffectTiming.OnActionEnd);
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionEnd);

        // motionID ������ ActionTurnManager�� å���̹Ƿ� ���⿡�� �ڵ尡 �����ϴ�.
        Debug.Log($"<color=green>ACTION: {actionUser.name}�� �̵� �׼��� �Ϸ�Ǿ����ϴ�.</color>");
    }
}
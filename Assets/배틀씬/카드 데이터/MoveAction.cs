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

    protected override IEnumerator InternalExecute()
    {
        Debug.Log($"<color=green>ACTION: {actionUser.name}��(��) {actionTargetTile.name}(��)�� �̵��� �õ��մϴ�.</color>");

        // --- 1. �׼� ���� ���� ---
        ExecuteVFXByTiming(EffectTiming.OnActionStart);
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionStart);

        Vector3Int targetPos = actionTargetTile.GetComponent<Tile3D>().gridPosition;
        UnitController unitOnTarget = GridManager3D.instance.GetUnitAtPosition(targetPos);
        bool collision = (unitOnTarget != null && unitOnTarget != actionUser);

        if (collision)
        {
            // 1. �浹 ������ �ߵ��� �ΰ� ȿ��(�˹� ��)�� �ִ��� ���� Ȯ���մϴ�.
            bool hasImpactEffect = attachedEffects.Any(effect => effect.timing == EffectTiming.OnTargetImpact);

            // 2. VFX�� �浹 �� �׻� �߻���ŵ�ϴ�.
            ExecuteVFXByTiming(EffectTiming.OnTargetImpact, unitOnTarget);

            if (hasImpactEffect)
            {
                // 3-A. �ΰ� ȿ���� �ִٸ�: ȿ���� ���� ������ ��ٸ��ϴ�. (�����ڴ� ���������� ����)
                Debug.Log("[MoveAction] �浹: �ΰ� ȿ��(�˹� ��)�� �ߵ��մϴ�.");
                yield return ExecuteEffectsByTiming(EffectTiming.OnTargetImpact);
            }
            else
            {
                // 3-B. �ΰ� ȿ���� ���ٸ�: �����ڰ� �ڷ� ƨ�ܳ��� �������� �����մϴ�.
                Debug.Log("[MoveAction] �浹: �ΰ� ȿ���� �����Ƿ� �������մϴ�.");
                yield return actionUser.StartCoroutine(actionUser.RecoilCoroutine(actionTargetTile.transform.position));
            }
        }
        else
        {
            // --- �ٽ� �̵� ���� (�浹���� �ʾ��� ���) ---
            Animator animator = actionUser.GetComponent<Animator>();
            if (animator != null) animator.SetInteger("motionID", this.motionID);

            yield return actionUser.StartCoroutine(actionUser.MoveCoroutine(actionTargetTile, moveSpeed));

            if (animator != null) animator.SetInteger("motionID", 0);

            // --- �̵� �Ϸ� ���� ---
            ExecuteVFXByTiming(EffectTiming.AfterMove);
            yield return ExecuteEffectsByTiming(EffectTiming.AfterMove);
        }

        // --- �׼� ���� ���� ---
        ExecuteVFXByTiming(EffectTiming.OnActionEnd);
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionEnd);

        Debug.Log($"<color=green>ACTION: {actionUser.name}�� �̵� �׼��� �Ϸ�Ǿ����ϴ�.</color>");
    }
}
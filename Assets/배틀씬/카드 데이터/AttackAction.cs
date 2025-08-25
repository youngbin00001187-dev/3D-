using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class AttackAction : GameAction
{
    [Header("Ÿ���� ����")]
    [Tooltip("���� ������ Ÿ�� �����Դϴ�. (Vector3Int ���)")]
    public List<Vector3Int> attackRange = new List<Vector3Int>();

    [Header("������")]
    public int damage;
    [Tooltip("�ǰ� �����Դϴ�. (Vector3Int ���)")]
    public List<Vector3Int> areaOfEffect;
    public int motionID = 2;
    private readonly float defaultWaitTime = 0.5f;

    public override List<GameObject> GetTargetableTiles(UnitController user)
    {
        List<GameObject> tiles = new List<GameObject>();
        Vector3Int userPos = user.GetGridPosition();
        foreach (var vector in attackRange)
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
        List<GameObject> tiles = new List<GameObject>();
        Vector3Int originPos = targetTile.GetComponent<Tile3D>().gridPosition;
        Vector3Int attackerPos = user.GetGridPosition();
        Vector3Int attackDirection = originPos - attackerPos;
        if (attackDirection == Vector3Int.zero) attackDirection = new Vector3Int(0, 0, 1);

        foreach (var baseVector in areaOfEffect)
        {
            Vector3Int finalVector = RotateVector(baseVector, attackDirection);
            Vector3Int hitPos = originPos + finalVector;
            GameObject hitPosTile = GridManager3D.instance.GetTileAtPosition(hitPos);
            if (hitPosTile != null) tiles.Add(hitPosTile);
        }
        return tiles;
    }

    protected override IEnumerator InternalExecute()
    {
        Debug.Log($"<color=red>ACTION: {actionUser.name} starts attacking {actionTargetTile.name}.</color>");

        // --- 1. �׼� ���� ���� ---
        ExecuteVFXByTiming(EffectTiming.OnActionStart);
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionStart);

        Animator animator = actionUser.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetInteger("motionID", this.motionID);
            yield return new WaitForSeconds(defaultWaitTime);
        }

        // --- 2. �ٽ� ���� �� ȿ�� ���� ���� ---
        List<GameObject> impactedTiles = GetActionImpactTiles(actionUser, actionTargetTile);
        foreach (var hitTile in impactedTiles)
        {
            UnitController victim = GridManager3D.instance.GetUnitAtPosition(hitTile.GetComponent<Tile3D>().gridPosition);
            if (victim != null && victim != actionUser)
            {
                // ���� ����
                victim.TakeImpact(this.damage);

                // ��� ���� �� VFX �߻� (�� Ÿ�ٸ���)
                ExecuteVFXByTiming(EffectTiming.OnTargetImpact, victim);
            }
        }

        // ��� ���� �� ���� ȿ�� �߻� (�� Ÿ�� ���� 1ȸ)
        yield return ExecuteEffectsByTiming(EffectTiming.OnTargetImpact);

        if (animator != null) { animator.SetInteger("motionID", 0); }

        // --- 3. �׼� ���� ���� ---
        ExecuteVFXByTiming(EffectTiming.OnActionEnd);
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionEnd);

        Debug.Log($"<color=red>ACTION: {actionUser.name} attack completed.</color>");
    }


    /// <summary>
    /// 3D ����(XZ ���)���� ���� ���⿡ �°� ������ 8�������� ȸ����ŵ�ϴ�.
    /// </summary>
    private Vector3Int RotateVector(Vector3Int vector, Vector3Int direction)
    {
        // 1. ���� ������ ������ ����մϴ�. (0�� = ������)
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

        // 2. ������ 45�� ������ ���� ����� 8���� ������ '����'�մϴ�.
        int snappedAngle = Mathf.RoundToInt(angle / 45f) * 45;

        // 3. ������ ������ ���� ���͸� ��ȯ�մϴ�.
        switch (snappedAngle)
        {
            case 0:     // ������ (E)
                return new Vector3Int(vector.z, 0, -vector.x);
            case 45:    // ������ �� (NE)
                return new Vector3Int(vector.x + vector.z, 0, vector.z - vector.x);
            case 90:    // �� (N)
                return vector; // (x, 0, z) - ���� ����
            case 135:   // ���� �� (NW)
                return new Vector3Int(vector.x - vector.z, 0, vector.x + vector.z);
            case 180:   // ���� (W)
            case -180:
                return new Vector3Int(-vector.z, 0, vector.x);
            case -135:  // ���� �Ʒ� (SW)
                return new Vector3Int(-vector.x - vector.z, 0, -vector.z + vector.x);
            case -90:   // �Ʒ� (S)
                return new Vector3Int(-vector.x, 0, -vector.z);
            case -45:   // ������ �Ʒ� (SE)
                return new Vector3Int(-vector.x + vector.z, 0, -vector.x - vector.z);
            default:
                return vector;
        }
    }
}
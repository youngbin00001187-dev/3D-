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
    // ���� [����] �� vfxId �ʵ�� �� �̻� ������ ������, ȥ���� ���� ���� �ּ� ó���ϰų� �����ϴ� ���� �����մϴ�. ����
    // public int vfxId = 0; 
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
        ExecuteVFXByTiming(EffectTiming.OnActionStart);
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionStart);

        Animator animator = actionUser.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetInteger("motionID", this.motionID);
            yield return new WaitForSeconds(defaultWaitTime);
        }

        List<GameObject> impactedTiles = GetActionImpactTiles(actionUser, actionTargetTile);
        foreach (var hitTile in impactedTiles)
        {
            UnitController victim = GridManager3D.instance.GetUnitAtPosition(hitTile.GetComponent<Tile3D>().gridPosition);
            if (victim != null && victim != actionUser)
            {
                victim.TakeImpact(this.damage);

                // ���� [����] VFXManager�� ���� ȣ���ϴ� �ߺ� �ڵ带 �����߽��ϴ�. ����
                // ���� ��� Ÿ�� ������ ����Ʈ�� �Ʒ��� ExecuteVFXByTiming�� �����մϴ�.

                // ��� ���� �� VFX �߻� (�� Ÿ�ٸ���)
                ExecuteVFXByTiming(EffectTiming.OnTargetImpact, victim);
            }
        }

        yield return ExecuteEffectsByTiming(EffectTiming.OnTargetImpact);

        if (animator != null) { animator.SetInteger("motionID", 0); }

        ExecuteVFXByTiming(EffectTiming.OnActionEnd);
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionEnd);
    }

    private Vector3Int RotateVector(Vector3Int vector, Vector3Int direction)
    {
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
        int snappedAngle = Mathf.RoundToInt(angle / 45f) * 45;

        switch (snappedAngle)
        {
            case 0: return new Vector3Int(vector.z, 0, -vector.x);
            case 45: return new Vector3Int(vector.x + vector.z, 0, vector.z - vector.x);
            case 90: return vector;
            case 135: return new Vector3Int(vector.x - vector.z, 0, vector.x + vector.z);
            case 180: case -180: return new Vector3Int(-vector.z, 0, vector.x);
            case -135: return new Vector3Int(-vector.x - vector.z, 0, -vector.z + vector.x);
            case -90: return new Vector3Int(-vector.x, 0, -vector.z);
            case -45: return new Vector3Int(-vector.x + vector.z, 0, -vector.x - vector.z);
            default: return vector;
        }
    }
}


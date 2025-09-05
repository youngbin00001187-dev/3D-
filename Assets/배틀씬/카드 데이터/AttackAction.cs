using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class AttackAction : GameAction
{
    [Header("타겟팅 설정")]
    [Tooltip("공격 가능한 타일 범위입니다. (Vector3Int 사용)")]
    public List<Vector3Int> attackRange = new List<Vector3Int>();

    [Header("설정값")]
    public int damage;
    [Tooltip("피격 범위입니다. (Vector3Int 사용)")]
    public List<Vector3Int> areaOfEffect;
    public int motionID = 2;
    // ▼▼▼ [수정] 이 vfxId 필드는 더 이상 사용되지 않으며, 혼란을 막기 위해 주석 처리하거나 삭제하는 것을 권장합니다. ▼▼▼
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

                // ▼▼▼ [수정] VFXManager를 직접 호출하는 중복 코드를 제거했습니다. ▼▼▼
                // 이제 모든 타격 시점의 이펙트는 아래의 ExecuteVFXByTiming이 전담합니다.

                // 대상 적중 시 VFX 발생 (각 타겟마다)
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


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

        // --- 1. 액션 시작 시점 ---
        ExecuteVFXByTiming(EffectTiming.OnActionStart);
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionStart);

        Animator animator = actionUser.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetInteger("motionID", this.motionID);
            yield return new WaitForSeconds(defaultWaitTime);
        }

        // --- 2. 핵심 피해 및 효과 적용 로직 ---
        List<GameObject> impactedTiles = GetActionImpactTiles(actionUser, actionTargetTile);
        foreach (var hitTile in impactedTiles)
        {
            UnitController victim = GridManager3D.instance.GetUnitAtPosition(hitTile.GetComponent<Tile3D>().gridPosition);
            if (victim != null && victim != actionUser)
            {
                // 피해 적용
                victim.TakeImpact(this.damage);

                // 대상 적중 시 VFX 발생 (각 타겟마다)
                ExecuteVFXByTiming(EffectTiming.OnTargetImpact, victim);
            }
        }

        // 대상 적중 시 게임 효과 발생 (주 타겟 기준 1회)
        yield return ExecuteEffectsByTiming(EffectTiming.OnTargetImpact);

        if (animator != null) { animator.SetInteger("motionID", 0); }

        // --- 3. 액션 종료 시점 ---
        ExecuteVFXByTiming(EffectTiming.OnActionEnd);
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionEnd);

        Debug.Log($"<color=red>ACTION: {actionUser.name} attack completed.</color>");
    }


    /// <summary>
    /// 3D 공간(XZ 평면)에서 공격 방향에 맞게 범위를 8방향으로 회전시킵니다.
    /// </summary>
    private Vector3Int RotateVector(Vector3Int vector, Vector3Int direction)
    {
        // 1. 방향 벡터의 각도를 계산합니다. (0도 = 오른쪽)
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

        // 2. 각도를 45도 단위로 가장 가까운 8방향 각도로 '보정'합니다.
        int snappedAngle = Mathf.RoundToInt(angle / 45f) * 45;

        // 3. 보정된 각도에 따라 벡터를 변환합니다.
        switch (snappedAngle)
        {
            case 0:     // 오른쪽 (E)
                return new Vector3Int(vector.z, 0, -vector.x);
            case 45:    // 오른쪽 위 (NE)
                return new Vector3Int(vector.x + vector.z, 0, vector.z - vector.x);
            case 90:    // 위 (N)
                return vector; // (x, 0, z) - 기준 방향
            case 135:   // 왼쪽 위 (NW)
                return new Vector3Int(vector.x - vector.z, 0, vector.x + vector.z);
            case 180:   // 왼쪽 (W)
            case -180:
                return new Vector3Int(-vector.z, 0, vector.x);
            case -135:  // 왼쪽 아래 (SW)
                return new Vector3Int(-vector.x - vector.z, 0, -vector.z + vector.x);
            case -90:   // 아래 (S)
                return new Vector3Int(-vector.x, 0, -vector.z);
            case -45:   // 오른쪽 아래 (SE)
                return new Vector3Int(-vector.x + vector.z, 0, -vector.x - vector.z);
            default:
                return vector;
        }
    }
}
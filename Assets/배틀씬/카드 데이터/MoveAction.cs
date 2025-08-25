using System.Collections;
using System.Collections.Generic;
using System.Linq; // .Any()를 사용하기 위해 추가
using UnityEngine;

[System.Serializable]
public class MoveAction : GameAction
{
    [Header("Targeting Settings")]
    [Tooltip("이동 가능한 타일 범위입니다. (Vector3Int 사용)")]
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
        Debug.Log($"<color=green>ACTION: {actionUser.name}이(가) {actionTargetTile.name}(으)로 이동을 시도합니다.</color>");

        // --- 1. 액션 시작 시점 ---
        ExecuteVFXByTiming(EffectTiming.OnActionStart);
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionStart);

        Vector3Int targetPos = actionTargetTile.GetComponent<Tile3D>().gridPosition;
        UnitController unitOnTarget = GridManager3D.instance.GetUnitAtPosition(targetPos);
        bool collision = (unitOnTarget != null && unitOnTarget != actionUser);

        if (collision)
        {
            // 1. 충돌 시점에 발동할 부가 효과(넉백 등)가 있는지 먼저 확인합니다.
            bool hasImpactEffect = attachedEffects.Any(effect => effect.timing == EffectTiming.OnTargetImpact);

            // 2. VFX는 충돌 시 항상 발생시킵니다.
            ExecuteVFXByTiming(EffectTiming.OnTargetImpact, unitOnTarget);

            if (hasImpactEffect)
            {
                // 3-A. 부가 효과가 있다면: 효과가 끝날 때까지 기다립니다. (시전자는 리코일하지 않음)
                Debug.Log("[MoveAction] 충돌: 부가 효과(넉백 등)를 발동합니다.");
                yield return ExecuteEffectsByTiming(EffectTiming.OnTargetImpact);
            }
            else
            {
                // 3-B. 부가 효과가 없다면: 시전자가 뒤로 튕겨나는 리코일을 실행합니다.
                Debug.Log("[MoveAction] 충돌: 부가 효과가 없으므로 리코일합니다.");
                yield return actionUser.StartCoroutine(actionUser.RecoilCoroutine(actionTargetTile.transform.position));
            }
        }
        else
        {
            // --- 핵심 이동 로직 (충돌하지 않았을 경우) ---
            Animator animator = actionUser.GetComponent<Animator>();
            if (animator != null) animator.SetInteger("motionID", this.motionID);

            yield return actionUser.StartCoroutine(actionUser.MoveCoroutine(actionTargetTile, moveSpeed));

            if (animator != null) animator.SetInteger("motionID", 0);

            // --- 이동 완료 시점 ---
            ExecuteVFXByTiming(EffectTiming.AfterMove);
            yield return ExecuteEffectsByTiming(EffectTiming.AfterMove);
        }

        // --- 액션 종료 시점 ---
        ExecuteVFXByTiming(EffectTiming.OnActionEnd);
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionEnd);

        Debug.Log($"<color=green>ACTION: {actionUser.name}의 이동 액션이 완료되었습니다.</color>");
    }
}
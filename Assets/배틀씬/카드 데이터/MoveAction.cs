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

    // MoveAction.cs

    // MoveAction.cs

    protected override IEnumerator InternalExecute()
    {
        Debug.Log($"<color=green>ACTION: {actionUser.name}이(가) {actionTargetTile.name}(으)로 이동을 시도합니다.</color>");

        // 1. 시작 타일 기억, 시작 효과 실행
        GameObject startingTile = actionUser.currentTile;
        ExecuteVFXByTiming(EffectTiming.OnActionStart);
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionStart); // 인수 1개 버전 호출

        // 2. 애니메이션 시작 및 이동
        Animator animator = actionUser.GetComponent<Animator>();
        if (animator != null) animator.SetInteger("motionID", this.motionID);

        yield return actionUser.MoveCoroutine(actionTargetTile, moveSpeed);

        // 3. 넉백 효과에만 시작 위치 정보 주입
        foreach (var effect in attachedEffects)
        {
            if (EffectManager.Instance.GetEffect(effect.effectType) is KnockbackEffect knockbackEffect)
            {
                knockbackEffect.sourceTileOverride = startingTile;
            }
        }
        // 넉백 효과 실행 (인수 1개 버전 호출)
        yield return ExecuteEffectsByTiming(EffectTiming.OnTargetImpact);

        // 4. 최종 상태 확인
        Vector3Int targetPos = actionTargetTile.GetComponent<Tile3D>().gridPosition;
        var unitsOnTarget = GridManager3D.instance.GetUnitsAtPosition(targetPos);
        bool isStillOverlapping = unitsOnTarget.Any(u => u != actionUser);

        if (isStillOverlapping)
        {
            // 5-A. 이동 실패 시: 리코일 후 논리적 위치 복귀
            Debug.Log("[MoveAction] 최종 확인 결과 경로가 막혀, 리코일합니다.");
            UnitController otherUnit = unitsOnTarget.First(u => u != actionUser);
            ExecuteVFXByTiming(EffectTiming.OnTargetImpact, otherUnit);

            yield return actionUser.RecoilCoroutine(actionUser.transform.position);
            actionUser.MoveToTile(startingTile);
        }
        else
        {
            // 5-B. 이동 성공 시: 이동 완료 효과 실행
            Debug.Log("[MoveAction] 최종 확인 결과 경로가 비어 이동을 확정합니다.");
            ExecuteVFXByTiming(EffectTiming.AfterMove);
            yield return ExecuteEffectsByTiming(EffectTiming.AfterMove); // 인수 1개 버전 호출
        }

        // 6. 공통 종료 효과 (인수 1개 버전 호출)
        ExecuteVFXByTiming(EffectTiming.OnActionEnd);
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionEnd);

        // motionID 리셋은 ActionTurnManager의 책임이므로 여기에는 코드가 없습니다.
        Debug.Log($"<color=green>ACTION: {actionUser.name}의 이동 액션이 완료되었습니다.</color>");
    }
}
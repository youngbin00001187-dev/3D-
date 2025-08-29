using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System;
using System.Linq;

[System.Serializable]
public class KnockbackEffect : GameEffect
{
    [Header("넉백 설정")]
    public int knockbackDistance = 1;
    public float moveSpeed = 3f;
    public float pushInterval = 0.2f;
    public float firstImpactInterval = 0.4f;
    public float arcHeight = 0.5f;
    public float startDelay = 0f;

    [System.NonSerialized] public GameObject sourceTileOverride;

    private struct KnockbackPlan
    {
        public UnitController unit;
        public GameObject targetTile;
        public bool isWallHit;
        public Vector3Int direction;
    }

    public override IEnumerator Apply(UnitController user, GameObject targetTile, Action onComplete)
    {
        Tile3D initialTile3D = targetTile.GetComponent<Tile3D>();
        if (initialTile3D == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        UnitController initialTarget = GridManager3D.instance.GetUnitAtPosition(initialTile3D.gridPosition);

        if (initialTarget == null || initialTarget == user)
        {
            Debug.LogWarning("[KnockbackEffect] Apply: 타겟 타일에 유닛이 없거나 자기 자신입니다.");
            onComplete?.Invoke();
            yield break;
        }

        // *** 핵심 수정: sourceTileOverride 사용 ***
        Vector3Int sourcePosition;
        if (sourceTileOverride != null)
        {
            sourcePosition = sourceTileOverride.GetComponent<Tile3D>().gridPosition;
            Debug.Log($"<color=yellow>[KnockbackEffect] sourceTileOverride 사용: {sourcePosition}</color>");
        }
        else
        {
            sourcePosition = user.GetGridPosition();
            Debug.Log($"<color=orange>[KnockbackEffect] 현재 위치 사용: {sourcePosition}</color>");
        }

        // 방향 계산 시 sourceTileOverride 또는 현재 위치 사용
        Vector3Int rawDir = initialTarget.GetGridPosition() - sourcePosition;
        if (rawDir == Vector3Int.zero) rawDir = Vector3Int.forward;

        Vector3Int fixedDirection = new Vector3Int(
            rawDir.x == 0 ? 0 : (int)Mathf.Sign(rawDir.x),
            0,
            rawDir.z == 0 ? 0 : (int)Mathf.Sign(rawDir.z)
        );

        Debug.Log($"<color=cyan>[KnockbackEffect] 방향 계산: 소스={sourcePosition}, 타겟={initialTarget.GetGridPosition()}, 방향={fixedDirection}</color>");

        // sourceTileOverride 사용 후 초기화 (다음 사용을 위해)
        sourceTileOverride = null;

        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        List<KnockbackPlan> knockbackPlans = new List<KnockbackPlan>();
        PlanChainKnockback(initialTarget, fixedDirection, knockbackDistance, knockbackPlans);

        string planLog = string.Join(", ", knockbackPlans.Select(p =>
            $"{p.unit.name} -> {(p.isWallHit ? "WALL" : p.targetTile.GetComponent<Tile3D>().gridPosition.ToString())}"
        ));
        Debug.Log($"<color=lightblue>[KnockbackEffect] 계획 수립 완료: [{planLog}]</color>");

        if (knockbackPlans.Count == 0 || knockbackPlans.All(p => p.isWallHit))
        {
            Debug.LogWarning($"[KnockbackEffect] 경로 막힘. {initialTarget.name} 리코일만 실행.");
            yield return initialTarget.StartCoroutine(
                initialTarget.RecoilCoroutine(
                    initialTarget.transform.position + (Vector3)fixedDirection * 0.5f
                )
            );
            initialTarget.TakeImpact(0);
            onComplete?.Invoke();
            yield break;
        }

        yield return ExecuteVisualKnockback(knockbackPlans, onComplete);
        Debug.Log("<color=cyan>[KnockbackEffect] Apply 종료.</color>");
    }

    private void PlanChainKnockback(UnitController currentUnit, Vector3Int fixedDirection, int distanceRemaining, List<KnockbackPlan> plans)
    {
        if (distanceRemaining <= 0) return;

        Vector3Int currentPos = currentUnit.GetGridPosition();
        Vector3Int destinationPos = currentPos + fixedDirection;
        GameObject destinationTile = GridManager3D.instance.GetTileAtPosition(destinationPos);

        Debug.Log($"[Plan] {currentUnit.name} 계획: {currentPos} -> {destinationPos} (방향: {fixedDirection}, 남은거리: {distanceRemaining})");

        if (destinationTile == null || !destinationTile.GetComponent<Tile3D>().isWalkable)
        {
            plans.Add(new KnockbackPlan { unit = currentUnit, isWallHit = true, direction = fixedDirection });
            return;
        }

        UnitController occupant = GridManager3D.instance.GetUnitAtPosition(destinationPos);
        if (occupant != null && occupant != currentUnit)
        {
            PlanChainKnockback(occupant, fixedDirection, distanceRemaining, plans);

            bool occupantBlocked = plans.Any(p => p.unit == occupant && p.isWallHit);
            if (occupantBlocked)
            {
                plans.Add(new KnockbackPlan { unit = currentUnit, isWallHit = true, direction = fixedDirection });
                return;
            }
        }

        plans.Add(new KnockbackPlan { unit = currentUnit, targetTile = destinationTile, isWallHit = false, direction = fixedDirection });

        if (distanceRemaining > 1)
        {
            PlanChainKnockback(currentUnit, fixedDirection, distanceRemaining - 1, plans);
        }
    }

    private IEnumerator ExecuteVisualKnockback(List<KnockbackPlan> plans, Action onComplete)
    {
        Debug.Log("<color=lime>[VisualKnockback] 시각 연출 시작...</color>");
        plans.Reverse();

        // [수정] 시퀀스를 생성할 때 SetUpdate(true)를 추가하여 Time.timeScale의 영향을 받도록 합니다.
        var sequence = DOTween.Sequence().SetUpdate(true);

        float delay = 0f;
        bool isFirstInChain = true;

        // 이동 시작 전, 모든 유닛의 현재 위치를 그리드에서 미리 해제합니다.
        foreach (var plan in plans)
        {
            if (!plan.isWallHit)
                GridManager3D.instance.UnregisterUnitPosition(plan.unit, plan.unit.GetGridPosition());
        }

        // DOTween 시퀀스를 사용하여 모든 유닛의 시각적 이동/리코일 애니메이션을 구성합니다.
        foreach (var plan in plans)
        {
            if (plan.isWallHit)
            {
                Vector3 recoilTargetPos = plan.unit.transform.position + (Vector3)plan.direction * 0.4f;
                var recoilTween = DOTween.Sequence()
                    .Append(plan.unit.transform.DOMove(recoilTargetPos, 0.15f))
                    .Append(plan.unit.transform.DOMove(plan.unit.transform.position, 0.15f))
                    .OnComplete(() => plan.unit.TakeImpact(0));
                sequence.Insert(delay, recoilTween);
            }
            else
            {
                float distance = Vector3.Distance(plan.unit.transform.position, plan.targetTile.transform.position);
                float duration = Mathf.Max(distance / moveSpeed, 0.5f);
                var moveTween = plan.unit.transform.DOJump(
                    plan.targetTile.transform.position,
                    arcHeight, 1, duration
                ).SetEase(Ease.OutQuad)
                 .OnComplete(() => plan.unit.TakeImpact(0));
                sequence.Insert(delay, moveTween);
            }
            delay += isFirstInChain ? firstImpactInterval : pushInterval;
            isFirstInChain = false;
        }

        // 모든 시각적 애니메이션이 끝날 때까지 기다립니다.
        yield return sequence.WaitForCompletion();

        // 모든 애니메이션이 끝난 후, 논리적 위치를 갱신하고 행동을 재조정합니다.
        foreach (var plan in plans)
        {
            if (!plan.isWallHit)
            {
                plan.unit.MoveToTile(plan.targetTile);
                if (plan.unit is EnemyController enemy)
                {
                    enemy.ReplanActionFromNewPosition();
                }
            }
        }

        onComplete?.Invoke();
    }
}
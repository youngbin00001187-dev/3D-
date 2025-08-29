using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System;
using System.Linq;

[System.Serializable]
public class KnockbackEffect : GameEffect
{
    [Header("�˹� ����")]
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
            Debug.LogWarning("[KnockbackEffect] Apply: Ÿ�� Ÿ�Ͽ� ������ ���ų� �ڱ� �ڽ��Դϴ�.");
            onComplete?.Invoke();
            yield break;
        }

        // *** �ٽ� ����: sourceTileOverride ��� ***
        Vector3Int sourcePosition;
        if (sourceTileOverride != null)
        {
            sourcePosition = sourceTileOverride.GetComponent<Tile3D>().gridPosition;
            Debug.Log($"<color=yellow>[KnockbackEffect] sourceTileOverride ���: {sourcePosition}</color>");
        }
        else
        {
            sourcePosition = user.GetGridPosition();
            Debug.Log($"<color=orange>[KnockbackEffect] ���� ��ġ ���: {sourcePosition}</color>");
        }

        // ���� ��� �� sourceTileOverride �Ǵ� ���� ��ġ ���
        Vector3Int rawDir = initialTarget.GetGridPosition() - sourcePosition;
        if (rawDir == Vector3Int.zero) rawDir = Vector3Int.forward;

        Vector3Int fixedDirection = new Vector3Int(
            rawDir.x == 0 ? 0 : (int)Mathf.Sign(rawDir.x),
            0,
            rawDir.z == 0 ? 0 : (int)Mathf.Sign(rawDir.z)
        );

        Debug.Log($"<color=cyan>[KnockbackEffect] ���� ���: �ҽ�={sourcePosition}, Ÿ��={initialTarget.GetGridPosition()}, ����={fixedDirection}</color>");

        // sourceTileOverride ��� �� �ʱ�ȭ (���� ����� ����)
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
        Debug.Log($"<color=lightblue>[KnockbackEffect] ��ȹ ���� �Ϸ�: [{planLog}]</color>");

        if (knockbackPlans.Count == 0 || knockbackPlans.All(p => p.isWallHit))
        {
            Debug.LogWarning($"[KnockbackEffect] ��� ����. {initialTarget.name} �����ϸ� ����.");
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
        Debug.Log("<color=cyan>[KnockbackEffect] Apply ����.</color>");
    }

    private void PlanChainKnockback(UnitController currentUnit, Vector3Int fixedDirection, int distanceRemaining, List<KnockbackPlan> plans)
    {
        if (distanceRemaining <= 0) return;

        Vector3Int currentPos = currentUnit.GetGridPosition();
        Vector3Int destinationPos = currentPos + fixedDirection;
        GameObject destinationTile = GridManager3D.instance.GetTileAtPosition(destinationPos);

        Debug.Log($"[Plan] {currentUnit.name} ��ȹ: {currentPos} -> {destinationPos} (����: {fixedDirection}, �����Ÿ�: {distanceRemaining})");

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
        Debug.Log("<color=lime>[VisualKnockback] �ð� ���� ����...</color>");
        plans.Reverse();

        // [����] �������� ������ �� SetUpdate(true)�� �߰��Ͽ� Time.timeScale�� ������ �޵��� �մϴ�.
        var sequence = DOTween.Sequence().SetUpdate(true);

        float delay = 0f;
        bool isFirstInChain = true;

        // �̵� ���� ��, ��� ������ ���� ��ġ�� �׸��忡�� �̸� �����մϴ�.
        foreach (var plan in plans)
        {
            if (!plan.isWallHit)
                GridManager3D.instance.UnregisterUnitPosition(plan.unit, plan.unit.GetGridPosition());
        }

        // DOTween �������� ����Ͽ� ��� ������ �ð��� �̵�/������ �ִϸ��̼��� �����մϴ�.
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

        // ��� �ð��� �ִϸ��̼��� ���� ������ ��ٸ��ϴ�.
        yield return sequence.WaitForCompletion();

        // ��� �ִϸ��̼��� ���� ��, ���� ��ġ�� �����ϰ� �ൿ�� �������մϴ�.
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
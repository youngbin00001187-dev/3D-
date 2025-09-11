using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ThrowAction : GameAction
{
    [Header("타겟팅 설정")]
    public List<Vector3Int> attackRange = new List<Vector3Int>();

    [Header("발사 패턴 설정 (AOE)")]
    [Tooltip("클릭한 타일을 기준으로, 투사체가 발사될 상대 위치 목록입니다.")]
    public List<Vector3Int> areaOfEffect = new List<Vector3Int> { Vector3Int.zero };

    [Header("투사체 설정")]
    public int projectileId = 0;
    public int motionID = 3;
    [Tooltip("다중 발사 시 각 투사체 사이의 시간 간격입니다.")]
    public float launchDelay = 0.1f; // 여러 발 쏠 때의 간격

    // GetTargetableTiles와 GetActionImpactTiles 메서드는 변경할 필요가 없습니다.
    // 플레이어는 여전히 단일 타겟을 지정하고, 그 타겟을 기준으로 발사 패턴이 결정되기 때문입니다.
    public override List<GameObject> GetTargetableTiles(UnitController user)
    {
        List<GameObject> tiles = new List<GameObject>();
        Vector3Int userPos = user.GetGridPosition();
        foreach (var vector in attackRange)
        {
            GameObject tile = GridManager3D.instance.GetTileAtPosition(userPos + vector);
            if (tile != null) tiles.Add(tile);
        }
        return tiles;
    }

    public override List<GameObject> GetActionImpactTiles(UnitController user, GameObject targetTile)
    {
        List<GameObject> tiles = new List<GameObject>();
        if (targetTile == null) return tiles;
        Tile3D targetTileComponent = targetTile.GetComponent<Tile3D>();
        if (targetTileComponent == null) return tiles;
        Vector3Int originPos = targetTileComponent.gridPosition;
        foreach (var vector in areaOfEffect)
        {
            Vector3Int hitPos = originPos + vector;
            GameObject hitPosTile = GridManager3D.instance.GetTileAtPosition(hitPos);
            if (hitPosTile != null) tiles.Add(hitPosTile);
        }
        return tiles;
    }

    /// <summary>
    /// 실제 액션 실행 로직 (다중 발사 및 모든 투사체 대기 기능으로 수정)
    /// </summary>
    protected override IEnumerator InternalExecute()
    {
        // 1. 애니메이션 재생
        Animator animator = actionUser.UnitAnimator;
        if (animator != null)
        {
            animator.SetInteger("motionID", this.motionID);
        }
        // 애니메이션을 위한 짧은 대기
        yield return new WaitForSeconds(0.1f);

        if (ProjectileManager.Instance != null)
        {
            // 2. 발사할 최종 목표 타일 목록 계산
            List<GameObject> finalTargetTiles = GetActionImpactTiles(actionUser, actionTargetTile);

            if (finalTargetTiles.Count == 0)
            {
                Debug.LogWarning("[ThrowAction] 발사할 유효한 타겟이 없습니다.");
                yield break;
            }

            // 3. 모든 투사체가 작업을 마쳤는지 추적하기 위한 '카운터' 설정
            int projectilesRemaining = finalTargetTiles.Count;
            Action onCompleteCallback = () => { projectilesRemaining--; };

            Debug.Log($"<color=yellow>[ThrowAction] {projectilesRemaining}개의 투사체 동시 발사를 시작합니다.</color>");

            // 4. 계산된 모든 타일을 향해 순차적으로 투사체 발사
            foreach (var tile in finalTargetTiles)
            {
                ProjectileManager.Instance.LaunchProjectile(actionUser, tile, projectileId, onCompleteCallback);
                if (launchDelay > 0)
                {
                    yield return new WaitForSeconds(launchDelay); // 각 발사 사이의 딜레이
                }
            }

            // 5. 카운터가 0이 될 때까지 (모든 투사체가 완료 신호를 보낼 때까지) 대기
            yield return new WaitUntil(() => projectilesRemaining <= 0);

            Debug.Log("<color=green>[ThrowAction] 모든 투사체의 작업 완료 신호 수신! 액션을 완전히 종료합니다.</color>");
        }
        else
        {
            Debug.LogError("[ThrowAction] ProjectileManager.Instance를 찾을 수 없습니다!");
        }
    }
}
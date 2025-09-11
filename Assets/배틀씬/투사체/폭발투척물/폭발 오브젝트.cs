using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class TweenExplosiveProjectile : AbstractProjectile
{
    // ▼▼▼ [핵심 수정] 타일의 상태를 모든 투사체가 공유하도록 static 변수로 관리 ▼▼▼
    private static Dictionary<Tile3D, Vector3> _tileOriginalPositions = new Dictionary<Tile3D, Vector3>();
    private static Dictionary<Tile3D, int> _tileHitCounts = new Dictionary<Tile3D, int>();
    private static Dictionary<Tile3D, Sequence> _activeSequences = new Dictionary<Tile3D, Sequence>();

    [Header("직선 투사체 설정")]
    public float moveDuration = 0.5f;
    public int damage = 10;
    public Ease easeType = Ease.Linear;

    [Header("착탄 시각 효과 설정")]
    public Color impactTileColor = new Color(1f, 0.5f, 0.5f);
    public float tileFlashDuration = 0.4f;
    public float tileHopHeight = 0.2f;
    public float tileHopDuration = 0.3f;
    [Tooltip("중복 타격 시 추가로 튀어오르는 높이 비율입니다.")]
    [Range(0.1f, 1f)]
    public float additionalHopRatio = 0.5f;

    protected override void Launch()
    {
        StartCoroutine(MoveAndImpactCoroutine());
    }

    private IEnumerator MoveAndImpactCoroutine()
    {
        // 새로운 다중 발사가 시작될 때마다 이전 기록을 모두 초기화합니다.
        _tileOriginalPositions.Clear();
        _tileHitCounts.Clear();
        _activeSequences.Clear();

        yield return transform.DOMove(_targetTile.transform.position, moveDuration).SetEase(easeType).WaitForCompletion();

        Vector3Int originPos = _targetTile.GetComponent<Tile3D>().gridPosition;
        List<Vector3Int> crossPattern = new List<Vector3Int>
        {
            new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1)
        };

        foreach (var pattern in crossPattern)
        {
            GameObject tileObject = GridManager3D.instance.GetTileAtPosition(originPos + pattern);
            if (tileObject != null)
            {
                ImpactTileEffect(tileObject);
                UnitController victim = GridManager3D.instance.GetUnitAtPosition(originPos + pattern);
                if (victim != null) victim.TakeImpact(damage);
            }
        }

        _onComplete?.Invoke();
        Destroy(gameObject);
    }

    private void ImpactTileEffect(GameObject tileObject)
    {
        Tile3D tile = tileObject.GetComponent<Tile3D>();
        if (tile == null) return;

        if (!_tileOriginalPositions.ContainsKey(tile))
        {
            _tileOriginalPositions[tile] = tile.transform.position;
        }
        Vector3 originalPosition = _tileOriginalPositions[tile];

        _tileHitCounts.TryGetValue(tile, out int hitCount);
        hitCount++;
        _tileHitCounts[tile] = hitCount;

        if (_activeSequences.ContainsKey(tile))
        {
            // ▼▼▼ [핵심 수정] ▼▼▼
            // Kill()에 true 파라미터를 추가하여, 애니메이션을 종료하기 전에
            // 최종 상태(원래 색상, 원래 위치)로 즉시 돌려놓도록 합니다.
            _activeSequences[tile].Kill(true);
        }

        float peakHopHeight = tileHopHeight + ((hitCount - 1) * tileHopHeight * additionalHopRatio);

        Sequence newSequence = DOTween.Sequence();

        newSequence.Join(tile.MyMaterial.DOColor(impactTileColor, tileFlashDuration / 2f).SetLoops(2, LoopType.Yoyo));
        newSequence.Join(tile.transform.DOJump(originalPosition, peakHopHeight, 1, tileHopDuration));

        newSequence.OnComplete(() => {
            _tileHitCounts.Remove(tile);
            _activeSequences.Remove(tile);
        });

        _activeSequences[tile] = newSequence;
    }
}
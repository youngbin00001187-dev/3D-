using UnityEngine;
using System;
using System.Collections;
using DG.Tweening; // DOTween을 사용하기 위해 이 네임스페이스를 추가합니다.

public class TweenLineProjectile : AbstractProjectile
{
    [Header("직선 투사체 설정")]
    [Tooltip("목표 지점까지 날아가는 데 걸리는 시간(초)입니다.")]
    public float moveDuration = 0.5f;
    [Tooltip("목표 지점에 있는 유닛에게 가할 피해량입니다.")]
    public int damage = 10;
    [Tooltip("이동에 사용할 DOTween Ease 효과입니다.")]
    public Ease easeType = Ease.Linear;

    /// <summary>
    /// 부모의 Initialize에 의해 호출되어 실제 로직(코루틴)을 시작합니다.
    /// </summary>
    protected override void Launch()
    {
        StartCoroutine(MoveAndImpactCoroutine());
    }

    /// <summary>
    /// DOTween으로 이동하고, 도착 시 피해를 입히는 전체 과정을 담당하는 코루틴입니다.
    /// </summary>
    private IEnumerator MoveAndImpactCoroutine()
    {
        // 1. DOTween을 사용하여 목표 지점(_targetTile)으로 이동하는 '트윈'을 생성하고 실행합니다.
        Tweener moveTween = transform.DOMove(_targetTile.transform.position, moveDuration).SetEase(easeType);

        // 트윈 애니메이션이 끝날 때까지 여기서 기다립니다.
        yield return moveTween.WaitForCompletion();

        // 2. 이동이 완료되면 착탄 로직을 실행합니다.
        Debug.Log($"[TweenLineProjectile] {_targetTile.name}에 명중! {damage}의 피해를 입힙니다.");

        // 목표 타일에 있는 유닛을 찾습니다.
        Tile3D tile = _targetTile.GetComponent<Tile3D>();
        if (tile != null)
        {
            UnitController victim = GridManager3D.instance.GetUnitAtPosition(tile.gridPosition);
            if (victim != null)
            {
                // 유닛에게 피해를 줍니다.
                victim.TakeImpact(damage);
            }
        }

        // 여기에 착탄 VFX를 재생하는 코드를 추가할 수 있습니다.
        // VFXManager.Instance.PlayHitEffect(transform.position, vfxId);

        // 3. 모든 임무를 마쳤으므로, 완료 신호를 보냅니다.
        Debug.Log("<color=cyan>[TweenLineProjectile] 모든 임무 완료! 완료 신호를 보냅니다.</color>");
        _onComplete?.Invoke();

        // 4. 자기 자신을 파괴합니다.
        Destroy(gameObject);
    }
}
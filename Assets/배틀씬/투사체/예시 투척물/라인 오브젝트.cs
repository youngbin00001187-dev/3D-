using UnityEngine;
using System;
using System.Collections;
using DG.Tweening; // DOTween�� ����ϱ� ���� �� ���ӽ����̽��� �߰��մϴ�.

public class TweenLineProjectile : AbstractProjectile
{
    [Header("���� ����ü ����")]
    [Tooltip("��ǥ �������� ���ư��� �� �ɸ��� �ð�(��)�Դϴ�.")]
    public float moveDuration = 0.5f;
    [Tooltip("��ǥ ������ �ִ� ���ֿ��� ���� ���ط��Դϴ�.")]
    public int damage = 10;
    [Tooltip("�̵��� ����� DOTween Ease ȿ���Դϴ�.")]
    public Ease easeType = Ease.Linear;

    /// <summary>
    /// �θ��� Initialize�� ���� ȣ��Ǿ� ���� ����(�ڷ�ƾ)�� �����մϴ�.
    /// </summary>
    protected override void Launch()
    {
        StartCoroutine(MoveAndImpactCoroutine());
    }

    /// <summary>
    /// DOTween���� �̵��ϰ�, ���� �� ���ظ� ������ ��ü ������ ����ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    private IEnumerator MoveAndImpactCoroutine()
    {
        // 1. DOTween�� ����Ͽ� ��ǥ ����(_targetTile)���� �̵��ϴ� 'Ʈ��'�� �����ϰ� �����մϴ�.
        Tweener moveTween = transform.DOMove(_targetTile.transform.position, moveDuration).SetEase(easeType);

        // Ʈ�� �ִϸ��̼��� ���� ������ ���⼭ ��ٸ��ϴ�.
        yield return moveTween.WaitForCompletion();

        // 2. �̵��� �Ϸ�Ǹ� ��ź ������ �����մϴ�.
        Debug.Log($"[TweenLineProjectile] {_targetTile.name}�� ����! {damage}�� ���ظ� �����ϴ�.");

        // ��ǥ Ÿ�Ͽ� �ִ� ������ ã���ϴ�.
        Tile3D tile = _targetTile.GetComponent<Tile3D>();
        if (tile != null)
        {
            UnitController victim = GridManager3D.instance.GetUnitAtPosition(tile.gridPosition);
            if (victim != null)
            {
                // ���ֿ��� ���ظ� �ݴϴ�.
                victim.TakeImpact(damage);
            }
        }

        // ���⿡ ��ź VFX�� ����ϴ� �ڵ带 �߰��� �� �ֽ��ϴ�.
        // VFXManager.Instance.PlayHitEffect(transform.position, vfxId);

        // 3. ��� �ӹ��� �������Ƿ�, �Ϸ� ��ȣ�� �����ϴ�.
        Debug.Log("<color=cyan>[TweenLineProjectile] ��� �ӹ� �Ϸ�! �Ϸ� ��ȣ�� �����ϴ�.</color>");
        _onComplete?.Invoke();

        // 4. �ڱ� �ڽ��� �ı��մϴ�.
        Destroy(gameObject);
    }
}
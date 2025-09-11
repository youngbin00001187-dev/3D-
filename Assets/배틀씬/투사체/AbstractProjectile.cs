using UnityEngine;
using System;

public abstract class AbstractProjectile : MonoBehaviour
{
    // ���� ��� �ڽ��� �����ϴ� �ٽ� ������ ����
    protected UnitController _user;
    protected GameObject _targetTile;
    protected Action _onComplete;

    /// <summary>
    /// ProjectileManager�� ȣ���Ͽ� ��� ����ü�� �⺻ ������ �����մϴ�.
    /// �� �޼���� final�̶� �ڽĿ��� �������̵� �� �� �����ϴ�.
    /// </summary>
    public void Initialize(UnitController user, GameObject targetTile, Action onComplete = null)
    {
        _user = user;
        _targetTile = targetTile;
        _onComplete = onComplete;

        // �ʱ�ȭ�� ������ �ڽ� Ŭ������ ���� ������ �����ŵ�ϴ�.
        Launch();
    }

    /// <summary>
    /// �� ����ü�� ���� �̵� �� ��ź ������ �����ϴ� �߻� �޼����Դϴ�.
    /// ��� �ڽ� Ŭ������ �� �޼��带 �ݵ�� �����ؾ� �մϴ�.
    /// </summary>
    protected abstract void Launch();
}
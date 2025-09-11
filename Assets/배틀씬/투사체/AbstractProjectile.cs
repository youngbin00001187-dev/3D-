using UnityEngine;
using System;

public abstract class AbstractProjectile : MonoBehaviour
{
    // ▼▼▼ 모든 자식이 공유하는 핵심 데이터 ▼▼▼
    protected UnitController _user;
    protected GameObject _targetTile;
    protected Action _onComplete;

    /// <summary>
    /// ProjectileManager가 호출하여 모든 투사체의 기본 정보를 설정합니다.
    /// 이 메서드는 final이라 자식에서 오버라이드 할 수 없습니다.
    /// </summary>
    public void Initialize(UnitController user, GameObject targetTile, Action onComplete = null)
    {
        _user = user;
        _targetTile = targetTile;
        _onComplete = onComplete;

        // 초기화가 끝나면 자식 클래스의 실제 로직을 실행시킵니다.
        Launch();
    }

    /// <summary>
    /// 이 투사체의 실제 이동 및 착탄 로직을 시작하는 추상 메서드입니다.
    /// 모든 자식 클래스는 이 메서드를 반드시 구현해야 합니다.
    /// </summary>
    protected abstract void Launch();
}
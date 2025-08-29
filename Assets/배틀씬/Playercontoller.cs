using UnityEngine;

/// <summary>
/// UnitController를 상속받는 플레이어 전용 클래스입니다.
/// </summary>
public class PlayerController : UnitController
{
    [Header("브레이크 시스템")]
    [Tooltip("턴당 최대 브레이크 횟수입니다.")]
    public int maxBreaksPerTurn = 1;
    private int _currentBreaksLeft;

    // --- [신규] 브레이크 관련 함수들 ---

    /// <summary>
    /// 현재 남은 브레이크 횟수를 반환합니다.
    /// </summary>
    public bool HasBreaksLeft()
    {
        return _currentBreaksLeft > 0;
    }

    /// <summary>
    /// 브레이크 횟수를 1 소모합니다.
    /// </summary>
    public void UseBreak()
    {
        if (HasBreaksLeft())
        {
            _currentBreaksLeft--;
            Debug.Log($"<color=purple>[BREAK] 브레이크 사용! 남은 횟수: {_currentBreaksLeft}</color>");
            // TODO: 여기에 브레이크 사용 UI 갱신 로직을 연결할 수 있습니다.
        }
    }

    /// <summary>
    /// 턴 시작 시 브레이크 횟수를 최대로 초기화합니다.
    /// </summary>
    public void ResetBreaks()
    {
        _currentBreaksLeft = maxBreaksPerTurn;
    }
}
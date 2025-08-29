using UnityEngine;
using System;
using System.Collections;

public class BattleEventManager : MonoBehaviour
{
    public static BattleEventManager instance;

    public event Action OnSetupGrid;
    public event Action OnGridGenerationComplete;
    public event Action OnAllUnitsPlaced;
    public event Action OnBattleStart;
    public event Action OnActionPhaseStart;

    // [신규] 카드 매니저가 초기화를 마쳤을 때 발생할 이벤트입니다.
    public event Action OnCardManagerReady;

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    private void OnEnable()
    {
        // 이 핸들러는 더 이상 큰 의미가 없으므로 유지하거나 삭제해도 좋습니다.
        // OnAllUnitsPlaced += HandleAllUnitsPlaced;
    }

    private void OnDisable()
    {
        // OnAllUnitsPlaced -= HandleAllUnitsPlaced;
    }

    // OnBattleStart 이벤트는 현재 구독자가 없으므로, 이 함수도 나중에 정리할 수 있습니다.
    private void HandleAllUnitsPlaced()
    {
        // RaiseBattleStart();
    }

    public void RaiseSetupGrid()
    {
        Debug.Log("<color=cyan>EVENT: OnSetupGrid 발생! 그리드 생성을 시작합니다.</color>");
        OnSetupGrid?.Invoke();
    }

    public void RaiseGridGenerationComplete()
    {
        Debug.Log("<color=cyan>EVENT: OnGridGenerationComplete 발생! 그리드 생성이 완료되었습니다.</color>");
        OnGridGenerationComplete?.Invoke();
    }

    public void RaiseAllUnitsPlaced()
    {
        Debug.Log("<color=cyan>EVENT: OnAllUnitsPlaced 발생! 모든 유닛 배치가 완료되었습니다.</color>");
        OnAllUnitsPlaced?.Invoke();
    }

    public void RaiseBattleStart()
    {
        Debug.Log("<color=red>EVENT: OnBattleStart 발생! 전투를 시작합니다!</color>");
        OnBattleStart?.Invoke();
    }

    public void RaiseActionPhaseStart()
    {
        Debug.Log("<color=yellow>EVENT: OnActionPhaseStart 발생! 액션 페이즈를 시작합니다.</color>");
        OnActionPhaseStart?.Invoke();
    }

    /// <summary>
    /// [신규] CardManager가 준비 완료 신호를 보낼 때 호출할 이벤트 발송 함수입니다.
    /// </summary>
    public void RaiseCardManagerReady()
    {
        // [수정] 로그 메시지를 이벤트의 역할에 맞게 명확하게 변경했습니다.
        Debug.Log("<color=green>EVENT: OnCardManagerReady 발생! GameManager의 턴 시작을 준비합니다.</color>");
        OnCardManagerReady?.Invoke();
    }
}
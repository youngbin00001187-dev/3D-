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
    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    private void OnEnable()
    {
        // '유닛 배치 완료' 신호를 받으면, '전투 시작' 신호를 보냅니다.
        OnAllUnitsPlaced += HandleAllUnitsPlaced;
    }

    private void OnDisable()
    {
        OnAllUnitsPlaced -= HandleAllUnitsPlaced;
    }

    private void HandleAllUnitsPlaced()
    {
        RaiseBattleStart();
    }

    // --- 외부에서 호출할 수 있는 '방송 버튼' 함수들 ---

    public void RaiseSetupGrid()
    {
        if (OnSetupGrid != null)
        {
            Debug.Log("<color=cyan>EVENT: OnSetupGrid 발생! 그리드 생성을 시작합니다.</color>");
            OnSetupGrid.Invoke();
        }
        else
        {
            Debug.LogWarning("--- BattleEventManager: OnSetupGrid 이벤트를 구독한 스크립트가 없습니다! ---");
        }
    }

    public void RaiseGridGenerationComplete()
    {
        if (OnGridGenerationComplete != null)
        {
            Debug.Log("<color=cyan>EVENT: OnGridGenerationComplete 발생! 그리드 생성이 완료되었습니다.</color>");
            OnGridGenerationComplete.Invoke();
        }
        else
        {
            Debug.LogWarning("--- BattleEventManager: OnGridGenerationComplete 이벤트를 구독한 스크립트가 없습니다! ---");
        }
    }

    public void RaiseAllUnitsPlaced()
    {
        if (OnAllUnitsPlaced != null)
        {
            Debug.Log("<color=cyan>EVENT: OnAllUnitsPlaced 발생! 모든 유닛 배치가 완료되었습니다.</color>");
            OnAllUnitsPlaced.Invoke();
        }
        else
        {
            Debug.LogWarning("--- BattleEventManager: OnAllUnitsPlaced 이벤트를 구독한 스크립트가 없습니다! ---");
        }
    }

    public void RaiseBattleStart()
    {
        if (OnBattleStart != null)
        {
            Debug.Log("<color=red>EVENT: OnBattleStart 발생! 전투를 시작합니다!</color>");
            OnBattleStart.Invoke();
        }
        else
        {
            Debug.LogWarning("--- BattleEventManager: OnBattleStart 이벤트를 구독한 스크립트가 없습니다! ---");
        }
    }
    public void RaiseActionPhaseStart()
    {
        if (OnActionPhaseStart != null)
        {
            Debug.Log("<color=yellow>EVENT: OnActionPhaseStart 발생! 액션 페이즈를 시작합니다.</color>");
            OnActionPhaseStart.Invoke();
        }
        else
        {
            Debug.LogWarning("--- BattleEventManager: OnActionPhaseStart 이벤트를 구독한 스크립트가 없습니다! ---");
        }
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
}

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
        // '���� ��ġ �Ϸ�' ��ȣ�� ������, '���� ����' ��ȣ�� �����ϴ�.
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

    // --- �ܺο��� ȣ���� �� �ִ� '��� ��ư' �Լ��� ---

    public void RaiseSetupGrid()
    {
        if (OnSetupGrid != null)
        {
            Debug.Log("<color=cyan>EVENT: OnSetupGrid �߻�! �׸��� ������ �����մϴ�.</color>");
            OnSetupGrid.Invoke();
        }
        else
        {
            Debug.LogWarning("--- BattleEventManager: OnSetupGrid �̺�Ʈ�� ������ ��ũ��Ʈ�� �����ϴ�! ---");
        }
    }

    public void RaiseGridGenerationComplete()
    {
        if (OnGridGenerationComplete != null)
        {
            Debug.Log("<color=cyan>EVENT: OnGridGenerationComplete �߻�! �׸��� ������ �Ϸ�Ǿ����ϴ�.</color>");
            OnGridGenerationComplete.Invoke();
        }
        else
        {
            Debug.LogWarning("--- BattleEventManager: OnGridGenerationComplete �̺�Ʈ�� ������ ��ũ��Ʈ�� �����ϴ�! ---");
        }
    }

    public void RaiseAllUnitsPlaced()
    {
        if (OnAllUnitsPlaced != null)
        {
            Debug.Log("<color=cyan>EVENT: OnAllUnitsPlaced �߻�! ��� ���� ��ġ�� �Ϸ�Ǿ����ϴ�.</color>");
            OnAllUnitsPlaced.Invoke();
        }
        else
        {
            Debug.LogWarning("--- BattleEventManager: OnAllUnitsPlaced �̺�Ʈ�� ������ ��ũ��Ʈ�� �����ϴ�! ---");
        }
    }

    public void RaiseBattleStart()
    {
        if (OnBattleStart != null)
        {
            Debug.Log("<color=red>EVENT: OnBattleStart �߻�! ������ �����մϴ�!</color>");
            OnBattleStart.Invoke();
        }
        else
        {
            Debug.LogWarning("--- BattleEventManager: OnBattleStart �̺�Ʈ�� ������ ��ũ��Ʈ�� �����ϴ�! ---");
        }
    }
    public void RaiseActionPhaseStart()
    {
        if (OnActionPhaseStart != null)
        {
            Debug.Log("<color=yellow>EVENT: OnActionPhaseStart �߻�! �׼� ����� �����մϴ�.</color>");
            OnActionPhaseStart.Invoke();
        }
        else
        {
            Debug.LogWarning("--- BattleEventManager: OnActionPhaseStart �̺�Ʈ�� ������ ��ũ��Ʈ�� �����ϴ�! ---");
        }
    }
    // ��������������������
}

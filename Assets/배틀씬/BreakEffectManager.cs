using UnityEngine;

/// <summary>
/// '브레이크 타임'(인터럽트) 동안 발동될 특수 효과를 관리하는 싱글턴 매니저입니다.
/// </summary>
public class BreakEffectManager : MonoBehaviour
{
    public static BreakEffectManager Instance { get; private set; }

    private SandevistanGhostSpawner _ghostSpawner;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.OnAllUnitsPlaced += FindAndAssignSpawner;
        }
    }

    private void OnDisable()
    {
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.OnAllUnitsPlaced -= FindAndAssignSpawner;
        }
    }

    /// <summary>
    /// 모든 유닛이 배치된 후, 씬에 있는 SandevistanGhostSpawner를 찾아 할당합니다.
    /// </summary>
    private void FindAndAssignSpawner()
    {
        // [수정] Unity 최신 버전에 권장되는 함수로 변경합니다.
        _ghostSpawner = FindFirstObjectByType<SandevistanGhostSpawner>();

        if (_ghostSpawner != null)
        {
            Debug.Log("[BreakEffectManager] 씬에 있는 SandevistanGhostSpawner를 성공적으로 찾아서 연결했습니다.");
        }
        else
        {
            Debug.LogWarning("[BreakEffectManager] 씬에서 SandevistanGhostSpawner를 찾지 못했습니다!");
        }
    }

    public void StartBreakEffect()
    {
        Debug.Log("<color=#FF00FF>--- Break Effect ON ---</color>");
        Time.timeScale = 0.5f;

        if (_ghostSpawner != null)
        {
            _ghostSpawner.StartSpawning();
        }
    }

    public void StopBreakEffect()
    {
        Debug.Log("<color=#FF00FF>--- Break Effect OFF ---</color>");
        Time.timeScale = 1.0f;

        if (_ghostSpawner != null)
        {
            _ghostSpawner.StopSpawning();
        }
    }
}
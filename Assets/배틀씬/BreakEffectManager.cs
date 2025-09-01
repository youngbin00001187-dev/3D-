using UnityEngine;
using System.Collections; // 코루틴을 위해 추가

/// <summary>
/// '브레이크 타임'(인터럽트) 동안 발동될 특수 효과를 관리하는 싱글턴 매니저입니다.
/// </summary>
public class BreakEffectManager : MonoBehaviour
{
    public static BreakEffectManager Instance { get; private set; }

    [Header("효과 설정")]
    [Tooltip("브레이크 효과가 발동되었을 때 적용될 시간 배율입니다.")]
    [SerializeField] private float timeScaleOnBreak = 0.5f;

    // ▼▼▼ 여기에 잔존 시간 변수를 다시 가져왔습니다 ▼▼▼
    [Tooltip("효과 종료 신호 후, 시간 느리게 효과가 추가로 유지되는 시간입니다.")]
    [SerializeField] private float timeEffectLingerDuration = 0.3f;

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

    private void FindAndAssignSpawner()
    {
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
        Time.timeScale = timeScaleOnBreak;

        if (_ghostSpawner != null)
        {
            _ghostSpawner.StartSpawning();
        }
    }

    // ▼▼▼ Stop 메서드를 코루틴으로 변경하고 이름을 명확하게 바꿨습니다 ▼▼▼
    /// <summary>
    /// 브레이크 효과의 종료 시퀀스를 시작합니다. 
    /// 일부 효과는 즉시, 일부 효과는 지연되어 종료됩니다.
    /// </summary>
    public IEnumerator StopBreakEffectSequence()
    {
        Debug.Log("<color=#FF00FF>--- Break Effect OFF Sequence Start ---</color>");

        // 1. 잔상 효과처럼 즉시 꺼져야 하는 효과들을 먼저 종료합니다.
        if (_ghostSpawner != null)
        {
            _ghostSpawner.StopSpawning();
            Debug.Log("[BreakEffectManager] 잔상 효과 즉시 중지.");
        }

        // 2. 잔존 시간만큼 기다립니다.
        if (timeEffectLingerDuration > 0)
        {
            // Time.timeScale에 영향을 받지 않도록 WaitForSecondsRealtime을 사용합니다.
            yield return new WaitForSecondsRealtime(timeEffectLingerDuration);
        }

        // 3. 시간 느리게 효과처럼 지연되어 꺼져야 하는 효과들을 마지막에 종료합니다.
        Time.timeScale = 1.0f;
        Debug.Log("[BreakEffectManager] 시간 효과 중지. 정상 속도로 복귀.");
    }
}
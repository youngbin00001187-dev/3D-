using UnityEngine;

/// <summary>
/// '�극��ũ Ÿ��'(���ͷ�Ʈ) ���� �ߵ��� Ư�� ȿ���� �����ϴ� �̱��� �Ŵ����Դϴ�.
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
    /// ��� ������ ��ġ�� ��, ���� �ִ� SandevistanGhostSpawner�� ã�� �Ҵ��մϴ�.
    /// </summary>
    private void FindAndAssignSpawner()
    {
        // [����] Unity �ֽ� ������ ����Ǵ� �Լ��� �����մϴ�.
        _ghostSpawner = FindFirstObjectByType<SandevistanGhostSpawner>();

        if (_ghostSpawner != null)
        {
            Debug.Log("[BreakEffectManager] ���� �ִ� SandevistanGhostSpawner�� ���������� ã�Ƽ� �����߽��ϴ�.");
        }
        else
        {
            Debug.LogWarning("[BreakEffectManager] ������ SandevistanGhostSpawner�� ã�� ���߽��ϴ�!");
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
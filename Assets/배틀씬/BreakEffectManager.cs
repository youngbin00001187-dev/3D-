using UnityEngine;
using System.Collections; // �ڷ�ƾ�� ���� �߰�

/// <summary>
/// '�극��ũ Ÿ��'(���ͷ�Ʈ) ���� �ߵ��� Ư�� ȿ���� �����ϴ� �̱��� �Ŵ����Դϴ�.
/// </summary>
public class BreakEffectManager : MonoBehaviour
{
    public static BreakEffectManager Instance { get; private set; }

    [Header("ȿ�� ����")]
    [Tooltip("�극��ũ ȿ���� �ߵ��Ǿ��� �� ����� �ð� �����Դϴ�.")]
    [SerializeField] private float timeScaleOnBreak = 0.5f;

    // ���� ���⿡ ���� �ð� ������ �ٽ� �����Խ��ϴ� ����
    [Tooltip("ȿ�� ���� ��ȣ ��, �ð� ������ ȿ���� �߰��� �����Ǵ� �ð��Դϴ�.")]
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
        Time.timeScale = timeScaleOnBreak;

        if (_ghostSpawner != null)
        {
            _ghostSpawner.StartSpawning();
        }
    }

    // ���� Stop �޼��带 �ڷ�ƾ���� �����ϰ� �̸��� ��Ȯ�ϰ� �ٲ���ϴ� ����
    /// <summary>
    /// �극��ũ ȿ���� ���� �������� �����մϴ�. 
    /// �Ϻ� ȿ���� ���, �Ϻ� ȿ���� �����Ǿ� ����˴ϴ�.
    /// </summary>
    public IEnumerator StopBreakEffectSequence()
    {
        Debug.Log("<color=#FF00FF>--- Break Effect OFF Sequence Start ---</color>");

        // 1. �ܻ� ȿ��ó�� ��� ������ �ϴ� ȿ������ ���� �����մϴ�.
        if (_ghostSpawner != null)
        {
            _ghostSpawner.StopSpawning();
            Debug.Log("[BreakEffectManager] �ܻ� ȿ�� ��� ����.");
        }

        // 2. ���� �ð���ŭ ��ٸ��ϴ�.
        if (timeEffectLingerDuration > 0)
        {
            // Time.timeScale�� ������ ���� �ʵ��� WaitForSecondsRealtime�� ����մϴ�.
            yield return new WaitForSecondsRealtime(timeEffectLingerDuration);
        }

        // 3. �ð� ������ ȿ��ó�� �����Ǿ� ������ �ϴ� ȿ������ �������� �����մϴ�.
        Time.timeScale = 1.0f;
        Debug.Log("[BreakEffectManager] �ð� ȿ�� ����. ���� �ӵ��� ����.");
    }
}
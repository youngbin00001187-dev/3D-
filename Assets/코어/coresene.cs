using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // �� ��ȯ ����� ����ϱ� ���� �� ���� �߰��մϴ�.

/// <summary>
/// CoreScene�� UI�� �帧�� �����մϴ�.
/// ���� ��带 �����ϰ� ���� ������ ���� ��ȯ�ϴ� ������ �մϴ�.
/// </summary>
public class CoreSceneManager : MonoBehaviour
{
    [Header("UI ����")]
    [Tooltip("���� ������ ��ȯ�ϴ� ��ư�Դϴ�.")]
    public Button startBattleButton;

    void Start()
    {
        if (startBattleButton != null)
        {
            startBattleButton.onClick.AddListener(StartBattleMode);
        }
        else
        {
            Debug.LogWarning("[CoreSceneManager] ���� ���� ��ư�� ������� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// ���� ��带 �����ϰ�, BattleScene���� ���� ��ȯ�մϴ�.
    /// </summary>
    public void StartBattleMode()
    {
        // 1. GlobalManager�� �ִ��� Ȯ���ϰ�, ���� ��带 �����մϴ�.
        if (GlobalManager.instance != null)
        {
            Debug.Log("���� ��� ������ �غ��մϴ�.");
        }
        else
        {
            Debug.LogError("[CoreSceneManager] GlobalManager�� ã�� �� �����ϴ�! CoreScene�� GlobalManager�� �ִ��� Ȯ�����ּ���.");
            return;
        }

        // 2. SceneManager�� �̿��� "BattleScene"�� ���� �ε��մϴ�.
        Debug.Log("BattleScene���� ���� ��ȯ�մϴ�.");
        SceneManager.LoadScene("BattleScene");
    }
}
using UnityEngine;

/// <summary>
/// UnitController�� ��ӹ޴� �÷��̾� ���� Ŭ�����Դϴ�.
/// </summary>
public class PlayerController : UnitController
{
    [Header("�극��ũ �ý���")]
    [Tooltip("�ϴ� �ִ� �극��ũ Ƚ���Դϴ�.")]
    public int maxBreaksPerTurn = 1;
    private int _currentBreaksLeft;

    // --- [�ű�] �극��ũ ���� �Լ��� ---

    /// <summary>
    /// ���� ���� �극��ũ Ƚ���� ��ȯ�մϴ�.
    /// </summary>
    public bool HasBreaksLeft()
    {
        return _currentBreaksLeft > 0;
    }

    /// <summary>
    /// �극��ũ Ƚ���� 1 �Ҹ��մϴ�.
    /// </summary>
    public void UseBreak()
    {
        if (HasBreaksLeft())
        {
            _currentBreaksLeft--;
            Debug.Log($"<color=purple>[BREAK] �극��ũ ���! ���� Ƚ��: {_currentBreaksLeft}</color>");
            // TODO: ���⿡ �극��ũ ��� UI ���� ������ ������ �� �ֽ��ϴ�.
        }
    }

    /// <summary>
    /// �� ���� �� �극��ũ Ƚ���� �ִ�� �ʱ�ȭ�մϴ�.
    /// </summary>
    public void ResetBreaks()
    {
        _currentBreaksLeft = maxBreaksPerTurn;
    }
}
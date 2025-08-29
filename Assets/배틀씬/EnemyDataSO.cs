using UnityEngine;
using System.Collections.Generic;

// ���� AI Ÿ���� ����� �����մϴ�.
public enum E_MoveType
{
    Fixed,       // ������ ���� ������ ������ Ÿ��
    ChasePlayer  // �÷��̾�� ���� ����� Ÿ��
}

// [System.Serializable]�� ���� �ν����� â���� ������ �� �ֽ��ϴ�.
[System.Serializable]
public class EnemyAction
{
    [Tooltip("�ν����Ϳ��� �˾ƺ��� ������ �ൿ�� �̸��� �����ݴϴ�.")]
    public string actionName;

    [Tooltip("�� �ൿ�� � ������� Ÿ���� ������ �����մϴ�.")]
    public E_MoveType movementType;

    [Tooltip("�� �ൿ�� ������ ����� ī��(GameAction)�Դϴ�.")]
    public CardDataSO referenceCard;
}

// ScriptableObject�� �����ϱ� ���� �޴� ��θ� �����մϴ�.
[CreateAssetMenu(fileName = "New Enemy Data", menuName = "3DProject/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [Header("�⺻ ����")]
    [Tooltip("���� �̸��Դϴ�.")]
    public string enemyName;
    [Tooltip("������ ������ ���� �������Դϴ�.")]
    public GameObject enemyPrefab;
    [Tooltip("���� �ִ� ü���Դϴ�.")]
    public int maxHealth;

    // ���� ���⿡ actionsPerTurn ������ �߰��߽��ϴ� ����
    [Header("���� ��Ģ")]
    [Tooltip("�� ���� �� �Ͽ� �ൿ�� �� �ִ� Ƚ���Դϴ�.")]
    public int actionsPerTurn = 1;
    // �������������������������������������

    [Header("�ൿ ����")]
    [Tooltip("�� ���� ������� ����� �ൿ ����Դϴ�.")]
    public List<EnemyAction> actionPattern;
}
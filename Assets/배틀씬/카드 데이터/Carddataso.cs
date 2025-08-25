using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Card", menuName = "3DProject/Card Data")]
public class CardDataSO : ScriptableObject
{
    [Header("ī�� �⺻ ����")]
    public string cardID;
    public string cardName;
    public Sprite cardImage;
    [TextArea(3, 5)]
    public string description;

    [Header("�ǵ� ǥ�� ���� (3D)")]
    [Tooltip("�� ī�带 ���� ����ϰų� �÷��̾ ȣ���� �� ǥ�õ� ���� �����Դϴ�.")]
    public List<Vector3Int> intentPredictionRange = new List<Vector3Int>();

    // �ɹ� ���� �ʵ�� �����մϴ�.

    [Header("ī�� �׼� ������")]
    [SerializeReference]
    public List<GameAction> actionSequence = new List<GameAction>();

    // ���� ���⿡ ContextMenu ��� �߰� ����
    [ContextMenu("�׼� ������/Move Action �߰�")]
    private void AddMoveAction()
    {
        actionSequence.Add(new MoveAction());
    }

    [ContextMenu("�׼� ������/Attack Action �߰�")]
    private void AddAttackAction()
    {
        actionSequence.Add(new AttackAction());
    }
    // �����������������������
}
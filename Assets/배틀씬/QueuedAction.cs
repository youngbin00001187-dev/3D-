using UnityEngine;

public struct QueuedAction
{
    public UnitController User;
    public CardDataSO SourceCard;
    public GameObject TargetTile; // �÷��̾��� ���� ��ǥ ������ ��� ���
    public Vector3Int? RelativeVector; // [�ű�] ���� ��� ��ǥ ������ ���� �߰�
}
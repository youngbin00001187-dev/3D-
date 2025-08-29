using UnityEngine;
using System;
using System.Collections;

[System.Serializable]
public class StunEffect : GameEffect
{
    [Header("���� ����")]
    [Tooltip("��뿡�� ������ ���� ���� �� ���Դϴ�.")]
    public int stunDuration = 1;

    public override IEnumerator Apply(UnitController user, GameObject targetTile, Action onComplete)
    {
        Tile3D targetTile3D = targetTile.GetComponent<Tile3D>();
        if (targetTile3D != null)
        {
            UnitController targetUnit = GridManager3D.instance.GetUnitAtPosition(targetTile3D.gridPosition);
            if (targetUnit != null)
            {
                // Ÿ�� ���ֿ��� ������ �����մϴ�.
                targetUnit.ApplyStun(stunDuration);
            }
        }

        // ���� ȿ���� ��� ����ǹǷ�, �ٷ� �Ϸ� �ݹ��� ȣ���մϴ�.
        onComplete?.Invoke();
        yield break;
    }
}
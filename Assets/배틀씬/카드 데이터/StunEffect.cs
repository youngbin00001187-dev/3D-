using UnityEngine;
using System;
using System.Collections;

[System.Serializable]
public class StunEffect : GameEffect
{
    [Header("스턴 설정")]
    [Tooltip("상대에게 적용할 스턴 지속 턴 수입니다.")]
    public int stunDuration = 1;

    public override IEnumerator Apply(UnitController user, GameObject targetTile, Action onComplete)
    {
        Tile3D targetTile3D = targetTile.GetComponent<Tile3D>();
        if (targetTile3D != null)
        {
            UnitController targetUnit = GridManager3D.instance.GetUnitAtPosition(targetTile3D.gridPosition);
            if (targetUnit != null)
            {
                // 타겟 유닛에게 스턴을 적용합니다.
                targetUnit.ApplyStun(stunDuration);
            }
        }

        // 스턴 효과는 즉시 적용되므로, 바로 완료 콜백을 호출합니다.
        onComplete?.Invoke();
        yield break;
    }
}
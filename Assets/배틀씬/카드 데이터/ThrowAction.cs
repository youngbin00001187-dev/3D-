using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ThrowAction : GameAction
{
    [Header("Ÿ���� ����")]
    public List<Vector3Int> attackRange = new List<Vector3Int>();

    [Header("�߻� ���� ���� (AOE)")]
    [Tooltip("Ŭ���� Ÿ���� ��������, ����ü�� �߻�� ��� ��ġ ����Դϴ�.")]
    public List<Vector3Int> areaOfEffect = new List<Vector3Int> { Vector3Int.zero };

    [Header("����ü ����")]
    public int projectileId = 0;
    public int motionID = 3;
    [Tooltip("���� �߻� �� �� ����ü ������ �ð� �����Դϴ�.")]
    public float launchDelay = 0.1f; // ���� �� �� ���� ����

    // GetTargetableTiles�� GetActionImpactTiles �޼���� ������ �ʿ䰡 �����ϴ�.
    // �÷��̾�� ������ ���� Ÿ���� �����ϰ�, �� Ÿ���� �������� �߻� ������ �����Ǳ� �����Դϴ�.
    public override List<GameObject> GetTargetableTiles(UnitController user)
    {
        List<GameObject> tiles = new List<GameObject>();
        Vector3Int userPos = user.GetGridPosition();
        foreach (var vector in attackRange)
        {
            GameObject tile = GridManager3D.instance.GetTileAtPosition(userPos + vector);
            if (tile != null) tiles.Add(tile);
        }
        return tiles;
    }

    public override List<GameObject> GetActionImpactTiles(UnitController user, GameObject targetTile)
    {
        List<GameObject> tiles = new List<GameObject>();
        if (targetTile == null) return tiles;
        Tile3D targetTileComponent = targetTile.GetComponent<Tile3D>();
        if (targetTileComponent == null) return tiles;
        Vector3Int originPos = targetTileComponent.gridPosition;
        foreach (var vector in areaOfEffect)
        {
            Vector3Int hitPos = originPos + vector;
            GameObject hitPosTile = GridManager3D.instance.GetTileAtPosition(hitPos);
            if (hitPosTile != null) tiles.Add(hitPosTile);
        }
        return tiles;
    }

    /// <summary>
    /// ���� �׼� ���� ���� (���� �߻� �� ��� ����ü ��� ������� ����)
    /// </summary>
    protected override IEnumerator InternalExecute()
    {
        // 1. �ִϸ��̼� ���
        Animator animator = actionUser.UnitAnimator;
        if (animator != null)
        {
            animator.SetInteger("motionID", this.motionID);
        }
        // �ִϸ��̼��� ���� ª�� ���
        yield return new WaitForSeconds(0.1f);

        if (ProjectileManager.Instance != null)
        {
            // 2. �߻��� ���� ��ǥ Ÿ�� ��� ���
            List<GameObject> finalTargetTiles = GetActionImpactTiles(actionUser, actionTargetTile);

            if (finalTargetTiles.Count == 0)
            {
                Debug.LogWarning("[ThrowAction] �߻��� ��ȿ�� Ÿ���� �����ϴ�.");
                yield break;
            }

            // 3. ��� ����ü�� �۾��� ���ƴ��� �����ϱ� ���� 'ī����' ����
            int projectilesRemaining = finalTargetTiles.Count;
            Action onCompleteCallback = () => { projectilesRemaining--; };

            Debug.Log($"<color=yellow>[ThrowAction] {projectilesRemaining}���� ����ü ���� �߻縦 �����մϴ�.</color>");

            // 4. ���� ��� Ÿ���� ���� ���������� ����ü �߻�
            foreach (var tile in finalTargetTiles)
            {
                ProjectileManager.Instance.LaunchProjectile(actionUser, tile, projectileId, onCompleteCallback);
                if (launchDelay > 0)
                {
                    yield return new WaitForSeconds(launchDelay); // �� �߻� ������ ������
                }
            }

            // 5. ī���Ͱ� 0�� �� ������ (��� ����ü�� �Ϸ� ��ȣ�� ���� ������) ���
            yield return new WaitUntil(() => projectilesRemaining <= 0);

            Debug.Log("<color=green>[ThrowAction] ��� ����ü�� �۾� �Ϸ� ��ȣ ����! �׼��� ������ �����մϴ�.</color>");
        }
        else
        {
            Debug.LogError("[ThrowAction] ProjectileManager.Instance�� ã�� �� �����ϴ�!");
        }
    }
}
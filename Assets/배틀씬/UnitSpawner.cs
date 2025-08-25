using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// BattleEventManager�� ��ȣ�� �޾� BattleInitializer�� �����͸� �����Ͽ�,
/// �÷��̾�� �� ������ �ʿ� ��ġ�ϰ� ���� �Ŵ����� ������ �����ϴ� ������ �մϴ�.
/// </summary>
public class UnitSpawner : MonoBehaviour
{
    [Header("������ ����")]
    [Tooltip("�÷��̾�� ����� �������Դϴ�.")]
    public GameObject playerPrefab;

    // ���� ���⿡ CardManager ������ �߰��߽��ϴ� ����
    [Header("�Ŵ��� ����")]
    [Tooltip("�÷��̾� ������ ������ CardManager�Դϴ�.")]
    public CardManager cardManager;
    // ���������������������������

    [Header("��ġ ����")]
    [Tooltip("�÷��̾ ������ �׸��� ��ǥ�Դϴ�.")]
    public Vector3Int playerStartPosition = new Vector3Int(4, 0, 0);
    [Tooltip("������ ������ �׸��� ��ǥ ����Դϴ�.")]
    public List<Vector3Int> enemyStartPositions;

    [Header("���� �ִϸ��̼� ����")]
    [Tooltip("������ �󸶳� ���� ������ �������� �����մϴ�.")]
    public float spawnHeightOffset = 5.0f;
    [Tooltip("�������� �� �ɸ��� �ð�(��)�Դϴ�.")]
    public float dropDuration = 0.5f;

    private void OnEnable()
    {
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.OnGridGenerationComplete += OnGridReady;
        }
    }

    private void OnDisable()
    {
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.OnGridGenerationComplete -= OnGridReady;
        }
    }

    private void OnGridReady()
    {
        EncounterSO encounter = BattleInitializer.instance.currentEncounter;
        if (encounter == null) return;

        SpawnPlayer();
        SpawnEnemies(encounter.enemies);

        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.RaiseAllUnitsPlaced();
        }
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null) return;

        GameObject tile = GridManager3D.instance.GetTileAtPosition(playerStartPosition);
        if (tile != null)
        {
            Vector3 endPosition = tile.transform.position;
            Vector3 startPosition = endPosition + new Vector3(0, spawnHeightOffset, 0);

            GameObject newPlayerObject = Instantiate(playerPrefab, startPosition, Quaternion.identity);

            PlayerController playerController = newPlayerObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.currentTile = tile;
                GridManager3D.instance.RegisterUnitPosition(playerController, playerStartPosition);

                // ���� �� �κ��� �ٽ��Դϴ� ����
                // 1. CardManager�� ����Ǿ� �ִ��� Ȯ���մϴ�.
                if (cardManager != null)
                {
                    // 2. CardManager�� playerController ������, ��� ������ �÷��̾ �������ݴϴ�.
                    cardManager.playerController = playerController;
                    Debug.Log($"<color=green>CardManager�� ������ �÷��̾�({playerController.name})�� �����߽��ϴ�.</color>");
                }
                else
                {
                    Debug.LogWarning("[UnitSpawner] CardManager�� ������� �ʾ� �÷��̾� ������ ������ �� �����ϴ�.");
                }
                // �������������������
            }

            newPlayerObject.transform.DOMove(endPosition, dropDuration).SetEase(Ease.OutBounce);
        }
    }

    private void SpawnEnemies(List<EnemyDataSO> enemiesToSpawn)
    {
        for (int i = 0; i < enemiesToSpawn.Count; i++)
        {
            if (i >= enemyStartPositions.Count) break;

            GameObject enemyPrefab = enemiesToSpawn[i].enemyPrefab;
            Vector3Int spawnPos = enemyStartPositions[i];
            GameObject tile = GridManager3D.instance.GetTileAtPosition(spawnPos);

            if (enemyPrefab != null && tile != null)
            {
                Vector3 endPosition = tile.transform.position;
                Vector3 startPosition = endPosition + new Vector3(0, spawnHeightOffset, 0);

                GameObject newEnemyObject = Instantiate(enemyPrefab, startPosition, Quaternion.identity);

                EnemyController enemyController = newEnemyObject.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.currentTile = tile;
                    GridManager3D.instance.RegisterUnitPosition(enemyController, spawnPos);
                }

                newEnemyObject.transform.DOMove(endPosition, dropDuration).SetEase(Ease.OutBounce);
            }
        }
    }
}
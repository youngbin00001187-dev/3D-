using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// BattleEventManager의 신호를 받아 BattleInitializer의 데이터를 참조하여,
/// 플레이어와 적 유닛을 맵에 배치하고 관련 매니저에 정보를 전달하는 역할을 합니다.
/// </summary>
public class UnitSpawner : MonoBehaviour
{
    [Header("프리펩 설정")]
    [Tooltip("플레이어로 사용할 프리펩입니다.")]
    public GameObject playerPrefab;

    // ▼▼▼ 여기에 CardManager 참조를 추가했습니다 ▼▼▼
    [Header("매니저 연결")]
    [Tooltip("플레이어 정보를 전달할 CardManager입니다.")]
    public CardManager cardManager;
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    [Header("위치 설정")]
    [Tooltip("플레이어가 생성될 그리드 좌표입니다.")]
    public Vector3Int playerStartPosition = new Vector3Int(4, 0, 0);
    [Tooltip("적들이 생성될 그리드 좌표 목록입니다.")]
    public List<Vector3Int> enemyStartPositions;

    [Header("등장 애니메이션 설정")]
    [Tooltip("유닛이 얼마나 높은 곳에서 떨어질지 결정합니다.")]
    public float spawnHeightOffset = 5.0f;
    [Tooltip("떨어지는 데 걸리는 시간(초)입니다.")]
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

                // ▼▼▼ 이 부분이 핵심입니다 ▼▼▼
                // 1. CardManager가 연결되어 있는지 확인합니다.
                if (cardManager != null)
                {
                    // 2. CardManager의 playerController 변수에, 방금 생성한 플레이어를 연결해줍니다.
                    cardManager.playerController = playerController;
                    Debug.Log($"<color=green>CardManager에 생성된 플레이어({playerController.name})를 연결했습니다.</color>");
                }
                else
                {
                    Debug.LogWarning("[UnitSpawner] CardManager가 연결되지 않아 플레이어 정보를 전달할 수 없습니다.");
                }
                // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
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
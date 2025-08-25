using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager3D : MonoBehaviour
{
    public static GridManager3D instance;

    [Header("맵 크기 설정")]
    public int gridWidth = 8;
    public int gridHeight = 5;

    [Header("인공 원근감 및 위치 설정")]
    public float perspectiveFactor = 0.15f;
    public float verticalSpacing = 0.7f;
    public float horizontalSpacing = 1.0f;
    public float shearFactor = -0.5f;

    [Header("오브젝트 설정")]
    public GameObject tilePrefab;

    [Header("펼침 효과 설정")]
    public float tileRevealDelay = 0.01f;
    public float tileAnimationDuration = 0.2f;

    private Dictionary<Vector3Int, GameObject> grid = new Dictionary<Vector3Int, GameObject>();
    // ▼▼▼ 이전에 주석 처리했던 유닛 관리 기능을 다시 활성화했습니다. ▼▼▼
    private Dictionary<Vector3Int, UnitController> unitsOnGrid = new Dictionary<Vector3Int, UnitController>();

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    private void OnEnable()
    {
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.OnSetupGrid += StartGridGeneration;
        }
    }

    private void OnDisable()
    {
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.OnSetupGrid -= StartGridGeneration;
        }
    }

    public void StartGridGeneration()
    {
        StartCoroutine(GenerateGridCoroutine());
    }

    public IEnumerator GenerateGridCoroutine()
    {
        if (tilePrefab == null) yield break;

        foreach (Transform child in transform) { Destroy(child.gameObject); }
        grid.Clear();
        unitsOnGrid.Clear(); // 유닛 딕셔너리도 함께 초기화합니다.

        float currentZOffset = 0;

        for (int z = 0; z < gridHeight; z++)
        {
            float scale = 1f + (z * perspectiveFactor);
            float currentTileWidth = tilePrefab.transform.localScale.x * scale;
            float rowWidth = gridWidth * (currentTileWidth * horizontalSpacing);
            float startX = -rowWidth / 2f + (currentTileWidth * horizontalSpacing) / 2f;

            float currentTileDepth = tilePrefab.transform.localScale.z * scale;
            float currentSpacing = currentTileDepth * verticalSpacing;

            for (int x = 0; x < gridWidth; x++)
            {
                GameObject newTile = Instantiate(tilePrefab, transform);
                Vector3 finalScale = tilePrefab.transform.localScale * scale;
                newTile.transform.localScale = Vector3.zero;

                float shearOffset = (currentZOffset + (currentSpacing / 2f)) * shearFactor;
                float posX = startX + (x * (currentTileWidth * horizontalSpacing)) + shearOffset;
                float posZ = currentZOffset;

                newTile.transform.localPosition = new Vector3(posX, 0, posZ);

                newTile.name = $"Tile_{x}_{z}";
                Vector3Int gridPos = new Vector3Int(x, 0, z);

                Tile3D tileComponent = newTile.GetComponent<Tile3D>();
                if (tileComponent != null) { tileComponent.gridPosition = gridPos; }

                grid[gridPos] = newTile;

                StartCoroutine(AnimateTileAppearance(newTile, finalScale));
                yield return new WaitForSeconds(tileRevealDelay);
            }

            currentZOffset += currentSpacing;
        }

        // ▼▼▼ 모든 타일 생성이 시작된 후, 마지막 애니메이션이 끝날 때까지 잠시 기다립니다. ▼▼▼
        yield return new WaitForSeconds(tileAnimationDuration);

        // ▼▼▼ 그리드 생성이 완전히 끝났다고 BattleEventManager에게 알립니다. ▼▼▼
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.RaiseGridGenerationComplete();
        }
    }

    private IEnumerator AnimateTileAppearance(GameObject tile, Vector3 targetScale)
    {
        float timer = 0f;
        while (timer < tileAnimationDuration)
        {
            tile.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, timer / tileAnimationDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        tile.transform.localScale = targetScale;
        // ※※※ 여기 있던 이벤트 호출 코드를 삭제했습니다. ※※※
    }

    // --- 유닛 관리 기능 ---
    public void RegisterUnitPosition(UnitController unit, Vector3Int position)
    {
        if (unitsOnGrid.ContainsKey(position))
        {
            unitsOnGrid[position] = unit;
        }
        else
        {
            unitsOnGrid.Add(position, unit);
        }
    }

    public void UnregisterUnitPosition(UnitController unit, Vector3Int position)
    {
        if (unitsOnGrid.ContainsKey(position) && unitsOnGrid[position] == unit)
        {
            unitsOnGrid.Remove(position);
        }
    }

    public UnitController GetUnitAtPosition(Vector3Int position)
    {
        unitsOnGrid.TryGetValue(position, out UnitController unit);
        return unit;
    }

    public GameObject GetTileAtPosition(Vector3Int position)
    {
        grid.TryGetValue(position, out GameObject tile);
        return tile;
    }
}
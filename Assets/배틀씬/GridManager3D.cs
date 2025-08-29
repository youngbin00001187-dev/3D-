using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    // [수정] 한 타일에 여러 유닛이 있을 수 있도록 List로 변경
    private Dictionary<Vector3Int, List<UnitController>> unitsOnGrid = new Dictionary<Vector3Int, List<UnitController>>();

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
        unitsOnGrid.Clear();

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

        yield return new WaitForSeconds(tileAnimationDuration);

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
    }

    // --- 유닛 관리 기능 (List 지원하도록 수정) ---

    /// <summary>
    /// [수정] 특정 위치에 유닛을 '추가'합니다.
    /// </summary>
    public void RegisterUnitPosition(UnitController unit, Vector3Int position)
    {
        // 해당 위치에 리스트가 없으면 새로 생성
        if (!unitsOnGrid.ContainsKey(position))
        {
            unitsOnGrid[position] = new List<UnitController>();
        }
        // 리스트에 아직 해당 유닛이 없으면 추가
        if (!unitsOnGrid[position].Contains(unit))
        {
            unitsOnGrid[position].Add(unit);
        }
    }

    /// <summary>
    /// [수정] 특정 위치에서 유닛을 '제거'합니다.
    /// </summary>
    public void UnregisterUnitPosition(UnitController unit, Vector3Int position)
    {
        if (unitsOnGrid.ContainsKey(position))
        {
            unitsOnGrid[position].Remove(unit);
            // 유닛을 제거한 후 리스트가 비었으면, 딕셔너리에서 해당 키를 삭제
            if (unitsOnGrid[position].Count == 0)
            {
                unitsOnGrid.Remove(position);
            }
        }
    }

    /// <summary>
    /// [수정] 특정 위치의 '첫 번째' 유닛을 반환합니다. (기존 코드 호환용)
    /// </summary>
    public UnitController GetUnitAtPosition(Vector3Int position)
    {
        if (unitsOnGrid.ContainsKey(position) && unitsOnGrid[position].Count > 0)
        {
            return unitsOnGrid[position][0]; // 리스트의 첫 번째 유닛 반환
        }
        return null;
    }

    /// <summary>
    /// [신규] 특정 위치에 있는 '모든' 유닛의 리스트를 반환합니다.
    /// </summary>
    public List<UnitController> GetUnitsAtPosition(Vector3Int position)
    {
        if (unitsOnGrid.ContainsKey(position))
        {
            // 리스트의 복사본을 반환하여 외부에서의 수정을 방지
            return new List<UnitController>(unitsOnGrid[position]);
        }
        // 해당 위치에 유닛이 없으면 null 대신 빈 리스트를 반환
        return new List<UnitController>();
    }

    public GameObject GetTileAtPosition(Vector3Int position)
    {
        grid.TryGetValue(position, out GameObject tile);
        return tile;
    }
}
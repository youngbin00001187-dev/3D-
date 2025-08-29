using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GridManager3D : MonoBehaviour
{
    public static GridManager3D instance;

    [Header("�� ũ�� ����")]
    public int gridWidth = 8;
    public int gridHeight = 5;

    [Header("�ΰ� ���ٰ� �� ��ġ ����")]
    public float perspectiveFactor = 0.15f;
    public float verticalSpacing = 0.7f;
    public float horizontalSpacing = 1.0f;
    public float shearFactor = -0.5f;

    [Header("������Ʈ ����")]
    public GameObject tilePrefab;

    [Header("��ħ ȿ�� ����")]
    public float tileRevealDelay = 0.01f;
    public float tileAnimationDuration = 0.2f;

    private Dictionary<Vector3Int, GameObject> grid = new Dictionary<Vector3Int, GameObject>();

    // [����] �� Ÿ�Ͽ� ���� ������ ���� �� �ֵ��� List�� ����
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

    // --- ���� ���� ��� (List �����ϵ��� ����) ---

    /// <summary>
    /// [����] Ư�� ��ġ�� ������ '�߰�'�մϴ�.
    /// </summary>
    public void RegisterUnitPosition(UnitController unit, Vector3Int position)
    {
        // �ش� ��ġ�� ����Ʈ�� ������ ���� ����
        if (!unitsOnGrid.ContainsKey(position))
        {
            unitsOnGrid[position] = new List<UnitController>();
        }
        // ����Ʈ�� ���� �ش� ������ ������ �߰�
        if (!unitsOnGrid[position].Contains(unit))
        {
            unitsOnGrid[position].Add(unit);
        }
    }

    /// <summary>
    /// [����] Ư�� ��ġ���� ������ '����'�մϴ�.
    /// </summary>
    public void UnregisterUnitPosition(UnitController unit, Vector3Int position)
    {
        if (unitsOnGrid.ContainsKey(position))
        {
            unitsOnGrid[position].Remove(unit);
            // ������ ������ �� ����Ʈ�� �������, ��ųʸ����� �ش� Ű�� ����
            if (unitsOnGrid[position].Count == 0)
            {
                unitsOnGrid.Remove(position);
            }
        }
    }

    /// <summary>
    /// [����] Ư�� ��ġ�� 'ù ��°' ������ ��ȯ�մϴ�. (���� �ڵ� ȣȯ��)
    /// </summary>
    public UnitController GetUnitAtPosition(Vector3Int position)
    {
        if (unitsOnGrid.ContainsKey(position) && unitsOnGrid[position].Count > 0)
        {
            return unitsOnGrid[position][0]; // ����Ʈ�� ù ��° ���� ��ȯ
        }
        return null;
    }

    /// <summary>
    /// [�ű�] Ư�� ��ġ�� �ִ� '���' ������ ����Ʈ�� ��ȯ�մϴ�.
    /// </summary>
    public List<UnitController> GetUnitsAtPosition(Vector3Int position)
    {
        if (unitsOnGrid.ContainsKey(position))
        {
            // ����Ʈ�� ���纻�� ��ȯ�Ͽ� �ܺο����� ������ ����
            return new List<UnitController>(unitsOnGrid[position]);
        }
        // �ش� ��ġ�� ������ ������ null ��� �� ����Ʈ�� ��ȯ
        return new List<UnitController>();
    }

    public GameObject GetTileAtPosition(Vector3Int position)
    {
        grid.TryGetValue(position, out GameObject tile);
        return tile;
    }
}
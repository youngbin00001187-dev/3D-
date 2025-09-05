using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Ÿ�� ���̶���Ʈ�� ���������� �����մϴ�.
/// ���̶���Ʈ ��û�� �޾� �켱������ ���� ������ �����ϰ� ǥ���մϴ�.
/// </summary>
public class HighlightManager : MonoBehaviour
{
    public static HighlightManager instance;

    // ���̶���Ʈ�� ������ �켱������ �����ϴ� Enum�Դϴ�. (���� �������� �켱������ ����)
    public enum HighlightType
    {
        PlayerTarget,  // ���콺 ȣ�� (����׿�, �ֿ켱)
        PlayerPreview,      // �÷��̾��� ���� ���� ����
        EnemyIntent,         // ���� ���� ���� ����
        Hover
    }

    [System.Serializable]
    public struct HighlightColorMapping
    {
        public HighlightType type;
        public Color color;
    }

    [Header("���̶���Ʈ ���� ����")]
    [Tooltip("�� ���̶���Ʈ Ÿ�Կ� ����� ������ �����մϴ�.")]
    public List<HighlightColorMapping> highlightColorSettings;

    [Header("����� ����")]
    [Tooltip("Ÿ���� �ִ� ���̾ �����ؾ� �մϴ�.")]
    public LayerMask tileLayer;
    public Camera raycastCamera;
    private Tile3D _lastHoveredTile;
    private Dictionary<HighlightType, Color> highlightColorMap = new Dictionary<HighlightType, Color>();
    private Dictionary<Tile3D, List<HighlightType>> highlightedTiles = new Dictionary<Tile3D, List<HighlightType>>();
    private Dictionary<Tile3D, Color> originalTileColors = new Dictionary<Tile3D, Color>();

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }

        foreach (var mapping in highlightColorSettings)
        {
            highlightColorMap[mapping.type] = mapping.color;
        }
    }

    void Update()
    {
        // ī�޶� �ν����Ϳ� �Ҵ���� �ʾ����� ���� ������ ���� ������ �ߴ��մϴ�.
        if (raycastCamera == null)
        {
            return;
        }

        // �Ҵ�� ī�޶� �������� ����ĳ��Ʈ�� �����մϴ�.
        Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);
        Tile3D currentHoveredTile = null;

        if (Physics.Raycast(ray, out hit, 200f, tileLayer))
        {
            currentHoveredTile = hit.collider.GetComponent<Tile3D>();
        }

        if (currentHoveredTile != _lastHoveredTile)
        {
            if (_lastHoveredTile != null)
            {
                RemoveHighlight(new List<Tile3D> { _lastHoveredTile }, HighlightType.Hover);
            }

            if (currentHoveredTile != null)
            {
                AddHighlight(new List<Tile3D> { currentHoveredTile }, HighlightType.Hover);
            }

            _lastHoveredTile = currentHoveredTile;
        }
    }

    public void AddHighlight(List<Tile3D> tiles, HighlightType type)
    {
        foreach (var tile in tiles)
        {
            if (tile == null) continue;

            if (!highlightedTiles.ContainsKey(tile))
            {
                highlightedTiles[tile] = new List<HighlightType>();
                if (tile.MyMaterial != null)
                {
                    originalTileColors[tile] = tile.MyMaterial.color;
                }
            }

            if (!highlightedTiles[tile].Contains(type))
            {
                highlightedTiles[tile].Add(type);
            }

            UpdateTileColor(tile);
        }
    }

    public void RemoveHighlight(List<Tile3D> tiles, HighlightType type)
    {
        foreach (var tile in tiles)
        {
            if (tile == null || !highlightedTiles.ContainsKey(tile)) continue;

            highlightedTiles[tile].Remove(type);
            UpdateTileColor(tile);
        }
    }

    private void UpdateTileColor(Tile3D tile)
    {
        if (tile.MyMaterial == null) return;

        if (highlightedTiles.ContainsKey(tile) && highlightedTiles[tile].Count > 0)
        {
            HighlightType highestPriorityType = highlightedTiles[tile].Min();

            if (highlightColorMap.ContainsKey(highestPriorityType))
            {
                tile.MyMaterial.color = highlightColorMap[highestPriorityType];
            }
        }
        else
        {
            if (originalTileColors.ContainsKey(tile))
            {
                tile.MyMaterial.color = originalTileColors[tile];
            }
            if (highlightedTiles.ContainsKey(tile))
            {
                highlightedTiles.Remove(tile);
            }
            if (originalTileColors.ContainsKey(tile))
            {
                originalTileColors.Remove(tile);
            }
        }
    }

    /// <summary>
    /// Ư�� Ÿ���� ���̶���Ʈ�� ��� �����ϴ� �Լ��� �߰��մϴ�.
    /// </summary>
    public void ClearAllHighlightsOfType(HighlightType type)
    {
        List<Tile3D> tilesToRemove = new List<Tile3D>();

        // ���� ������ Ÿ���� ���ȭ�մϴ�.
        foreach (var pair in highlightedTiles)
        {
            if (pair.Value.Contains(type))
            {
                tilesToRemove.Add(pair.Key);
            }
        }

        // ���ȭ�� Ÿ���� ���̶���Ʈ�� �����մϴ�.
        foreach (var tile in tilesToRemove)
        {
            RemoveHighlight(new List<Tile3D> { tile }, type);
        }
    }
}
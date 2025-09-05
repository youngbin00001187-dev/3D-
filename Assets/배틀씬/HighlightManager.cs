using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 타일 하이라이트를 전문적으로 관리합니다.
/// 하이라이트 요청을 받아 우선순위에 따라 색상을 결정하고 표시합니다.
/// </summary>
public class HighlightManager : MonoBehaviour
{
    public static HighlightManager instance;

    // 하이라이트의 종류와 우선순위를 정의하는 Enum입니다. (위에 있을수록 우선순위가 높음)
    public enum HighlightType
    {
        PlayerTarget,  // 마우스 호버 (디버그용, 최우선)
        PlayerPreview,      // 플레이어의 예상 공격 범위
        EnemyIntent,         // 적의 공격 예고 범위
        Hover
    }

    [System.Serializable]
    public struct HighlightColorMapping
    {
        public HighlightType type;
        public Color color;
    }

    [Header("하이라이트 색상 설정")]
    [Tooltip("각 하이라이트 타입에 사용할 색상을 지정합니다.")]
    public List<HighlightColorMapping> highlightColorSettings;

    [Header("디버그 설정")]
    [Tooltip("타일이 있는 레이어를 지정해야 합니다.")]
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
        // 카메라가 인스펙터에 할당되지 않았으면 오류 방지를 위해 실행을 중단합니다.
        if (raycastCamera == null)
        {
            return;
        }

        // 할당된 카메라를 기준으로 레이캐스트를 실행합니다.
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
    /// 특정 타입의 하이라이트를 모두 제거하는 함수를 추가합니다.
    /// </summary>
    public void ClearAllHighlightsOfType(HighlightType type)
    {
        List<Tile3D> tilesToRemove = new List<Tile3D>();

        // 먼저 제거할 타일을 목록화합니다.
        foreach (var pair in highlightedTiles)
        {
            if (pair.Value.Contains(type))
            {
                tilesToRemove.Add(pair.Key);
            }
        }

        // 목록화된 타일의 하이라이트를 제거합니다.
        foreach (var tile in tilesToRemove)
        {
            RemoveHighlight(new List<Tile3D> { tile }, type);
        }
    }
}
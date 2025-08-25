using UnityEngine;

/// <summary>
/// 3D 타일이 자신의 그리드 좌표를 저장하고, 고유한 머티리얼 및 메시를 관리하기 위한 컴포넌트입니다.
/// </summary>
public class Tile3D : MonoBehaviour
{
    [Header("타일 정보")]
    public Vector3Int gridPosition;

    [Header("메시 변형")]
    [Tooltip("타일의 윗부분을 수평으로 밀어 평행사변형을 만듭니다. (Shear)")]
    [Range(-1f, 1f)]
    public float shearAmount = 0.0f;

    // 이 타일의 고유한 머티리얼 인스턴스를 저장하여 외부에 공개하는 프로퍼티입니다.
    public Material MyMaterial { get; private set; }

    private Renderer tileRenderer;
    private MeshFilter meshFilter;

    void Awake()
    {
        // --- 1. 고유 머티리얼 생성 ---
        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer == null)
        {
            Debug.LogError("Tile3D 오브젝트에 Renderer 컴포넌트가 없습니다!", this.gameObject);
            return;
        }
        // .material 프로퍼티에 접근하는 순간, 자동으로 이 오브젝트만을 위한 머티리얼 복사본이 생성됩니다.
        MyMaterial = tileRenderer.material;

        // --- 2. 평행사변형 메시 생성 ---
        // shearAmount 값이 0이 아닐 경우에만 메시를 변형시킵니다.
        if (shearAmount != 0)
        {
            DeformMesh();
        }
    }

    /// <summary>
    /// 이 타일의 메시를 평행사변형으로 변형시키는 함수입니다.
    /// </summary>
    private void DeformMesh()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("Tile3D 오브젝트에 MeshFilter 컴포넌트가 없습니다!", this.gameObject);
            return;
        }

        // 원본 메시를 수정하지 않도록, 이 오브젝트만의 고유한 메시 복사본을 생성합니다.
        Mesh mesh = meshFilter.mesh;

        // 메시의 모든 꼭짓점(vertex) 정보를 가져옵니다.
        Vector3[] vertices = mesh.vertices;

        // 모든 꼭짓점을 순회하며 Y값이 0보다 큰 (즉, 위쪽에 있는) 꼭짓점을 찾습니다.
        for (int i = 0; i < vertices.Length; i++)
        {
            // 납작한 큐브의 윗면은 Y값이 0.5 근처입니다.
            if (vertices[i].y > 0)
            {
                // 위쪽 꼭짓점의 X 위치를 shearAmount만큼 밀어줍니다.
                vertices[i].x += shearAmount;
            }
        }

        // 수정된 꼭짓점 정보를 다시 메시에 적용합니다.
        mesh.vertices = vertices;

        // 변경된 메시에 맞춰 경계선을 재계산하도록 합니다.
        mesh.RecalculateBounds();

        // MeshCollider가 있다면 그것도 업데이트 해줍니다. (클릭 판정을 위해)
        MeshCollider collider = GetComponent<MeshCollider>();
        if (collider != null)
        {
            collider.sharedMesh = mesh;
        }
    }
}
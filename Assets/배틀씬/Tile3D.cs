using UnityEngine;

/// <summary>
/// 3D Ÿ���� �ڽ��� �׸��� ��ǥ�� �����ϰ�, ������ ��Ƽ���� �� �޽ø� �����ϱ� ���� ������Ʈ�Դϴ�.
/// </summary>
public class Tile3D : MonoBehaviour
{
    [Header("Ÿ�� ����")]
    public Vector3Int gridPosition;

    [Header("�޽� ����")]
    [Tooltip("Ÿ���� ���κ��� �������� �о� ����纯���� ����ϴ�. (Shear)")]
    [Range(-1f, 1f)]
    public float shearAmount = 0.0f;

    // �� Ÿ���� ������ ��Ƽ���� �ν��Ͻ��� �����Ͽ� �ܺο� �����ϴ� ������Ƽ�Դϴ�.
    public Material MyMaterial { get; private set; }

    private Renderer tileRenderer;
    private MeshFilter meshFilter;

    void Awake()
    {
        // --- 1. ���� ��Ƽ���� ���� ---
        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer == null)
        {
            Debug.LogError("Tile3D ������Ʈ�� Renderer ������Ʈ�� �����ϴ�!", this.gameObject);
            return;
        }
        // .material ������Ƽ�� �����ϴ� ����, �ڵ����� �� ������Ʈ���� ���� ��Ƽ���� ���纻�� �����˴ϴ�.
        MyMaterial = tileRenderer.material;

        // --- 2. ����纯�� �޽� ���� ---
        // shearAmount ���� 0�� �ƴ� ��쿡�� �޽ø� ������ŵ�ϴ�.
        if (shearAmount != 0)
        {
            DeformMesh();
        }
    }

    /// <summary>
    /// �� Ÿ���� �޽ø� ����纯������ ������Ű�� �Լ��Դϴ�.
    /// </summary>
    private void DeformMesh()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("Tile3D ������Ʈ�� MeshFilter ������Ʈ�� �����ϴ�!", this.gameObject);
            return;
        }

        // ���� �޽ø� �������� �ʵ���, �� ������Ʈ���� ������ �޽� ���纻�� �����մϴ�.
        Mesh mesh = meshFilter.mesh;

        // �޽��� ��� ������(vertex) ������ �����ɴϴ�.
        Vector3[] vertices = mesh.vertices;

        // ��� �������� ��ȸ�ϸ� Y���� 0���� ū (��, ���ʿ� �ִ�) �������� ã���ϴ�.
        for (int i = 0; i < vertices.Length; i++)
        {
            // ������ ť���� ������ Y���� 0.5 ��ó�Դϴ�.
            if (vertices[i].y > 0)
            {
                // ���� �������� X ��ġ�� shearAmount��ŭ �о��ݴϴ�.
                vertices[i].x += shearAmount;
            }
        }

        // ������ ������ ������ �ٽ� �޽ÿ� �����մϴ�.
        mesh.vertices = vertices;

        // ����� �޽ÿ� ���� ��輱�� �����ϵ��� �մϴ�.
        mesh.RecalculateBounds();

        // MeshCollider�� �ִٸ� �װ͵� ������Ʈ ���ݴϴ�. (Ŭ�� ������ ����)
        MeshCollider collider = GetComponent<MeshCollider>();
        if (collider != null)
        {
            collider.sharedMesh = mesh;
        }
    }
}
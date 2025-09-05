using UnityEngine;

public class CameraDragZoom : MonoBehaviour
{
    [Header("�� ����(���� ��ǥ)")]
    public float mapMinX = -20f;
    public float mapMaxX = 20f;
    public float mapMinY = -10f;
    public float mapMaxY = 10f;

    [Header("�� ����")]
    public float zoomSpeed = 2f;
    public float minZoom = 2f; // �ּ� �� ũ��
    private float defaultZoom; // ���� Orthographic Size (�ִ� �ܾƿ�)

    private Camera cam;
    private Vector3 dragOriginWorld;
    private bool isDragging = false;
    private float baseZ; // z ����

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        baseZ = transform.position.z;
        defaultZoom = cam.orthographicSize;
    }

    void Update()
    {
        HandleDrag();
        HandleZoom();
        ClampToBounds();
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOriginWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 currentWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 delta = dragOriginWorld - currentWorld;
            delta.z = 0f;
            transform.position += delta;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 beforeZoom = cam.ScreenToWorldPoint(Input.mousePosition);

            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, defaultZoom);

            Vector3 afterZoom = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 offset = beforeZoom - afterZoom;
            transform.position += offset;
        }
    }

    void ClampToBounds()
    {
        // ī�޶� ȭ�� ���� ũ�� ���
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        // �� ũ�� ���
        float mapWidth = mapMaxX - mapMinX;
        float mapHeight = mapMaxY - mapMinY;

        Vector3 pos = transform.position;

        // ī�޶� �ʺ��� ū ��� (�ܾƿ��� �ʹ� ���� �� ���)
        if (camWidth * 2 >= mapWidth)
        {
            // �� �߾ӿ� ����
            pos.x = (mapMinX + mapMaxX) * 0.5f;
        }
        else
        {
            // �������� ��� Ŭ����
            float minX = mapMinX + camWidth;
            float maxX = mapMaxX - camWidth;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
        }

        if (camHeight * 2 >= mapHeight)
        {
            // �� �߾ӿ� ����
            pos.y = (mapMinY + mapMaxY) * 0.5f;
        }
        else
        {
            // �������� ��� Ŭ����
            float minY = mapMinY + camHeight;
            float maxY = mapMaxY - camHeight;
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
        }

        pos.z = baseZ;
        transform.position = pos;
    }
}
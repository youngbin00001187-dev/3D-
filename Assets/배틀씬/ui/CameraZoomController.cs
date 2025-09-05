using UnityEngine;

public class CameraDragZoom : MonoBehaviour
{
    [Header("맵 영역(월드 좌표)")]
    public float mapMinX = -20f;
    public float mapMaxX = 20f;
    public float mapMinY = -10f;
    public float mapMaxY = 10f;

    [Header("줌 설정")]
    public float zoomSpeed = 2f;
    public float minZoom = 2f; // 최소 줌 크기
    private float defaultZoom; // 시작 Orthographic Size (최대 줌아웃)

    private Camera cam;
    private Vector3 dragOriginWorld;
    private bool isDragging = false;
    private float baseZ; // z 고정

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
        // 카메라 화면 절반 크기 계산
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        // 맵 크기 계산
        float mapWidth = mapMaxX - mapMinX;
        float mapHeight = mapMaxY - mapMinY;

        Vector3 pos = transform.position;

        // 카메라가 맵보다 큰 경우 (줌아웃이 너무 많이 된 경우)
        if (camWidth * 2 >= mapWidth)
        {
            // 맵 중앙에 고정
            pos.x = (mapMinX + mapMaxX) * 0.5f;
        }
        else
        {
            // 정상적인 경계 클램핑
            float minX = mapMinX + camWidth;
            float maxX = mapMaxX - camWidth;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
        }

        if (camHeight * 2 >= mapHeight)
        {
            // 맵 중앙에 고정
            pos.y = (mapMinY + mapMaxY) * 0.5f;
        }
        else
        {
            // 정상적인 경계 클램핑
            float minY = mapMinY + camHeight;
            float maxY = mapMaxY - camHeight;
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
        }

        pos.z = baseZ;
        transform.position = pos;
    }
}
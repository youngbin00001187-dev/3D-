using UnityEngine;

/// <summary>
/// 메인 카메라의 위치와 줌(Orthographic Size)을 매 프레임 따라가는 오버레이 카메라용 스크립트입니다.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFollower : MonoBehaviour
{
    [Header("따라갈 대상")]
    [Tooltip("기준이 될 메인 카메라를 여기에 연결하세요.")]
    public Camera mainCamera;

    private Camera thisCamera;

    void Awake()
    {
        // 자신의 카메라 컴포넌트를 미리 찾아둡니다.
        thisCamera = GetComponent<Camera>();
    }

    // LateUpdate는 모든 Update 호출이 끝난 후에 실행되므로,
    // 메인 카메라가 모든 이동과 줌을 마친 뒤에 따라가기에 가장 적합합니다.
    void LateUpdate()
    {
        if (mainCamera == null || thisCamera == null)
        {
            return;
        }

        // 1. 위치 동기화
        transform.position = mainCamera.transform.position;

        // 2. Orthographic Size (줌 배율) 동기화
        thisCamera.orthographicSize = mainCamera.orthographicSize;
    }
}

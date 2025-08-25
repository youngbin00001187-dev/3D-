using UnityEngine;

/// <summary>
/// 이 스크립트가 붙은 오브젝트가 항상 메인 카메라를 바라보게 만듭니다.
/// </summary>
public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // 성능을 위해 메인 카메라를 한 번만 찾아 저장해 둡니다.
        mainCamera = Camera.main;
    }

    // 모든 Update가 끝난 후 마지막에 실행되어 카메라 위치가 바뀌어도 정상 작동합니다.
    void LateUpdate()
    {
        if (mainCamera == null) return;

        // 카메라와 같은 방향을 바라보게 만듭니다.
        transform.forward = mainCamera.transform.forward;
    }
}
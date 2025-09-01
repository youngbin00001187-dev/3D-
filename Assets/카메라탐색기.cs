// WorldSpaceCanvasAssigner.cs

using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class WorldSpaceCanvasAssigner : MonoBehaviour
{
    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            // 'MainCamera' 태그 대신 우리가 만든 'GameplayCamera' 태그로 카메라를 찾습니다.
            GameObject camObject = GameObject.FindWithTag("GameplayCamera");
            if (camObject != null)
            {
                canvas.worldCamera = camObject.GetComponent<Camera>();
            }
            else
            {
                Debug.LogWarning("씬에서 'GameplayCamera' 태그를 가진 카메라를 찾을 수 없습니다!", this.gameObject);
            }
        }
    }
}
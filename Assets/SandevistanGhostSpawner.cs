using UnityEngine;
using System.Collections;

public class SandevistanGhostSpawner : MonoBehaviour
{
    [Header("타겟 설정")]
    public SpriteRenderer targetRenderer;

    [Header("이펙트 프리펩")]
    public GameObject ghostPrefab;

    [Header("잔상 생성 설정")]
    public float spawnInterval = 0.05f;

    [Header("잔상 표현 설정")]
    public float scatterRange = 0.4f;
    public float ghostLifetime = 0.4f;
    public Color[] ghostColors;

    [Header("이펙트 시작 딜레이")]
    public float startDelay = 0f;

    private Coroutine _spawnCoroutine;

    public void StartSpawning()
    {
        // [디버그] 이 함수가 BreakEffectManager로부터 호출되었는지 확인
        Debug.LogError("[Debug] StartSpawning() 호출됨!");

        if (targetRenderer == null || ghostPrefab == null)
        {
            // [디버그] 필수 변수가 할당되지 않았다면 에러 출력
            Debug.LogError("[Debug] Target Renderer 또는 Ghost Prefab이 비어있습니다! 인스펙터에서 할당해주세요.");
            return;
        }

        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
        }
        _spawnCoroutine = StartCoroutine(SpawnGhostsContinuously());
    }

    public void StopSpawning()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
            Debug.Log("[Sandevistan] 잔상 생성을 중지합니다.");
        }
    }

    private IEnumerator SpawnGhostsContinuously()
    {
        // [디버그] 코루틴이 실제로 시작되었는지 확인
        Debug.LogError("[Debug] SpawnGhostsContinuously 코루틴 시작!");

        if (startDelay > 0)
        {
            yield return new WaitForSecondsRealtime(startDelay);
        }

        while (true)
        {
            CreateGhost();
            yield return new WaitForSecondsRealtime(spawnInterval);
        }
    }

    private void CreateGhost()
    {
        // [디버그] 잔상이 실제로 생성되는지 확인
        Debug.LogError("[Debug] CreateGhost() 호출됨! 잔상을 생성합니다.");

        Vector3 spawnPosition = targetRenderer.transform.position;
        Quaternion spawnRotation = targetRenderer.transform.rotation;
        GameObject ghost = Instantiate(ghostPrefab, spawnPosition, spawnRotation);
        SpriteRenderer ghostRenderer = ghost.GetComponent<SpriteRenderer>();
        ghost.transform.localScale = targetRenderer.transform.localScale;
        ghostRenderer.sprite = targetRenderer.sprite;
        ghostRenderer.sortingLayerID = targetRenderer.sortingLayerID;
        ghostRenderer.sortingOrder = targetRenderer.sortingOrder - 1;
        if (ghostColors != null && ghostColors.Length > 0)
        {
            ghostRenderer.color = ghostColors[Random.Range(0, ghostColors.Length)];
        }
        Vector3 offset = new Vector3(Random.Range(-scatterRange, scatterRange), Random.Range(-scatterRange * 0.5f, scatterRange * 0.5f), 0f);
        ghost.transform.position += offset;
        StartCoroutine(FadeAndDestroy(ghostRenderer, ghostLifetime));
    }

    private IEnumerator FadeAndDestroy(SpriteRenderer sr, float duration)
    {
        float timer = 0f;
        Color originalColor = sr.color;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, timer / duration);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        Destroy(sr.gameObject);
    }
}
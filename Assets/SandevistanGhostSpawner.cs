using UnityEngine;
using System.Collections;

/// <summary>
/// 타겟 SpriteRenderer를 따라다니며 잔상(Ghost) 이펙트를 생성합니다.
/// Sandevistan과 같은 시간 왜곡 효과에 사용될 수 있습니다.
/// </summary>
public class SandevistanGhostSpawner : MonoBehaviour
{
    [Header("타겟 설정")]
    [SerializeField]
    private SpriteRenderer targetRenderer;

    [Header("이펙트 프리펩")]
    [SerializeField]
    private GameObject ghostPrefab;

    [Header("잔상 생성 설정")]
    [SerializeField, Tooltip("각 잔상이 생성되는 시간 간격 (실시간 기준)")]
    private float spawnInterval = 0.05f;

    [Header("잔상 표현 설정")]
    [SerializeField, Tooltip("잔상이 생성될 때 원본 위치로부터 흩어지는 최대 범위")]
    private float scatterRange = 0.4f;
    [SerializeField, Tooltip("생성된 잔상이 사라지기까지 걸리는 시간")]
    private float ghostLifetime = 0.4f;
    [SerializeField, Tooltip("잔상에 순차적으로 적용될 색상 배열")]
    private Color[] ghostColors;

    [Header("이펙트 시작 딜레이")]
    [SerializeField, Tooltip("StartSpawning 호출 후 실제 이펙트가 시작되기까지의 딜레이")]
    private float startDelay = 0f;

    // --- Private 변수 ---
    private Coroutine _spawnCoroutine;
    private int _colorIndex = 0;
    private WaitForSecondsRealtime _spawnWait;

    private void Awake()
    {
        // 성능 최적화를 위해 WaitForSecondsRealtime 인스턴스를 미리 캐싱합니다.
        // 이 코드는 '얼마나 기다릴지'에 대한 '타이머'를 미리 만들어두는 것일 뿐,
        // '어디에 생성할지'에 대한 위치 정보와는 전혀 관련이 없습니다.
        _spawnWait = new WaitForSecondsRealtime(spawnInterval);
    }

    /// <summary>
    /// 잔상 생성을 시작합니다. 이미 실행 중인 경우, 기존 루틴을 중지하고 새로 시작합니다.
    /// </summary>
    public void StartSpawning()
    {
        if (targetRenderer == null || ghostPrefab == null)
        {
            Debug.LogError("[Sandevistan] Target Renderer 또는 Ghost Prefab이 할당되지 않았습니다! 인스펙터에서 할당해주세요.");
            return;
        }

        // 기존 코루틴이 있다면 중지
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
        }

        // 잔상 생성을 시작할 때마다 색상 인덱스를 0으로 리셋
        _colorIndex = 0;
        _spawnCoroutine = StartCoroutine(SpawnGhostsContinuously());
    }

    /// <summary>
    /// 진행 중인 잔상 생성을 중지합니다.
    /// </summary>
    public void StopSpawning()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
            Debug.Log("[Sandevistan] 잔상 생성을 중지합니다.");
        }
    }

    /// <summary>
    /// 설정된 간격(spawnInterval)에 따라 지속적으로 잔상을 생성하는 코루틴입니다.
    /// </summary>
    private IEnumerator SpawnGhostsContinuously()
    {
        if (startDelay > 0)
        {
            yield return new WaitForSecondsRealtime(startDelay);
        }

        while (true)
        {
            // 이 함수가 호출될 때마다 플레이어의 '현재' 위치를 읽어가므로
            // 플레이어를 계속 따라다니게 됩니다.
            CreateGhost();

            // 여기서 미리 만들어 둔 '타이머'를 사용해 잠시 기다립니다.
            yield return _spawnWait;
        }
    }

    /// <summary>
    /// 잔상(Ghost) 게임 오브젝트 하나를 생성하고 초기화합니다.
    /// </summary>
    private void CreateGhost()
    {
        // ▼▼▼ 바로 이 부분에서 '매번' 플레이어의 '현재' 위치를 가져옵니다! ▼▼▼
        Vector3 spawnPosition = targetRenderer.transform.position;
        Quaternion spawnRotation = targetRenderer.transform.rotation;

        GameObject ghost = Instantiate(ghostPrefab, spawnPosition, spawnRotation);
        ghost.transform.localScale = targetRenderer.transform.localScale;

        if (ghost.TryGetComponent<SpriteRenderer>(out var ghostRenderer))
        {
            // 타겟 렌더러의 현재 상태를 복사
            ghostRenderer.sprite = targetRenderer.sprite;
            ghostRenderer.sortingLayerID = targetRenderer.sortingLayerID;
            ghostRenderer.sortingOrder = targetRenderer.sortingOrder - 1; // 잔상이 타겟보다 뒤에 보이도록 설정

            // 설정된 색상 배열에서 순차적으로 색상 적용
            if (ghostColors != null && ghostColors.Length > 0)
            {
                ghostRenderer.color = ghostColors[_colorIndex];
                _colorIndex = (_colorIndex + 1) % ghostColors.Length; // 다음 색상 인덱스로, 배열 끝에 도달하면 처음으로
            }

            // 위치에 약간의 무작위성을 더해 흩날리는 효과 적용
            Vector3 offset = new Vector3(Random.Range(-scatterRange, scatterRange), Random.Range(-scatterRange * 0.5f, scatterRange * 0.5f), 0f);
            ghost.transform.position += offset;

            // 잔상이 서서히 사라지도록 페이드 아웃 코루틴 시작
            StartCoroutine(FadeAndDestroy(ghostRenderer, ghostLifetime));
        }
        else
        {
            Debug.LogWarning($"[Sandevistan] Ghost Prefab '{ghostPrefab.name}'에 SpriteRenderer 컴포넌트가 없습니다.");
            Destroy(ghost); // SpriteRenderer가 없으면 이펙트가 동작하지 않으므로 오브젝트 파괴
        }
    }

    /// <summary>
    /// 지정된 시간 동안 SpriteRenderer를 서서히 투명하게 만든 후 게임 오브젝트를 파괴합니다.
    /// </summary>
    private IEnumerator FadeAndDestroy(SpriteRenderer sr, float duration)
    {
        float timer = 0f;
        Color originalColor = sr.color;

        if (duration <= 0)
        {
            Destroy(sr.gameObject);
            yield break;
        }

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
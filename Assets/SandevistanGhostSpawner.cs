using UnityEngine;
using System.Collections;

public class SandevistanGhostSpawner : MonoBehaviour
{
    [Header("Ÿ�� ����")]
    public SpriteRenderer targetRenderer;

    [Header("����Ʈ ������")]
    public GameObject ghostPrefab;

    [Header("�ܻ� ���� ����")]
    public float spawnInterval = 0.05f;

    [Header("�ܻ� ǥ�� ����")]
    public float scatterRange = 0.4f;
    public float ghostLifetime = 0.4f;
    public Color[] ghostColors;

    [Header("����Ʈ ���� ������")]
    public float startDelay = 0f;

    private Coroutine _spawnCoroutine;

    public void StartSpawning()
    {
        // [�����] �� �Լ��� BreakEffectManager�κ��� ȣ��Ǿ����� Ȯ��
        Debug.LogError("[Debug] StartSpawning() ȣ���!");

        if (targetRenderer == null || ghostPrefab == null)
        {
            // [�����] �ʼ� ������ �Ҵ���� �ʾҴٸ� ���� ���
            Debug.LogError("[Debug] Target Renderer �Ǵ� Ghost Prefab�� ����ֽ��ϴ�! �ν����Ϳ��� �Ҵ����ּ���.");
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
            Debug.Log("[Sandevistan] �ܻ� ������ �����մϴ�.");
        }
    }

    private IEnumerator SpawnGhostsContinuously()
    {
        // [�����] �ڷ�ƾ�� ������ ���۵Ǿ����� Ȯ��
        Debug.LogError("[Debug] SpawnGhostsContinuously �ڷ�ƾ ����!");

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
        // [�����] �ܻ��� ������ �����Ǵ��� Ȯ��
        Debug.LogError("[Debug] CreateGhost() ȣ���! �ܻ��� �����մϴ�.");

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
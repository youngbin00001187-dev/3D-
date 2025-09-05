using UnityEngine;
using System.Collections;

/// <summary>
/// Ÿ�� SpriteRenderer�� ����ٴϸ� �ܻ�(Ghost) ����Ʈ�� �����մϴ�.
/// Sandevistan�� ���� �ð� �ְ� ȿ���� ���� �� �ֽ��ϴ�.
/// </summary>
public class SandevistanGhostSpawner : MonoBehaviour
{
    [Header("Ÿ�� ����")]
    [SerializeField]
    private SpriteRenderer targetRenderer;

    [Header("����Ʈ ������")]
    [SerializeField]
    private GameObject ghostPrefab;

    [Header("�ܻ� ���� ����")]
    [SerializeField, Tooltip("�� �ܻ��� �����Ǵ� �ð� ���� (�ǽð� ����)")]
    private float spawnInterval = 0.05f;

    [Header("�ܻ� ǥ�� ����")]
    [SerializeField, Tooltip("�ܻ��� ������ �� ���� ��ġ�κ��� ������� �ִ� ����")]
    private float scatterRange = 0.4f;
    [SerializeField, Tooltip("������ �ܻ��� ���������� �ɸ��� �ð�")]
    private float ghostLifetime = 0.4f;
    [SerializeField, Tooltip("�ܻ� ���������� ����� ���� �迭")]
    private Color[] ghostColors;

    [Header("����Ʈ ���� ������")]
    [SerializeField, Tooltip("StartSpawning ȣ�� �� ���� ����Ʈ�� ���۵Ǳ������ ������")]
    private float startDelay = 0f;

    // --- Private ���� ---
    private Coroutine _spawnCoroutine;
    private int _colorIndex = 0;
    private WaitForSecondsRealtime _spawnWait;

    private void Awake()
    {
        // ���� ����ȭ�� ���� WaitForSecondsRealtime �ν��Ͻ��� �̸� ĳ���մϴ�.
        // �� �ڵ�� '�󸶳� ��ٸ���'�� ���� 'Ÿ�̸�'�� �̸� �����δ� ���� ��,
        // '��� ��������'�� ���� ��ġ �����ʹ� ���� ������ �����ϴ�.
        _spawnWait = new WaitForSecondsRealtime(spawnInterval);
    }

    /// <summary>
    /// �ܻ� ������ �����մϴ�. �̹� ���� ���� ���, ���� ��ƾ�� �����ϰ� ���� �����մϴ�.
    /// </summary>
    public void StartSpawning()
    {
        if (targetRenderer == null || ghostPrefab == null)
        {
            Debug.LogError("[Sandevistan] Target Renderer �Ǵ� Ghost Prefab�� �Ҵ���� �ʾҽ��ϴ�! �ν����Ϳ��� �Ҵ����ּ���.");
            return;
        }

        // ���� �ڷ�ƾ�� �ִٸ� ����
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
        }

        // �ܻ� ������ ������ ������ ���� �ε����� 0���� ����
        _colorIndex = 0;
        _spawnCoroutine = StartCoroutine(SpawnGhostsContinuously());
    }

    /// <summary>
    /// ���� ���� �ܻ� ������ �����մϴ�.
    /// </summary>
    public void StopSpawning()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
            Debug.Log("[Sandevistan] �ܻ� ������ �����մϴ�.");
        }
    }

    /// <summary>
    /// ������ ����(spawnInterval)�� ���� ���������� �ܻ��� �����ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    private IEnumerator SpawnGhostsContinuously()
    {
        if (startDelay > 0)
        {
            yield return new WaitForSecondsRealtime(startDelay);
        }

        while (true)
        {
            // �� �Լ��� ȣ��� ������ �÷��̾��� '����' ��ġ�� �о�Ƿ�
            // �÷��̾ ��� ����ٴϰ� �˴ϴ�.
            CreateGhost();

            // ���⼭ �̸� ����� �� 'Ÿ�̸�'�� ����� ��� ��ٸ��ϴ�.
            yield return _spawnWait;
        }
    }

    /// <summary>
    /// �ܻ�(Ghost) ���� ������Ʈ �ϳ��� �����ϰ� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void CreateGhost()
    {
        // ���� �ٷ� �� �κп��� '�Ź�' �÷��̾��� '����' ��ġ�� �����ɴϴ�! ����
        Vector3 spawnPosition = targetRenderer.transform.position;
        Quaternion spawnRotation = targetRenderer.transform.rotation;

        GameObject ghost = Instantiate(ghostPrefab, spawnPosition, spawnRotation);
        ghost.transform.localScale = targetRenderer.transform.localScale;

        if (ghost.TryGetComponent<SpriteRenderer>(out var ghostRenderer))
        {
            // Ÿ�� �������� ���� ���¸� ����
            ghostRenderer.sprite = targetRenderer.sprite;
            ghostRenderer.sortingLayerID = targetRenderer.sortingLayerID;
            ghostRenderer.sortingOrder = targetRenderer.sortingOrder - 1; // �ܻ��� Ÿ�ٺ��� �ڿ� ���̵��� ����

            // ������ ���� �迭���� ���������� ���� ����
            if (ghostColors != null && ghostColors.Length > 0)
            {
                ghostRenderer.color = ghostColors[_colorIndex];
                _colorIndex = (_colorIndex + 1) % ghostColors.Length; // ���� ���� �ε�����, �迭 ���� �����ϸ� ó������
            }

            // ��ġ�� �ణ�� ���������� ���� �𳯸��� ȿ�� ����
            Vector3 offset = new Vector3(Random.Range(-scatterRange, scatterRange), Random.Range(-scatterRange * 0.5f, scatterRange * 0.5f), 0f);
            ghost.transform.position += offset;

            // �ܻ��� ������ ��������� ���̵� �ƿ� �ڷ�ƾ ����
            StartCoroutine(FadeAndDestroy(ghostRenderer, ghostLifetime));
        }
        else
        {
            Debug.LogWarning($"[Sandevistan] Ghost Prefab '{ghostPrefab.name}'�� SpriteRenderer ������Ʈ�� �����ϴ�.");
            Destroy(ghost); // SpriteRenderer�� ������ ����Ʈ�� �������� �����Ƿ� ������Ʈ �ı�
        }
    }

    /// <summary>
    /// ������ �ð� ���� SpriteRenderer�� ������ �����ϰ� ���� �� ���� ������Ʈ�� �ı��մϴ�.
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
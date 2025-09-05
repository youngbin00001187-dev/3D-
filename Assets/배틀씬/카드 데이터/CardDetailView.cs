using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections; // �ڷ�ƾ�� ����ϱ� ���� �߰�

/// <summary>
/// ī�� �� ����(�̸�, ����)�� ǥ���ϴ� UI�� �����մϴ�.
/// Ȱ��ȭ�Ǹ� ���콺 Ŀ���� ����ٴϸ�, �������� �ִϸ��̼��� �����ݴϴ�.
/// </summary>
public class CardDetailView : MonoBehaviour
{
    public static CardDetailView Instance { get; private set; }

    [Header("UI ����")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject contentObject; // �ؽ�Ʈ�� ���̾ƿ� �׷��� �ִ� �г�
    [SerializeField] private RectTransform maskingPanelRect; // Mask ������Ʈ�� ��� �̹����� �ִ� �г�

    [Header("���콺 ���� ����")]
    [SerializeField] private Vector2 followOffset = new Vector2(20f, -20f);

    [Header("�ִϸ��̼� ����")]
    [SerializeField] private float revealDuration = 0.25f; // �������� �ð�
    [SerializeField] private float revealStartDelay = 0.05f; // �������� ���� �� ��� �ð�

    private bool isFollowing = false;
    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private RectTransform contentRectTransform;
    private Tweener _revealTweener;
    private Coroutine _showCoroutine; // ���� ���� Show ������ �����ϱ� ���� ����

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }

        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();

        if (contentObject != null)
        {
            contentRectTransform = contentObject.GetComponent<RectTransform>();
        }

        if (contentObject != null)
        {
            contentObject.SetActive(false);
            if (maskingPanelRect != null)
            {
                maskingPanelRect.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (isFollowing)
        {
            if (parentCanvas == null) return;
            Vector2 anchoredPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                Input.mousePosition,
                parentCanvas.worldCamera,
                out anchoredPos
            );
            rectTransform.anchoredPosition = anchoredPos + followOffset;
        }
    }

    public void Show(CardDataSO cardData)
    {
        if (cardData == null || contentObject == null || maskingPanelRect == null) return;

        // ������ ���� ���̴� Show/Hide ������ �ִٸ� ��� �ߴ�
        _revealTweener?.Kill();
        if (_showCoroutine != null)
        {
            StopCoroutine(_showCoroutine);
        }

        // ���ο� Show ������ ����
        _showCoroutine = StartCoroutine(ShowSequence(cardData));
    }

    private IEnumerator ShowSequence(CardDataSO cardData)
    {
        // 1. UI ��ҵ��� ���� Ȱ��ȭ�ϰ� �ؽ�Ʈ�� �����մϴ�.
        maskingPanelRect.gameObject.SetActive(true);
        contentObject.SetActive(true);
        if (nameText != null) nameText.text = cardData.cardName;
        if (descriptionText != null) descriptionText.text = cardData.description;

        // 2. 2�ܰ迡 ���� ���̾ƿ��� ����ȭ��ŵ�ϴ�.
        // 1�ܰ�: ��� ������ ��û�ϰ� �� �������� ��ٷ� 1�� ����� �ݿ��մϴ�.
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRectTransform);
        yield return new WaitForEndOfFrame();

        // 2�ܰ�: ���� �����ӿ��� �� �� �� ������ ��û�Ͽ� ���� ũ�⸦ Ȯ���մϴ�.
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRectTransform);

        // 3. ���� ContentSizeFitter�� ���� ������ ���� ���� ũ�⸦ �о�ɴϴ�.
        // LayoutUtility ��� RectTransform�� ���� ũ��(rect)�� ����� �� ��Ȯ�մϴ�.
        float targetWidth = contentRectTransform.rect.width;
        float targetHeight = contentRectTransform.rect.height;

        // 4. ����ŷ �г��� ���̴� �������� ���߰�, �ʺ�� 0���� �ʱ�ȭ�մϴ�.
        maskingPanelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
        maskingPanelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);

        // 5. ����ŷ �г��� �ʺ� ��ǥ �ʺ���� �ִϸ��̼��մϴ�.
        _revealTweener = maskingPanelRect.DOSizeDelta(new Vector2(targetWidth, targetHeight), revealDuration)
            .SetEase(Ease.OutCubic)
            .SetDelay(revealStartDelay);

        isFollowing = true;
        _showCoroutine = null; // �ڷ�ƾ �۾� �Ϸ�
    }

    public void Hide()
    {
        if (maskingPanelRect == null || !maskingPanelRect.gameObject.activeInHierarchy)
        {
            return;
        }

        isFollowing = false;

        // ���� ���� Show �ڷ�ƾ�� �ִٸ� �ߴ�
        if (_showCoroutine != null)
        {
            StopCoroutine(_showCoroutine);
            _showCoroutine = null;
        }

        // �ִϸ��̼����� �����
        _revealTweener?.Kill();
        _revealTweener = maskingPanelRect.DOSizeDelta(new Vector2(0, maskingPanelRect.sizeDelta.y), revealDuration)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                contentObject.SetActive(false);
                maskingPanelRect.gameObject.SetActive(false);
            });
    }
}

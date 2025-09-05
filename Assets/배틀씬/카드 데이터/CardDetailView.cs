using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections; // 코루틴을 사용하기 위해 추가

/// <summary>
/// 카드 상세 정보(이름, 설명)를 표시하는 UI를 관리합니다.
/// 활성화되면 마우스 커서를 따라다니며, 펼쳐지는 애니메이션을 보여줍니다.
/// </summary>
public class CardDetailView : MonoBehaviour
{
    public static CardDetailView Instance { get; private set; }

    [Header("UI 참조")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject contentObject; // 텍스트와 레이아웃 그룹이 있는 패널
    [SerializeField] private RectTransform maskingPanelRect; // Mask 컴포넌트와 배경 이미지가 있는 패널

    [Header("마우스 추적 설정")]
    [SerializeField] private Vector2 followOffset = new Vector2(20f, -20f);

    [Header("애니메이션 설정")]
    [SerializeField] private float revealDuration = 0.25f; // 펼쳐지는 시간
    [SerializeField] private float revealStartDelay = 0.05f; // 펼쳐지기 시작 전 대기 시간

    private bool isFollowing = false;
    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private RectTransform contentRectTransform;
    private Tweener _revealTweener;
    private Coroutine _showCoroutine; // 실행 중인 Show 로직을 제어하기 위한 변수

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

        // 이전에 실행 중이던 Show/Hide 로직이 있다면 모두 중단
        _revealTweener?.Kill();
        if (_showCoroutine != null)
        {
            StopCoroutine(_showCoroutine);
        }

        // 새로운 Show 시퀀스 시작
        _showCoroutine = StartCoroutine(ShowSequence(cardData));
    }

    private IEnumerator ShowSequence(CardDataSO cardData)
    {
        // 1. UI 요소들을 먼저 활성화하고 텍스트를 설정합니다.
        maskingPanelRect.gameObject.SetActive(true);
        contentObject.SetActive(true);
        if (nameText != null) nameText.text = cardData.cardName;
        if (descriptionText != null) descriptionText.text = cardData.description;

        // 2. 2단계에 걸쳐 레이아웃을 안정화시킵니다.
        // 1단계: 즉시 재계산을 요청하고 한 프레임을 기다려 1차 계산을 반영합니다.
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRectTransform);
        yield return new WaitForEndOfFrame();

        // 2단계: 다음 프레임에서 한 번 더 재계산을 요청하여 최종 크기를 확정합니다.
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRectTransform);

        // 3. 이제 ContentSizeFitter에 의해 완전히 계산된 최종 크기를 읽어옵니다.
        // LayoutUtility 대신 RectTransform의 실제 크기(rect)를 사용해 더 정확합니다.
        float targetWidth = contentRectTransform.rect.width;
        float targetHeight = contentRectTransform.rect.height;

        // 4. 마스킹 패널의 높이는 콘텐츠에 맞추고, 너비는 0으로 초기화합니다.
        maskingPanelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
        maskingPanelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);

        // 5. 마스킹 패널의 너비를 목표 너비까지 애니메이션합니다.
        _revealTweener = maskingPanelRect.DOSizeDelta(new Vector2(targetWidth, targetHeight), revealDuration)
            .SetEase(Ease.OutCubic)
            .SetDelay(revealStartDelay);

        isFollowing = true;
        _showCoroutine = null; // 코루틴 작업 완료
    }

    public void Hide()
    {
        if (maskingPanelRect == null || !maskingPanelRect.gameObject.activeInHierarchy)
        {
            return;
        }

        isFollowing = false;

        // 실행 중인 Show 코루틴이 있다면 중단
        if (_showCoroutine != null)
        {
            StopCoroutine(_showCoroutine);
            _showCoroutine = null;
        }

        // 애니메이션으로 숨기기
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

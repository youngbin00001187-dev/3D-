using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI 연결")]
    public TextMeshProUGUI nameText;
    public Image artImage;

    [Header("UI 상호작용 설정")]
    [Tooltip("상세 정보 UI가 뜨기까지의 호버 시간(초)입니다.")]
    public float detailViewHoverDelay = 0.5f;

    [Header("깜박임 방지 설정")]
    [Tooltip("마우스가 벗어난 후 실제로 숨기기까지의 딜레이 시간")]
    public float hideDelay = 0.1f;

    [Header("정적 상태 관리")]
    private static CardUI _currentHoveredCard; // 현재 호버된 카드를 전역으로 관리

    private CardDataSO _cardData;
    private CardManager _cardManager;
    private Coroutine _showDetailViewCoroutine;
    private Coroutine _hideDetailViewCoroutine;
    private bool _isDetailViewShown = false;
    private bool _isMouseOver = false;

    /// <summary>
    /// 카드 UI를 설정합니다.
    /// </summary>
    public void Setup(CardDataSO data, CardManager manager)
    {
        _cardData = data;
        _cardManager = manager;

        if (nameText != null && _cardData != null)
        {
            nameText.text = _cardData.cardName;
        }
        else
        {
            Debug.LogError($"[CardUI] nameText 또는 cardData가 null입니다! nameText: {nameText}, cardData: {_cardData}");
        }

        if (artImage != null && _cardData != null)
        {
            artImage.sprite = _cardData.cardImage;
        }
        else
        {
            Debug.LogError($"[CardUI] artImage 또는 cardData가 null입니다! artImage: {artImage}, cardData: {_cardData}");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isMouseOver = true;

        // 이전에 호버된 카드가 있다면 강제로 정리
        if (_currentHoveredCard != null && _currentHoveredCard != this)
        {
            _currentHoveredCard.ForceHideDetailView();
        }

        // 현재 카드를 전역 호버 카드로 설정
        _currentHoveredCard = this;

        // 카드 확대 효과
        transform.localScale = Vector3.one * 1.1f;

        // CardManager에게 호버 시작 알림
        if (_cardManager != null)
        {
            _cardManager.HandleCardHoverEnter(_cardData);
        }

        // 숨기기 코루틴이 실행 중이면 중단
        if (_hideDetailViewCoroutine != null)
        {
            StopCoroutine(_hideDetailViewCoroutine);
            _hideDetailViewCoroutine = null;
        }

        // 이미 표시된 상태가 아니고, 표시 코루틴이 실행 중이 아닐 때만 새로 시작
        if (!_isDetailViewShown && _showDetailViewCoroutine == null)
        {
            _showDetailViewCoroutine = StartCoroutine(ShowDetailViewAfterDelay());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isMouseOver = false;

        // 카드 크기 원복
        transform.localScale = Vector3.one;

        // CardManager에게 호버 종료 알림
        if (_cardManager != null)
        {
            _cardManager.HandleCardHoverExit();
        }

        // 표시 코루틴 중단
        if (_showDetailViewCoroutine != null)
        {
            StopCoroutine(_showDetailViewCoroutine);
            _showDetailViewCoroutine = null;
        }

        // 현재 호버된 카드가 자신이면 null로 설정
        if (_currentHoveredCard == this)
        {
            // 딜레이를 두고 숨기기 (깜박임 방지)
            if (_hideDetailViewCoroutine != null)
            {
                StopCoroutine(_hideDetailViewCoroutine);
            }
            _hideDetailViewCoroutine = StartCoroutine(HideDetailViewAfterDelay());
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_cardData == null) return;

        if (_cardManager != null)
        {
            _cardManager.OnCardClicked(this.gameObject, _cardData);
        }
    }

    /// <summary>
    /// 지정된 시간 이후에 카드 상세 정보를 표시하는 코루틴입니다.
    /// </summary>
    private IEnumerator ShowDetailViewAfterDelay()
    {
        yield return new WaitForSeconds(detailViewHoverDelay);

        // 마우스가 여전히 올려져 있을 때만 표시
        if (_isMouseOver && CardDetailView.Instance != null && _cardData != null)
        {
            CardDetailView.Instance.Show(_cardData);
            _isDetailViewShown = true;
        }

        _showDetailViewCoroutine = null;
    }

    /// <summary>
    /// 딜레이 후 상세 정보를 숨기는 코루틴입니다. (깜박임 방지)
    /// </summary>
    private IEnumerator HideDetailViewAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);

        // 딜레이 시간 동안 마우스가 다시 올려지지 않았을 때만 숨기기
        if (!_isMouseOver && _currentHoveredCard == this)
        {
            if (CardDetailView.Instance != null)
            {
                CardDetailView.Instance.Hide();
            }
            _isDetailViewShown = false;
            _currentHoveredCard = null; // 전역 상태도 정리
        }

        _hideDetailViewCoroutine = null;
    }

    /// <summary>
    /// 강제로 상세 정보를 숨깁니다. (외부에서 호출 가능)
    /// </summary>
    public void ForceHideDetailView()
    {
        // 모든 코루틴 중단
        if (_showDetailViewCoroutine != null)
        {
            StopCoroutine(_showDetailViewCoroutine);
            _showDetailViewCoroutine = null;
        }

        if (_hideDetailViewCoroutine != null)
        {
            StopCoroutine(_hideDetailViewCoroutine);
            _hideDetailViewCoroutine = null;
        }

        // 상세 정보 숨기기
        if (CardDetailView.Instance != null)
        {
            CardDetailView.Instance.Hide();
        }

        _isDetailViewShown = false;
        _isMouseOver = false;

        // 전역 상태 정리 (자신이 현재 호버된 카드일 때만)
        if (_currentHoveredCard == this)
        {
            _currentHoveredCard = null;
        }
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때 상세 정보 강제 숨기기
        ForceHideDetailView();
    }

    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때 상세 정보 강제 숨기기
        ForceHideDetailView();
    }

    public CardDataSO GetCardData()
    {
        return _cardData;
    }

    /// <summary>
    /// 디버그용: 현재 카드 상태를 출력합니다.
    /// </summary>
    [ContextMenu("Debug Card UI State")]
    public void DebugCardUIState()
    {
        Debug.Log($"=== CardUI 상태 ===\n" +
                  $"Card Data: {(_cardData != null ? _cardData.cardName : "NULL")}\n" +
                  $"Card Manager: {(_cardManager != null ? "연결됨" : "NULL")}\n" +
                  $"Name Text: {(nameText != null ? nameText.text : "NULL")}\n" +
                  $"Art Image: {(artImage != null ? "연결됨" : "NULL")}\n" +
                  $"Mouse Over: {_isMouseOver}\n" +
                  $"Detail View Shown: {_isDetailViewShown}\n" +
                  $"Show Coroutine: {(_showDetailViewCoroutine != null ? "실행 중" : "중지됨")}\n" +
                  $"Hide Coroutine: {(_hideDetailViewCoroutine != null ? "실행 중" : "중지됨")}");
    }
}
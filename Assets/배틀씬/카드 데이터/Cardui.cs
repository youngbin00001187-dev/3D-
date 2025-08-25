using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI 연결")]
    public TextMeshProUGUI nameText;
    public Image artImage;

    private CardDataSO _cardData;
    private CardManager _cardManager;

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
        transform.localScale = Vector3.one * 1.1f;

        // ▼▼▼ 여기에 호버 시작 신호 전송 코드 추가 ▼▼▼
        if (_cardManager != null)
        {
            _cardManager.HandleCardHoverEnter(_cardData);
        }
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;

        // ▼▼▼ 여기에 호버 종료 신호 전송 코드 추가 ▼▼▼
        if (_cardManager != null)
        {
            _cardManager.HandleCardHoverExit();
        }
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_cardData == null)
        {
            Debug.LogError("[CardUI] 카드 데이터가 null입니다!");
            return;
        }

        Debug.Log($"{_cardData.cardName} 카드가 클릭되었습니다.");
    if (_cardManager != null)
    {
        // ▼▼▼ CardManager의 OnCardClicked 함수를 호출합니다 ▼▼▼
        _cardManager.OnCardClicked(this.gameObject, _cardData);
    }
    }

    /// <summary>
    /// 카드 데이터를 반환합니다.
    /// </summary>
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
                  $"Art Image: {(artImage != null ? "연결됨" : "NULL")}");
    }
}
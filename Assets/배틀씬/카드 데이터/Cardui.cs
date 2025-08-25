using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI ����")]
    public TextMeshProUGUI nameText;
    public Image artImage;

    private CardDataSO _cardData;
    private CardManager _cardManager;

    /// <summary>
    /// ī�� UI�� �����մϴ�.
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
            Debug.LogError($"[CardUI] nameText �Ǵ� cardData�� null�Դϴ�! nameText: {nameText}, cardData: {_cardData}");
        }

        if (artImage != null && _cardData != null)
        {
            artImage.sprite = _cardData.cardImage;
        }
        else
        {
            Debug.LogError($"[CardUI] artImage �Ǵ� cardData�� null�Դϴ�! artImage: {artImage}, cardData: {_cardData}");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * 1.1f;

        // ���� ���⿡ ȣ�� ���� ��ȣ ���� �ڵ� �߰� ����
        if (_cardManager != null)
        {
            _cardManager.HandleCardHoverEnter(_cardData);
        }
        // ����������������������
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;

        // ���� ���⿡ ȣ�� ���� ��ȣ ���� �ڵ� �߰� ����
        if (_cardManager != null)
        {
            _cardManager.HandleCardHoverExit();
        }
        // ����������������������
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_cardData == null)
        {
            Debug.LogError("[CardUI] ī�� �����Ͱ� null�Դϴ�!");
            return;
        }

        Debug.Log($"{_cardData.cardName} ī�尡 Ŭ���Ǿ����ϴ�.");
    if (_cardManager != null)
    {
        // ���� CardManager�� OnCardClicked �Լ��� ȣ���մϴ� ����
        _cardManager.OnCardClicked(this.gameObject, _cardData);
    }
    }

    /// <summary>
    /// ī�� �����͸� ��ȯ�մϴ�.
    /// </summary>
    public CardDataSO GetCardData()
    {
        return _cardData;
    }

    /// <summary>
    /// ����׿�: ���� ī�� ���¸� ����մϴ�.
    /// </summary>
    [ContextMenu("Debug Card UI State")]
    public void DebugCardUIState()
    {
        Debug.Log($"=== CardUI ���� ===\n" +
                  $"Card Data: {(_cardData != null ? _cardData.cardName : "NULL")}\n" +
                  $"Card Manager: {(_cardManager != null ? "�����" : "NULL")}\n" +
                  $"Name Text: {(nameText != null ? nameText.text : "NULL")}\n" +
                  $"Art Image: {(artImage != null ? "�����" : "NULL")}");
    }
}
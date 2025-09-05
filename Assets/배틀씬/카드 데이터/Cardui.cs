using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI ����")]
    public TextMeshProUGUI nameText;
    public Image artImage;

    [Header("UI ��ȣ�ۿ� ����")]
    [Tooltip("�� ���� UI�� �߱������ ȣ�� �ð�(��)�Դϴ�.")]
    public float detailViewHoverDelay = 0.5f;

    [Header("������ ���� ����")]
    [Tooltip("���콺�� ��� �� ������ ���������� ������ �ð�")]
    public float hideDelay = 0.1f;

    [Header("���� ���� ����")]
    private static CardUI _currentHoveredCard; // ���� ȣ���� ī�带 �������� ����

    private CardDataSO _cardData;
    private CardManager _cardManager;
    private Coroutine _showDetailViewCoroutine;
    private Coroutine _hideDetailViewCoroutine;
    private bool _isDetailViewShown = false;
    private bool _isMouseOver = false;

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
        _isMouseOver = true;

        // ������ ȣ���� ī�尡 �ִٸ� ������ ����
        if (_currentHoveredCard != null && _currentHoveredCard != this)
        {
            _currentHoveredCard.ForceHideDetailView();
        }

        // ���� ī�带 ���� ȣ�� ī��� ����
        _currentHoveredCard = this;

        // ī�� Ȯ�� ȿ��
        transform.localScale = Vector3.one * 1.1f;

        // CardManager���� ȣ�� ���� �˸�
        if (_cardManager != null)
        {
            _cardManager.HandleCardHoverEnter(_cardData);
        }

        // ����� �ڷ�ƾ�� ���� ���̸� �ߴ�
        if (_hideDetailViewCoroutine != null)
        {
            StopCoroutine(_hideDetailViewCoroutine);
            _hideDetailViewCoroutine = null;
        }

        // �̹� ǥ�õ� ���°� �ƴϰ�, ǥ�� �ڷ�ƾ�� ���� ���� �ƴ� ���� ���� ����
        if (!_isDetailViewShown && _showDetailViewCoroutine == null)
        {
            _showDetailViewCoroutine = StartCoroutine(ShowDetailViewAfterDelay());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isMouseOver = false;

        // ī�� ũ�� ����
        transform.localScale = Vector3.one;

        // CardManager���� ȣ�� ���� �˸�
        if (_cardManager != null)
        {
            _cardManager.HandleCardHoverExit();
        }

        // ǥ�� �ڷ�ƾ �ߴ�
        if (_showDetailViewCoroutine != null)
        {
            StopCoroutine(_showDetailViewCoroutine);
            _showDetailViewCoroutine = null;
        }

        // ���� ȣ���� ī�尡 �ڽ��̸� null�� ����
        if (_currentHoveredCard == this)
        {
            // �����̸� �ΰ� ����� (������ ����)
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
    /// ������ �ð� ���Ŀ� ī�� �� ������ ǥ���ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    private IEnumerator ShowDetailViewAfterDelay()
    {
        yield return new WaitForSeconds(detailViewHoverDelay);

        // ���콺�� ������ �÷��� ���� ���� ǥ��
        if (_isMouseOver && CardDetailView.Instance != null && _cardData != null)
        {
            CardDetailView.Instance.Show(_cardData);
            _isDetailViewShown = true;
        }

        _showDetailViewCoroutine = null;
    }

    /// <summary>
    /// ������ �� �� ������ ����� �ڷ�ƾ�Դϴ�. (������ ����)
    /// </summary>
    private IEnumerator HideDetailViewAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);

        // ������ �ð� ���� ���콺�� �ٽ� �÷����� �ʾ��� ���� �����
        if (!_isMouseOver && _currentHoveredCard == this)
        {
            if (CardDetailView.Instance != null)
            {
                CardDetailView.Instance.Hide();
            }
            _isDetailViewShown = false;
            _currentHoveredCard = null; // ���� ���µ� ����
        }

        _hideDetailViewCoroutine = null;
    }

    /// <summary>
    /// ������ �� ������ ����ϴ�. (�ܺο��� ȣ�� ����)
    /// </summary>
    public void ForceHideDetailView()
    {
        // ��� �ڷ�ƾ �ߴ�
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

        // �� ���� �����
        if (CardDetailView.Instance != null)
        {
            CardDetailView.Instance.Hide();
        }

        _isDetailViewShown = false;
        _isMouseOver = false;

        // ���� ���� ���� (�ڽ��� ���� ȣ���� ī���� ����)
        if (_currentHoveredCard == this)
        {
            _currentHoveredCard = null;
        }
    }

    private void OnDisable()
    {
        // ������Ʈ�� ��Ȱ��ȭ�� �� �� ���� ���� �����
        ForceHideDetailView();
    }

    private void OnDestroy()
    {
        // ������Ʈ�� �ı��� �� �� ���� ���� �����
        ForceHideDetailView();
    }

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
                  $"Art Image: {(artImage != null ? "�����" : "NULL")}\n" +
                  $"Mouse Over: {_isMouseOver}\n" +
                  $"Detail View Shown: {_isDetailViewShown}\n" +
                  $"Show Coroutine: {(_showDetailViewCoroutine != null ? "���� ��" : "������")}\n" +
                  $"Hide Coroutine: {(_hideDetailViewCoroutine != null ? "���� ��" : "������")}");
    }
}
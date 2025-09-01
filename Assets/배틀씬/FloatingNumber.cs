using UnityEngine;
using TMPro;
using DG.Tweening; // DOTween�� ����ϱ� ���� �� ���� �߰��մϴ�.

/// <summary>
/// ������/���� ������ ǥ�ÿ� ������ �����ϴ� ��ũ��Ʈ�Դϴ�.
/// DOTween�� ����Ͽ� �����Դϴ�.
/// </summary>
public class FloatingNumber : MonoBehaviour
{
    [Header("UI ����")]
    [Tooltip("���ڸ� ǥ���� TextMeshProUGUI ������Ʈ�Դϴ�.")]
    [SerializeField] private TextMeshProUGUI numberText;

    [Header("���� ����")]
    [Tooltip("�������� �Ծ��� �� ǥ�õ� �ܰ��� �����Դϴ�.")]
    [SerializeField] private Color damageOutlineColor = Color.red;
    [Tooltip("ü���� ȸ������ �� ǥ�õ� �ܰ��� �����Դϴ�.")]
    [SerializeField] private Color healOutlineColor = Color.green;

    // ���� �ִϸ��̼� ���� �������� DOTween ���� ������ �����߽��ϴ�. ����
    [Header("Ʈ�� �ִϸ��̼� ����")]
    [Tooltip("�ִϸ��̼��� ���ӵǴ� �ð�(��)�Դϴ�.")]
    [SerializeField] private float floatDuration = 1.0f;
    [Tooltip("���� �������� �Ÿ��Դϴ�. (ĵ���� ����)")]
    [SerializeField] private float floatDistance = 50f;
    // �������������������������������������

    // --- Animator ���� �ڵ�� ��� �����Ǿ����ϴ�. ---

    void Awake()
    {
        if (numberText == null) numberText = GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>
    /// VFXManager�� ȣ���Ͽ� ����, ������ �����ϰ� �ִϸ��̼��� �����մϴ�.
    /// </summary>
    public void Setup(int amount)
    {
        if (numberText == null) return;

        // 1. ���� �ؽ�Ʈ�� �ܰ��� ������ �����մϴ�.
        numberText.text = amount > 0 ? $"+{amount}" : amount.ToString();
        if (amount < 0)
        {
            numberText.outlineColor = damageOutlineColor;
            numberText.outlineWidth = 0.2f;
        }
        else
        {
            numberText.outlineColor = healOutlineColor;
            numberText.outlineWidth = 0.2f;
        }

        // ���� DOTween �ִϸ��̼� ������ ���⿡ �߰��߽��ϴ�. ����
        // 2. �ִϸ��̼��� ����ϰ�, ������ ������Ʈ�� �ı��մϴ�.
        PlayFloatingAnimation();
    }

    private void PlayFloatingAnimation()
    {
        // ���� ����� ��ġ�� �����մϴ�.
        Color startColor = numberText.color;
        numberText.color = new Color(startColor.r, startColor.g, startColor.b, 1); // ���İ��� 1�� �ʱ�ȭ
        Vector3 startPosition = transform.localPosition;

        // DOTween �������� �����մϴ�.
        Sequence sequence = DOTween.Sequence();

        // �������� �� ���� Ʈ���� ���ÿ� �߰��մϴ�.
        // 1. Y������ floatDistance��ŭ floatDuration�� ���� �̵�
        sequence.Join(transform.DOLocalMoveY(startPosition.y + floatDistance, floatDuration).SetEase(Ease.OutQuad));
        // 2. floatDuration�� ���� �������� �ؽ�Ʈ�� ������ �����ϰ� (Fade Out)
        sequence.Join(numberText.DOFade(0, floatDuration / 2).SetDelay(floatDuration / 2));

        // ������ �ִϸ��̼��� ��� ������ OnComplete �ݹ��� ȣ��˴ϴ�.
        sequence.OnComplete(() =>
        {
            // �� ������Ʈ�� ������ �ı��մϴ�.
            Destroy(gameObject);
        });
    }

    // --- DestroyOnAnimationEnd() �޼���� �� �̻� �ʿ� �����Ƿ� �����Ǿ����ϴ�. ---
}
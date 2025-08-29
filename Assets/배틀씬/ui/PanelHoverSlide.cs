using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelHoverSlide : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("�����̵� ����")]
    public float slideDistance = 100f; // ȣ�� �� ���� �̵� �Ÿ�
    public float maxY = 1f;            // �ִ� Y�� ����
    public float slideSpeed = 0.3f;    // �̵� �ӵ�

    [Header("Spacing ����")]
    public float hoverSpacing = 50f;   // ȣ�� �� ��ǥ spacing
    public float spacingSpeed = 5f;    // spacing ��ȭ �ӵ�

    private Vector3 originalPos;
    private Vector3 targetPos;
    private bool isHover = false;

    private HorizontalLayoutGroup layoutGroup;
    private float originalSpacing;
    private float targetSpacing;

    void Awake()
    {
        originalPos = transform.localPosition;

        layoutGroup = GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null)
        {
            originalSpacing = layoutGroup.spacing;
        }
    }

    void Update()
    {
        // ----------------------
        // �г� �����̵� ó��
        // ----------------------
        targetPos = isHover
            ? originalPos + new Vector3(0, slideDistance, 0)
            : originalPos;

        targetPos.y = Mathf.Min(targetPos.y, maxY);

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * slideSpeed);

        // ----------------------
        // HorizontalLayoutGroup spacing ó��
        // ----------------------
        if (layoutGroup != null)
        {
            targetSpacing = isHover ? hoverSpacing : originalSpacing;
            layoutGroup.spacing = Mathf.Lerp(layoutGroup.spacing, targetSpacing, Time.deltaTime * spacingSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHover = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHover = false;
    }
}

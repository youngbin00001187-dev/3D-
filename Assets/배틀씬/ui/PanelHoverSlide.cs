using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelHoverSlide : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("슬라이드 설정")]
    public float slideDistance = 100f; // 호버 시 위로 이동 거리
    public float maxY = 1f;            // 최대 Y값 제한
    public float slideSpeed = 0.3f;    // 이동 속도

    [Header("Spacing 설정")]
    public float hoverSpacing = 50f;   // 호버 시 목표 spacing
    public float spacingSpeed = 5f;    // spacing 변화 속도

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
        // 패널 슬라이드 처리
        // ----------------------
        targetPos = isHover
            ? originalPos + new Vector3(0, slideDistance, 0)
            : originalPos;

        targetPos.y = Mathf.Min(targetPos.y, maxY);

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * slideSpeed);

        // ----------------------
        // HorizontalLayoutGroup spacing 처리
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

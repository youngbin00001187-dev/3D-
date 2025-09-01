using UnityEngine;
using TMPro;
using DG.Tweening; // DOTween을 사용하기 위해 이 줄을 추가합니다.

/// <summary>
/// 데미지/힐량 숫자의 표시와 동작을 제어하는 스크립트입니다.
/// DOTween을 사용하여 움직입니다.
/// </summary>
public class FloatingNumber : MonoBehaviour
{
    [Header("UI 연결")]
    [Tooltip("숫자를 표시할 TextMeshProUGUI 컴포넌트입니다.")]
    [SerializeField] private TextMeshProUGUI numberText;

    [Header("색상 설정")]
    [Tooltip("데미지를 입었을 때 표시될 외곽선 색상입니다.")]
    [SerializeField] private Color damageOutlineColor = Color.red;
    [Tooltip("체력을 회복했을 때 표시될 외곽선 색상입니다.")]
    [SerializeField] private Color healOutlineColor = Color.green;

    // ▼▼▼ 애니메이션 관련 변수들을 DOTween 설정 변수로 변경했습니다. ▼▼▼
    [Header("트윈 애니메이션 설정")]
    [Tooltip("애니메이션이 지속되는 시간(초)입니다.")]
    [SerializeField] private float floatDuration = 1.0f;
    [Tooltip("위로 떠오르는 거리입니다. (캔버스 단위)")]
    [SerializeField] private float floatDistance = 50f;
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    // --- Animator 관련 코드는 모두 삭제되었습니다. ---

    void Awake()
    {
        if (numberText == null) numberText = GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>
    /// VFXManager가 호출하여 숫자, 색상을 설정하고 애니메이션을 시작합니다.
    /// </summary>
    public void Setup(int amount)
    {
        if (numberText == null) return;

        // 1. 숫자 텍스트와 외곽선 색상을 설정합니다.
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

        // ▼▼▼ DOTween 애니메이션 로직을 여기에 추가했습니다. ▼▼▼
        // 2. 애니메이션을 재생하고, 끝나면 오브젝트를 파괴합니다.
        PlayFloatingAnimation();
    }

    private void PlayFloatingAnimation()
    {
        // 시작 색상과 위치를 저장합니다.
        Color startColor = numberText.color;
        numberText.color = new Color(startColor.r, startColor.g, startColor.b, 1); // 알파값을 1로 초기화
        Vector3 startPosition = transform.localPosition;

        // DOTween 시퀀스를 생성합니다.
        Sequence sequence = DOTween.Sequence();

        // 시퀀스에 두 개의 트윈을 동시에 추가합니다.
        // 1. Y축으로 floatDistance만큼 floatDuration초 동안 이동
        sequence.Join(transform.DOLocalMoveY(startPosition.y + floatDistance, floatDuration).SetEase(Ease.OutQuad));
        // 2. floatDuration의 절반 지점부터 텍스트를 서서히 투명하게 (Fade Out)
        sequence.Join(numberText.DOFade(0, floatDuration / 2).SetDelay(floatDuration / 2));

        // 시퀀스 애니메이션이 모두 끝나면 OnComplete 콜백이 호출됩니다.
        sequence.OnComplete(() =>
        {
            // 이 오브젝트를 스스로 파괴합니다.
            Destroy(gameObject);
        });
    }

    // --- DestroyOnAnimationEnd() 메서드는 더 이상 필요 없으므로 삭제되었습니다. ---
}
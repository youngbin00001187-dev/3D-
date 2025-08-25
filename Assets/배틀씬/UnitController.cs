using UnityEngine;
using System.Collections;
using System;
using DG.Tweening; // 리코일 코루틴에 필요합니다.

/// <summary>
/// 유닛의 상태를 정의하는 Enum입니다.
/// </summary>
public enum UnitState
{
    Normal,
    Stun
}

public abstract class UnitController : MonoBehaviour
{
    public enum UnitType { Player, Enemy }
    public UnitType unitType;

    [Header("공통 능력치")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("유닛 상태")]
    [Tooltip("유닛의 현재 상태입니다. (예: Normal, Stun)")]
    public UnitState currentState = UnitState.Normal;

    [Header("공통 상태")]
    public GameObject currentTile;
    protected bool isActing = false;

    // 3D 모델과 2D 스프라이트 모두를 참조하기 위해 공통 부모 클래스인 Renderer를 사용합니다.
    protected Renderer unitRenderer;
    private Color originalColor;

    // 이펙트 관련 변수 (2D 스크립트에서 가져왔으나, 현재 3D 프로젝트에서는 사용하지 않음)
    public Color hitColor = Color.red;
    public float hitColorDuration = 0.15f;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.2f;

    // 3D 모델에서는 Animator를 사용하는 방식이 다르므로 일단 주석 처리
    // protected Animator animator;

    // 2D 전용 변수 제거:
    // private Vector3 originalScale;
    // public float unitPerspectiveFactor = 0f;
    // public float scaleChangeSpeed = 12f;

    protected virtual void Awake()
    {
        // 2D 전용 Animator, SpriteRenderer 코드를 삭제합니다.
        // animator = GetComponent<Animator>();
        // unitSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        unitRenderer = GetComponentInChildren<Renderer>();

        currentHealth = maxHealth;
        // UpdateHealthUI(); // UI 시스템은 추후 3D에 맞게 재설계

        if (unitRenderer != null)
        {
            originalColor = unitRenderer.material.color;
        }
    }

    public virtual void TakeImpact(int damage)
    {
        // 1. 모든 충격에 공통적으로 적용될 시각 효과를 먼저 재생합니다.
        PlayHitEffect();
        ApplyHealthDamage(damage);
    }

    // 2D 전용 TakeDamage 함수는 삭제합니다.

    private void PlayHitEffect()
    {
        // 2D 전용 애니메이션, 색상 변경, 쉐이크 로직은 일단 주석 처리합니다.
        // if (animator != null) { StartCoroutine(PlayHitAnimation(99, 0.5f)); }
        // if (unitRenderer != null) { StartCoroutine(FlashColor(hitColor, hitColorDuration)); }
        // StartCoroutine(ShakeEffect(shakeDuration, shakeIntensity));
    }

    private void ApplyHealthDamage(int damage)
    {
        if (damage <= 0) return;
        currentHealth -= damage;
        // UpdateHealthUI(); // UI 시스템은 추후 3D에 맞게 재설계
        if (currentHealth <= 0) Die();
    }

    public void SetState(UnitState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        Debug.Log($"[UnitController] {gameObject.name}의 상태가 {newState}(으)로 변경되었습니다.");
    }

    // 2D 전용 OnPositionChanged, UpdateScale 함수는 삭제합니다.

    public void MoveToTile(GameObject tile)
    {
        if (tile != null)
        {
            if (currentTile != null)
            {
                // GridManager3D를 사용
                GridManager3D.instance.UnregisterUnitPosition(this, GetGridPosition());
            }

            transform.position = tile.transform.position;
            currentTile = tile;

            // GridManager3D를 사용
            GridManager3D.instance.RegisterUnitPosition(this, GetGridPosition());
        }
    }

    public IEnumerator MoveCoroutine(GameObject targetTile, float moveSpeed)
    {
        if (currentTile != null)
        {
            GridManager3D.instance.UnregisterUnitPosition(this, GetGridPosition());
        }

        Vector3 targetPosition = targetTile.transform.position;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;

        currentTile = targetTile;
        GridManager3D.instance.RegisterUnitPosition(this, GetGridPosition());
    }

    public IEnumerator RecoilCoroutine(Vector3 targetPosition, float recoilSpeed = 10f)
    {
        Vector3 startPosition = transform.position;
        Vector3 bumpPosition = Vector3.Lerp(startPosition, targetPosition, 0.4f);
        float duration = Vector3.Distance(startPosition, bumpPosition) / recoilSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, bumpPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = bumpPosition;

        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(bumpPosition, startPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = startPosition;
    }

    public Vector3Int GetGridPosition()
    {
        if (currentTile == null) { return new Vector3Int(-1, -1, -1); }

        Tile3D tileComponent = currentTile.GetComponent<Tile3D>();
        if (tileComponent != null)
        {
            return tileComponent.gridPosition;
        }
        // 2D 전용 이름 파싱 코드는 삭제합니다.
        return new Vector3Int(-1, -1, -1);
    }

    protected virtual void Die()
    {
        Debug.Log($"<color=red>{gameObject.name}이(가) 사망했습니다.</color>");

        if (currentTile != null)
        {
            GridManager3D.instance.UnregisterUnitPosition(this, GetGridPosition());
        }

        Destroy(gameObject, 1.5f);
    }
}
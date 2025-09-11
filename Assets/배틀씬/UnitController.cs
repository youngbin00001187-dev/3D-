using UnityEngine;
using System.Collections;
using System;
using DG.Tweening;

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

    private HealthBar _healthBar;

    [Header("유닛 상태")]
    public UnitState currentState = UnitState.Normal;
    private int _stunCounter = 0;

    [Header("행동 횟수")]
    public int maxActionsPerTurn = 1;
    private int _currentActionsLeft;

    [Header("공통 상태")]
    public GameObject currentTile;
    protected bool isActing = false;

    protected Renderer unitRenderer;
    private Color originalColor;
    private Canvas myCanvas;

    // ▼▼▼ [신규] Animator를 저장하고 외부에 알려줄 프로퍼티 ▼▼▼
    public Animator UnitAnimator { get; private set; }

    [Header("피격 효과")]
    public Color hitColor = Color.red;
    public float hitColorDuration = 0.3f;
    public float shakeIntensity = 0.3f;
    public float shakeDuration = 0.5f;

    protected virtual void Awake()
    {
        // ▼▼▼ [신규] 자기 자식에 있는 Animator를 찾아 변수에 저장하는 로직 ▼▼▼
        UnitAnimator = GetComponentInChildren<Animator>();

        unitRenderer = GetComponentInChildren<Renderer>();
        myCanvas = GetComponentInChildren<Canvas>();
        _healthBar = GetComponentInChildren<HealthBar>();
        currentHealth = maxHealth;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.sprite != null && sr.material != null)
        {
            sr.material.mainTexture = sr.sprite.texture;
        }

        if (unitRenderer != null)
        {
            originalColor = unitRenderer.material.color;
        }
    }

    public int GetCurrentActionsLeft() { return _currentActionsLeft; }
    public void DecrementActionsLeft()
    {
        if (_currentActionsLeft > 0) _currentActionsLeft--;
    }
    public void ResetActions() { _currentActionsLeft = maxActionsPerTurn; }

    public void ApplyStun(int duration)
    {
        if (duration <= 0) return;
        _stunCounter += duration;
        currentState = UnitState.Stun;
    }

    public bool ProcessStunStatus()
    {
        if (currentState != UnitState.Stun) return true;
        if (_stunCounter > 0)
        {
            _stunCounter--;
            if (_stunCounter <= 0)
            {
                currentState = UnitState.Normal;
            }
            return false;
        }
        currentState = UnitState.Normal;
        return true;
    }

    public virtual void TakeImpact(int damage)
    {
        ApplyHealthChange(-damage);
        PlaySelfHitEffect();
    }

    public virtual void Heal(int amount)
    {
        ApplyHealthChange(amount);
    }

    private void ApplyHealthChange(int amount)
    {
        if (amount == 0) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (_healthBar != null)
        {
            _healthBar.UpdateHealth(currentHealth, maxHealth);
        }

        ShowFloatingNumber(amount);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void PlaySelfHitEffect()
    {
        transform.DOShakePosition(shakeDuration, shakeIntensity);
        StartCoroutine(FlashRedCoroutine());
    }

    private IEnumerator FlashRedCoroutine()
    {
        if (unitRenderer != null)
        {
            unitRenderer.material.color = hitColor;
            yield return new WaitForSeconds(hitColorDuration);
            unitRenderer.material.color = originalColor;
        }
    }

    protected void ShowFloatingNumber(int amount)
    {
        if (amount == 0) return;
        if (VFXManager.Instance == null) return;

        Transform canvasTransform = myCanvas != null ? myCanvas.transform : null;
        VFXManager.Instance.ShowDamageNumber(canvasTransform, transform.position, amount);
    }

    public void MoveToTile(GameObject tile)
    {
        if (tile != null)
        {
            if (currentTile != null)
            {
                GridManager3D.instance.UnregisterUnitPosition(this, GetGridPosition());
            }
            transform.position = tile.transform.position;
            currentTile = tile;
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
        return tileComponent != null ? tileComponent.gridPosition : new Vector3Int(-1, -1, -1);
    }

    protected virtual void Die()
    {
        // 1. 가장 먼저 스턴 상태로 만들어, 혹시 모를 추가 행동을 즉시 방지합니다.
        currentState = UnitState.Stun;

        Debug.Log($"<color=red>{gameObject.name}이(가) 사망했습니다.</color>");

        // 2. 그리드에서 자신의 위치 정보를 제거합니다.
        if (currentTile != null)
        {
            GridManager3D.instance.UnregisterUnitPosition(this, GetGridPosition());
        }

        // 3. 일정 시간 후 게임 오브젝트를 파괴합니다.
        Destroy(gameObject, 1.5f);
    }
}
using UnityEngine;
using System.Collections;
using System;
using DG.Tweening; // 리코일 및 히트 이펙트에 필요합니다.

/// <summary>
/// 유닛의 상태를 정의하는 Enum입니다.
/// </summary>
public enum UnitState
{
    Normal, // 정상 상태
    Stun    // 행동 불가 상태
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

    private int _stunCounter = 0;

    [Header("행동 횟수")]
    [Tooltip("라운드당 최대 행동 횟수입니다.")]
    public int maxActionsPerTurn = 1;
    private int _currentActionsLeft;

    [Header("공통 상태")]
    public GameObject currentTile;
    protected bool isActing = false;

    protected Renderer unitRenderer;
    private Color originalColor;
    private Canvas myCanvas; // 유닛 자신의 World Space Canvas

    [Header("피격 효과")]
    public Color hitColor = Color.red;
    public float hitColorDuration = 0.15f;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.2f;

    protected virtual void Awake()
    {
        unitRenderer = GetComponentInChildren<Renderer>();
        myCanvas = GetComponentInChildren<Canvas>(); // 자신의 자식에서 Canvas를 찾습니다.
        currentHealth = maxHealth;

        // 머티리얼에 런타임 텍스처를 연결하는 코드
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

    // --- 행동 횟수 관리 ---
    public int GetCurrentActionsLeft()
    {
        return _currentActionsLeft;
    }

    public void DecrementActionsLeft()
    {
        if (_currentActionsLeft > 0)
        {
            _currentActionsLeft--;
        }
        else
        {
            Debug.LogWarning($"[UnitController] {gameObject.name}의 남은 행동 횟수가 이미 0입니다.");
        }
    }

    public void ResetActions()
    {
        _currentActionsLeft = maxActionsPerTurn;
    }

    // --- 스턴 상태 관리 ---
    public void ApplyStun(int duration)
    {
        if (duration <= 0) return;

        _stunCounter += duration;
        currentState = UnitState.Stun;
        Debug.Log($"<color=yellow>[STUN] {this.name}이(가) {duration}턴 동안 스턴 상태가 됩니다. (총 {_stunCounter}턴)</color>");
    }

    public bool ProcessStunStatus()
    {
        if (currentState != UnitState.Stun)
        {
            return true;
        }

        if (_stunCounter > 0)
        {
            _stunCounter--;
            Debug.Log($"<color=yellow>[STUN] {this.name}이(가) 행동을 스킵합니다. (남은 스턴: {_stunCounter}턴)</color>");

            if (_stunCounter <= 0)
            {
                currentState = UnitState.Normal;
                Debug.Log($"<color=green>[STUN] {this.name}의 스턴 상태가 해제됩니다.</color>");
            }
            return false;
        }

        currentState = UnitState.Normal;
        return true;
    }

    // --- 피격 및 생명력 관리 ---

    public virtual void TakeImpact(int damage)
    {
        ApplyHealthDamage(damage);
        PlayHitEffect();
        ShowFloatingNumber(-damage);
    }

    private void PlayHitEffect()
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

    /// <summary>
    /// 유닛의 캔버스에 데미지나 힐량 같은 숫자를 띄웁니다.
    /// </summary>
    /// <param name="amount">표시할 숫자 (양수: 힐, 음수: 데미지)</param>
    protected void ShowFloatingNumber(int amount)
    {
        // ▼▼▼ 디버그 로그 추가 ▼▼▼
        Debug.Log($"<color=cyan>[UnitController] ShowFloatingNumber 호출됨. 양: {amount}</color>");

        if (amount == 0)
        {
            Debug.Log("[UnitController] amount가 0이므로 숫자 표시를 중단합니다.");
            return;
        }

        if (VFXManager.Instance == null)
        {
            Debug.LogError("[UnitController] VFXManager.Instance를 찾을 수 없습니다!");
            return;
        }

        Transform canvasTransform = (myCanvas != null) ? myCanvas.transform : null;
        Debug.Log($"<color=cyan>[UnitController] VFXManager.ShowDamageNumber 호출 직전. 캔버스: {(canvasTransform != null ? canvasTransform.name : "없음")}</color>");
        VFXManager.Instance.ShowDamageNumber(canvasTransform, transform.position, amount);
    }

    private void ApplyHealthDamage(int damage)
    {
        if (damage <= 0) return;
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    public void SetState(UnitState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        Debug.Log($"[UnitController] {gameObject.name}의 상태가 {newState}(으)로 변경되었습니다.");
    }

    // --- 이동 및 위치 관리 ---
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
        if (tileComponent != null)
        {
            return tileComponent.gridPosition;
        }
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
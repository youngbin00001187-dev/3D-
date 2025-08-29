using UnityEngine;
using System.Collections;
using System;
using DG.Tweening; // 리코일 코루틴에 필요합니다.

/// <summary>
/// [신규] 유닛의 상태를 정의하는 Enum입니다.
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

    // [신규] 스턴 지속 턴을 카운트합니다.
    private int _stunCounter = 0;

    [Header("행동 횟수")]
    [Tooltip("라운드당 최대 행동 횟수입니다.")]
    public int maxActionsPerTurn = 1;
    [Tooltip("현재 라운드에 남은 행동 횟수입니다.")]
    private int _currentActionsLeft;

    [Header("공통 상태")]
    public GameObject currentTile;
    protected bool isActing = false;

    protected Renderer unitRenderer;
    private Color originalColor;

    public Color hitColor = Color.red;
    public float hitColorDuration = 0.15f;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.2f;

    protected virtual void Awake()
    {
        unitRenderer = GetComponentInChildren<Renderer>();
        currentHealth = maxHealth;

        // [신규] 머티리얼에 런타임 텍스처를 연결하는 코드
        // 자식에 있는 SpriteRenderer를 찾습니다.
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.sprite != null && sr.material != null)
        {
            // material 프로퍼티를 호출하는 순간, 이 렌더러만의 머티리얼 복제본이 생성됩니다.
            // 그 복제본의 메인 텍스처를 자신의 스프라이트 텍스처로 설정합니다.
            sr.material.mainTexture = sr.sprite.texture;
        }

        if (unitRenderer != null)
        {
            // originalColor는 텍스처가 적용된 후의 색상을 저장해야 하므로, 이 코드는 아래에 있는 것이 안전합니다.
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

    // --- [신규] 스턴 상태 관리 ---

    /// <summary>
    /// 유닛에게 스턴을 적용합니다.
    /// </summary>
    public void ApplyStun(int duration)
    {
        if (duration <= 0) return;

        _stunCounter += duration;
        currentState = UnitState.Stun;
        Debug.Log($"<color=yellow>[STUN] {this.name}이(가) {duration}턴 동안 스턴 상태가 됩니다. (총 {_stunCounter}턴)</color>");
        // TODO: 여기에 스턴 시각 효과(VFX)를 재생하는 코드를 추가할 수 있습니다.
    }

    /// <summary>
    /// 스턴 상태를 처리하고, 행동 가능 여부를 반환합니다.
    /// GameAction의 Execute()에서 호출됩니다.
    /// </summary>
    /// <returns>행동이 가능하면 true, 스턴으로 불가능하면 false</returns>
    public bool ProcessStunStatus()
    {
        if (currentState != UnitState.Stun)
        {
            return true; // 스턴 상태가 아니면 항상 행동 가능
        }

        if (_stunCounter > 0)
        {
            _stunCounter--;
            Debug.Log($"<color=yellow>[STUN] {this.name}이(가) 행동을 스킵합니다. (남은 스턴: {_stunCounter}턴)</color>");

            if (_stunCounter <= 0)
            {
                currentState = UnitState.Normal;
                Debug.Log($"<color=green>[STUN] {this.name}의 스턴 상태가 해제됩니다.</color>");
                // TODO: 여기에 스턴 해제 시각 효과(VFX)를 재생하는 코드를 추가할 수 있습니다.
            }
            return false; // 이번 턴은 행동 불가
        }

        // 스턴 상태이지만 카운터가 0 이하면 정상으로 되돌리고 행동 가능
        currentState = UnitState.Normal;
        return true;
    }

    // --- 피격 및 생명력 관리 ---

    public virtual void TakeImpact(int damage)
    {
        PlayHitEffect();
        ApplyHealthDamage(damage);
    }

    private void PlayHitEffect()
    {
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
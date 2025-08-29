using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyController : UnitController
{
    [Header("적 전용 데이터")]
    [Tooltip("이 적의 데이터를 담고 있는 SO 에셋입니다. Spawner가 할당합니다.")]
    public EnemyDataSO enemyData;

    // --- 내부 상태 변수 ---
    private int _actionPatternIndex = 0;

    // '계획'된 행동을 저장하는 변수들
    private CardDataSO _plannedCard;
    private Vector3Int _plannedRelativeVector;

    // 현재 표시된 의도 타일을 기억하는 변수
    private List<Tile3D> _currentIntentTiles = new List<Tile3D>();

    /// <summary>
    /// Spawner가 적을 생성한 직후 호출하여 기본 능력치를 설정합니다.
    /// </summary>
    public void Setup(EnemyDataSO data)
    {
        this.enemyData = data;
        this.maxHealth = data.maxHealth;
        this.currentHealth = this.maxHealth;
        this.maxActionsPerTurn = data.actionsPerTurn;
    }

    /// <summary>
    /// [계획 함수] 다음 행동(카드, 상대 방향)을 결정하여 내부 변수에 저장만 합니다.
    /// </summary>
    public void PlanNextAction()
    {
        if (enemyData == null || enemyData.actionPattern.Count == 0 || !HasMoreActionsThisRound())
        {
            _plannedCard = null; // 행동이 없으면 계획을 비웁니다.
            return;
        }

        EnemyAction currentEnemyAction = enemyData.actionPattern[_actionPatternIndex];
        _plannedCard = currentEnemyAction.referenceCard;

        if (_plannedCard == null || _plannedCard.actionSequence.Count == 0) return;

        GameAction gameAction = _plannedCard.actionSequence[0];
        List<GameObject> targetableTiles = gameAction.GetTargetableTiles(this);
        GameObject initialTargetTile = GetBestTargetTile(targetableTiles, currentEnemyAction.movementType);

        if (initialTargetTile == null)
        {
            _plannedRelativeVector = Vector3Int.forward; // 타겟이 없으면 기본 방향 (예: 전방)
            return;
        }

        Vector3Int targetPos = initialTargetTile.GetComponent<Tile3D>().gridPosition;
        _plannedRelativeVector = targetPos - GetGridPosition();

        _actionPatternIndex = (_actionPatternIndex + 1) % enemyData.actionPattern.Count;
    }

    /// <summary>
    /// [표시/재계산 함수] 현재 계획과 현재 위치를 바탕으로 의도 하이라이트를 그립니다.
    /// </summary>
    public void UpdateIntentDisplay()
    {
        if (HighlightManager.instance == null) return;

        ClearIntentDisplay();

        if (_plannedCard == null) return;

        Vector3Int finalTargetPos = GetGridPosition() + _plannedRelativeVector;
        GameObject finalTargetTile = GridManager3D.instance.GetTileAtPosition(finalTargetPos);

        if (finalTargetTile == null) return;

        GameAction gameAction = _plannedCard.actionSequence[0];
        var impactTiles = gameAction.GetActionImpactTiles(this, finalTargetTile)
                                     .Select(t => t.GetComponent<Tile3D>())
                                     .Where(t => t != null).ToList();

        HighlightManager.instance.AddHighlight(impactTiles, HighlightManager.HighlightType.EnemyIntent);
        _currentIntentTiles = impactTiles;
    }
    public void ReplanActionFromNewPosition()
    {
        Debug.Log($"<color=orange>[AI] {this.name}이(가) 넉백으로 인해 행동을 재조정합니다.</color>");

        // 1. 현재 위치에서 가장 최적인 다음 행동을 다시 계획합니다.
        PlanNextAction();
        // 2. 새로운 계획에 맞춰 의도 표시를 업데이트합니다.
        UpdateIntentDisplay();
    }
    /// <summary>
    /// [제거 함수] 현재 표시된 자신의 의도 하이라이트만 깨끗하게 지웁니다.
    /// </summary>
    public void ClearIntentDisplay()
    {
        if (HighlightManager.instance != null && _currentIntentTiles.Count > 0)
        {
            HighlightManager.instance.RemoveHighlight(_currentIntentTiles, HighlightManager.HighlightType.EnemyIntent);
            _currentIntentTiles.Clear();
        }
    }

    /// <summary>
    /// 계획된 행동을 ActionTurnManager 큐에 등록합니다.
    /// </summary>
    public void CommitActionToQueue()
    {
        if (_plannedCard == null) return;

        Vector3Int finalTargetPos = GetGridPosition() + _plannedRelativeVector;
        GameObject finalTargetTile = GridManager3D.instance.GetTileAtPosition(finalTargetPos);

        if (finalTargetTile != null)
        {
            QueuedAction action = new QueuedAction
            {
                User = this,
                SourceCard = _plannedCard,
                TargetTile = finalTargetTile
            };

            if (ActionTurnManager.Instance != null)
            {
                ActionTurnManager.Instance.AddActionToNormalQueue(action);
            }
        }
    }

    // --- 헬퍼 함수 ---

    private GameObject GetBestTargetTile(List<GameObject> targetableTiles, E_MoveType moveType)
    {
        if (targetableTiles == null || targetableTiles.Count == 0) return null;

        switch (moveType)
        {
            case E_MoveType.ChasePlayer:
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player == null) return targetableTiles.FirstOrDefault();

                Vector3Int playerPos = player.GetGridPosition();
                GameObject bestTile = null;
                float minDistance = float.MaxValue;

                foreach (var tile in targetableTiles)
                {
                    Vector3Int tilePos = tile.GetComponent<Tile3D>().gridPosition;
                    float distance = Vector3.Distance(tilePos, playerPos);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestTile = tile;
                    }
                }
                return bestTile;

            case E_MoveType.Fixed:
            default:
                return targetableTiles[Random.Range(0, targetableTiles.Count)];
        }
    }

    // --- 라운드 상태 관리 ---

    /// <summary>
    /// UnitController의 행동력 변수를 사용하여 남은 행동이 있는지 확인합니다.
    /// </summary>
    public bool HasMoreActionsThisRound()
    {
        return GetCurrentActionsLeft() > 0;
    }
}
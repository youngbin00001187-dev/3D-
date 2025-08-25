using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Card", menuName = "3DProject/Card Data")]
public class CardDataSO : ScriptableObject
{
    [Header("카드 기본 정보")]
    public string cardID;
    public string cardName;
    public Sprite cardImage;
    [TextArea(3, 5)]
    public string description;

    [Header("의도 표시 설정 (3D)")]
    [Tooltip("이 카드를 적이 사용하거나 플레이어가 호버할 때 표시될 예측 범위입니다.")]
    public List<Vector3Int> intentPredictionRange = new List<Vector3Int>();

    // 심법 관련 필드는 제외합니다.

    [Header("카드 액션 시퀀스")]
    [SerializeReference]
    public List<GameAction> actionSequence = new List<GameAction>();

    // ▼▼▼ 여기에 ContextMenu 기능 추가 ▼▼▼
    [ContextMenu("액션 시퀀스/Move Action 추가")]
    private void AddMoveAction()
    {
        actionSequence.Add(new MoveAction());
    }

    [ContextMenu("액션 시퀀스/Attack Action 추가")]
    private void AddAttackAction()
    {
        actionSequence.Add(new AttackAction());
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
}
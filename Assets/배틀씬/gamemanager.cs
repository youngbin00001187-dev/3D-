using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 전투 씬의 전체적인 게임 흐름(라운드, 페이즈)을 관리하는 총괄 매니저입니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum BattlePhase
    {
        Setup,              // 셋업 페이즈
        Draw,               // 드로우 페이즈
        CardSelection,      // 카드 선택 페이즈
        ActionExecution,    // 액션 실행 페이즈
        PostProcessing,     // 후처리 페이즈
        RoundEnd            // 라운드 종료
    }
    public BattlePhase currentPhase { get; private set; }

    // Button 참조는 CardManager로 이전되었으므로 여기서 삭제합니다.

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    // Start() 함수는 이제 비어있습니다.
    private void Start()
    {
    }

    private void OnEnable()
    {
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.OnBattleStart += OnBattleStarted;
            // '액션 페이즈 시작' 이벤트를 구독합니다.
            BattleEventManager.instance.OnActionPhaseStart += OnActionPhaseStarted;
        }
    }

    private void OnDisable()
    {
        if (BattleEventManager.instance != null)
        {
            BattleEventManager.instance.OnBattleStart -= OnBattleStarted;
            BattleEventManager.instance.OnActionPhaseStart -= OnActionPhaseStarted;
        }
    }

    private void OnBattleStarted()
    {
        StartCoroutine(RoundCoroutine());
    }

    private IEnumerator RoundCoroutine()
    {
        // --- 1. 셋업 페이즈 ---
        currentPhase = BattlePhase.Setup;
        Debug.Log("Phase: Setup");
        yield return null;

        // --- 2. 드로우 페이즈 ---
        currentPhase = BattlePhase.Draw;
        Debug.Log("Phase: Draw");
        // CardManager가 OnBattleStart 또는 OnNewRound 이벤트를 듣고 스스로 카드를 뽑습니다.
        yield return null;

        // --- 3. 카드 선택 페이즈 ---
        currentPhase = BattlePhase.CardSelection;
        Debug.Log("Phase: CardSelection");
        // 이 상태에서는 CardManager가 '진행' 버튼이 눌리기를 기다립니다.
    }

    /// <summary>
    /// '액션 페이즈 시작' 신호를 받았을 때 호출됩니다.
    /// </summary>
    private void OnActionPhaseStarted()
    {
        if (currentPhase == BattlePhase.CardSelection)
        {
            StartCoroutine(ActionPhaseCoroutine());
        }
    }

    private IEnumerator ActionPhaseCoroutine()
    {
        // --- 4. 액션 실행 페이즈 ---
        currentPhase = BattlePhase.ActionExecution;
        Debug.Log("Phase: ActionExecution");
        // TODO: TurnManager에게 액션 큐 처리를 지시
        yield return null;

        // --- 5. 후처리 페이즈 ---
        currentPhase = BattlePhase.PostProcessing;
        Debug.Log("Phase: PostProcessing");
        yield return null;

        // --- 6. 라운드 종료 ---
        currentPhase = BattlePhase.RoundEnd;
        Debug.Log("Phase: RoundEnd");

        StartCoroutine(RoundCoroutine()); // 다음 라운드를 시작합니다.
    }
}
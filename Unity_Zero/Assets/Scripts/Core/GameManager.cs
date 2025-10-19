using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 싱글톤: 어디서든 GameManager.Instance 로 접근
    public static GameManager Instance { get; private set; }

    // 스테이지 전체 제한 시간(초)
    [SerializeField] private float stageTimeLimit = 300f;

    // 공개용 읽기 전용 타이머(남은 시간 UI에서 표시)
    public float RemainingTime { get; private set; }

    // 퍼즐 전역 성공 여부(예: 모든 필수 퍼즐 완료 시 true)
    public bool AllRequiredPuzzlesCleared { get; private set; }

    private void Awake()
    {
        // 싱글톤 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        RemainingTime = stageTimeLimit;
    }

    private void OnEnable()
    {
        // 퍼즐 완료 이벤트 구독
        EventBus.OnPuzzleCleared += HandlePuzzleCleared;
    }

    private void OnDisable()
    {
        EventBus.OnPuzzleCleared -= HandlePuzzleCleared;
    }

    private void Update()
    {
        // 시간 감소
        RemainingTime -= Time.deltaTime;
        if (RemainingTime <= 0f)
        {
            RemainingTime = 0f;
            // 시간 종료 시 처리(실패 연출, 재시작 등)
            Debug.Log("[GameManager] Time Up! Stage Failed.");
            // 필요 시 씬 리로드 or 결과 UI 호출
        }
    }

    private void HandlePuzzleCleared(string puzzleId)
    {
        Debug.Log($"[GameManager] Puzzle Cleared: {puzzleId}");
        // 간단 로직: 필수 퍼즐이 1개 뿐이라고 가정(확장 가능)
        AllRequiredPuzzlesCleared = true;
        // 전역으로 "문 열림" 같은 이벤트 쏘고 싶으면 여기서 발행
        EventBus.RaiseAllRequiredPuzzlesCleared();
    }
}

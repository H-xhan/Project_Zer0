using UnityEngine;

/// 간단한 전역 게임 상태 관리 예시 (퍼즐, 제한 시간 등)
public class GameManager : MonoBehaviour
{
    // 전역 접근용 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }

    [Header("Stage Time Limit")]
    [Tooltip("스테이지 제한 시간(초)")]
    [SerializeField] private float stageTimeLimit = 300f;

    [Tooltip("현재 남은 시간(읽기 전용)")]
    public float RemainingTime { get; private set; }

    [Header("Puzzle State")]
    [Tooltip("모든 필수 퍼즐이 클리어되었는지 여부")]
    public bool AllRequiredPuzzlesCleared { get; private set; }

    private void Awake()
    {
        // 싱글톤 설정
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
        EventBus.OnPuzzleCleared += HandlePuzzleCleared;
    }

    private void OnDisable()
    {
        EventBus.OnPuzzleCleared -= HandlePuzzleCleared;
    }

    private void Update()
    {
        // 제한 시간 감소
        if (RemainingTime > 0f)
        {
            RemainingTime -= Time.deltaTime;
            if (RemainingTime <= 0f)
            {
                RemainingTime = 0f;
                HandleTimeUp();
            }
        }
    }

    // 퍼즐 클리어 알림 처리
    private void HandlePuzzleCleared(string puzzleId)
    {
        // 필요한 경우 puzzleId 기반 조건 처리 가능
        AllRequiredPuzzlesCleared = true;

        // 모든 필수 퍼즐 완료 이벤트 호출 (확장 시 조건 체크하여 호출)
        EventBus.RaiseAllRequiredPuzzlesCleared();
    }

    // 시간 종료 시 처리 (씬 리로드, 실패 UI 등 연결 지점)
    private void HandleTimeUp()
    {
        // 실제 게임 로직에 맞게 구현
        Debug.Log("[GameManager] Time Up");
    }
}

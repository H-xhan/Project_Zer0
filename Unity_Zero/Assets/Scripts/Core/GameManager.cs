using UnityEngine;

/// 간단한 전역 게임 상태 관리 예시 (퍼즐, 제한 시간 등)
public class GameManager : MonoBehaviour
{
    // 전역 접근용 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }

    [Header("Time System")]
    [Tooltip("씬에 존재하는 시간 시스템")]
    [SerializeField] private TimeSystemController timeSystem;

    [Tooltip("현재 남은 시간(초) - TimeSystemController에서 조회")]
    public float RemainingTime => timeSystem != null ? timeSystem.CurrentSeconds : 0f;

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

        if (timeSystem == null)
            timeSystem = Object.FindFirstObjectByType<TimeSystemController>();
    }

    private void OnEnable()
    {
        EventBus.OnPuzzleCleared += HandlePuzzleCleared;

        if (timeSystem == null)
            timeSystem = Object.FindFirstObjectByType<TimeSystemController>();

        if (timeSystem != null)
            timeSystem.OnTimeDepleted += HandleTimeUp;
    }

    private void OnDisable()
    {
        EventBus.OnPuzzleCleared -= HandlePuzzleCleared;

        if (timeSystem != null)
            timeSystem.OnTimeDepleted -= HandleTimeUp;
    }

    private void Update()
    {

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

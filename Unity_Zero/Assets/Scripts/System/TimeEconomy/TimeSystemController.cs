// TimeSystemController.cs (교체본)
using UnityEngine;

/// 시간 경제 시스템의 허브(코어 + 행동비용 위임)
[DisallowMultipleComponent]
public class TimeSystemController : MonoBehaviour
{
    [Header("Core")]
    public float initialSeconds = 900f;               // 시작 시간(초)
    public float baseDrainPerSecond = 0.0f;           // 초당 기본 소모
    public bool startRunning = true;                  // 시작 시 작동 여부

    [Header("Action Costs (추가 차감)")]               // ← 추가
    public float walkCostPerSecond = 1f;              // 걷는 중 초당 차감
    public float sprintCostPerSecond = 2f;            // 뛰는 중 초당 차감
    public float jumpCostOnce = 3f;                   // 점프 1회 차감

    private TimeWallet _wallet;                       // 지갑
    private bool _running;                            // 작동 플래그

    // 외부 조회용
    public float CurrentSeconds => _wallet != null ? _wallet.CurrentSeconds : 0f; // 남은 시간
    public bool IsRunning => _running;                                            // 작동 상태

    // 행동 비용 모듈(자동 부착)
    [HideInInspector] public TimeActionCostModule actionModule;

    // ===== 라이프사이클 =====
    void Awake()
    {
        Initialize();                                  // 허브 초기화

        // 모듈 자동 확보/초기화
        actionModule = GetComponent<TimeActionCostModule>();
        if (actionModule == null)
            actionModule = gameObject.AddComponent<TimeActionCostModule>();
        actionModule.hideFlags = HideFlags.HideInInspector;
        actionModule.Initialize(this);
    }

    void Update()
    {
        Tick(Time.deltaTime);                          // 기본 소모 처리
    }

    // ===== 허브 표준 진입점 =====
    public void Initialize()
    {
        _wallet = new TimeWallet(initialSeconds);      // 지갑 생성
        _wallet.OnChanged += HandleChanged;            // 변경 이벤트
        _wallet.OnDepleted += HandleDepleted;          // 0초 이벤트
        _running = startRunning;                       // 시작 상태
    }

    public void Tick(float deltaTime)
    {
        if (!_running || _wallet == null) return;
        if (baseDrainPerSecond > 0f && deltaTime > 0f)
        {
            float spend = baseDrainPerSecond * deltaTime;
            _wallet.Spend(spend, "BaseDrain");
        }
    }

    // ===== 외부 API =====
    public void AddSeconds(float seconds, string reason = "") { _wallet?.Add(seconds, reason); }
    public void SpendSeconds(float seconds, string reason = "") { _wallet?.Spend(seconds, reason); }
    public void SetRunning(bool running) { _running = running; }
    public void ResetTime(float seconds = -1f)
    {
        float v = (seconds >= 0f) ? seconds : initialSeconds;
        _wallet?.Reset(v);
    }

    // ===== 컨트롤러는 “호출만” (모듈 위임) =====      // ← 추가
    public void SpendForWalkDelta(float dt) { actionModule?.SpendForWalkDelta(dt); }
    public void SpendForSprintDelta(float dt) { actionModule?.SpendForSprintDelta(dt); }
    public void SpendForJumpEvent() { actionModule?.SpendForJumpEvent(); }

    // ===== 이벤트 핸들러 =====
    private void HandleChanged(float value) { /* UI에서 구독해서 사용 */ }
    private void HandleDepleted() { /* 0초 시 사망/리셋 등 연결 */ }
}

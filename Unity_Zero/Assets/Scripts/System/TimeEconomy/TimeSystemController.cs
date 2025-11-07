using System;
using TMPro;
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

    [Header("Damage Costs")]
    [Tooltip("받은 데미지(게임 수치) → 차감할 시간(초)로 변환하는 배율")]
    public float damageToSecondsScale = 1f;           // 10 데미지 → 10초 차감(기본 예시)
    [Tooltip("피해 후 잠깐 동안 중복 차감을 막는 무적 시간(초)")]
    public float damageIFrameSeconds = 0.2f;          // i-프레임 0.2초

    private float _damageIFrameRemain = 0f;           // 내부 i-프레임 타이머

    public float MaxSeconds => initialSeconds;        // UI용 최대치 공개 (권장)


    private TimeWallet _wallet;                       // 지갑
    private bool _running;                            // 작동 플래그

    [Header("Dynamic Cost Multiplier (from Efficiency)")]
    [Tooltip("EfficiencyModule에서 넘어오는 공통 배율값입니다.")]
    public float externalCostMultiplier = 1f;

    [Tooltip("외부 배율의 최소 클램프 값입니다.")]
    public float externalCostMin = 1.0f;

    [Tooltip("외부 배율의 최대 클램프 값입니다.")]
    public float externalCostMax = 3.0f;

    [Header("Efficiency Influence per Action")]
    [Tooltip("걷기 시간 비용에 효율 배율을 얼마나 반영할지 (0=영향 없음, 1=100% 적용)")]
    [Range(0f, 10f)] public float walkEffFactor = 1f;

    [Tooltip("스프린트 시간 비용에 효율 배율을 얼마나 반영할지")]
    [Range(0f, 10f)] public float sprintEffFactor = 1f;

    [Tooltip("점프 시간 비용에 효율 배율을 얼마나 반영할지")]
    [Range(0f, 10f)] public float jumpEffFactor = 1f;

    [Header("Debug")]
    public bool debugLog = true;

    public event Action<float, string, float> OnSpent;

    public void SetExternalCostMultiplier(float m)
    {
        externalCostMultiplier = Mathf.Clamp(m, externalCostMin, externalCostMax);
    }

    // 외부 조회용
    public float CurrentSeconds => _wallet != null ? _wallet.CurrentSeconds : 0f; // 남은 시간
    public bool IsRunning => _running;                                            // 작동 상태

    // 행동 비용 모듈(자동 부착)
    [HideInInspector] public TimeActionCostModule actionModule;


    void Start()
    {
        if (debugLog)
        {
            Debug.Log("[TEST] TimeSystemController Start() 호출됨");
            SpendSeconds(1f, "TestStart");
        }
    }

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

        if (debugLog)
            Debug.Log("[TIME] TimeSystemController Awake & Initialized");
    }

    void Update()
    {
        Tick(Time.deltaTime);                          // 기본 소모 처리

        if (_damageIFrameRemain > 0f)
            _damageIFrameRemain -= Time.deltaTime;    // i-프레임 감소

    }

    public void SpendForDamage(float damageAmount)    // PlayerController에서 호출
    {
        if (!_running || _wallet == null) return;     // 작동 체크
        if (damageAmount <= 0f) return;               // 방어
        if (_damageIFrameRemain > 0f) return;         // i-프레임 중이면 무시

        float seconds = damageAmount * Mathf.Max(0f, damageToSecondsScale); // 데미지→초 변환
        if (seconds <= 0f) return;

        _wallet.Spend(seconds, "Damage");             // 지갑에서 시간 차감
        _damageIFrameRemain = damageIFrameSeconds;    // i-프레임 리셋
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
        float baseDrain = baseDrainPerSecond * deltaTime; // 예시 변수명
        baseDrain *= externalCostMultiplier;              // 효율 배율 적용
        SpendSeconds(baseDrain, "Base");
    }

    // ===== 외부 API =====
    public void AddSeconds(float seconds, string reason = "") { _wallet?.Add(seconds, reason); }
    public void SpendSeconds(float seconds, string reason)
    {
        if (!_running || _wallet == null || seconds <= 0f) return;

        _wallet.Spend(seconds, reason);
        OnSpent?.Invoke(seconds, reason, externalCostMultiplier);

        if (debugLog)
        {
            float left = _wallet.CurrentSeconds;
            Debug.Log($"[TIME] -{seconds:F2}s  reason={reason}  mul={externalCostMultiplier:F2}  left={left:F2}s");
        }
    }
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

    public float GetWalkCostMultiplier()
    {
        return Mathf.Lerp(1f, externalCostMultiplier, walkEffFactor);
    }

    public float GetSprintCostMultiplier()
    {
        return Mathf.Lerp(1f, externalCostMultiplier, sprintEffFactor);
    }

    public float GetJumpCostMultiplier()
    {
        return Mathf.Lerp(1f, externalCostMultiplier, jumpEffFactor);
    }


}

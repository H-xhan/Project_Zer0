using System;
using UnityEngine;

/// 시간 자원을 관리하고, 행동별 시간 비용을 모듈에 위임하는 허브
[DisallowMultipleComponent]
public class TimeSystemController : MonoBehaviour
{
    [Header("Core")]
    [Tooltip("시작 시 지급할 시간(초)")]
    public float initialSeconds = 900f;

    [Tooltip("초당 기본 소모 시간(루프 진행 등)")]
    public float baseDrainPerSecond = 0f;

    [Tooltip("시작 시 시간 시스템을 자동으로 작동시킬지 여부")]
    public bool startRunning = true;

    [Header("Action Costs")]
    [Tooltip("걷는 동안 초당 차감할 시간(초)")]
    public float walkCostPerSecond = 1f;

    [Tooltip("뛰는 동안 초당 차감할 시간(초)")]
    public float sprintCostPerSecond = 2f;

    [Tooltip("점프 1회당 차감할 시간(초)")]
    public float jumpCostOnce = 3f;

    [Header("Damage Costs")]
    [Tooltip("데미지 1당 차감할 시간(초) 배율")]
    public float damageToSecondsScale = 1f;

    [Tooltip("피해 처리 후 추가 피해를 무시하는 시간(초)")]
    public float damageIFrameSeconds = 0.2f;

    [Header("Dynamic Cost Multiplier (from Efficiency)")]
    [Tooltip("EfficiencyModule에서 전달받는 시간 비용 배율")]
    [SerializeField] private float externalCostMultiplier = 1f;

    [Tooltip("외부 배율 최소값")]
    public float externalCostMin = 1f;

    [Tooltip("외부 배율 최대값")]
    public float externalCostMax = 3f;

    [Header("Efficiency Influence per Action")]
    [Tooltip("걷기 시간 비용에 효율 배율을 얼마나 반영할지")]
    [Range(0f, 10f)] public float walkEffFactor = 1f;

    [Tooltip("스프린트 시간 비용에 효율 배율을 얼마나 반영할지")]
    [Range(0f, 10f)] public float sprintEffFactor = 1f;

    [Tooltip("점프 시간 비용에 효율 배율을 얼마나 반영할지")]
    [Range(0f, 10f)] public float jumpEffFactor = 1f;


    [Tooltip("시간이 변경될 때 호출되는 이벤트 (새 값 전달)")]
    public event Action<float> OnTimeChanged;

    [Tooltip("시간이 0이 되었을 때 호출되는 이벤트")]
    public event Action OnTimeDepleted;

    [Tooltip("시간 소비 발생 시: (소비량, 이유, 외부배율)")]
    public event Action<float, string, float> OnSpent;

    // 내부 상태
    private TimeWallet _wallet;
    private bool _running;
    private float _damageIFrameRemain;

    // 행동 비용 전담 모듈
    public TimeActionCostModule actionModule { get; private set; }

    // 외부 조회용
    public float CurrentSeconds => _wallet != null ? _wallet.CurrentSeconds : 0f;
    public float MaxSeconds => initialSeconds;
    public bool IsRunning => _running;

    private void ApplyConfigFromDataController()
    {
        var data = DataController.Instance;
        if (data == null)
            return;

        var cfg = data.TimeConfig;
        if (cfg == null)
            return;

        // Core
        initialSeconds = cfg.initialSeconds;
        baseDrainPerSecond = cfg.baseDrainPerSecond;
        startRunning = cfg.startRunning;

        // Action Costs
        walkCostPerSecond = cfg.walkCostPerSecond;
        sprintCostPerSecond = cfg.sprintCostPerSecond;
        jumpCostOnce = cfg.jumpCostOnce;

        // Damage Costs
        damageToSecondsScale = cfg.damageToSecondsScale;
        damageIFrameSeconds = cfg.damageIFrameSeconds;

        // Dynamic Multiplier
        externalCostMin = cfg.externalCostMin;
        externalCostMax = cfg.externalCostMax;
        externalCostMultiplier = Mathf.Clamp(cfg.externalCostMultiplier, externalCostMin, externalCostMax);

        // Efficiency Influence
        walkEffFactor = cfg.walkEffFactor;
        sprintEffFactor = cfg.sprintEffFactor;
        jumpEffFactor = cfg.jumpEffFactor;
    }

    private void Awake()
    {
        ApplyConfigFromDataController();
        Initialize();

        // 행동 비용 모듈 확보 및 초기화
        actionModule = GetComponent<TimeActionCostModule>();
        if (actionModule == null)
        {
            actionModule = gameObject.AddComponent<TimeActionCostModule>();
            actionModule.hideFlags = HideFlags.HideInInspector;
        }
        actionModule.Initialize(this);
    }

    private void Update()
    {
        if (_running && _wallet != null)
            Tick(Time.deltaTime);

        if (_damageIFrameRemain > 0f)
            _damageIFrameRemain -= Time.deltaTime;
    }

    // 초기 설정 및 지갑 생성
    public void Initialize()
    {
        _wallet = new TimeWallet(initialSeconds);
        _wallet.OnChanged += HandleChangedInternal;
        _wallet.OnDepleted += HandleDepletedInternal;
        _running = startRunning;
    }

    // 매 프레임 기본 소모 처리
    public void Tick(float deltaTime)
    {
        if (baseDrainPerSecond <= 0f || deltaTime <= 0f)
            return;

        float cost = baseDrainPerSecond * deltaTime * externalCostMultiplier;
        SpendSeconds(cost, "Base");
    }

    // 효율 시스템에서 전달하는 외부 배율 설정
    public void SetExternalCostMultiplier(float multiplier)
    {
        externalCostMultiplier = Mathf.Clamp(multiplier, externalCostMin, externalCostMax);
    }

    // 시간 추가
    public void AddSeconds(float seconds, string reason = "")
    {
        if (_wallet == null || seconds <= 0f)
            return;

        _wallet.Add(seconds, reason);
    }

    // 시간 차감
    public void SpendSeconds(float seconds, string reason)
    {
        if (!_running || _wallet == null || seconds <= 0f)
            return;

        _wallet.Spend(seconds, reason);
        OnSpent?.Invoke(seconds, reason, externalCostMultiplier);
    }

    // 시스템 on/off
    public void SetRunning(bool running)
    {
        _running = running;
    }

    // 시간 리셋
    public void ResetTime(float seconds = -1f)
    {
        if (_wallet == null)
            return;

        float v = (seconds >= 0f) ? seconds : initialSeconds;
        _wallet.Reset(v);
    }

    // PlayerController에서 행동 시간 소모를 호출할 때 사용하는 래퍼
    public void SpendForWalkDelta(float deltaTime)
    {
        actionModule?.SpendForWalkDelta(deltaTime);
    }

    public void SpendForSprintDelta(float deltaTime)
    {
        actionModule?.SpendForSprintDelta(deltaTime);
    }

    public void SpendForJumpEvent()
    {
        actionModule?.SpendForJumpEvent();
    }

    // 데미지 기반 시간 차감
    public void SpendForDamage(float damageAmount)
    {
        if (!_running || _wallet == null)
            return;
        if (damageAmount <= 0f)
            return;
        if (_damageIFrameRemain > 0f)
            return;

        float seconds = damageAmount * Mathf.Max(0f, damageToSecondsScale);
        if (seconds <= 0f)
            return;

        _wallet.Spend(seconds, "Damage");
        _damageIFrameRemain = damageIFrameSeconds;
    }

    // 내부: 지갑 변경 이벤트를 외부 이벤트로 전달
    void HandleChangedInternal(float value)
    {
        OnTimeChanged?.Invoke(value);
    }

    // 내부: 시간 0 도달 이벤트 전달
    void HandleDepletedInternal()
    {
        OnTimeDepleted?.Invoke();
    }

    // TimeActionCostModule에서 사용하는 배율 계산용
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

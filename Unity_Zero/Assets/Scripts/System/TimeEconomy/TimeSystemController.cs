using System;
using UnityEngine;

/// 시간 자원을 관리하는 허브 (지갑 + 기본 소모 + 외부 배율)
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

    [Tooltip("시간이 변경될 때 호출되는 이벤트 (새 값 전달)")]
    public event Action<float> OnTimeChanged;

    [Tooltip("시간이 0이 되었을 때 호출되는 이벤트")]
    public event Action OnTimeDepleted;

    [Tooltip("시간 소비 발생 시: (소비량, 이유, 외부배율)")]
    public event Action<float, string, float> OnSpent;

    private TimeWallet _wallet;
    private bool _running;
    private float _damageIFrameRemain;

    public TimeActionCostModule actionModule { get; private set; }

    public float CurrentSeconds => _wallet != null ? _wallet.CurrentSeconds : 0f;
    public float MaxSeconds => initialSeconds;
    public bool IsRunning => _running;

    public float ExternalCostMultiplier => externalCostMultiplier;

    private void ApplyConfigFromDataController()
    {
        var data = DataController.Instance;
        if (data == null)
            return;

        var cfg = data.TimeConfig;
        if (cfg == null)
            return;

        initialSeconds = cfg.initialSeconds;
        baseDrainPerSecond = cfg.baseDrainPerSecond;
        startRunning = cfg.startRunning;

        damageToSecondsScale = cfg.damageToSecondsScale;
        damageIFrameSeconds = cfg.damageIFrameSeconds;

        externalCostMin = cfg.externalCostMin;
        externalCostMax = cfg.externalCostMax;
        externalCostMultiplier = Mathf.Clamp(cfg.externalCostMultiplier, externalCostMin, externalCostMax);
    }

    private void Awake()
    {
        // 1. 데이터(설정값) 먼저 불러오기
        ApplyConfigFromDataController();

        // 2. 시간 지갑 등 내부 시스템 초기화
        Initialize();

        // 3. 액션 모듈(비용 계산기) 연결 및 초기화
        actionModule = GetComponent<TimeActionCostModule>();
        if (actionModule == null)
        {
            actionModule = gameObject.AddComponent<TimeActionCostModule>();
            actionModule.hideFlags = HideFlags.HideInInspector;
        }
        actionModule.Initialize(this);

        Debug.Log($"[TimeSystem] Initialized. Seconds={initialSeconds}, Drain={baseDrainPerSecond}");
    }

    private void Update()
    {
        if (_running && _wallet != null)
            Tick(Time.deltaTime);

        if (_damageIFrameRemain > 0f)
            _damageIFrameRemain -= Time.deltaTime;
    }

    public void Initialize()
    {
        _wallet = new TimeWallet(initialSeconds);
        _wallet.OnChanged += HandleChangedInternal;
        _wallet.OnDepleted += HandleDepletedInternal;
        _running = startRunning;

        // 이 줄 추가: 시작하자마자 현재 값(예: 900초)을 UI에 브로드캐스트
        HandleChangedInternal(_wallet.CurrentSeconds);
    }

    // 매 프레임 기본 소모 처리
    public void Tick(float deltaTime)
    {
        if (baseDrainPerSecond <= 0f || deltaTime <= 0f)
            return;

        // 기본 소모는 외부 배율 없이 고정으로만 깎인다.
        float cost = baseDrainPerSecond * deltaTime;
        SpendSeconds(cost, "Base");
    }

    public void SetExternalCostMultiplier(float multiplier)
    {
        externalCostMultiplier = Mathf.Clamp(multiplier, externalCostMin, externalCostMax);
    }

    public void AddSeconds(float seconds, string reason = "")
    {
        if (_wallet == null || seconds <= 0f)
            return;

        _wallet.Add(seconds, reason);
    }

    public void SpendSeconds(float seconds, string reason)
    {
        if (!_running || _wallet == null || seconds <= 0f)
            return;

        _wallet.Spend(seconds, reason);
        OnSpent?.Invoke(seconds, reason, externalCostMultiplier);
    }

    public void SetRunning(bool running)
    {
        _running = running;
    }

    public void ResetTime(float seconds = -1f)
    {
        if (_wallet == null)
            return;

        float v = (seconds >= 0f) ? seconds : initialSeconds;
        _wallet.Reset(v);
    }

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

    private void HandleChangedInternal(float value)
    {
        OnTimeChanged?.Invoke(value);
    }

    private void HandleDepletedInternal()
    {
        OnTimeDepleted?.Invoke();
    }
}

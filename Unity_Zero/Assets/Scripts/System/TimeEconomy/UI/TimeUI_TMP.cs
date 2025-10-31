using UnityEngine;
using TMPro;
using System;

public class TimeUI_TMP : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TimeWallet wallet;
    [SerializeField] private TimeManager manager;
    [SerializeField] private TimeDebtSystem debtSystem; // 선택
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI infoText;

    [Header("Low Time Warning")]
    [SerializeField] private float warnThresholdSeconds = 60f;
    [SerializeField] private float blinkSpeed = 6f;

    [Header("Rate & Toast")]
    [SerializeField] private float rateLerp = 0.2f;     // 소모속도 표기 부드럽게
    [SerializeField] private float toastDuration = 2f;  // 최근 행동 토스트 표시 시간

    // 내부 상태
    private float _lastSeconds;
    private float _lastUpdateTime;
    private float _prevForRate;
    private float _rate;                 // (+)면 실측 감소율(초/초). UI에는 -_rate로 표기
    private string _lastActionText = "";
    private float _toastTimer = 0f;
    private float _lastSecondsPoll = -1f;

    private string Koreanize(string s)
    {
        // 예: "Jump -0.5s", "Sprinting -0.2s", "Move 3.2m -0.3s"
        return s
            .Replace("Jump", "점프")
            .Replace("Sprinting", "질주")
            .Replace("Move", "이동")
            .Replace("Reward", "보상")
            .Replace("Debt repayment", "부채 상환")
            .Replace("Upkeep tax", "유지비");
    }

    void Start()
    {
        if (wallet != null) Refresh(wallet.CurrentSeconds);
        else UpdateTexts(0f);
    }

    void Update()
    {
        // 토스트 타이머
        if (_toastTimer > 0f) _toastTimer -= Time.deltaTime;

        // 유지비/배율/부채는 매 프레임 갱신
        UpdateInfoLine();

        // 경고 깜빡임
        if (timeText != null && _lastSeconds <= warnThresholdSeconds)
        {
            float a = 0.5f + 0.5f * Mathf.Sin(Time.time * blinkSpeed);
            var col = timeText.color;
            col.a = Mathf.Lerp(0.45f, 1f, a);
            timeText.color = col;
        }

        if (wallet != null)
        {
            float nowSec = Mathf.Max(0f, wallet.CurrentSeconds);
            if (_lastSecondsPoll < 0f) _lastSecondsPoll = nowSec;

            float dt = Mathf.Max(0.0001f, Time.deltaTime);
            float instRate = (_lastSecondsPoll - nowSec) / dt; // 양수면 줄어드는 중
            _lastSecondsPoll = nowSec;

            // 부드럽게
            _rate = Mathf.Lerp(_rate, instRate, 0.2f);
        }
    }

    // ===== 콜백들 =====

    private void OnTxn(float deltaSeconds, string reason)
    {
        // 이유가 빈 문자열(기본 소모 등)이면 토스트 생략 가능
        if (string.IsNullOrEmpty(reason)) return;

        string sign = deltaSeconds >= 0 ? "+" : "-";
        float abs = Mathf.Abs(deltaSeconds);
        _lastActionText = $"{reason} {sign}{abs:0.##}s";
        _toastTimer = toastDuration;
    }

    private void Refresh(float seconds)
    {
        // 남은 시간 텍스트 갱신
        _lastSeconds = Mathf.Max(0f, seconds);
        UpdateTexts(_lastSeconds);

        // 실측 소모속도(초/초) 추정
        float now = Time.time;
        float dt = Mathf.Max(0.0001f, now - _lastUpdateTime);
        _lastUpdateTime = now;

        // (이전초 - 현재초) / dt => 양수면 감소 중
        float instRate = (_prevForRate - _lastSeconds) / dt;
        _prevForRate = _lastSeconds;
        _rate = Mathf.Lerp(_rate, instRate, rateLerp);
    }

    // ===== 표시 구성 =====

    private void UpdateTexts(float seconds)
    {
        if (timeText != null)
        {
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            timeText.text = $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
        }
        UpdateInfoLine();
    }

    private void UpdateInfoLine()
    {
        if (infoText == null) return;

        float mul = manager != null ? manager.CurrentZoneMultiplier : 1f;

        // Upkeep을 mm:ss 또는 h:mm:ss 로 표기
        string upkeepStr = "";
        if (manager != null)
        {
            float left = manager.TimeToNextUpkeep;
            TimeSpan us = TimeSpan.FromSeconds(left);
            string upStr = (us.TotalHours >= 1)
                ? $"{(int)us.TotalHours:00}:{us.Minutes:00}:{us.Seconds:00}"
                : $"{us.Minutes:00}:{us.Seconds:00}";
            upkeepStr = $" | 세금까지 {upStr}";
            // 영어로 하고 싶다면: upkeepStr = $" | Upkeep in {upStr}";
        }

        // Debt도 길어질 수 있으니 동일 포맷 적용
        string debtStr = "";
        if (debtSystem != null && debtSystem.CurrentDebtSeconds > 0f)
        {
            TimeSpan ds = TimeSpan.FromSeconds(debtSystem.CurrentDebtSeconds);
            string debtFmt = (ds.TotalHours >= 1)
                ? $"{(int)ds.TotalHours:00}:{ds.Minutes:00}:{ds.Seconds:00}"
                : $"{ds.Minutes:00}:{ds.Seconds:00}";
            debtStr = $" | 부채 {debtFmt}";
            // 영어: debtStr = $" | Debt {debtFmt}";
        }

        // 소모 속도는 음수로 표기(-s/s) → 절댓값이 클수록 빨리 닳는 중
        string rateStr = $" | 소모속도 {-_rate:0.00}s/s";

        // 최근 행동 토스트(2초간)
        string toast = (_toastTimer > 0f && !string.IsNullOrEmpty(_lastActionText))
            ? $"\n{_lastActionText}"
            : "";

        infoText.text = $"배율 x{mul:0.##}{rateStr}{upkeepStr}{debtStr}{toast}";
    }

    void Awake()
    {
        if (!wallet) wallet = FindFirstObjectByType<TimeWallet>();
        if (!manager) manager = FindFirstObjectByType<TimeManager>();
        if (!debtSystem) debtSystem = FindFirstObjectByType<TimeDebtSystem>();
    }

    void OnEnable()
    {
        if (wallet != null)
        {
            wallet.OnChanged += Refresh;
            wallet.OnTransaction += OnTxn;   // ✅ 트랜잭션 구독 (TimeWallet에 이벤트 추가되어 있어야 함)
        }
    }

    void OnDisable()
    {
        if (wallet != null)
        {
            wallet.OnChanged -= Refresh;
            wallet.OnTransaction -= OnTxn;
        }
    }
}

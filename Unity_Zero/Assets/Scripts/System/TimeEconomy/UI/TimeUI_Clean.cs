using TMPro;
using UnityEngine;
using System;

public class TimeUI_Clean : MonoBehaviour
{
    [Header("Refs")]
    public TimeManager manager;                       // 타임 매니저 참조
    public TimeWallet wallet;                         // 지갑
    public TimeDebtSystem debt;                       // 부채(선택)

    [Header("Texts")]
    public TextMeshProUGUI timeText;                  // 상단 시:분:초
    public TextMeshProUGUI valueMult;                 // 배율 표시
    public TextMeshProUGUI valueRate;                 // 소모속도
    public TextMeshProUGUI valueUpkeep;               // 유지비까지 남은 시간
    public TextMeshProUGUI valueBase;                 // 기본 소모(선택)
    public TMP_Text toastText;                        // ✅ 토스트 1개만 사용 (TMP_Text로 통일)

    [Header("Style")]
    public string labelMult = "배율";
    public string labelRate = "소모속도";
    public string labelUpkeep = "세금까지";
    public string labelBase = "기본소모";

    [Header("Toast")]
    [SerializeField] private float toastDuration = 2f; // 토스트 기본 지속 시간
    private float _toastTimer;                         // ✅ 타이머 1개만

    // 내부 상태
    private float _prevSec;                            // 이전 초(속도 계산용)
    private float _rate;                               // 추정 소모 속도(초/초)

    /// <summary>퀘스트/시스템 메시지를 잠깐 표시</summary>
    public void ShowToast(string message, float duration = -1f)
    {
        if (toastText == null)
        {
            Debug.LogWarning("[TimeUI] toastText 미할당");
            return;
        }

        toastText.text = message;
        _toastTimer = (duration > 0f) ? duration : toastDuration;
        toastText.enabled = true;
    }

    void Awake()
    {
        if (!manager) manager = FindFirstObjectByType<TimeManager>();
        if (!wallet) wallet = FindFirstObjectByType<TimeWallet>();
        if (!debt) debt = FindFirstObjectByType<TimeDebtSystem>();
    }

    void OnEnable()
    {
        if (wallet != null)
        {
            wallet.OnChanged += OnWalletChanged;
            wallet.OnTransaction += OnTxn;
        }
    }

    void OnDisable()
    {
        if (wallet != null)
        {
            wallet.OnChanged -= OnWalletChanged;
            wallet.OnTransaction -= OnTxn;
        }
    }

    void Update()
    {
        // ---- 실시간 소모속도 계산 ----
        if (wallet != null)
        {
            float now = wallet.CurrentSeconds;
            float dt = Mathf.Max(0.0001f, Time.deltaTime);
            float inst = (_prevSec - now) / dt;
            _rate = Mathf.Lerp(_rate, inst, 0.2f);
            _prevSec = now;
        }

        // ---- 토스트 타이머 처리 ----
        if (_toastTimer > 0f)
        {
            _toastTimer -= Time.deltaTime;
            if (toastText) toastText.enabled = true;
            if (_toastTimer <= 0f && toastText != null)
            {
                toastText.text = "";
                toastText.enabled = false;
            }
        }

        RefreshStatic();
    }

    void OnWalletChanged(float sec)
    {
        if (timeText)
        {
            var ts = TimeSpan.FromSeconds(Mathf.Max(0f, sec));
            timeText.text = $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
        }
    }

    void OnTxn(float delta, string reason)
    {
        if (string.IsNullOrEmpty(reason) || toastText == null) return;
        string sign = delta >= 0 ? "+" : "-";
        toastText.text = $"{K(reason)} {sign}{Mathf.Abs(delta):0.#}초";
        _toastTimer = toastDuration;
        toastText.enabled = true;
    }

    void RefreshStatic()
    {
        if (valueMult)
            valueMult.text = $"x{(manager ? manager.CurrentZoneMultiplier : 1f):0.##}";

        if (valueRate)
            valueRate.text = $"-{_rate:0.00}s/s".Replace("s/s", "초/초");

        if (valueUpkeep && manager != null)
        {
            var us = TimeSpan.FromSeconds(manager.TimeToNextUpkeep);
            string up = (us.TotalHours >= 1)
                ? $"{(int)us.TotalHours:00}:{us.Minutes:00}:{us.Seconds:00}"
                : $"{us.Minutes:00}:{us.Seconds:00}";
            valueUpkeep.text = up;
        }

        if (valueBase)
            valueBase.text = "-0.01s".Replace("s", "초");
    }

    string K(string s)
    {
        return s.Replace("Jump", "점프")
                .Replace("Sprinting", "질주")
                .Replace("Move", "이동")
                .Replace("Upkeep tax", "유지비")
                .Replace("Reward", "보상")
                .Replace("Debt repayment", "부채 상환");
    }
}

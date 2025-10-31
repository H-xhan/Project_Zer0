using TMPro;
using UnityEngine;
using System;

public class TimeUI_Clean : MonoBehaviour
{
    [Header("Refs")]
    public TimeManager manager;
    public TimeWallet wallet;
    public TimeDebtSystem debt;

    [Header("Texts")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI valueMult;
    public TextMeshProUGUI valueRate;
    public TextMeshProUGUI valueUpkeep;
    public TextMeshProUGUI valueBase;     // 선택
    public TextMeshProUGUI toastText;     // 선택

    [Header("Style")]
    public string labelMult = "배율";
    public string labelRate = "소모속도";
    public string labelUpkeep = "세금까지";
    public string labelBase = "기본소모";

    float _prevSec, _rate, _toastTimer;

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
        // 실시간 소모속도(초/초) 폴링
        if (wallet != null)
        {
            float now = wallet.CurrentSeconds;
            float dt = Mathf.Max(0.0001f, Time.deltaTime);
            float inst = (_prevSec - now) / dt; // 양수=줄어듦
            _rate = Mathf.Lerp(_rate, inst, 0.2f);
            _prevSec = now;
        }

        if (_toastTimer > 0f)
        {
            _toastTimer -= Time.deltaTime;
            if (toastText) toastText.enabled = true;
        }
        else if (toastText) toastText.enabled = false;

        RefreshStatic();
    }

    void OnWalletChanged(float sec)
    {
        // 상단 큰 타이머
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
        _toastTimer = 2f;
    }

    void RefreshStatic()
    {
        // 배율
        if (valueMult) valueMult.text = $"x{(manager ? manager.CurrentZoneMultiplier : 1f):0.##}";

        // 소모속도: 음수 표기(-초/초)
        if (valueRate) valueRate.text = $"-{_rate:0.00}s/s".Replace("s/s", "초/초");

        // 유지비까지
        if (valueUpkeep && manager != null)
        {
            var us = TimeSpan.FromSeconds(manager.TimeToNextUpkeep);
            string up = (us.TotalHours >= 1)
                ? $"{(int)us.TotalHours:00}:{us.Minutes:00}:{us.Seconds:00}"
                : $"{us.Minutes:00}:{us.Seconds:00}";
            valueUpkeep.text = up;
        }

        // 기본 소모(선택): 프레임당 표시는 지저분하면 숨겨도 OK
        if (valueBase) valueBase.text = "-0.01s".Replace("s", "초");
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

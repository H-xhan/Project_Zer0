using UnityEngine;
using TMPro;
using System;

public class TimeUI_TMP : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TimeWallet wallet;
    [SerializeField] private TimeManager manager;
    [SerializeField] private TimeDebtSystem debtSystem; // 선택: 없으면 null
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI infoText;  // 배율/유지비/부채 표기

    [Header("Low Time Warning")]
    [SerializeField] private float warnThresholdSeconds = 60f;
    [SerializeField] private float blinkSpeed = 6f; // 깜빡임 속도

    private float _lastSeconds;

    void Awake()
    {
        if (!wallet) wallet = FindFirstObjectByType<TimeWallet>();
        if (!manager) manager = FindFirstObjectByType<TimeManager>();
        if (!debtSystem) debtSystem = FindFirstObjectByType<TimeDebtSystem>();
    }

    void OnEnable()
    {
        if (wallet != null) wallet.OnChanged += Refresh;
    }

    void OnDisable()
    {
        if (wallet != null) wallet.OnChanged -= Refresh;
    }

    void Start()
    {
        if (wallet != null) Refresh(wallet.CurrentSeconds);
        else UpdateTexts(0f);
    }

    void Update()
    {
        // 유지비 타이머/배율/부채는 매 프레임 갱신
        UpdateInfoLine(_lastSeconds);

        // 경고 깜빡임
        if (timeText != null && _lastSeconds <= warnThresholdSeconds)
        {
            float a = 0.5f + 0.5f * Mathf.Sin(Time.time * blinkSpeed);
            var col = timeText.color;
            col.a = Mathf.Lerp(0.45f, 1f, a);
            timeText.color = col;
        }
    }

    private void Refresh(float seconds)
    {
        _lastSeconds = Mathf.Max(0f, seconds);
        UpdateTexts(_lastSeconds);
    }

    private void UpdateTexts(float seconds)
    {
        if (timeText != null)
        {
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            timeText.text = $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
        }
        UpdateInfoLine(seconds);
    }

    private void UpdateInfoLine(float seconds)
    {
        if (infoText == null) return;

        string debtStr = "";
        if (debtSystem != null && debtSystem.CurrentDebtSeconds > 0f)
        {
            TimeSpan ds = TimeSpan.FromSeconds(debtSystem.CurrentDebtSeconds);
            debtStr = $" | Debt {ds.Minutes:00}:{ds.Seconds:00}";
        }

        string upkeepStr = "";
        if (manager != null)
        {
            float left = manager.TimeToNextUpkeep;
            TimeSpan us = TimeSpan.FromSeconds(left);
            upkeepStr = $" | Upkeep in {us.Seconds:00}s";
        }

        float mul = manager != null ? manager.CurrentZoneMultiplier : 1f;
        infoText.text = $"x{mul:0.##}{upkeepStr}{debtStr}";
    }
}

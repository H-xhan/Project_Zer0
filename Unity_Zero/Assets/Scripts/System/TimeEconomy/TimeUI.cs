using UnityEngine;
using UnityEngine.UI;
using System;

public class TimeUI : MonoBehaviour
{
    [SerializeField] private TimeWallet wallet;
    [SerializeField] private Text timeText; // UnityEngine.UI.Text 사용 (간단)

    void Awake()
    {
        if (!wallet) wallet = FindFirstObjectByType<TimeWallet>();
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
    }

    private void Refresh(float seconds)
    {
        if (!timeText) return;

        TimeSpan ts = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));
        // HH:MM:SS 형식
        timeText.text = $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
    }
}

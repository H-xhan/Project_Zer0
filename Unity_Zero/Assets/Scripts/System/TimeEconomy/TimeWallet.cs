using UnityEngine;
using System;

public class TimeWallet : MonoBehaviour
{
    [SerializeField] private TimeConfig config;

    public float CurrentSeconds { get; private set; }

    public event Action OnDepleted;

    // 남은 시간 숫자 바뀔 때
    public event Action<float> OnChanged;
    // 트랜잭션(증/감 & 이유) 알림  → UI 토스트, 로그 등에 사용
    public event Action<float, string> OnTransaction;

    void Awake()
    {
        if (!config) config = Resources.Load<TimeConfig>("TimeConfig");
        if (!config) Debug.LogError("[TimeWallet] TimeConfig가 없습니다.");
    }

    public void ResetToInitial()
    {
        CurrentSeconds = Mathf.Max(0, config ? config.initialSeconds : 900);
        OnChanged?.Invoke(CurrentSeconds);
    }

    public void AddSeconds(float sec, string reason = "")
    {
        if (sec <= 0f) return;
        CurrentSeconds += sec;

        // 먼저 트랜잭션 이벤트
        OnTransaction?.Invoke(sec, reason);

        if (config && config.logTransactions && !string.IsNullOrEmpty(reason))
            Debug.Log($"[TimeWallet] +{sec:F2}s ({reason}), now {CurrentSeconds:F2}s");

        OnChanged?.Invoke(CurrentSeconds);
    }

    public void SpendSeconds(float sec, string reason = "")
    {
        if (sec <= 0f) return;
        CurrentSeconds -= sec;

        // 먼저 트랜잭션 이벤트
        OnTransaction?.Invoke(-sec, reason);

        if (config && config.logTransactions && !string.IsNullOrEmpty(reason))
            Debug.Log($"[TimeWallet] -{sec:F2}s ({reason}), now {CurrentSeconds:F2}s");

        OnChanged?.Invoke(CurrentSeconds);

        if (CurrentSeconds <= 0f)
        {
            CurrentSeconds = 0f;
            OnDepleted?.Invoke();
            // OnDepleted 이벤트를 쓰고 있다면 여기서 호출
            // OnDepleted?.Invoke();
        }
    }
}

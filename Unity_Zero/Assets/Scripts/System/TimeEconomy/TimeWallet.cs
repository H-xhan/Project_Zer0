using UnityEngine;
using System;

public class TimeWallet : MonoBehaviour
{
    // 남은 생존 시간(초). 소수점 허용.
    public float CurrentSeconds { get; private set; }

    public event Action<float> OnChanged;     // 남은 시간이 바뀔 때(초)
    public event Action OnDepleted;           // 0 이하가 되었을 때

    [SerializeField] private TimeConfig config;

    void Awake()
    {
        if (config == null)
        {
            Debug.LogError("[TimeWallet] TimeConfig가 비어있습니다.");
        }
    }

    // 초기화: 루프 시작 시 호출
    public void ResetToInitial()
    {
        CurrentSeconds = Mathf.Max(0, config != null ? config.initialSeconds : 900);
        OnChanged?.Invoke(CurrentSeconds);
    }

    // 시간 추가(+)
    public void AddSeconds(float sec, string reason = "")
    {
        CurrentSeconds += sec;
        if (config != null && config.logTransactions)
            Debug.Log($"[TimeWallet] +{sec:F2}s ({reason}), now {CurrentSeconds:F2}s");
        OnChanged?.Invoke(CurrentSeconds);
    }

    // 시간 차감(-)
    public void SpendSeconds(float sec, string reason = "")
    {
        if (sec <= 0f) return;
        CurrentSeconds -= sec;
        if (config != null && config.logTransactions)
            Debug.Log($"[TimeWallet] -{sec:F2}s ({reason}), now {CurrentSeconds:F2}s");

        OnChanged?.Invoke(CurrentSeconds);

        if (CurrentSeconds <= 0f)
        {
            CurrentSeconds = 0f;
            OnDepleted?.Invoke(); // 루프 종료/사망 처리 등
        }
    }
}

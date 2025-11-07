using System;
using UnityEngine;

/// 남은 시간을 관리하고 변경 이벤트를 제공하는 런타임 클래스
public class TimeWallet
{
    // 현재 남은 시간(초)
    public float CurrentSeconds { get; private set; }

    // 값 변경 및 0 도달 이벤트
    public event Action<float> OnChanged;
    public event Action OnDepleted;

    public TimeWallet(float initialSeconds)
    {
        CurrentSeconds = Mathf.Max(0f, initialSeconds);
        OnChanged?.Invoke(CurrentSeconds);
        if (CurrentSeconds <= 0f)
            OnDepleted?.Invoke();
    }

    // 특정 값으로 리셋
    public void Reset(float seconds)
    {
        CurrentSeconds = Mathf.Max(0f, seconds);
        OnChanged?.Invoke(CurrentSeconds);
        if (CurrentSeconds <= 0f)
            OnDepleted?.Invoke();
    }

    // 시간 추가
    public void Add(float seconds, string reason = "")
    {
        if (seconds <= 0f)
            return;

        CurrentSeconds += seconds;
        OnChanged?.Invoke(CurrentSeconds);
    }

    // 시간 차감
    public void Spend(float seconds, string reason = "")
    {
        if (seconds <= 0f)
            return;

        CurrentSeconds -= seconds;

        if (CurrentSeconds <= 0f)
        {
            CurrentSeconds = 0f;
            OnChanged?.Invoke(CurrentSeconds);
            OnDepleted?.Invoke();
            return;
        }

        OnChanged?.Invoke(CurrentSeconds);
    }
}

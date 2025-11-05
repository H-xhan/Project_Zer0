using UnityEngine;
using System;

/// 남은 시간을 보관하고 변경을 알리는 순수 런타임 클래스
public class TimeWallet
{
    public float CurrentSeconds { get; private set; } // 현재 보유 시간(초)

    // 이벤트: 값 변경, 바닥(0초) 도달
    public event Action<float> OnChanged;             // 값이 바뀔 때 (새 값)
    public event Action OnDepleted;                   // 0초가 되었을 때

    public TimeWallet(float initialSeconds)           // 생성자: 시작 시간 세팅
    {
        CurrentSeconds = Mathf.Max(0f, initialSeconds); // 음수 방지
        OnChanged?.Invoke(CurrentSeconds);               // 초기 알림(필요 시)
    }

    public void Reset(float seconds)                  // 임의의 값으로 리셋
    {
        CurrentSeconds = Mathf.Max(0f, seconds);        // 음수 방지
        OnChanged?.Invoke(CurrentSeconds);              // 변경 알림
        if (CurrentSeconds <= 0f) OnDepleted?.Invoke(); // 바로 0이면 소진 이벤트
    }

    public void Add(float seconds, string reason = "") // 시간 지급(+) 
    {
        if (seconds <= 0f) return;                      // 방어
        CurrentSeconds += seconds;                      // 더하기
        OnChanged?.Invoke(CurrentSeconds);              // 알림
    }

    public void Spend(float seconds, string reason = "")// 시간 차감(-)
    {
        if (seconds <= 0f) return;                      // 방어
        CurrentSeconds -= seconds;                      // 빼기
        if (CurrentSeconds <= 0f)                       // 바닥 체크
        {
            CurrentSeconds = 0f;                        // 하한 보정
            OnChanged?.Invoke(CurrentSeconds);          // 알림
            OnDepleted?.Invoke();                       // 소진 이벤트
            return;                                     // 종료
        }
        OnChanged?.Invoke(CurrentSeconds);              // 일반 알림
    }
}

using UnityEngine;

public class TimeDebtSystem : MonoBehaviour
{
    [Tooltip("초 단위 이자율/초 (예: 0.001 = 초당 0.1% 증가)")]
    public float interestPerSecond = 0.001f;

    public float CurrentDebtSeconds { get; private set; } // ⏱️ 빚(초)
    private TimeWallet _wallet;

    void Awake()
    {
        _wallet = FindFirstObjectByType<TimeWallet>();
    }

    void Update()
    {
        // 빚이 있으면 시간 경과에 따라 이자 증가
        if (CurrentDebtSeconds > 0f)
        {
            float interest = CurrentDebtSeconds * interestPerSecond * Time.deltaTime;
            CurrentDebtSeconds += interest;
        }
    }

    // 시간을 빌림(즉시 플레이어에게 지급하고, 부채로 기록)
    public void BorrowSeconds(float seconds)
    {
        if (seconds <= 0f || _wallet == null) return;
        CurrentDebtSeconds += seconds;
        _wallet.AddSeconds(seconds, "Borrowed time"); // ⏱️ 대출 지급
    }

    // 빚 상환: 플레이어 남은 시간에서 차감하여 빚을 줄임
    public void RepaySeconds(float seconds)
    {
        if (seconds <= 0f || _wallet == null || CurrentDebtSeconds <= 0f) return;

        float pay = Mathf.Min(seconds, _wallet.CurrentSeconds, CurrentDebtSeconds);
        if (pay > 0f)
        {
            _wallet.SpendSeconds(pay, "Debt repayment"); // ⏱️ 상환
            CurrentDebtSeconds -= pay;
        }
    }
}

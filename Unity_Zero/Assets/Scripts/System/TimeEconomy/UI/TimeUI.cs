using UnityEngine;
using TMPro;

/// 플레이 화면에 보유 시간을 표시하는 HUD
/// - TimeSystemController에서 남은 시간을 읽어와 mm:ss로 표시
/// - 값이 바뀔 때마다 이벤트로 자동 갱신
public class TimeUI : MonoBehaviour
{
    [Header("References")]
    public TimeSystemController timeSystem;     // 시간 시스템 허브
    public TextMeshProUGUI textTimer;           // TMP 텍스트 오브젝트

    private void Start()
    {
        // 허브가 지정 안 되어 있으면 자동으로 찾음
        if (!timeSystem)
            timeSystem = FindFirstObjectByType<TimeSystemController>();

        // 이벤트 구독
        if (timeSystem != null)
        {
            // TimeWallet이 초기화되어 있는지 확인
            var field = typeof(TimeSystemController).GetField("_wallet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var wallet = field?.GetValue(timeSystem) as TimeWallet;
            if (wallet != null)
                wallet.OnChanged += UpdateTimerText;
        }

        // 시작 시 즉시 갱신
        UpdateTimerText(timeSystem != null ? timeSystem.CurrentSeconds : 0f);
    }

    // 이벤트로 호출됨
    private void UpdateTimerText(float seconds)
    {
        if (textTimer == null) return;

        seconds = Mathf.Max(0f, seconds);
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int sec = Mathf.FloorToInt(seconds % 60f);

        textTimer.text = $"{minutes:00}:{sec:00}";
    }
}

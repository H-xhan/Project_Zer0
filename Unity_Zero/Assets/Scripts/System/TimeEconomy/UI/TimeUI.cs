using UnityEngine;
using TMPro;

/// 남은 시간을 mm:ss 형식으로 표시하는 HUD
public class TimeUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("시간 시스템 허브 (비워두면 자동 탐색)")]
    public TimeSystemController timeSystem;

    [Tooltip("시간 텍스트를 표시할 TMP UI")]
    public TextMeshProUGUI textTimer;

    private void OnEnable()
    {
        if (timeSystem == null)
            timeSystem = FindFirstObjectByType<TimeSystemController>();

        if (timeSystem != null)
        {
            timeSystem.OnTimeChanged += UpdateTimerText;
            UpdateTimerText(timeSystem.CurrentSeconds);
        }
        else
        {
            UpdateTimerText(0f);
        }
    }

    private void OnDisable()
    {
        if (timeSystem != null)
            timeSystem.OnTimeChanged -= UpdateTimerText;
    }

    // 남은 시간 값을 UI에 반영
    private void UpdateTimerText(float seconds)
    {
        if (textTimer == null)
            return;

        seconds = Mathf.Max(0f, seconds);
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int sec = Mathf.FloorToInt(seconds % 60f);

        textTimer.text = $"{minutes:00}:{sec:00}";
    }
}

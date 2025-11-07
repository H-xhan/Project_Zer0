using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 효율(0~100%)을 표시하는 UI. 효율이 낮을수록 행동 비용이 비싸짐.
/// 실제 배율 계산은 StaminaModule.ComputeCostMultiplier()에서 처리되고,
/// 여기서는 개발자/유저용 시각화만 담당.
/// </summary>
public class EfficiencyUI : MonoBehaviour
{
    [Header("Refs")]
    public PlayerController player;            // 플레이어 참조(없으면 자동 탐색)
    public Slider slider;                      // 0~1 슬라이더
    public TMP_Text percentText;               // "83%" 같은 숫자 표시(선택)

    [Header("Display")]
    [Tooltip("게이지 보간 속도 (값이 클수록 즉시 반응)")]
    public float lerpSpeed = 12f;

    // 색상 그라데이션(선택): 효율 0%일 때 left, 100%일 때 right
    public Color lowColor = new Color(0.85f, 0.35f, 0.25f);
    public Color highColor = new Color(0.25f, 0.85f, 0.55f);
    public Image fillImage;                    // 슬라이더 Fill 이미지(선택)

    float _visual; // 보간용

    void Awake()
    {
        if (!player) player = FindFirstObjectByType<PlayerController>();
        if (slider)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            _visual = 1f; // 시작 100%
            slider.value = _visual;
        }
    }

    void Update()
    {
        if (!player || !slider) return;

        // StaminaModule을 효율 제공자로 사용 (0~1)
        float t = player.efficiency.Normalized(); // 없다면 아래 주석 참고
        _visual = Mathf.Lerp(_visual, t, Time.deltaTime * lerpSpeed);
        slider.value = _visual;

        if (percentText)
        {
            int pct = Mathf.RoundToInt(_visual * 100f);
            percentText.text = pct + "%";
        }

        if (fillImage)
        {
            fillImage.color = Color.Lerp(lowColor, highColor, _visual);
        }
    }
}

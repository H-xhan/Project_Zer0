using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    [Header("Refs")]
    public PlayerController controller; // PlayerController를 넣는다. 비워두면 자동 탐색
    public Image fillImage;             // Image type Filled
    public Slider slider;               // UI Slider (optional)

    [Header("Display")]
    public bool useSlider = false;      // true면 slider 사용, false면 fillImage 사용
    public float lerpSpeed = 10f;       // smooth ui

    float _shown;

    void Awake()
    {
        if (controller == null)
            controller = FindFirstObjectByType<PlayerController>();

        // 초기값
        _shown = GetNorm();
        Apply(_shown, true);
    }

    void Update()
    {
        float target = GetNorm();
        _shown = Mathf.Lerp(_shown, target, lerpSpeed * Time.unscaledDeltaTime);
        Apply(_shown, false);
    }

    float GetNorm()
    {
        if (controller == null) return 0f;
        if (controller.stamina == null) return 0f;
        return Mathf.Clamp01(controller.stamina.Normalized());
    }

    void Apply(float v, bool instant)
    {
        if (useSlider)
        {
            if (slider != null)
            {
                if (instant) slider.value = v;
                else slider.value = v;
            }
        }
        else
        {
            if (fillImage != null)
            {
                if (instant) fillImage.fillAmount = v;
                else fillImage.fillAmount = v;
            }
        }
    }
}

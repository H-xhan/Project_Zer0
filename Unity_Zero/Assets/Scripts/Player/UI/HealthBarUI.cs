using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public PlayerController controller;
    public Slider slider;            // fill type slider
    public bool showNumbers = true;
    public UnityEngine.UI.Text numberText; // optional

    void OnEnable()
    {
        if (controller == null)
            controller = FindFirstObjectByType<PlayerController>();

        if (controller != null && controller.health != null)
        {
            controller.health.OnChanged += OnHealthChanged;
            // 초기 반영
            OnHealthChanged(controller.health.current, controller.health.max);
        }
    }

    void OnDisable()
    {
        if (controller != null && controller.health != null)
            controller.health.OnChanged -= OnHealthChanged;
    }

    void OnHealthChanged(float cur, float max)
    {
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = max;
            slider.value = cur;
        }

        if (showNumbers && numberText != null)
            numberText.text = ((int)cur).ToString() + " / " + ((int)max).ToString();
    }
}

using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    public PlayerStamina stamina;     // Player의 PlayerStamina 참조
    public Slider slider;             // UI Slider 참조

    void Awake()
    {
        if (stamina == null) stamina = Object.FindAnyObjectByType<PlayerStamina>();
        if (slider == null) slider = GetComponentInChildren<Slider>(true);
    }

    void OnEnable()
    {
        if (stamina != null) stamina.OnStaminaChanged += OnStaminaChanged;
        // 초기값 반영
        if (stamina != null) OnStaminaChanged(stamina.current, stamina.maxStamina);
    }

    void OnDisable()
    {
        if (stamina != null) stamina.OnStaminaChanged -= OnStaminaChanged;
    }

    void OnStaminaChanged(float cur, float max)
    {
        if (slider == null) return;
        slider.minValue = 0f;
        slider.maxValue = max;
        slider.value = cur;
    }
}

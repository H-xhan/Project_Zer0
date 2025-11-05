using UnityEngine;

[System.Serializable]
public class StaminaModule
{
    [Header("Capacity")]
    public float max = 100f;
    public float current = 100f;

    [Header("Drain rates per second")]
    public float walkDrainPerSecond = 5f;     // moving without sprint
    public float sprintDrainPerSecond = 20f;  // moving with sprint

    [Header("Instant costs")]
    public float jumpCost = 15f;              // spend once when jump triggered

    [Header("Regen")]
    public float idleRegenPerSecond = 15f;    // regen only when idle
    public bool regenOnlyWhenIdle = true;

    [Header("Rules")]
    public float minToSprint = 5f;            // minimal stamina to allow sprint
    public bool clampOnInit = true;

    public void Init()
    {
        if (clampOnInit)
            current = Mathf.Clamp(current, 0f, max);
    }

    // call once per frame
    public void Tick(float dt, bool isMoving, bool isSprinting, bool jumpTriggered)
    {
        // instant jump cost
        if (jumpTriggered && jumpCost > 0f)
            TrySpend(jumpCost);

        // drains
        if (isMoving)
        {
            if (isSprinting) Drain(sprintDrainPerSecond * dt);
            else Drain(walkDrainPerSecond * dt);
        }
        else
        {
            // regen only when idle
            if (!regenOnlyWhenIdle || !isMoving)
                Regen(idleRegenPerSecond * dt);
        }
    }

    public bool CanSprint()
    {
        return current >= minToSprint;
    }

    public bool TrySpend(float amount)
    {
        if (amount <= 0f) return true;
        if (current < amount) return false;
        current = Mathf.Max(0f, current - amount);
        return true;
    }

    public void Gain(float amount)
    {
        current = Mathf.Clamp(current + Mathf.Max(0f, amount), 0f, max);
    }

    public float Normalized()
    {
        return (max > 0f) ? current / max : 0f;
    }

    void Drain(float amount)
    {
        if (amount <= 0f) return;
        current = Mathf.Max(0f, current - amount);
    }

    void Regen(float amount)
    {
        if (amount <= 0f) return;
        current = Mathf.Min(max, current + amount);
    }
}

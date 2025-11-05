using System;
using UnityEngine;

[Serializable]
public class HealthModule
{
    [Header("Stats")]
    public float max = 100f;
    public float current = 100f;

    [Header("Regen")]
    public bool useRegen = false;
    public float regenPerSecond = 5f;
    public float regenDelay = 2f;

    [Header("Rules")]
    public bool clampOnInit = true;
    public bool invulnerable = false;
    public float invulnTime = 0.2f;

    // events
    public Action<float, float> OnChanged;   // current, max
    public Action<float> OnDamaged;          // damage amount
    public Action<float> OnHealed;           // heal amount
    public Action OnDead;

    // internal
    float _timeSinceHit = 0f;
    float _invulnTimer = 0f;

    public void Init()
    {
        if (clampOnInit) current = Mathf.Clamp(current, 0f, max);
        RaiseChanged();
    }

    public void Tick(float dt)
    {
        if (_invulnTimer > 0f) _invulnTimer -= dt;
        if (_timeSinceHit < regenDelay) _timeSinceHit += dt;

        if (useRegen && _timeSinceHit >= regenDelay && current > 0f && current < max)
        {
            current = Mathf.Min(max, current + regenPerSecond * dt);
            RaiseChanged();
        }
    }

    public bool IsDead()
    {
        return current <= 0f;
    }

    public bool CanTakeDamage()
    {
        if (invulnerable) return false;
        if (_invulnTimer > 0f) return false;
        return current > 0f;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;
        if (!CanTakeDamage()) return;

        current = Mathf.Max(0f, current - amount);
        _timeSinceHit = 0f;
        _invulnTimer = invulnTime;

        OnDamaged?.Invoke(amount);
        RaiseChanged();

        if (current <= 0f) OnDead?.Invoke();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        if (current <= 0f) return;

        float before = current;
        current = Mathf.Min(max, current + amount);
        float gained = current - before;

        if (gained > 0f)
        {
            OnHealed?.Invoke(gained);
            RaiseChanged();
        }
    }

    public float Normalized()
    {
        return max > 0f ? current / max : 0f;
    }

    void RaiseChanged()
    {
        OnChanged?.Invoke(current, max);
    }
}

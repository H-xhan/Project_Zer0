using System;
using UnityEngine;

[Serializable]
public class HealthModule
{
    [Header("Stats")]
    public float max = 100f;                         // 최대 체력
    public float current = 100f;                     // 현재 체력

    [Header("Regen")]
    public bool useRegen = false;                    // 리젠 사용 여부
    public float regenPerSecond = 5f;                // 초당 회복량
    public float regenDelay = 2f;                    // 피해 후 회복까지 지연

    [Header("Rules")]
    public bool clampOnInit = true;                  // Init 시 범위 보정
    public bool invulnerable = false;                // 완전 무적(피해 무시)
    public float invulnTime = 0.2f;                  // 피격 후 잠깐 무적

    // 이벤트
    public Action<float, float> OnChanged;           // (current, max)
    public Action<float> OnDamaged;                  // 받은 피해량
    public Action<float> OnHealed;                   // 실제 회복량
    public Action OnDead;                            // 사망
    public Action OnDied;                            // 사망(호환용, OnDead와 동일 시점 호출)

    // 내부 타이머
    float _timeSinceHit = 0f;                        // 마지막 피격 이후 경과
    float _invulnTimer = 0f;                         // 잠깐 무적 타이머
    bool _dead = false;                              // 사망 플래그

    // 컨트롤러 인스펙터 값 → 런타임에 반영하고 싶을 때 사용
    public void SyncSettings(float newMax, bool newUseRegen, float newRegenPerSec, float newRegenDelay, bool newInvuln, float newInvulnTime)
    {
        bool maxChanged = !Mathf.Approximately(max, newMax);
        max = Mathf.Max(1f, newMax);                // 0 방지
        useRegen = newUseRegen;
        regenPerSecond = Mathf.Max(0f, newRegenPerSec);
        regenDelay = Mathf.Max(0f, newRegenDelay);
        invulnerable = newInvuln;
        invulnTime = Mathf.Max(0f, newInvulnTime);

        if (maxChanged)
        {
            // 최대치가 바뀌면 현재 체력도 범위 내로 보정
            current = Mathf.Clamp(current, 0f, max);
            RaiseChanged();
        }
    }

    public void Init()
    {
        if (clampOnInit) current = Mathf.Clamp(current, 0f, max);
        _dead = current <= 0f;
        _invulnTimer = 0f;
        _timeSinceHit = regenDelay;                  // 시작 시 즉시 리젠 허용을 원하면 regenDelay로
        RaiseChanged();
    }

    public void Tick(float dt)
    {
        if (_invulnTimer > 0f) _invulnTimer -= dt;
        if (_timeSinceHit < regenDelay) _timeSinceHit += dt;

        // 리젠
        if (useRegen && !_dead && current < max && _timeSinceHit >= regenDelay)
        {
            float before = current;
            current = Mathf.Min(max, current + regenPerSecond * dt);
            float gained = current - before;
            if (gained > 0f)
            {
                OnHealed?.Invoke(gained);
                RaiseChanged();
            }
        }
    }

    public bool IsDead() => _dead;

    public bool CanTakeDamage()
    {
        if (_dead) return false;
        if (invulnerable) return false;
        if (_invulnTimer > 0f) return false;
        return true;
    }

    // UIDamageZone 등에서 기대하는 이름(래퍼)
    public void ApplyDamage(float amount)            // 외부 호환용
    {
        TakeDamage(amount);
    }

    // 실제 피해 처리
    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;
        if (!CanTakeDamage()) return;

        float before = current;
        current = Mathf.Max(0f, current - amount);
        float taken = before - current;

        _timeSinceHit = 0f;                          // 리젠 지연 리셋
        _invulnTimer = invulnTime;

        if (taken > 0f) OnDamaged?.Invoke(taken);
        RaiseChanged();

        if (current <= 0f && !_dead)
        {
            _dead = true;
            OnDead?.Invoke();
            OnDied?.Invoke();                        // 호환 이벤트도 함께 호출
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        if (_dead) return;

        float before = current;
        current = Mathf.Min(max, current + amount);
        float gained = current - before;

        if (gained > 0f)
        {
            OnHealed?.Invoke(gained);
            RaiseChanged();
        }
    }

    // 즉시 사망 처리(연출/트랩 등에서 사용)
    public void Kill()
    {
        if (_dead) return;
        current = 0f;
        _invulnTimer = 0f;
        _timeSinceHit = regenDelay;
        _dead = true;
        RaiseChanged();
        OnDead?.Invoke();
        OnDied?.Invoke();
    }

    // 부활: 체력 회복(기본=풀)
    public void Revive(float hp = -1f)
    {
        float target = (hp < 0f) ? max : Mathf.Clamp(hp, 0f, max);
        current = target;
        _dead = current <= 0f;
        _invulnTimer = 0f;
        _timeSinceHit = regenDelay;
        RaiseChanged();
    }

    // 현재/최대값 직접 세팅(툴/디버그용)
    public void SetCurrent(float value)
    {
        current = Mathf.Clamp(value, 0f, max);
        _dead = current <= 0f;
        RaiseChanged();
    }

    public void SetMax(float value, bool keepRatio = true)
    {
        value = Mathf.Max(1f, value);
        if (keepRatio)
        {
            float ratio = Normalized();
            max = value;
            current = Mathf.Clamp(ratio * max, 0f, max);
        }
        else
        {
            max = value;
            current = Mathf.Clamp(current, 0f, max);
        }
        RaiseChanged();
    }

    public float Normalized() => max > 0f ? current / max : 0f;

    void RaiseChanged() => OnChanged?.Invoke(current, max);
}

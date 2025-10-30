using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;   // 최대 체력
    public float currentHealth;      // 현재 체력

    [Header("UI References")]
    public Slider healthBar;         // 체력바 슬라이더
    public Image healthFill;         // ✅ Slider의 Fill(Image) 참조

    [Header("Health Bar Colors")]
    public Color healthyColor = new Color(0f, 0.8f, 0f);   // 초록  (약 80~100%)
    public Color warningColor = new Color(1f, 0.8f, 0f);   // 노랑  (약 30~80%)
    public Color criticalColor = new Color(0.9f, 0.1f, 0f); // 빨강  (0~30%)

    [Header("Critical FX")]
    public float criticalThreshold = 0.3f;  // 체력비 30% 이하일 때 위험 상태
    public float pulseSpeed = 6f;           // 깜빡임 속도(크면 더 빨리)
    public float minAlpha = 0.6f;           // 깜빡임 시 최소 투명도
    public float maxAlpha = 1.0f;           // 깜빡임 시 최대 투명도

    [Header("FX References")]
    public DamageFlash damageFlash;

    private Animator anim;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();

        // 초기 UI 세팅
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;                 // 슬라이더 최대값 = 최대 체력
            healthBar.value = currentHealth;                // 시작값 = 현재 체력
        }
        UpdateHealthBarVisual();                            // ✅ 시작 시 색/연출 동기화
    }

    void Update()
    {
        // 테스트 입력 (원하면 나중에 제거)
        if (Input.GetKeyDown(KeyCode.H)) TakeDamage(20f);   // H로 20 데미지
        if (Input.GetKeyDown(KeyCode.J)) Heal(10f);         // J로 10 회복

        // 위험 구간이면 깜빡임 적용 (Time.deltaTime 기반)
        ApplyCriticalPulseIfNeeded();
    }

    // ==== 퍼블릭 API ====
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;                                  // 체력 감소
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();                                        // UI 값 갱신

        // 🔔 여기서 플래시 호출 (현재 체력비 전달)
        if (damageFlash != null)
        {
            float ratio = currentHealth / Mathf.Max(1f, maxHealth);
            damageFlash.Flash(amount, ratio, criticalThreshold);  // amount, 체력비, 위험 임계값 사용
        }

        if (currentHealth <= 0) Die();
        // else if (anim) anim.SetTrigger("Hit");
    }

    // ... (나머지 그대로)

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;                            // 체력 회복
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();                                  // 값/비주얼 갱신
    }

    // ==== 내부 로직 ====
    private void UpdateHealthBar()
    {
        if (healthBar != null) healthBar.value = currentHealth; // 슬라이더 값 갱신
        UpdateHealthBarVisual();                                  // 색상/연출 갱신
    }

    private void UpdateHealthBarVisual()
    {
        if (healthFill == null) return;

        float t = currentHealth / Mathf.Max(1f, maxHealth); // 0~1 체력비

        // ✔ 색상 보간: 0~0.3=빨강, 0.3~0.8=노랑, 0.8~1=초록으로 자연스럽게
        if (t <= criticalThreshold)
        {
            // 0~0.3: 빨강 고정(깜빡임은 alpha만 변함)
            healthFill.color = criticalColor;
        }
        else if (t <= 0.8f)
        {
            // 0.3~0.8: 빨강→노랑 보간
            float k = Mathf.InverseLerp(criticalThreshold, 0.8f, t);
            healthFill.color = Color.Lerp(criticalColor, warningColor, k);
        }
        else
        {
            // 0.8~1: 노랑→초록 보간
            float k = Mathf.InverseLerp(0.8f, 1f, t);
            healthFill.color = Color.Lerp(warningColor, healthyColor, k);
        }
    }

    private void ApplyCriticalPulseIfNeeded()
    {
        if (healthFill == null) return;

        float ratio = currentHealth / Mathf.Max(1f, maxHealth); // 체력비(0~1)
        Color c = healthFill.color;                             // 현재 색상 유지

        if (ratio <= criticalThreshold)
        {
            // 위험 구간: 알파를 sin으로 깜빡이게
            float a = Mathf.Lerp(minAlpha, maxAlpha,
                                 0.5f * (1f + Mathf.Sin(Time.time * pulseSpeed)));
            c.a = a;                    // 알파만 변경 (색상은 빨강 유지)
        }
        else
        {
            c.a = 1f;                   // 안전/경고 구간: 깜빡임 끔
        }

        healthFill.color = c;           // 적용
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("플레이어 사망!");
        if (anim != null) anim.SetTrigger("Die");
        GetComponent<PlayerMovement>().enabled = false;     // 이동 비활성화
    }

    
}

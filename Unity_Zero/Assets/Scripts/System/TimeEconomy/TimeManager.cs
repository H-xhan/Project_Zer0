using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(TimeWallet))]
public class TimeManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private TimeConfig config;

    [Header("Refs")]
    [SerializeField] private TimeWallet wallet;

    [Header("State")]
    public bool running = true;
    private float zoneMultiplier = 1f;
    private float upkeepTimer;

    public float CurrentZoneMultiplier => zoneMultiplier;
    public float TimeToNextUpkeep => Mathf.Max(0f,
        (config != null ? config.periodicUpkeepSeconds : 0f) - upkeepTimer);

    public TimeWallet Wallet => wallet;

    void Reset()
    {
        if (!wallet) wallet = GetComponent<TimeWallet>();
    }

    void OnValidate()
    {
        if (!wallet) wallet = GetComponent<TimeWallet>();
    }

    void Awake()
    {
        if (!wallet) wallet = GetComponent<TimeWallet>();

        if (!config)
            config = Resources.Load<TimeConfig>("TimeConfig");

        if (!config)
            Debug.LogError("[시간 관리자] ⚠️ TimeConfig 파일을 찾을 수 없습니다. 'Assets/Resources/TimeConfig.asset'을 생성해주세요.");
        else
            Debug.Log("[시간 관리자] ✅ TimeConfig 로드 완료");
    }

    void Start()
    {
        if (!config) { enabled = false; return; }

        wallet.ResetToInitial();
        upkeepTimer = 0f;

        wallet.OnDepleted += HandleDepleted;

        Debug.Log("[시간 관리자] 💾 시간 루프 시작됨");
    }

    void OnDestroy()
    {
        if (wallet != null) wallet.OnDepleted -= HandleDepleted;
    }

    void Update()
    {
        if (!running || config == null) return;

        float delta = Time.deltaTime * config.baseDrainPerSecond * zoneMultiplier;
        if (delta > 0f) wallet.SpendSeconds(delta, "기본 시간 소모");

        upkeepTimer += Time.deltaTime;
        if (upkeepTimer >= config.periodicUpkeepSeconds)
        {
            upkeepTimer = 0f;

            if (config.upkeepFlatCost > 0f)
                wallet.SpendSeconds(config.upkeepFlatCost, "정기 유지비 지출");

            Debug.Log($"[시간 관리자] 🕒 유지비({config.upkeepFlatCost}s) 차감됨");
        }
    }

    public void SetZoneMultiplier(float mul)
    {
        zoneMultiplier = Mathf.Max(0f, mul);
        Debug.Log($"[시간 관리자] 구역 배율 변경됨 → x{zoneMultiplier:0.##}");
    }

    private void HandleDepleted()
    {
        running = false;
        Debug.Log("💀 [시간 관리자] 시간이 모두 소진되었습니다. 루프 종료 또는 사망 처리로 이동합니다.");

        if (config.loopTaxRate > 0f)
        {
            Debug.Log($"💰 [시간 관리자] 루프 세금이 적용됩니다: {config.loopTaxRate * 100f:F1}%");
        }

        // TODO: 이후 루프 재시작/기억 복원 등 처리 연결
    }
}

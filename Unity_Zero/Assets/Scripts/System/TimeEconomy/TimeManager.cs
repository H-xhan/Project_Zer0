using UnityEngine;

[DefaultExecutionOrder(-100)] // 일찍 업데이트되게
public class TimeManager : MonoBehaviour
{
    [SerializeField] private TimeConfig config;
    [SerializeField] private TimeWallet wallet;

    [Header("State")]
    public bool running = true;                  // 일시정지/메뉴용
    private float zoneMultiplier = 1f;           // 구역 배율(트리거로 변경)
    private float upkeepTimer;                   // 유지비 타이머

    public float CurrentZoneMultiplier => zoneMultiplier;          // UI에서 표기용
    public float TimeToNextUpkeep => Mathf.Max(0f, (config != null ? config.periodicUpkeepSeconds : 0f) - upkeepTimer);

    void Awake()
    {
        if (!wallet) wallet = FindFirstObjectByType<TimeWallet>();
        if (!config) Debug.LogError("[TimeManager] TimeConfig이 필요합니다.");

        if (!config) config = Resources.Load<TimeConfig>("TimeConfig");

        if (!config) Debug.LogError("[TimeManager] TimeConfig를 찾을 수 없습니다. Assets/Resources/TimeConfig.asset 생성해주세요.");
    }

    void Start()
    {
        if (!config)
            config = Resources.Load<TimeConfig>("TimeConfig");
        // 루프 시작: 초기값 세팅
        wallet.ResetToInitial();

        upkeepTimer = config.periodicUpkeepSeconds;

        // 데드 핸들러: 0초 되면 루프 종료 처리
        wallet.OnDepleted += HandleDepleted;
    }

    void OnDestroy()
    {
        if (wallet != null) wallet.OnDepleted -= HandleDepleted;
    }

    void Update()
    {
        if (!running || config == null) return;

        // 1) 기본 초당 감산 (구역 배율 반영)
        float delta = Time.deltaTime * config.baseDrainPerSecond * zoneMultiplier;
        if (delta > 0f) wallet.SpendSeconds(delta, "Base drain");

        // 2) 주기적 유지비
        upkeepTimer += Time.deltaTime;
        if (upkeepTimer >= config.periodicUpkeepSeconds)
        {
            upkeepTimer = 0f;
            if (config.upkeepFlatCost > 0f)
                wallet.SpendSeconds(config.upkeepFlatCost, "Periodic upkeep");

            wallet.SpendSeconds(delta, "");
        }
    }

    // 위험구역 등에서 배율을 바꿀 때 외부에서 호출
    public void SetZoneMultiplier(float mul)
    {
        zoneMultiplier = Mathf.Max(0f, mul);
    }

    // 루프 종료 시(사망/시간 0) 호출되는 처리
    private void HandleDepleted()
    {
        running = false;
        Debug.Log("[TimeManager] Time depleted -> 루프 종료/사망 처리 진입");

        // 루프 세금(저장고에 남아있는 자산 과세 컨셉을 단순화하여,
        // 다음 루프 시작 시 초기 시간에서 % 차감 같은 방식으로 변형해도 됨)
        if (config.loopTaxRate > 0f)
        {
            // 여기서는 간단히 로그만 남김(실제 과세는 다음 루프 시작 논리에서 반영 가능)
            Debug.Log($"[TimeManager] Loop tax would apply: {config.loopTaxRate * 100f:F1}%");
        }

        // TODO: 리스폰/기억 보정/메타 진행 처리 연결
    }
}

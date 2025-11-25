using System.Collections.Generic;
using UnityEngine;

public class TBSDeviceController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("플레이어 스탯을 관리하는 컴포넌트 (인스펙터에서 드래그 연결 필수!)")]
    public PlayerStats playerStats;

    // [수정] 컴포넌트 참조가 아니라, 직접 생성해서 관리하는 모듈 변수
    [Tooltip("액티브 스킬 실행을 담당하는 내부 모듈")]
    public ActiveSkillModule skillModule = new ActiveSkillModule();

    [Header("State")]
    [SerializeField] private TBSHardwareState _hardwareState;
    [SerializeField] private int _maxRam = 100;
    [SerializeField] private List<TBSAppSO> _equippedApps = new List<TBSAppSO>();

    // [중요] 다른 스크립트(ActiveSkillModule)에서 접근하기 위한 프로퍼티
    public IReadOnlyList<TBSAppSO> EquippedApps => _equippedApps;
    public TBSHardwareState HardwareState => _hardwareState;

    public int MaxRam
    {
        get => _maxRam;
        set => _maxRam = Mathf.Max(0, value);
    }

    // 현재 사용 중인 용량 계산
    public int CurrentRamUsage
    {
        get
        {
            int sum = 0;
            for (int i = 0; i < _equippedApps.Count; i++)
            {
                if (_equippedApps[i] != null)
                    sum += _equippedApps[i].memoryCost;
            }
            return sum;
        }
    }

    private void Awake()
    {
        var player = GetComponent<PlayerController>();
        TimeSystemController timeSystem = null;

        if (player != null)
            timeSystem = player.timeSystem;

        if (timeSystem == null)
            timeSystem = FindFirstObjectByType<TimeSystemController>();

        skillModule.Initialize(player, this, timeSystem);
    }

    private void Start()
    {
        // 1. 패시브 효과 적용 등을 위해 재장착 로직 수행
        var appsToInit = new List<TBSAppSO>(_equippedApps);
        _equippedApps.Clear();

        foreach (var app in appsToInit)
        {
            if (app != null) Equip(app);
        }

        // [추가] 혹시 모르니 스킬 모듈 강제 갱신 (이게 없어서 로드 안 됐을 수도 있음)
        if (skillModule != null)
            skillModule.RefreshSkills();
    }

    private void Update()
    {
        // [중요] 매 프레임 스킬 모듈을 업데이트해야 Rewind 기록이 남습니다.
        if (skillModule != null)
            skillModule.Tick(Time.deltaTime);
    }


    // PlayerController에서 호출
    public void UseQuickSlot(int slotIndex)
    {
        if (_equippedApps == null || slotIndex < 0 || slotIndex >= _equippedApps.Count)
        {
            Debug.LogWarning($"[TBSDeviceController] 잘못된 퀵슬롯 인덱스 {slotIndex}.");
            return;
        }

        var app = _equippedApps[slotIndex];
        if (app == null)
        {
            Debug.LogWarning($"[TBSDeviceController] 슬롯 {slotIndex} 에 장착된 앱이 없습니다.");
            return;
        }

        Debug.Log($"[TBSDeviceController] {slotIndex}번 퀵슬롯 {app.appId} 앱 실행 요청.");

        if (skillModule == null)
        {
            Debug.LogError("[TBSDeviceController] ActiveSkillModule 이 초기화되지 않았습니다.");
            return;
        }

        // 실제 스킬 실행은 모듈에 위임
        skillModule.ExecuteSkill(slotIndex);
    }

    // 장착 가능 여부 확인
    public bool CanEquip(TBSAppSO app)
    {
        if (app == null) return false;
        if (_equippedApps.Contains(app)) return false; // 이미 장착됨

        int after = CurrentRamUsage + app.memoryCost;
        return after <= _maxRam;
    }

    public bool Equip(TBSAppSO app)
    {
        if (!CanEquip(app)) return false;

        _equippedApps.Add(app);
        ApplyAppEffects(app, true);

        // 스킬 모듈 갱신
        if (app.appType == TBSAppType.Active)
            skillModule.RefreshSkills();

        return true;
    }

    public bool Unequip(TBSAppSO app)
    {
        if (app == null) return false;

        if (_equippedApps.Remove(app))
        {
            ApplyAppEffects(app, false);

            // 해제 시 스킬 모듈도 갱신
            if (app.appType == TBSAppType.Active)
                skillModule.RefreshSkills();

            return true;
        }
        return false;
    }

    private void ApplyAppEffects(TBSAppSO app, bool isEquipping)
    {
        if (app == null) return;

        if (app.appType == TBSAppType.Passive && playerStats != null)
        {
            playerStats.ApplyAppStats(app, isEquipping);
        }

        // 액티브 앱은 ActiveSkillModule이 RefreshSkills()에서 처리함
    }

    // 하드웨어 레벨을 외부에서 읽을 수 있게 프로퍼티 추가
    public int RamLevel => _hardwareState.ramLevel;
    public int CpuLevel => _hardwareState.cpuLevel;
    public int BatteryLevel => _hardwareState.batteryLevel;
    public int HeatsinkLevel => _hardwareState.heatsinkLevel;

    // 아래 계수들은 나중에 TimeConfigSO나 별도 HardwareConfigSO로 빼면 됨
    public float GetCpuCooldownMultiplier()
    {
        // 예시: 레벨별 쿨타임 배수
        // 0레벨: 1.00, 1레벨: 0.95, 2레벨: 0.90, 3레벨: 0.85
        switch (CpuLevel)
        {
            default:
            case 0: return 1.00f;
            case 1: return 0.95f;
            case 2: return 0.90f;
            case 3: return 0.85f;
        }
    }

    public float GetCpuTimeCostMultiplier()
    {
        // 예시: 레벨별 시간 소모 배수
        // 0: 1.00, 1: 0.97, 2: 0.94, 3: 0.90
        switch (CpuLevel)
        {
            default:
            case 0: return 1.00f;
            case 1: return 0.97f;
            case 2: return 0.94f;
            case 3: return 0.90f;
        }
    }

    public float GetBatteryDamageMultiplier()
    {
        // 예시: 스킬 데미지 배수
        // 0: 1.00, 1: 1.10, 2: 1.20, 3: 1.35
        switch (BatteryLevel)
        {
            default:
            case 0: return 1.00f;
            case 1: return 1.10f;
            case 2: return 1.20f;
            case 3: return 1.35f;
        }
    }

    public float GetHeatsinkHitTimeLossMultiplier()
    {
        // 예시: 피격 시 시간 손실 감소
        // 0: 1.00, 1: 0.9, 2: 0.8, 3: 0.7
        switch (HeatsinkLevel)
        {
            default:
            case 0: return 1.00f;
            case 1: return 0.90f;
            case 2: return 0.80f;
            case 3: return 0.70f;
        }
    }
}
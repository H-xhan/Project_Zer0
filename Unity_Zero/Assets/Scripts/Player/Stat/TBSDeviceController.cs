using System.Collections.Generic;
using UnityEngine;

public class TBSDeviceController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("플레이어 스탯을 관리하는 컴포넌트 (인스펙터에서 드래그 연결 필수!)")]
    public PlayerStats playerStats;

    [Header("State")]
    [SerializeField] private TBSHardwareState _hardwareState; // 하드웨어 레벨 관리
    [SerializeField] private int _maxRam = 100;               // 최대 용량 (나중에 하드웨어 스탯으로 연결)

    [Tooltip("현재 장착된 앱 리스트")]
    [SerializeField] private List<TBSAppSO> _equippedApps = new List<TBSAppSO>();

    private void Start()
    {
        // [테스트용] 게임 시작 시, 인스펙터 리스트에 미리 넣어둔 앱들을 실제로 적용함
        // 주의: 리스트를 복사해서 순회해야 함 (중복 방지 로직 등이 꼬일 수 있음)
        var appsToInit = new List<TBSAppSO>(_equippedApps);

        // 일단 리스트를 비우고 하나씩 정식으로 장착 절차를 밟음
        _equippedApps.Clear();

        foreach (var app in appsToInit)
        {
            Equip(app); // 이제 진짜로 스탯이 적용됨!
        }
    }

    // 외부에서 읽기 전용으로 접근
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

    // 장착 가능 여부 확인 (용량 체크 + 중복 체크)
    public bool CanEquip(TBSAppSO app)
    {
        if (app == null) return false;
        if (_equippedApps.Contains(app)) return false; // 이미 장착됨

        int after = CurrentRamUsage + app.memoryCost;
        return after <= _maxRam;
    }

    // 앱 장착
    public bool Equip(TBSAppSO app)
    {
        if (!CanEquip(app)) return false;

        _equippedApps.Add(app);
        ApplyAppEffects(app, true); // 효과 켜기
        return true;
    }

    // 앱 해제
    public bool Unequip(TBSAppSO app)
    {
        if (app == null) return false;

        if (_equippedApps.Remove(app))
        {
            ApplyAppEffects(app, false); // 효과 끄기
            return true;
        }
        return false;
    }

    // [핵심] 앱 효과 적용/해제 연결부
    private void ApplyAppEffects(TBSAppSO app, bool isEquipping)
    {
        if (app == null) return;

        // 1. 패시브 앱 (스탯 강화)
        // PlayerStats가 알아서 'statBoosts' 리스트를 뒤져서 적용해줌 (GPT 걱정 해결!)
        if (app.appType == TBSAppType.Passive)
        {
            if (playerStats != null)
            {
                playerStats.ApplyAppStats(app, isEquipping);
            }
            else
            {
                Debug.LogWarning("[TBSDevice] PlayerStats가 연결되지 않았습니다!");
            }
        }

        // 2. 액티브 앱 (스킬)
        if (app.appType == TBSAppType.Active)
        {
            // 나중에 SkillManager 등에 등록하는 로직 추가
            // 예: SkillManager.Instance.SetSkill(app, isEquipping);
        }
    }
}
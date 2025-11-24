using UnityEngine;

[DefaultExecutionOrder(-500)]
[DisallowMultipleComponent]
public class DataController : MonoBehaviour
{
    public static DataController Instance { get; private set; }

    [Header("Time Config")]
    [Tooltip("시간 시스템 기본 설정 데이터")]
    [SerializeField] private TimeConfigSO timeConfig;
    public TimeConfigSO TimeConfig => timeConfig;

    [Header("Player Config")]
    [Tooltip("이동 관련 설정 데이터")]
    [SerializeField] private MovementConfigSO movementConfig;
    public MovementConfigSO MovementConfig => movementConfig;

    [Tooltip("효율(에너지) 관련 설정 데이터")]
    [SerializeField] private EfficiencyConfigSO efficiencyConfig;
    public EfficiencyConfigSO EfficiencyConfig => efficiencyConfig;

    [Tooltip("애니메이션 파라미터 설정 데이터")]
    [SerializeField] private AnimConfigSO animConfig;
    public AnimConfigSO AnimConfig => animConfig;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("[DataController] 초기화 완료 (Priority -500)");
    }
}

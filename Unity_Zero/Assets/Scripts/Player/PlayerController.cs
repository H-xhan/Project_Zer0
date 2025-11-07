using UnityEngine;
using static PlayerController;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ===== 내부 모듈 (인스펙터에서 숨김, 코드 내에서만 사용) =====
    [HideInInspector] public MovementModule movement = new MovementModule();              // 이동 모듈
    [HideInInspector] public PlayerAnimModule animModule = new PlayerAnimModule();        // 애니메이션 모듈
    [HideInInspector] public EfficiencyModule efficiency = new EfficiencyModule();                 // 스태미너 모듈

    // ===== 외부 시스템 =====
    [Header("External Systems")]
    public InventoryModule inventory;                                                     // 인벤토리 시스템
    public InventoryPickupModule inventoryPickup;                                         // 아이템 줍기 시스템
    public TimeSystemController timeSystem;                                               // 시간 경제/생존 시간 시스템(핵심)

    // ===== UI & 참조 =====
    [Header("UI / References")]
    public HitOverlayUI hitOverlay;                                                       // 피격 화면 오버레이
    public GameObject inventoryUI;                                                        // 인벤토리 UI 루트
    public KeyCode inventoryKey = KeyCode.Tab;                                            // 인벤토리 토글 키
    public Animator animatorSource;                                                       // 애니메이터 소스

    private CharacterController _cc;                                                      // 캐릭터컨트롤러 캐시
    private bool _inventoryOpen;                                                          // 인벤토리 열림 여부

    [Header("Debug")]
    public bool debugEfficiency = true;
    private float _dbgNextTime = 0f;

    // ===== 인스펙터에서 조절할 수 있는 튜닝값 =====
    [System.Serializable]
    public struct MovementTuning
    {
        [Header("이동 관련")]
        [Tooltip("걷기 속도 (기본 이동 속도)")] public float walkSpeed;                   // 기본 속도
        [Tooltip("스프린트 시 이동 속도 배수 (1.7 = 70% 빠름)")] public float sprintMultiplier; // 스프린트 배수
        [Tooltip("스프린트 키 설정")] public KeyCode sprintKey;                           // 스프린트 키
        [Tooltip("지상에서만 스프린트 가능 여부")] public bool sprintOnlyOnGround;         // 공중 스프린트 금지

        [Header("점프 물리 설정")]
        [Tooltip("점프 높이 (m 단위)")] public float jumpHeight;                          // 점프 높이
        [Tooltip("중력 가속도 (-값이 커질수록 빠르게 낙하)")] public float gravity;        // 중력
        [Tooltip("지면에 붙는 정도 (음수로 유지)")] public float groundedStick;            // 지면 부착력

        [Header("점프 타이밍")]
        [Tooltip("점프 쿨다운 (연속 점프 불가 시간, 초 단위)")] public float jumpCooldown; // 점프 쿨다운
        [Tooltip("코요테 타임 (착지 후 잠깐 점프 허용 시간)")] public float coyoteTime;    // 코요테 타임
        [Tooltip("점프 버퍼 (입력 미리 받아주는 시간)")] public float jumpBufferTime;      // 점프 버퍼

        [Header("스프린트 소진 정지")]
        [Tooltip("스태미너 소진 시 즉시 멈출지 여부")] public bool stopOnSprintExhaust;     // 소진 시 정지
        [Tooltip("스태미너 소진 후 정지 유지 시간 (초 단위)")] public float exhaustStopDuration; // 정지 유지
    }

    [System.Serializable]
    public struct EfficiencyTuning
    {
        [Header("Capacity")]
        [Tooltip("효율(스태미너)의 최대치. 기본 100이 일반적입니다.")]
        public float max;

        [Header("Walk / Sprint / Jump Drain")]
        [Tooltip("걷는 중 초당 효율 소모량입니다. 0이면 걷기 시 효율이 줄지 않습니다.")]
        public float walkDrainPerSecond;

        [Tooltip("스프린트(Shift) 중 초당 효율 소모량입니다.")]
        public float sprintDrainPerSecond;

        [Tooltip("점프 1회당 소모되는 효율 수치입니다.")]
        public float jumpCost;

        [Header("Regen")]
        [Tooltip("정지 시 초당 효율 회복량입니다.")]
        public float idleRegenPerSecond;

        [Tooltip("이동 중 초당 효율 회복량입니다.")]
        public float moveRegenPerSecond;

        [Tooltip("효율이 소모된 후 회복이 시작되기까지의 지연 시간(초)입니다.")]
        public float regenDelay;
    }


    [System.Serializable]
    public struct AnimTuning
    {
        [Header("애니메이션 전환 감속/가속")]
        [Tooltip("이동 속도 증가 시 애니메이션 감속 비율 (값이 높을수록 빠르게 반응)")] public float dampUp; // 가속 감쇠
        [Tooltip("이동 속도 감소 시 애니메이션 감속 비율")] public float dampDown;                           // 감속 감쇠
        [Tooltip("이동이 거의 멈췄을 때 속도 0으로 스냅되는 임계값")] public float stopSnapThreshold;       // 스냅 임계
    }

    [Header("Movement Settings")]
    public MovementTuning movementTuning = new MovementTuning
    {
        walkSpeed = 4f,                 // 기본 속도
        sprintMultiplier = 1.7f,        // 스프린트 배수
        sprintKey = KeyCode.LeftShift,  // 스프린트 키
        sprintOnlyOnGround = true,      // 지상에서만 스프린트

        jumpHeight = 1.2f,              // 점프 높이
        gravity = -20f,                 // 중력
        groundedStick = -2f,            // 지면 부착력

        jumpCooldown = 0.2f,            // 점프 쿨다운
        coyoteTime = 0.1f,              // 코요테
        jumpBufferTime = 0.1f,          // 버퍼

        stopOnSprintExhaust = true,     // 소진 시 정지
        exhaustStopDuration = 0.2f      // 정지 유지 시간
    };

    [Header("Efficiency Settings")]
    public EfficiencyTuning efficiencyTuning = new EfficiencyTuning
    {
        max = 100f,
        walkDrainPerSecond = 0f,     // 걷기 시 소모 없음
        sprintDrainPerSecond = 20f,  // 스프린트 시 초당 20 소모
        jumpCost = 15f,              // 점프 1회당 15 소모
        idleRegenPerSecond = 15f,    // 정지 회복
        moveRegenPerSecond = 5f,     // 이동 회복
        regenDelay = 0.6f            // 회복 지연
    };


    [Header("Animation Settings")]
    public AnimTuning animTuning = new AnimTuning
    {
        dampUp = 0.08f,                 // 가속 감쇠
        dampDown = 0.04f,               // 감속 감쇠
        stopSnapThreshold = 0.08f       // 스냅 임계
    };

    // ===== 초기화 =====
    void Awake()
    {
        _cc = GetComponent<CharacterController>();                                       // 캐릭터컨트롤러 캐시

        efficiency.Init();                                                                  // 스태미너 초기화
        movement.Initialize(_cc, transform, efficiency);                                    // 이동 모듈 초기화

        if (!animatorSource)                                                             // 애니메이터 참조 보정
            animatorSource = GetComponentInChildren<Animator>(true);
        animModule.Init(animatorSource);                                                 // 애니메이션 모듈 초기화

        if (inventory != null) inventory.Init();                                         // 인벤토리 초기화
        if (inventoryPickup != null) inventoryPickup.Init(transform, inventory);         // 아이템 줍기 초기화
        if (timeSystem == null)                                                          // 타임시스템 자동 탐색
            timeSystem = FindFirstObjectByType<TimeSystemController>();

        if (inventoryUI != null) inventoryUI.SetActive(false);                           // 인벤토리 UI 기본 비활성

        Cursor.lockState = CursorLockMode.Locked;                                        // 마우스 잠금
        Cursor.visible = false;                                                          // 커서 숨김
        _inventoryOpen = false;                                                          // 인벤토리 닫힘

        ApplySettings();                                                                 // 인스펙터 값 1회 반영
    }

    // ===== 프레임 루프 =====
    void Update()
    {
        if (Input.GetKeyDown(inventoryKey)) ToggleInventory();                           // 인벤토리 토글
        if (_inventoryOpen) return;                                                      // 열려 있으면 입력 차단

        ApplySettings();                                                                 // 인스펙터 값 실시간 반영

        // === 이동 ===
        movement.Tick();                                                                 // 이동 처리
        bool moving = movement.HasMoveInput();                                           // 이동 중 여부
        bool sprintingPre = movement.IsSprinting();                                      // 스프린트 입력 여부
        bool jumpTrig = movement.ConsumeJumpTriggered();                                 // 점프 트리거 소비

        // === 효율 ===
        efficiency.Tick(Time.deltaTime, moving, sprintingPre);

        if (timeSystem != null)
        {
            timeSystem.SetExternalCostMultiplier(efficiency.ComputeCostMultiplier());

            if (debugEfficiency && Time.unscaledTime >= _dbgNextTime)
            {
                float eff01 = efficiency.Normalized();
                Debug.Log($"[EFF] {eff01 * 100f:0}%   mul=x{timeSystem.externalCostMultiplier:0.00}   moving={moving}  sprint={sprintingPre}");
                _dbgNextTime = Time.unscaledTime + 0.3f; // 0.3초 간격
            }
        }
        bool sprinting = movement.IsSprinting();                                         // 실제 스프린트 여부

        // === 애니메이션 ===
        animModule.Tick(Time.deltaTime,
                        movement.GetPlanarSpeed(),
                        movement.IsGrounded(),
                        movement.GetVerticalVelocity(),
                        jumpTrig,
                        sprinting);                                                      // 애니메이션 갱신

        // === 시간 경제(이동/점프 비용) ===
        if (timeSystem != null)
        {
            if (moving)
            {
                if (sprinting) timeSystem.SpendForSprintDelta(Time.deltaTime);           // 스프린트 시간 비용
                else timeSystem.SpendForWalkDelta(Time.deltaTime);                       // 걷기 시간 비용
            }
            if (jumpTrig) timeSystem.SpendForJumpEvent();                                // 점프 이벤트 비용
        }

        // === 인터랙션 ===
        if (inventoryPickup != null) inventoryPickup.Tick();                             // 아이템 줍기
    }

    // ===== 인스펙터 값 → 모듈에 반영 =====
    private void ApplySettings()
    {
        movement.SyncSettings(
            movementTuning.jumpCooldown, movementTuning.coyoteTime, movementTuning.jumpBufferTime,
            movementTuning.walkSpeed, movementTuning.sprintMultiplier,
            movementTuning.sprintKey, movementTuning.sprintOnlyOnGround,
            movementTuning.jumpHeight, movementTuning.gravity, movementTuning.groundedStick,
            movementTuning.stopOnSprintExhaust, movementTuning.exhaustStopDuration
        );                                                                               // 이동 튜닝 반영

        efficiency.SyncSettings(
            efficiencyTuning.walkDrainPerSecond,
            efficiencyTuning.sprintDrainPerSecond,
            efficiencyTuning.idleRegenPerSecond,
            efficiencyTuning.moveRegenPerSecond,
            efficiencyTuning.regenDelay,
            efficiencyTuning.jumpCost,
            efficiencyTuning.max
        );                                                                              // 효율 튜닝 반영

        
    }

    private void ToggleInventory()
    {
        if (inventoryUI == null) return;                                                 // UI 없으면 무시
        _inventoryOpen = !_inventoryOpen;                                                // 토글
        inventoryUI.SetActive(_inventoryOpen);                                           // 활성/비활성

        if (_inventoryOpen)
        {
            var invUI = inventoryUI.GetComponent<InventoryUI>();                         // UI 갱신
            if (invUI != null) invUI.ForceRefresh();
            Cursor.lockState = CursorLockMode.None;                                      // 커서 해제
            Cursor.visible = true;                                                       // 커서 표시
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;                                    // 커서 잠금
            Cursor.visible = false;                                                      // 커서 숨김
        }
    }

    // === 데미지 → 생존 시간 차감(핵심) ===
    public void ApplyDamage(float amount)
    {
        if (timeSystem != null)
        {
            timeSystem.SpendForDamage(amount);                                           // 피해량만큼 생존 시간 차감
        }

        if (hitOverlay != null)
        {
            hitOverlay.Flash(amount);                                                    // 피격 화면 플래시 연출
        }
    }
}

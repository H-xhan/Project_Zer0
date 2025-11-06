using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ===== 내부 모듈 (인스펙터에서 숨김, 코드 내에서만 사용) =====
    [HideInInspector] public MovementModule movement = new MovementModule();
    [HideInInspector] public PlayerAnimModule animModule = new PlayerAnimModule();
    [HideInInspector] public StaminaModule stamina = new StaminaModule();
    [HideInInspector] public HealthModule health = new HealthModule();

    // ===== 외부 시스템 =====
    [Header("External Systems")]
    public InventoryModule inventory;
    public InventoryPickupModule inventoryPickup;
    public TimeSystemController timeSystem;

    // ===== UI & 참조 =====
    [Header("UI / References")]
    public HitOverlayUI hitOverlay;
    public GameObject inventoryUI;
    public KeyCode inventoryKey = KeyCode.Tab;
    public Animator animatorSource;

    private System.Action<float> _onDamagedHandler;
    private CharacterController _cc;
    private bool _inventoryOpen;

    // ===== 인스펙터에서 조절할 수 있는 튜닝값 =====
    [System.Serializable]
    public struct MovementTuning
    {
        [Header("이동 관련")]
        [Tooltip("걷기 속도 (기본 이동 속도)")]
        public float walkSpeed;

        [Tooltip("스프린트 시 이동 속도 배수 (1.7 = 70% 빠름)")]
        public float sprintMultiplier;

        [Tooltip("스프린트 키 설정")]
        public KeyCode sprintKey;

        [Tooltip("지상에서만 스프린트 가능 여부")]
        public bool sprintOnlyOnGround;

        [Header("점프 물리 설정")]
        [Tooltip("점프 높이 (m 단위)")]
        public float jumpHeight;

        [Tooltip("중력 가속도 (-값이 커질수록 빠르게 낙하)")]
        public float gravity;

        [Tooltip("지면에 붙는 정도 (음수로 유지)")]
        public float groundedStick;

        [Header("점프 타이밍")]
        [Tooltip("점프 쿨다운 (연속 점프 불가 시간, 초 단위)")]
        public float jumpCooldown;

        [Tooltip("코요테 타임 (착지 후 잠깐 점프 허용 시간)")]
        public float coyoteTime;

        [Tooltip("점프 버퍼 (입력 미리 받아주는 시간)")]
        public float jumpBufferTime;

        [Header("스프린트 소진 정지")]
        [Tooltip("스태미너 소진 시 즉시 멈출지 여부")]
        public bool stopOnSprintExhaust;

        [Tooltip("스태미너 소진 후 정지 유지 시간 (초 단위)")]
        public float exhaustStopDuration;
    }

    // === StaminaTuning ===
    [System.Serializable]
    public struct StaminaTuning
    {
        [Header("스프린트 관련")]
        [Tooltip("이 값 이상일 때만 스프린트 가능")]
        public float minToSprint;

        [Tooltip("스프린트 중 초당 스태미너 감소량")]
        public float sprintDrainPerSecond;

        [Header("회복 관련")]
        [Tooltip("가만히 있을 때 초당 스태미너 회복량")]
        public float idleRegenPerSecond;

        [Tooltip("이동 중 초당 스태미너 회복량")]
        public float moveRegenPerSecond;

        [Tooltip("소모 후 회복 시작까지 지연 시간")]
        public float regenDelay;

        [Header("행동 비용")]
        [Tooltip("점프 1회당 소모되는 스태미너량")]
        public float jumpCost;
    }

    // === AnimTuning ===
    [System.Serializable]
    public struct AnimTuning
    {
        [Header("애니메이션 전환 감속/가속")]
        [Tooltip("이동 속도 증가 시 애니메이션 감속 비율 (값이 높을수록 빠르게 반응)")]
        public float dampUp;

        [Tooltip("이동 속도 감소 시 애니메이션 감속 비율")]
        public float dampDown;

        [Tooltip("이동이 거의 멈췄을 때 속도 0으로 스냅되는 임계값")]
        public float stopSnapThreshold;
    }

    // === HealthTuning ===
    [System.Serializable]
    public struct HealthTuning
    {
        [Header("체력 설정")]
        [Tooltip("최대 체력 값")]
        public float max;

        [Tooltip("자동 회복 기능 사용 여부")]
        public bool useRegen;

        [Tooltip("초당 체력 회복량 (useRegen이 켜져 있을 때만 적용)")]
        public float regenPerSecond;

        [Tooltip("피격 후 회복이 시작되기까지의 지연 시간")]
        public float regenDelay;

        [Header("무적/피격 관련")]
        [Tooltip("피해를 전혀 받지 않도록 설정")]
        public bool invulnerable;

        [Tooltip("피격 시 잠깐 무적이 되는 시간")]
        public float invulnTime;
    }

    // ===== 인스펙터 노출 튜닝 섹션 =====
    [Header("Movement Settings")]
    public MovementTuning movementTuning = new MovementTuning
    {
        walkSpeed = 4f,
        sprintMultiplier = 1.7f,
        sprintKey = KeyCode.LeftShift,
        sprintOnlyOnGround = true,
        jumpHeight = 1.2f,
        gravity = -20f,
        groundedStick = -2f,
        jumpCooldown = 0.2f,
        coyoteTime = 0.1f,
        jumpBufferTime = 0.1f,
        stopOnSprintExhaust = true,
        exhaustStopDuration = 0.2f
    };

    [Header("Stamina Settings")]
    public StaminaTuning staminaTuning = new StaminaTuning
    {
        minToSprint = 5f,
        sprintDrainPerSecond = 20f,
        idleRegenPerSecond = 15f,
        moveRegenPerSecond = 5f,
        regenDelay = 0.6f,
        jumpCost = 15f
    };

    [Header("Animation Settings")]
    public AnimTuning animTuning = new AnimTuning
    {
        dampUp = 0.08f,
        dampDown = 0.04f,
        stopSnapThreshold = 0.08f
    };

    [Header("Health Settings")]
    public HealthTuning healthTuning = new HealthTuning
    {
        max = 100f,
        useRegen = false,
        regenPerSecond = 5f,
        regenDelay = 2f,
        invulnerable = false,
        invulnTime = 0.2f
    };

    // ===== 초기화 =====
    void Awake()
    {
        _cc = GetComponent<CharacterController>();

        stamina.Init();
        movement.Initialize(_cc, transform, stamina);
        if (!animatorSource)
            animatorSource = GetComponentInChildren<Animator>(true);
        animModule.Init(animatorSource);
        health.Init();

        if (hitOverlay != null)
        {
            _onDamagedHandler = amt => hitOverlay.Flash(amt);
            health.OnDamaged += _onDamagedHandler;
        }

        if (inventory != null) inventory.Init();
        if (inventoryPickup != null) inventoryPickup.Init(transform, inventory);
        if (timeSystem == null) timeSystem = FindFirstObjectByType<TimeSystemController>();
        if (inventoryUI != null) inventoryUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _inventoryOpen = false;

        ApplySettings(); // 초기값 한 번 반영
    }

    // ===== 프레임 루프 =====
    void Update()
    {
        if (Input.GetKeyDown(inventoryKey)) ToggleInventory();
        if (_inventoryOpen) return;

        ApplySettings(); // 인스펙터 값 실시간 반영

        // === 이동 ===
        movement.Tick();
        bool moving = movement.HasMoveInput();
        bool sprintingPre = movement.IsSprinting();
        bool jumpTrig = movement.ConsumeJumpTriggered();

        // === 스태미너 ===
        stamina.Tick(Time.deltaTime, moving, sprintingPre);
        if (sprintingPre && !stamina.CanSprint())
            movement.ForceStopSprint();

        bool sprinting = movement.IsSprinting();

        // === 애니메이션 ===
        animModule.Tick(Time.deltaTime,
                        movement.GetPlanarSpeed(),
                        movement.IsGrounded(),
                        movement.GetVerticalVelocity(),
                        jumpTrig,
                        sprinting);

        // === 체력 ===
        health.Tick(Time.deltaTime);

        // === 시간 경제 ===
        if (timeSystem != null)
        {
            if (moving)
            {
                if (sprinting) timeSystem.SpendForSprintDelta(Time.deltaTime);
                else timeSystem.SpendForWalkDelta(Time.deltaTime);
            }
            if (jumpTrig) timeSystem.SpendForJumpEvent();
        }

        // === 인터랙션 ===
        if (inventoryPickup != null) inventoryPickup.Tick();
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
        );

        stamina.SyncSettings(
            staminaTuning.minToSprint, staminaTuning.sprintDrainPerSecond,
            staminaTuning.idleRegenPerSecond, staminaTuning.moveRegenPerSecond,
            staminaTuning.regenDelay, staminaTuning.jumpCost
        );

        animModule.SyncSettings(animTuning.dampUp, animTuning.dampDown, animTuning.stopSnapThreshold);
        health.SyncSettings(healthTuning.max, healthTuning.useRegen, healthTuning.regenPerSecond,
                            healthTuning.regenDelay, healthTuning.invulnerable, healthTuning.invulnTime);
    }

    private void ToggleInventory()
    {
        if (inventoryUI == null) return;
        _inventoryOpen = !_inventoryOpen;
        inventoryUI.SetActive(_inventoryOpen);

        if (_inventoryOpen)
        {
            var invUI = inventoryUI.GetComponent<InventoryUI>();
            if (invUI != null) invUI.ForceRefresh();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnDestroy()
    {
        if (health != null && _onDamagedHandler != null)
            health.OnDamaged -= _onDamagedHandler;
    }

    public void ApplyDamage(float amount)
    {
        if (health == null) return;
        health.ApplyDamage(amount);
    }
}

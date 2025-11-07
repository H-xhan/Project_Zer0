using UnityEngine;
using static PlayerController;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ===== 내부 모듈 (인스펙터에서 숨김, 코드 내에서만 사용) =====
    [HideInInspector] public MovementModule movement = new MovementModule();              // 이동 모듈
    [HideInInspector] public PlayerAnimModule animModule = new PlayerAnimModule();        // 애니메이션 모듈
    [HideInInspector] public EfficiencyModule efficiency = new EfficiencyModule();        // 효율(스태미너 대체) 모듈

    // ===== 외부 시스템 =====
    [Header("External Systems")]
    public InventoryModule inventory;                                                     // 인벤토리 시스템
    public InventoryPickupModule inventoryPickup;                                         // 아이템 줍기 시스템
    public TimeSystemController timeSystem;                                               // 시간 경제/생존 시간 시스템(핵심)

    [Header("Quest / State (Optional)")]
    public PlayerQuestLog questLog;                                                       // 퀘스트 상태 확인용 (없으면 비워둬도 됨)

    // ===== 카메라 / 시점 설정 =====
    [Header("Camera / View Settings")]
    public CameraRigMouseLookTPS cameraRig;                                               // 카메라 리그 참조
    public CameraRigMouseLookTPS.CameraMode defaultCameraMode = CameraRigMouseLookTPS.CameraMode.FirstPerson; // 기본 시점
    public KeyCode viewToggleKey = KeyCode.V;                                             // 시점 전환 키
    public bool allowThirdPersonToggle = true;                                            // 3인칭 토글 허용 여부
    public bool restrictThirdPersonDuringQuest = true;                                    // 퀘스트 진행 중 3인칭 금지

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
        [Tooltip("걷기 속도 (기본 이동 속도)")] public float walkSpeed;
        [Tooltip("스프린트 시 이동 속도 배수 (1.7 = 70% 빠름)")] public float sprintMultiplier;
        [Tooltip("스프린트 키 설정")] public KeyCode sprintKey;
        [Tooltip("지상에서만 스프린트 가능 여부")] public bool sprintOnlyOnGround;

        [Header("점프 물리 설정")]
        [Tooltip("점프 높이 (m 단위)")] public float jumpHeight;
        [Tooltip("중력 가속도 (-값이 커질수록 빠르게 낙하)")] public float gravity;
        [Tooltip("지면에 붙는 정도 (음수로 유지)")] public float groundedStick;

        [Header("점프 타이밍")]
        [Tooltip("점프 쿨다운 (연속 점프 불가 시간, 초 단위)")] public float jumpCooldown;
        [Tooltip("코요테 타임 (착지 후 잠깐 점프 허용 시간)")] public float coyoteTime;
        [Tooltip("점프 버퍼 (입력 미리 받아주는 시간)")] public float jumpBufferTime;

        [Header("스프린트 소진 정지")]
        [Tooltip("효율 소진 시 즉시 멈출지 여부")] public bool stopOnSprintExhaust;
        [Tooltip("소진 후 강제 정지 유지 시간(초)")] public float exhaustStopDuration;
    }

    [System.Serializable]
    public struct EfficiencyTuning
    {
        [Header("Capacity")]
        [Tooltip("효율(스태미너)의 최대치")] public float max;

        [Header("Walk / Sprint / Jump Drain")]
        [Tooltip("걷기 중 초당 효율 소모량 (0이면 소모 없음)")] public float walkDrainPerSecond;
        [Tooltip("스프린트 중 초당 효율 소모량")] public float sprintDrainPerSecond;
        [Tooltip("점프 1회당 소모 효율량")] public float jumpCost;

        [Header("Regen")]
        [Tooltip("정지 시 초당 효율 회복량")] public float idleRegenPerSecond;
        [Tooltip("이동 중 초당 효율 회복량")] public float moveRegenPerSecond;
        [Tooltip("효율 소모 후 회복 시작까지 지연 시간(초)")] public float regenDelay;
    }

    [System.Serializable]
    public struct AnimTuning
    {
        [Header("애니메이션 전환 감속/가속")]
        [Tooltip("속도 증가 시 반응 속도")] public float dampUp;
        [Tooltip("속도 감소 시 반응 속도")] public float dampDown;
        [Tooltip("거의 멈췄을 때 0으로 스냅되는 임계값")] public float stopSnapThreshold;
    }

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

    [Header("Efficiency Settings")]
    public EfficiencyTuning efficiencyTuning = new EfficiencyTuning
    {
        max = 100f,
        walkDrainPerSecond = 0f,
        sprintDrainPerSecond = 20f,
        jumpCost = 15f,
        idleRegenPerSecond = 15f,
        moveRegenPerSecond = 5f,
        regenDelay = 0.6f
    };

    [Header("Animation Settings")]
    public AnimTuning animTuning = new AnimTuning
    {
        dampUp = 0.08f,
        dampDown = 0.04f,
        stopSnapThreshold = 0.08f
    };

    // ===== 초기화 =====
    void Awake()
    {
        _cc = GetComponent<CharacterController>();                                       // 캐릭터컨트롤러 캐시

        efficiency.Init();                                                              // 효율 모듈 초기화
        movement.Initialize(_cc, transform, efficiency);                                // 이동 모듈 초기화

        if (!animatorSource)
            animatorSource = GetComponentInChildren<Animator>(true);                    // 애니메이터 자동 참조
        animModule.Init(animatorSource);                                                // 애니메이션 모듈 초기화

        if (inventory != null) inventory.Init();                                        // 인벤토리 초기화
        if (inventoryPickup != null) inventoryPickup.Init(transform, inventory);        // 아이템 줍기 초기화
        if (timeSystem == null)
            timeSystem = FindFirstObjectByType<TimeSystemController>();                 // 타임 시스템 자동 탐색

        if (cameraRig == null)
            cameraRig = FindFirstObjectByType<CameraRigMouseLookTPS>();                 // 카메라 리그 자동 탐색 (없으면 null)

        if (inventoryUI != null) inventoryUI.SetActive(false);                          // 인벤토리 UI 비활성

        Cursor.lockState = CursorLockMode.Locked;                                       // 마우스 잠금
        Cursor.visible = false;                                                         // 커서 숨김
        _inventoryOpen = false;                                                         // 인벤토리 닫힘

        ApplySettings();                                                                // 인스펙터 값 1회 반영

        // 시작 시점 모드 적용
        if (cameraRig != null)
        {
            cameraRig.SetMode(defaultCameraMode);                                       // 기본 1인칭으로 시작(설정값 기준)
        }

        if (questLog == null)
        {
            questLog = GetComponent<PlayerQuestLog>();
            if (questLog == null)
            {
                // 혹시 실수로 안 붙였으면 자동으로 달아줌 (원하면 빼도 됨)
                questLog = gameObject.AddComponent<PlayerQuestLog>();
                Debug.LogWarning("[PlayerController] PlayerQuestLog가 없어 자동 추가했습니다.");
            }
        }
    }

    // ===== 프레임 루프 =====
    void Update()
    {
        // 인벤토리 토글
        if (Input.GetKeyDown(inventoryKey))
            ToggleInventory();

        // 인벤토리 열려 있으면 조작/시점 변경 막기
        if (_inventoryOpen) return;

        // 시점 토글 처리 (퀘스트/상태 조건 반영)
        HandleViewToggle();

        // 인스펙터 값 실시간 반영 (튜닝용)
        ApplySettings();

        // === 이동 처리 ===
        movement.Tick();
        bool moving = movement.HasMoveInput();
        bool sprintingPre = movement.IsSprinting();
        bool jumpTrig = movement.ConsumeJumpTriggered();

        // === 효율 처리 ===
        efficiency.Tick(Time.deltaTime, moving, sprintingPre);

        if (timeSystem != null)
        {
            timeSystem.SetExternalCostMultiplier(efficiency.ComputeCostMultiplier());

            if (debugEfficiency && Time.unscaledTime >= _dbgNextTime)
            {
                float eff01 = efficiency.Normalized();
                Debug.Log($"[EFF] {eff01 * 100f:0}%   mul=x{timeSystem.externalCostMultiplier:0.00}   moving={moving}  sprint={sprintingPre}");
                _dbgNextTime = Time.unscaledTime + 0.3f;
            }
        }

        bool sprinting = movement.IsSprinting();

        // === 애니메이션 처리 ===
        animModule.Tick(
            Time.deltaTime,
            movement.GetPlanarSpeed(),
            movement.IsGrounded(),
            movement.GetVerticalVelocity(),
            jumpTrig,
            sprinting
        );

        // === 시간 경제(행동 비용) 처리 ===
        if (timeSystem != null)
        {
            if (moving)
            {
                if (sprinting) timeSystem.SpendForSprintDelta(Time.deltaTime);
                else timeSystem.SpendForWalkDelta(Time.deltaTime);
            }

            if (jumpTrig)
                timeSystem.SpendForJumpEvent();
        }

        // === 인터랙션 처리 ===
        if (inventoryPickup != null)
            inventoryPickup.Tick();
    }

    // ===== 시점 토글 처리 =====
    private void HandleViewToggle()
    {
        if (cameraRig == null) return;                                                  // 카메라 리그 없으면 처리 안 함
        if (!allowThirdPersonToggle) return;                                            // 전체적으로 비활성화 시 무시

        // 퀘스트 진행 중 / 위험 상태면 3인칭 강제 금지
        if (restrictThirdPersonDuringQuest && !CanUseThirdPerson())
        {
            if (cameraRig.CurrentMode != CameraRigMouseLookTPS.CameraMode.FirstPerson)
            {
                cameraRig.SetMode(CameraRigMouseLookTPS.CameraMode.FirstPerson);        // 자동 1인칭 복귀
            }
            return;
        }

        // 토글 입력 처리
        if (Input.GetKeyDown(viewToggleKey))
        {
            var next =
                cameraRig.CurrentMode == CameraRigMouseLookTPS.CameraMode.FirstPerson
                ? CameraRigMouseLookTPS.CameraMode.ThirdPerson
                : CameraRigMouseLookTPS.CameraMode.FirstPerson;

            cameraRig.SetMode(next);                                                    // 모드 전환
        }
    }

    // ===== 3인칭 사용 가능 조건 체크 =====
    private bool CanUseThirdPerson()
    {
        if (!allowThirdPersonToggle) return false;
        if (questLog == null) return true;

        // 퀘스트 로그에서 카메라 락이 걸려 있으면 무조건 3인칭 금지
        if (questLog.IsCameraLocked())
            return false;

        if (restrictThirdPersonDuringQuest)
        {
            if (questLog.HasActiveQuest()) return false;
            if (questLog.IsInTimedOrDangerQuest()) return false;
        }

        return true;
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

        efficiency.SyncSettings(
            efficiencyTuning.walkDrainPerSecond,
            efficiencyTuning.sprintDrainPerSecond,
            efficiencyTuning.idleRegenPerSecond,
            efficiencyTuning.moveRegenPerSecond,
            efficiencyTuning.regenDelay,
            efficiencyTuning.jumpCost,
            efficiencyTuning.max
        );
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

    // === 데미지 → 생존 시간 차감 ===
    public void ApplyDamage(float amount)
    {
        if (timeSystem != null)
            timeSystem.SpendForDamage(amount);

        if (hitOverlay != null)
            hitOverlay.Flash(amount);
    }
}

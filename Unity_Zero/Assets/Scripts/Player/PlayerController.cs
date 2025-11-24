using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // 플레이어 동작을 담당하는 내부 모듈 인스턴스 (인스펙터 비노출)
    [HideInInspector] public MovementModule movement = new MovementModule();
    [HideInInspector] public PlayerAnimModule animModule = new PlayerAnimModule();
    [HideInInspector] public EfficiencyModule efficiency = new EfficiencyModule();

    [Header("External Systems")]
    [Tooltip("플레이어 인벤토리 시스템 참조")]
    public InventoryModule inventory;

    [Tooltip("아이템 상호작용을 처리하는 픽업 모듈")]
    public InventoryPickupModule inventoryPickup;

    [Tooltip("시간 자원을 관리하는 핵심 시스템")]
    public TimeSystemController timeSystem;

    [Tooltip("플레이어 스탯 관리자 (연결 필수)")]
    public PlayerStats playerStats;

    // [추가] 디바이스 컨트롤러 참조
    private TBSDeviceController _deviceController;

    [Header("Quest (Optional)")]
    [Tooltip("카메라 잠금, 진행 상태 등을 확인하는 퀘스트 로그")]
    public PlayerQuestLog questLog;

    [Header("Camera / View Settings")]
    [Tooltip("플레이어 시점을 제어하는 카메라 리그")]
    public CameraRigMouseLookTPS cameraRig;

    [Tooltip("게임 시작 시 사용할 기본 시점 모드")]
    public CameraRigMouseLookTPS.CameraMode defaultCameraMode =
        CameraRigMouseLookTPS.CameraMode.FirstPerson;

    [Tooltip("1인칭 / 3인칭 시점을 전환하는 입력 키")]
    public KeyCode viewToggleKey = KeyCode.V;

    [Tooltip("시점 전환 기능 사용 여부")]
    public bool allowThirdPersonToggle = true;

    [Tooltip("특정 퀘스트 상황에서 3인칭 전환을 제한할지 여부")]
    public bool restrictThirdPersonDuringQuest = true;

    [Header("UI / References")]
    [Tooltip("피격 시 화면에 표시되는 오버레이 UI")]
    public HitOverlayUI hitOverlay;

    [Tooltip("인벤토리 UI 루트 오브젝트")]
    public GameObject inventoryUI;

    [Tooltip("인벤토리 UI 열기/닫기 키")]
    public KeyCode inventoryKey = KeyCode.Tab;

    [Tooltip("플레이어 애니메이터 (비워두면 자식에서 검색)")]
    public Animator animatorSource;

    // 이동과 관련된 실제 물리 이동을 처리하는 컴포넌트
    private CharacterController _cc;

    // 인벤토리 UI 열림 여부
    private bool _inventoryOpen;

    // 이동 관련 튜닝 구조체 (인스펙터에서 조절)
    [System.Serializable]
    public struct MovementTuning
    {
        [Header("이동 속도")]
        [Tooltip("걷기 속도")]
        public float walkSpeed;

        [Tooltip("스프린트 시 속도 배수")]
        public float sprintMultiplier;

        [Tooltip("스프린트 입력 키")]
        public KeyCode sprintKey;

        [Tooltip("지상에서만 스프린트 허용 여부")]
        public bool sprintOnlyOnGround;

        [Header("점프 및 중력")]
        [Tooltip("점프 높이 (미터 단위)")]
        public float jumpHeight;

        [Tooltip("중력 가속도 (음수 권장)")]
        public float gravity;

        [Tooltip("경사면에서 지면에 붙게 하는 보정 값 (음수 권장)")]
        public float groundedStick;

        [Header("점프 타이밍")]
        [Tooltip("점프 후 다음 점프까지 최소 대기 시간")]
        public float jumpCooldown;

        [Tooltip("코요테 타임 (착지 직후 점프 허용 시간)")]
        public float coyoteTime;

        [Tooltip("점프 버퍼 시간 (입력을 저장하는 시간)")]
        public float jumpBufferTime;

        [Header("스프린트 소진 처리")]
        [Tooltip("효율 소진 시 스프린트를 즉시 중단할지 여부")]
        public bool stopOnSprintExhaust;

        [Tooltip("효율 소진 후 강제 제약 지속 시간")]
        public float exhaustStopDuration;
    }

    // 효율 시스템 튜닝 구조체 (행동별 소모, 회복 규칙)
    [System.Serializable]
    public struct EfficiencyTuning
    {
        [Header("최대 효율")]
        [Tooltip("효율 최대 값")]
        public float max;

        [Header("행동별 소모량")]
        [Tooltip("걷기 중 초당 효율 소모량 (0이면 소모 없음)")]
        public float walkDrainPerSecond;

        [Tooltip("스프린트 중 초당 효율 소모량")]
        public float sprintDrainPerSecond;

        [Tooltip("점프 1회당 효율 소모량")]
        public float jumpCost;

        [Header("회복")]
        [Tooltip("정지 상태에서 초당 효율 회복량")]
        public float idleRegenPerSecond;

        [Tooltip("이동 중 초당 효율 회복량")]
        public float moveRegenPerSecond;

        [Tooltip("소모 후 회복 시작까지의 대기 시간")]
        public float regenDelay;
    }

    // 애니메이션 반응 튜닝 구조체
    [System.Serializable]
    public struct AnimTuning
    {
        [Header("이동 애니메이션 반응")]
        [Tooltip("속도 증가 시 애니메이터 반응 속도")]
        public float dampUp;

        [Tooltip("속도 감소 시 애니메이터 반응 속도")]
        public float dampDown;

        [Tooltip("이동 속도가 이 값 이하이면 정지로 처리")]
        public float stopSnapThreshold;
    }

    [Header("Movement Settings")]
    [Tooltip("플레이어 이동, 점프, 스프린트 설정 값 (데이터에서 주입)")]
    [HideInInspector] public MovementTuning movementTuning;

    [Header("Efficiency Settings")]
    [Tooltip("플레이어 효율(스태미너 유사) 관련 설정 값 (데이터에서 주입)")]
    [HideInInspector] public EfficiencyTuning efficiencyTuning;

    [Header("Animation Settings")]
    [Tooltip("애니메이션 보간 및 정지 처리 설정 값 (데이터에서 주입)")]
    [HideInInspector] public AnimTuning animTuning;

    private void Awake()
    {
        // 필수 컴포넌트 캐싱
        _cc = GetComponent<CharacterController>();

        // 효율 시스템 초기화
        efficiency.Init();

        // 이동 모듈 초기화 (캐릭터컨트롤러, 트랜스폼, 효율 모듈 참조)
        movement.Initialize(_cc, transform, efficiency);

        // 애니메이터 자동 검색
        if (!animatorSource)
            animatorSource = GetComponentInChildren<Animator>(true);
        animModule.Init(animatorSource);

        // 디바이스 컨트롤러 찾기
        _deviceController = GetComponent<TBSDeviceController>();

        // 인벤토리 초기화
        if (inventory != null)
            inventory.Init();

        // 아이템 픽업 모듈 초기화
        if (inventoryPickup != null)
            inventoryPickup.Init(transform, inventory);

        // 시간 시스템 자동 참조
        if (timeSystem == null)
            timeSystem = FindFirstObjectByType<TimeSystemController>();

        // 카메라 리그 자동 참조
        if (cameraRig == null)
            cameraRig = FindFirstObjectByType<CameraRigMouseLookTPS>();

        // 퀘스트 로그 자동 참조
        if (questLog == null)
            questLog = GetComponent<PlayerQuestLog>();

        // 인벤토리 UI 기본 비활성화
        if (inventoryUI != null)
            inventoryUI.SetActive(false);

        // 초기 마우스 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _inventoryOpen = false;

        // 데이터 테이블에서 튜닝 값 주입
        ApplyConfigFromDataController();

        // 인스펙터 튜닝값을 모듈에 적용
        ApplySettings();

        // 시작 시 카메라 모드 설정
        if (cameraRig != null)
            cameraRig.SetMode(defaultCameraMode);
    }

    private void Update()
    {
        // [중요] 매 프레임 스탯 동기화 (Stat -> Module)
        if (movement != null && playerStats != null)
        {
            // 스탯 값을 가져와서 이동 모듈에 주입
            movement.SetWalkSpeed(playerStats.WalkSpeed.Value);
        }

        // [추가] Q, E 키 입력 시 디바이스에게 "스킬 써!" 명령
        if (_deviceController != null)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log("Q 키 입력 감지됨! (0번 슬롯 실행 요청)");
                _deviceController.UseQuickSlot(0); // 0번 슬롯 실행
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("E 키 입력 감지됨! (1번 슬롯 실행 요청)");
                _deviceController.UseQuickSlot(1); // 1번 슬롯 실행
            }
        }

        // 인벤토리 열기/닫기 입력 처리
        if (Input.GetKeyDown(inventoryKey))
            ToggleInventory();

        // 인벤토리 열린 상태에서는 움직임과 시점 입력을 막는다
        if (_inventoryOpen)
            return;

        // 시점 전환 처리
        HandleViewToggle();

        // 이동 입력 및 점프 처리
        movement.Tick();

        bool moving = movement.HasMoveInput();
        bool sprintingPre = movement.IsSprinting();
        bool jumpTrig = movement.ConsumeJumpTriggered();

        // 1. 효율 모듈 갱신 
        efficiency.Tick(Time.deltaTime, moving, sprintingPre);

        // 2. 갱신된 효율에 따른 패널티 계산 -> 시간 시스템에 적용
        if (timeSystem != null)
        {
            // 효율 모듈에서 계산한 계단식 배율(1.0, 1.5, 2.5, 3.5)을 가져와서 세팅
            float currentPenalty = efficiency.ComputeCostMultiplier();
            timeSystem.SetExternalCostMultiplier(currentPenalty);
        }

        bool sprinting = movement.IsSprinting();

        // 애니메이션 업데이트 (Speed, Jump, Fall, Land, Sprint)
        animModule.Tick(
            Time.deltaTime,
            movement.GetPlanarSpeed(),
            movement.IsGrounded(),
            movement.GetVerticalVelocity(),
            jumpTrig,
            sprinting
        );

        // Turn 애니메이션 업데이트 (마우스 X 기준)
        float mouseX = Input.GetAxisRaw("Mouse X");
        animModule.UpdateTurn(
            mouseX,
            movement.GetPlanarSpeed(),
            movement.IsGrounded()
            );

        // 3. 시간 자원 소모 처리 (행동 기준 + 효율 페널티는 내부에서 적용됨)
        if (timeSystem != null)
        {
            if (moving)
            {
                if (sprinting)
                    timeSystem.SpendForSprintDelta(Time.deltaTime);
                else
                    timeSystem.SpendForWalkDelta(Time.deltaTime);
            }

            if (jumpTrig)
                timeSystem.SpendForJumpEvent();
        }

        // 아이템 줍기 처리
        if (inventoryPickup != null)
            inventoryPickup.Tick();
    }

    // 시점 전환 처리 로직
    private void HandleViewToggle()
    {
        if (cameraRig == null)
            return;

        if (!allowThirdPersonToggle)
            return;

        // 제한 조건에서 강제 1인칭 유지
        if (restrictThirdPersonDuringQuest && !CanUseThirdPerson())
        {
            if (cameraRig.CurrentMode != CameraRigMouseLookTPS.CameraMode.FirstPerson)
                cameraRig.SetMode(CameraRigMouseLookTPS.CameraMode.FirstPerson);

            return;
        }

        // 토글 키 입력 시 1인칭/3인칭 전환
        if (Input.GetKeyDown(viewToggleKey))
        {
            var next = cameraRig.CurrentMode == CameraRigMouseLookTPS.CameraMode.FirstPerson
                ? CameraRigMouseLookTPS.CameraMode.ThirdPerson
                : CameraRigMouseLookTPS.CameraMode.FirstPerson;

            cameraRig.SetMode(next);
        }
    }

    // 현재 상황에서 3인칭 사용 가능한지 확인
    private bool CanUseThirdPerson()
    {
        if (!allowThirdPersonToggle)
            return true;

        if (questLog == null)
            return true;

        // 퀘스트에서 카메라를 잠그는 경우
        if (questLog.IsCameraLocked())
            return false;

        if (restrictThirdPersonDuringQuest)
        {
            if (questLog.HasActiveQuest())
                return false;

            if (questLog.IsInTimedOrDangerQuest())
                return false;
        }

        return true;
    }

    // 인스펙터 튜닝값을 각 모듈로 전달
    private void ApplySettings()
    {
        movement.SyncSettings(
            movementTuning.jumpCooldown,
            movementTuning.coyoteTime,
            movementTuning.jumpBufferTime,
            movementTuning.walkSpeed,
            movementTuning.sprintMultiplier,
            movementTuning.sprintKey,
            movementTuning.sprintOnlyOnGround,
            movementTuning.jumpHeight,
            movementTuning.gravity,
            movementTuning.groundedStick,
            movementTuning.stopOnSprintExhaust,
            movementTuning.exhaustStopDuration
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

    private void ApplyConfigFromDataController()
    {
        var data = DataController.Instance;
        if (data == null)
            return;

        // Movement
        var moveCfg = data.MovementConfig;
        if (moveCfg != null)
        {
            movementTuning.walkSpeed = moveCfg.walkSpeed;
            movementTuning.sprintMultiplier = moveCfg.sprintMultiplier;
            movementTuning.sprintKey = moveCfg.sprintKey;
            movementTuning.sprintOnlyOnGround = moveCfg.sprintOnlyOnGround;

            movementTuning.jumpHeight = moveCfg.jumpHeight;
            movementTuning.jumpCooldown = moveCfg.jumpCooldown;
            movementTuning.coyoteTime = moveCfg.coyoteTime;
            movementTuning.jumpBufferTime = moveCfg.jumpBufferTime;

            movementTuning.gravity = moveCfg.gravity;
            movementTuning.groundedStick = moveCfg.groundedStick;

            movementTuning.stopOnSprintExhaust = moveCfg.stopOnSprintExhaust;
            movementTuning.exhaustStopDuration = moveCfg.exhaustStopDuration;
        }

        // Efficiency
        var effCfg = data.EfficiencyConfig;
        if (effCfg != null)
        {
            efficiencyTuning.max = effCfg.max;
            efficiencyTuning.walkDrainPerSecond = effCfg.walkDrainPerSecond;
            efficiencyTuning.sprintDrainPerSecond = effCfg.sprintDrainPerSecond;
            efficiencyTuning.jumpCost = effCfg.jumpCost;
            efficiencyTuning.idleRegenPerSecond = effCfg.idleRegenPerSecond;
            efficiencyTuning.moveRegenPerSecond = effCfg.moveRegenPerSecond;
            efficiencyTuning.regenDelay = effCfg.regenDelay;
        }

        // Anim
        var animCfg = data.AnimConfig;
        if (animCfg != null)
        {
            animTuning.dampUp = animCfg.dampUp;
            animTuning.dampDown = animCfg.dampDown;
            animTuning.stopSnapThreshold = animCfg.stopSnapThreshold;
        }
    }

    // 인벤토리 UI 열기/닫기와 마우스 상태 처리
    private void ToggleInventory()
    {
        if (inventoryUI == null)
            return;

        _inventoryOpen = !_inventoryOpen;
        inventoryUI.SetActive(_inventoryOpen);

        if (_inventoryOpen)
        {
            var invUI = inventoryUI.GetComponent<InventoryUI>();
            if (invUI != null)
                invUI.ForceRefresh();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // 외부에서 호출하는 피해 처리: 피해량만큼 시간 시스템에 전달하고 히트 이펙트 표시
    public void ApplyDamage(float amount)
    {
        if (timeSystem != null)
            timeSystem.SpendForDamage(amount);

        if (hitOverlay != null)
            hitOverlay.Flash(amount);
    }
}
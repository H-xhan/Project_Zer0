using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // Modules
    public MovementModule movement;              // move, jump, gravity
    public PlayerAnimModule animModule;          // animator param driver
    public StaminaModule stamina;                // stamina logic
    public InventoryModule inventory;            // data only
    public InventoryPickupModule inventoryPickup;// pickup helper
    public HealthModule health;                  // hp logic
    public TimeSystemController timeSystem;     //  time economy hub

    private System.Action<float> _onDamagedHandler;

    // Screen hit flash (UI)
    public HitOverlayUI hitOverlay;              // full-screen red flash

    // UI
    public GameObject inventoryUI;               // inventory panel root
    public KeyCode inventoryKey = KeyCode.Tab;

    // Animator ref
    public Animator animatorSource;

    CharacterController _cc;
    bool _inventoryOpen;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();

        // init modules
        if (stamina != null)
            stamina.Init();

        if (movement != null)
            movement.Initialize(_cc, transform, stamina);

        if (animatorSource == null)
            animatorSource = GetComponentInChildren<Animator>(true);

        if (animModule != null)
            animModule.Init(animatorSource);

        if (inventory != null)
            inventory.Init();

        if (inventoryPickup != null)
            inventoryPickup.Init(transform, inventory);

        if (health != null)
            health.Init();

        // bind screen hit flash
        if (health != null && hitOverlay != null)
        {
            _onDamagedHandler = amt => hitOverlay.Flash(amt);
            health.OnDamaged += _onDamagedHandler;
        }

        // start state
        if (inventoryUI != null)
            inventoryUI.SetActive(false);

         if (timeSystem == null)
        timeSystem = FindFirstObjectByType<TimeSystemController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _inventoryOpen = false;
    }


    void Update()
    {
        // inventory toggle
        if (Input.GetKeyDown(inventoryKey))
            ToggleInventory();

        // stop player input when inventory is open
        if (_inventoryOpen)
            return;

        // 1) 이동 처리(스프린트 의지/입력에 따라 이번 프레임 가정 상태 계산)
        if (movement != null)
            movement.Tick();

        // 2) 이번 프레임 상태 한 번만 읽어오기
        bool moving = movement != null && movement.HasMoveInput();
        bool sprintingPre = movement != null && movement.IsSprinting();
        bool jumpTrig = movement != null && movement.ConsumeJumpTriggered();

        // 3) 스태미너 틱(이번 프레임 가정 상태로 소모/회복 계산)
        if (stamina != null)
            stamina.Tick(Time.deltaTime, moving, sprintingPre);

        // 4) 스태미너 고갈되면 같은 프레임에 스프린트 즉시 해제
        if (stamina != null && sprintingPre && !stamina.CanSprint() && movement != null)
            movement.ForceStopSprint();

        // 5) 해제 반영된 스프린트 상태 재확인
        bool sprinting = movement != null && movement.IsSprinting();

        // 6) 애니메이션
        if (animModule != null && movement != null)
        {
            float planar = movement.GetPlanarSpeed();
            bool grounded = movement.IsGrounded();
            float vY = movement.GetVerticalVelocity();
            animModule.Tick(Time.deltaTime, planar, grounded, vY, jumpTrig, sprinting);
        }

        // 7) 시간 경제(추가 차감)
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

        // pickup
        if (inventoryPickup != null)
            inventoryPickup.Tick();

        // animation
        if (animModule != null && movement != null)
        {
            float planar = movement.GetPlanarSpeed();
            bool grounded = movement.IsGrounded();
            float vY = movement.GetVerticalVelocity();
            animModule.Tick(Time.deltaTime, planar, grounded, vY, jumpTrig, sprinting);
        }
    }

    void ToggleInventory()
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

    // external helpers
    public void ApplyDamage(float amount)
    {
        if (health != null) health.TakeDamage(amount);
    }

    public void ApplyHeal(float amount)
    {
        if (health != null) health.Heal(amount);
    }

    void OnDestroy()
    {
        if (health != null && _onDamagedHandler != null)
            health.OnDamaged -= _onDamagedHandler;
    }
}

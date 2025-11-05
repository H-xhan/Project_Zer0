using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Modules")]
    public MovementModule movement;
    public PlayerAnimModule animModule;
    public StaminaModule stamina;
    public InventoryModule inventory;
    public InventoryPickupModule inventoryPickup;

    [Header("UI")]
    public GameObject inventoryUI;        // 인벤토리 패널
    public KeyCode inventoryKey = KeyCode.Tab;

    [Header("Animation Ref")]
    public Animator animatorSource;

    CharacterController _cc;
    bool _inventoryOpen;                  // 현재 인벤토리 열림 상태

    void Awake()
    {
        _cc = GetComponent<CharacterController>();

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
    }

    void Update()
    {
        // 인벤토리 토글
        if (Input.GetKeyDown(inventoryKey))
        {
            ToggleInventory();
        }

        // 인벤토리 열려 있을 때는 입력 막기
        if (_inventoryOpen)
            return;

        // --- 기존 모듈 처리 ---
        if (movement != null)
            movement.Tick();

        bool sprinting = movement != null && movement.IsSprinting();
        bool moving = movement != null && movement.HasMoveInput();
        bool jumpTrig = movement != null && movement.ConsumeJumpTriggered();

        if (stamina != null)
            stamina.Tick(Time.deltaTime, moving, sprinting, jumpTrig);

        if (inventoryPickup != null)
            inventoryPickup.Tick();

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
}

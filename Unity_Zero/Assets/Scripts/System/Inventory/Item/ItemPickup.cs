using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [Header("What to give")]
    public ItemSO item;
    public int amount = 1;

    [Header("How to pick")]
    public KeyCode key = KeyCode.F;
    public bool autoOnTouch = false;

    [Header("FX")]
    public float rotateY = 45f;

    private bool _inRange;
    private Inventory _inventory;

    void Awake()
    {
        // 트리거 강제
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        // 물리 튀는 것 방지
        var rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Update()
    {
        if (rotateY != 0f)
            transform.Rotate(0f, rotateY * Time.unscaledDeltaTime, 0f, Space.World);

        if (!_inRange || _inventory == null || autoOnTouch) return;

        if (Input.GetKeyDown(key))
        {
            Debug.Log("[ItemPickup] F pressed in range");
            TryPickup();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[ItemPickup] OnTriggerEnter by {other.name}, tag={other.tag}");
        if (!other.CompareTag("Player")) return;

        _inventory = other.GetComponentInParent<Inventory>();
        _inRange = true;

        if (autoOnTouch) TryPickup();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _inRange = false;
        _inventory = null;
    }

    void TryPickup()
    {
        if (item == null)
        {
            Debug.LogWarning("[ItemPickup] ItemSO is null.");
            return;
        }
        if (_inventory == null)
        {
            Debug.LogError("[ItemPickup] Inventory reference is null.");
            return;
        }

        int before = amount;
        int remain = _inventory.AddItem(item, amount);
        int picked = before - remain;

        Debug.Log($"[ItemPickup] TryPickup => picked={picked}, remainOnGround={remain}");

        if (picked > 0)
        {
            amount = remain;

            // ✅ 퀘스트 시스템 연동 (Collect 퀘스트 체크)
            var tracker = FindFirstObjectByType<QuestTracker>();
            if (tracker != null && item != null)
            {
                tracker.NotifyCollect(item.name, picked);
                // item.name → QuestSO.objectiveParam 과 일치해야 함 (예: "Potion")
            }

            if (amount <= 0) Destroy(gameObject);
        }
        else
        {
            Debug.Log("[ItemPickup] Inventory full or not accepted.");
        }
    }
}

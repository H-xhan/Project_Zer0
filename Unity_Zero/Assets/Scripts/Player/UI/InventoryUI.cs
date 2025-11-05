using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Refs")]
    public PlayerController controller;   // PlayerController reference
    public Transform slotParent;          // Grid parent
    public GameObject slotPrefab;         // Slot prefab (must have 2 Texts: name, count)

    // cache
    private bool _built = false;

    void OnEnable()
    {
        if (controller == null)
            controller = FindFirstObjectByType<PlayerController>();

        if (controller != null && controller.inventory != null)
        {
            controller.inventory.OnInventoryChanged += Refresh;
            BuildIfNeeded();
            Refresh();
        }
    }

    void OnDisable()
    {
        if (controller != null && controller.inventory != null)
            controller.inventory.OnInventoryChanged -= Refresh;
    }

    // make public so PlayerController can call ForceRefresh -> BuildIfNeeded
    public void BuildIfNeeded()
    {
        if (_built) return;
        if (controller == null || controller.inventory == null) return;
        if (slotParent == null || slotPrefab == null) return;

        // clear current children
        for (int i = slotParent.childCount - 1; i >= 0; i--)
            Destroy(slotParent.GetChild(i).gameObject);

        // build slots by slotCount
        int count = controller.inventory.slotCount;
        for (int i = 0; i < count; i++)
            Instantiate(slotPrefab, slotParent);

        _built = true;
    }

    // make public so ForceRefresh can call it
    public void Refresh()
    {
        if (controller == null || controller.inventory == null) return;
        if (slotParent == null) return;

        int n = Mathf.Min(slotParent.childCount, controller.inventory.slotCount);
        for (int i = 0; i < n; i++)
        {
            var slot = controller.inventory.GetSlot(i);
            var go = slotParent.GetChild(i).gameObject;

            // expects two UGUI Texts: [0]=name, [1]=count
            var texts = go.GetComponentsInChildren<Text>(true);
            if (texts.Length >= 2)
            {
                if (slot.IsEmpty)
                {
                    texts[0].text = "-";
                    texts[1].text = "";
                }
                else
                {
                    texts[0].text = (slot.item != null) ? slot.item.displayName : "Unknown";
                    texts[1].text = (slot.item != null && slot.item.stackable) ? slot.count.ToString() : "";
                }
            }

            // optional icon if the prefab has an Image somewhere
            var img = go.GetComponentInChildren<Image>(true);
            if (img != null)
            {
                if (slot.IsEmpty) { img.enabled = false; img.sprite = null; }
                else { img.enabled = (slot.item.icon != null); img.sprite = slot.item.icon; }
            }
        }
    }

    // call this when opening the inventory (from PlayerController.ToggleInventory)
    public void ForceRefresh()
    {
        _built = false;     // force rebuild
        BuildIfNeeded();    // rebuild slots according to current slotCount
        Refresh();          // update texts and icons
    }
}

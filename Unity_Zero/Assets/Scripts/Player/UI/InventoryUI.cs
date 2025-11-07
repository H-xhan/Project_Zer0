using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("인벤토리를 보유한 플레이어 컨트롤러")]
    public PlayerController controller;

    [Tooltip("슬롯 항목들이 배치될 부모 트랜스폼")]
    public Transform slotParent;

    [Tooltip("단일 인벤토리 슬롯 프리팹")]
    public GameObject slotPrefab;

    // 슬롯이 한 번 생성되었는지 여부
    private bool _built;

    private void OnEnable()
    {
        // 플레이어 자동 참조
        if (controller == null)
            controller = FindFirstObjectByType<PlayerController>();

        // 인벤토리 변경 이벤트 구독
        if (controller != null && controller.inventory != null)
        {
            controller.inventory.OnInventoryChanged += Refresh;
            BuildIfNeeded();
            Refresh();
        }
    }

    private void OnDisable()
    {
        // 인벤토리 변경 이벤트 해제
        if (controller != null && controller.inventory != null)
            controller.inventory.OnInventoryChanged -= Refresh;
    }

    // 슬롯 UI가 아직 없으면 인벤토리 크기에 맞춰 생성
    public void BuildIfNeeded()
    {
        if (_built)
            return;
        if (controller == null || controller.inventory == null)
            return;
        if (slotParent == null || slotPrefab == null)
            return;

        // 기존 슬롯 제거
        for (int i = slotParent.childCount - 1; i >= 0; i--)
            Destroy(slotParent.GetChild(i).gameObject);

        // 인벤토리 슬롯 수만큼 프리팹 생성
        int count = controller.inventory.slotCount;
        for (int i = 0; i < count; i++)
            Instantiate(slotPrefab, slotParent);

        _built = true;
    }

    // 슬롯별 표시 정보 갱신
    public void Refresh()
    {
        if (controller == null || controller.inventory == null)
            return;
        if (slotParent == null)
            return;

        int n = Mathf.Min(slotParent.childCount, controller.inventory.slotCount);

        for (int i = 0; i < n; i++)
        {
            var slot = controller.inventory.GetSlot(i);
            var go = slotParent.GetChild(i).gameObject;

            // Text 컴포넌트: [0] 이름, [1] 개수 로 사용
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
                    texts[0].text = slot.item != null ? slot.item.displayName : "Unknown";
                    texts[1].text = (slot.item != null && slot.item.stackable)
                        ? slot.count.ToString()
                        : "";
                }
            }

            // 아이콘 이미지 처리
            var img = go.GetComponentInChildren<Image>(true);
            if (img != null)
            {
                if (slot.IsEmpty)
                {
                    img.enabled = false;
                    img.sprite = null;
                }
                else
                {
                    img.enabled = slot.item.icon != null;
                    img.sprite = slot.item.icon;
                    img.enabled = slot.item.icon != null;
                }
            }
        }
    }

    // 강제 재생성 및 갱신 (인벤토리 구조가 변경되었을 때 사용)
    public void ForceRefresh()
    {
        _built = false;
        BuildIfNeeded();
        Refresh();
    }
}

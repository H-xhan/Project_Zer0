using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Slots")]
    public int slotCount = 20;                     // 슬롯 개수
    public List<InventorySlot> slots = new();      // 슬롯들

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.Tab;
    private InventoryUI inventoryUI;// 인벤토리 열기/닫기

    public bool IsOpen { get; private set; }       // UI 열림 상태

    // UI에게 알려줄 이벤트 (UI는 이걸 구독해서 재그리기)
    public event Action OnInventoryChanged;
    public event Action<bool> OnInventoryOpenStateChanged;

    void Awake()
    {
        // 슬롯 리스트 초기화 (빈 슬롯 채우기)
        for (int i = slots.Count; i < slotCount; i++)
            slots.Add(new InventorySlot()); // 빈 슬롯 추가
    }

    private void Start()
    {
        // Unity 2023+ 권장 API
#if UNITY_2023_1_OR_NEWER
        inventoryUI = UnityEngine.Object.FindFirstObjectByType<InventoryUI>();
#else
inventoryUI = UnityEngine.Object.FindObjectOfType<InventoryUI>();
#endif

        if (inventoryUI == null)
        {
            Debug.LogWarning("Inventory.cs: 씬에서 InventoryUI를 찾지 못했습니다.");
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleOpen(); // 한 줄로 정리
        }


    }



    // ====== 공개 API ======

    public void ToggleOpen() => SetOpen(!IsOpen);

    public void SetOpen(bool open)
    {
        if (IsOpen == open) return;
        IsOpen = open;
        OnInventoryOpenStateChanged?.Invoke(IsOpen); // UI가 이걸 구독
        Debug.Log($"Inventory {(IsOpen ? "OPEN" : "CLOSE")}");
    }

    // 아이템 추가: 가능한 만큼 넣고, 남은 수량 반환(0이면 전부 성공)
    public int AddItem(ItemSO item, int amount)
    {
        if (item == null || amount <= 0) return amount; // 잘못된 입력 방어

        // 1) 같은 아이템 스택 먼저 채우기
        for (int i = 0; i < slots.Count && amount > 0; i++)
        {
            var s = slots[i];
            if (!s.IsEmpty && s.item == item && item.maxStack > 1) // 같은 아이템, 스택 가능
            {
                int move = Mathf.Min(s.SpaceLeft, amount);          // 넣을 수 있는 만큼
                s.count += move;                                    // 수량 증가
                amount -= move;                                     // 남은 수량 감소
            }
        }

        // 2) 빈 슬롯에 채우기
        for (int i = 0; i < slots.Count && amount > 0; i++)
        {
            var s = slots[i];
            if (s.IsEmpty)                                          // 빈 슬롯 발견
            {
                s.item = item;                                      // 아이템 할당
                int move = Mathf.Min(item.maxStack, amount);        // 스택 최대 고려
                s.count = move;                                     // 수량 설정
                amount -= move;                                     // 남은 수량 감소
            }
        }

        if (amountChanged()) OnInventoryChanged?.Invoke();          // UI 갱신 트리거
        return amount;                                              // 남은 수량(0이면 성공)

        // 내부: 변경 감지 (간단히 항상 true로 둬도됨)
        bool amountChanged() => true;
    }

    // 아이템 제거: 가능한 만큼 빼고, 실제로 뺀 수량 반환
    public int RemoveItem(ItemSO item, int amount)
    {
        if (item == null || amount <= 0) return 0;

        int removed = 0;

        // 뒤에서부터 빼면 UI에서 보기에 "최근 채운 곳부터 빠지는" 느낌도 연출 가능
        for (int i = slots.Count - 1; i >= 0 && removed < amount; i--)
        {
            var s = slots[i];
            if (!s.IsEmpty && s.item == item)
            {
                int take = Mathf.Min(s.count, amount - removed); // 뺄 수 있는 만큼
                s.count -= take;                                  // 수량 감소
                removed += take;

                if (s.count <= 0)                                 // 0이하가 되면 슬롯 비우기
                {
                    s.item = null;
                    s.count = 0;
                }
            }
        }

        if (removed > 0) OnInventoryChanged?.Invoke();            // UI 갱신
        return removed;                                           // 실제 제거량
    }

    // 슬롯 단위 사용(예: 소비아이템)
    public bool UseSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return false;

        var s = slots[index];
        if (s.IsEmpty) return false;

        // 아이템 타입에 따른 처리
        switch (s.item.type)
        {
            case ItemType.Consumable:
                // 소비 아이템: power를 회복량 등으로 사용
                var consumer = GetComponent<ItemConsumer>();          // 플레이어에 붙은 소비 처리자
                if (consumer != null)
                    consumer.Consume(s.item);                         // 실제 효과 적용
                s.count -= 1;                                         // 하나 소모
                if (s.count <= 0) { s.item = null; s.count = 0; }     // 비우기
                OnInventoryChanged?.Invoke();                         // UI 갱신
                return true;

            case ItemType.Equipment:
                // 장비: v2에서 장착 시스템으로 확장 (여긴 placeholder)
                Debug.Log($"장비 장착 예정: {s.item.displayName}");
                return false;

            default:
                Debug.Log($"사용 불가 타입: {s.item.type}");
                return false;
        }


    }


}

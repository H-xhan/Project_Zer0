using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryModule
{
    [Serializable]
    public struct Slot
    {
        public ItemSO item;
        public int count;

        // 아이템이 없거나 수량이 0 이하면 빈 슬롯으로 간주
        public bool IsEmpty => item == null || count <= 0;
    }

    [Header("Inventory")]
    [Tooltip("인벤토리 슬롯 개수")]
    public int slotCount = 20;

    // 실제 슬롯 리스트
    List<Slot> _slots;

    // 외부에서 읽기 전용으로 접근할 수 있는 슬롯 목록
    public IReadOnlyList<Slot> Slots => _slots;

    // UI 등에 변경사항을 알리기 위한 이벤트
    public Action OnInventoryChanged;

    // 초기화: 슬롯 개수에 맞게 리스트 생성
    public void Init()
    {
        if (_slots == null || _slots.Count != slotCount)
        {
            _slots = new List<Slot>(slotCount);
            for (int i = 0; i < slotCount; i++)
                _slots.Add(new Slot());
        }

        RaiseChanged();
    }

    // 아이템 추가 시도
    public bool TryAdd(ItemSO item, int amount)
    {
        if (item == null || amount <= 0 || _slots == null)
            return false;

        int remaining = amount;

        // 1) 스택 가능한 아이템이면 기존 스택에 채우기
        if (item.stackable)
        {
            for (int i = 0; i < _slots.Count && remaining > 0; i++)
            {
                if (_slots[i].item == item && _slots[i].count < item.maxStack)
                {
                    int canPut = Mathf.Min(item.maxStack - _slots[i].count, remaining);
                    var s = _slots[i];
                    s.count += canPut;
                    _slots[i] = s;
                    remaining -= canPut;
                }
            }
        }

        // 2) 남은 수량을 빈 슬롯에 채우기
        for (int i = 0; i < _slots.Count && remaining > 0; i++)
        {
            if (_slots[i].IsEmpty)
            {
                var s = _slots[i];
                s.item = item;

                if (item.stackable)
                {
                    int put = Mathf.Min(item.maxStack, remaining);
                    s.count = put;
                    remaining -= put;
                }
                else
                {
                    s.count = 1;
                    remaining -= 1;
                }

                _slots[i] = s;
            }
        }

        bool addedAny = remaining < amount;
        if (addedAny)
            RaiseChanged();

        return addedAny;
    }

    // 특정 아이템 제거 시도
    public bool TryRemove(ItemSO item, int amount)
    {
        if (item == null || amount <= 0 || _slots == null)
            return false;

        int need = amount;

        // 전체 보유 수량 확인
        int have = 0;
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i].item == item)
                have += _slots[i].count;
        }

        if (have < need)
            return false;

        // 슬롯에서 차감
        for (int i = 0; i < _slots.Count && need > 0; i++)
        {
            if (_slots[i].item == item)
            {
                int take = Mathf.Min(_slots[i].count, need);
                var s = _slots[i];
                s.count -= take;

                if (s.count <= 0)
                {
                    s.item = null;
                    s.count = 0;
                }

                _slots[i] = s;
                need -= take;
            }
        }

        RaiseChanged();
        return true;
    }

    // 인덱스로 슬롯 정보 조회
    public Slot GetSlot(int index)
    {
        if (_slots == null || index < 0 || index >= _slots.Count)
            return default;

        return _slots[index];
    }

    // 변경 이벤트 호출
    void RaiseChanged()
    {
        OnInventoryChanged?.Invoke();
    }
}

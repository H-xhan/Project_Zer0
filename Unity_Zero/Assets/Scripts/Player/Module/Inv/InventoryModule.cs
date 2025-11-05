using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryModule
{
    [Serializable]
    public struct Slot
    {
        public ItemSO item;
        public int count;

        public bool IsEmpty => item == null || count <= 0;
    }

    [Header("Inventory")]
    public int slotCount = 20;

    // internal
    private List<Slot> _slots;
    public IReadOnlyList<Slot> Slots => _slots;

    // event for UI
    public Action OnInventoryChanged;

    public void Init()
    {
        if (_slots == null || _slots.Count != slotCount)
        {
            _slots = new List<Slot>(slotCount);
            for (int i = 0; i < slotCount; i++) _slots.Add(new Slot());
        }
        RaiseChanged();
    }

    public bool TryAdd(ItemSO item, int amount)
    {
        if (item == null || amount <= 0) return false;

        int remaining = amount;

        // stack into existing
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

        // fill empty slots
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
        if (addedAny) RaiseChanged();
        return addedAny;
    }

    public bool TryRemove(ItemSO item, int amount)
    {
        if (item == null || amount <= 0) return false;

        int need = amount;

        // count first
        int have = 0;
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i].item == item) have += _slots[i].count;
        }
        if (have < need) return false;

        // remove
        for (int i = 0; i < _slots.Count && need > 0; i++)
        {
            if (_slots[i].item == item)
            {
                int take = Mathf.Min(_slots[i].count, need);
                var s = _slots[i];
                s.count -= take;
                if (s.count <= 0) { s.item = null; s.count = 0; }
                _slots[i] = s;
                need -= take;
            }
        }

        RaiseChanged();
        return true;
    }

    public Slot GetSlot(int index)
    {
        if (_slots == null || index < 0 || index >= _slots.Count) return default;
        return _slots[index];
    }

    void RaiseChanged()
    {
        OnInventoryChanged?.Invoke();
    }
}

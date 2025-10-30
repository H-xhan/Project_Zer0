using System;
using UnityEngine;

// 직렬화를 위해 class로 두고, 에디터/저장에서 그대로 보이게 함
[Serializable]
public class InventorySlot
{
    public ItemSO item;           // 아이템 데이터(Null이면 빈 슬롯)
    public int count;             // 수량

    public bool IsEmpty => item == null || count <= 0; // 빈 슬롯 판단

    public int SpaceLeft => IsEmpty ? (item != null ? item.maxStack : int.MaxValue)
                                    : Mathf.Max(0, item.maxStack - count); // 남은 스택 여유
}

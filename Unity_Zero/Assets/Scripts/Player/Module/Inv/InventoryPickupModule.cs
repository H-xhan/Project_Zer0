using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventoryPickupModule
{
    [Header("Settings")]
    public KeyCode pickupKey = KeyCode.F;
    public float pickupRange = 2.0f;       // 감지 반경
    public LayerMask itemLayer;            // 아이템 레이어

    [Header("FX")]
    public bool autoRotateItems = true;
    public float rotateSpeed = 45f;

    // internal references
    private Transform _player;
    private InventoryModule _inventory;

    // 캐시에 가까운 아이템 저장
    private List<ItemPickupTarget> _nearItems = new List<ItemPickupTarget>();

    public void Init(Transform player, InventoryModule inv)
    {
        _player = player;
        _inventory = inv;
    }

    public void Tick()
    {
        if (_player == null || _inventory == null) return;

        // 주변 아이템 탐색
        FindNearbyItems();

        // 입력 감지
        if (Input.GetKeyDown(pickupKey))
            TryPickupClosest();
    }

    void FindNearbyItems()
    {
        _nearItems.Clear();

        Collider[] hits = Physics.OverlapSphere(_player.position, pickupRange, itemLayer, QueryTriggerInteraction.Collide);
        foreach (var hit in hits)
        {
            var target = hit.GetComponent<ItemPickupTarget>();
            if (target != null)
                _nearItems.Add(target);
        }

        // 시각적 효과 (회전 등)
        if (autoRotateItems && _nearItems.Count > 0)
        {
            foreach (var t in _nearItems)
            {
                if (t != null && t.transform != null)
                    t.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
            }
        }
    }

    void TryPickupClosest()
    {
        if (_nearItems.Count == 0) return;

        ItemPickupTarget closest = null;
        float minDist = float.MaxValue;

        foreach (var t in _nearItems)
        {
            if (t == null) continue;
            float dist = Vector3.Distance(_player.position, t.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = t;
            }
        }

        if (closest != null)
        {
            bool ok = _inventory.TryAdd(closest.item, closest.amount);
            if (ok)
            {
                Object.Destroy(closest.gameObject);
            }
        }
    }
}

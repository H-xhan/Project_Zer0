using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventoryPickupModule
{
    [Header("Settings")]
    [Tooltip("아이템 줍기 입력 키")]
    public KeyCode pickupKey = KeyCode.F;

    [Tooltip("아이템 감지 반경")]
    public float pickupRange = 2.0f;

    [Tooltip("아이템이 속한 레이어 마스크")]
    public LayerMask itemLayer;

    [Header("FX")]
    [Tooltip("감지된 아이템을 자동 회전시킬지 여부")]
    public bool autoRotateItems = true;

    [Tooltip("아이템 자동 회전 속도")]
    public float rotateSpeed = 45f;

    // 플레이어 위치 참조
    Transform _player;

    // 인벤토리 참조
    InventoryModule _inventory;

    // 근처 아이템 캐시 목록
    readonly List<ItemPickupTarget> _nearItems = new List<ItemPickupTarget>();

    // 초기 설정: PlayerController에서 호출
    public void Init(Transform player, InventoryModule inv)
    {
        _player = player;
        _inventory = inv;
    }

    // 매 프레임 업데이트: PlayerController에서 Tick 호출
    public void Tick()
    {
        if (_player == null || _inventory == null)
            return;

        FindNearbyItems();

        // 입력으로 가장 가까운 아이템 줍기
        if (Input.GetKeyDown(pickupKey))
            TryPickupClosest();
    }

    // 주변 아이템 탐색 및 회전 이펙트 처리
    void FindNearbyItems()
    {
        _nearItems.Clear();

        Collider[] hits = Physics.OverlapSphere(
            _player.position,
            pickupRange,
            itemLayer,
            QueryTriggerInteraction.Collide
        );

        foreach (var hit in hits)
        {
            var target = hit.GetComponent<ItemPickupTarget>();
            if (target != null)
                _nearItems.Add(target);
        }

        if (autoRotateItems && _nearItems.Count > 0)
        {
            foreach (var t in _nearItems)
            {
                if (t != null && t.transform != null)
                    t.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
            }
        }
    }

    // 가장 가까운 아이템을 찾아 인벤토리에 추가
    void TryPickupClosest()
    {
        if (_nearItems.Count == 0)
            return;

        ItemPickupTarget closest = null;
        float minDist = float.MaxValue;

        foreach (var t in _nearItems)
        {
            if (t == null)
                continue;

            float dist = Vector3.Distance(_player.position, t.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = t;
            }
        }

        if (closest == null)
            return;

        bool added = _inventory.TryAdd(closest.item, closest.amount);
        if (added)
            Object.Destroy(closest.gameObject);
    }
}

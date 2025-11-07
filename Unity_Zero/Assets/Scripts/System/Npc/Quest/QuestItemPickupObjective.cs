using UnityEngine;

[RequireComponent(typeof(Collider))]
public class QuestItemPickupObjective : MonoBehaviour
{
    public QuestSO targetQuest;                 // ex) Q002_CollectItem
    public ItemSO item;
    public int amount = 1;
    public KeyCode interactKey = KeyCode.F;
    public bool autoPickupOnTouch = false;

    bool _playerInRange;
    bool _pickedUp;
    PlayerController _player;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;                  // 아이템은 트리거 유지
    }

    void OnTriggerEnter(Collider other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        _playerInRange = true;
        _player = pc;

        if (autoPickupOnTouch)
            TryPickup();
    }

    void OnTriggerExit(Collider other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        if (pc == _player)
        {
            _playerInRange = false;
            _player = null;
        }
    }

    void Update()
    {
        if (_pickedUp) return;
        if (!_playerInRange) return;
        if (!autoPickupOnTouch && Input.GetKeyDown(interactKey))
            TryPickup();
    }

    void TryPickup()
    {
        if (_pickedUp) return;
        if (_player == null) return;
        if (targetQuest == null || _player.questLog == null) return;

        // 타겟 퀘스트를 실제로 받은 상태가 아니면 무시
        if (!_player.questLog.IsQuestActive(targetQuest))
            return;

        // 인벤토리 추가
        if (item != null && amount > 0 && _player.inventory != null)
        {
            bool added = _player.inventory.TryAdd(item, amount);
            if (!added)
            {
                Debug.Log("[QuestItemPickupObjective] 인벤토리 공간 부족");
                return;
            }
        }

        // 퀘스트 완료
        _player.questLog.CompleteQuest(targetQuest);
        Debug.Log($"[QuestItemPickupObjective] {targetQuest.title} 완료 (아이템 회수)");

        _pickedUp = true;
        Destroy(gameObject);
    }
}

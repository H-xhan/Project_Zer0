using UnityEngine;

[RequireComponent(typeof(Collider))]
public class QuestItemPickupObjective : MonoBehaviour
{
    [Header("연동 퀘스트")]
    [Tooltip("이 아이템으로 완료할 퀘스트")]
    public QuestSO targetQuest;

    [Header("아이템 설정")]
    [Tooltip("플레이어에게 지급할 아이템")]
    public ItemSO item;

    [Tooltip("지급할 아이템 개수")]
    public int amount = 1;

    [Header("상호작용 설정")]
    [Tooltip("직접 상호작용으로 픽업할 때 사용하는 키")]
    public KeyCode interactKey = KeyCode.F;

    [Tooltip("트리거에 닿는 즉시 자동 픽업할지 여부")]
    public bool autoPickupOnTouch = false;

    // 상태 관리용
    bool _playerInRange;
    bool _pickedUp;
    PlayerController _player;

    private void Reset()
    {
        // 픽업 오브젝트는 트리거 사용
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null)
            return;

        _playerInRange = true;
        _player = pc;

        if (autoPickupOnTouch)
            TryPickup();
    }

    private void OnTriggerExit(Collider other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null)
            return;

        if (pc == _player)
        {
            _playerInRange = false;
            _player = null;
        }
    }

    private void Update()
    {
        if (_pickedUp)
            return;
        if (!_playerInRange)
            return;

        if (!autoPickupOnTouch && Input.GetKeyDown(interactKey))
            TryPickup();
    }

    // 퀘스트 진행 조건 확인 후 아이템 지급 및 완료 처리
    void TryPickup()
    {
        if (_pickedUp)
            return;
        if (_player == null)
            return;
        if (targetQuest == null || _player.questLog == null)
            return;

        // 퀘스트를 실제로 받은 상태가 아니면 처리하지 않음
        if (!_player.questLog.IsQuestActive(targetQuest))
            return;

        // 인벤토리에 추가
        if (item != null && amount > 0 && _player.inventory != null)
        {
            bool added = _player.inventory.TryAdd(item, amount);
            if (!added)
                return;
        }

        // 퀘스트 완료 처리
        _player.questLog.CompleteQuest(targetQuest);

        _pickedUp = true;
        Destroy(gameObject);
    }
}

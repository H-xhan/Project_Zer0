using UnityEngine;

[RequireComponent(typeof(Collider))]
public class QuestGiver : MonoBehaviour
{
    [Header("설정")]
    public string displayName = "NPC";
    public QuestSO quest;
    public KeyCode interactKey = KeyCode.F;

    private QuestTracker tracker;
    private bool _inRange;

    void Awake()
    {
        // 트리거 전용 콜라이더(캐릭터컨트롤러 X)
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        tracker = FindFirstObjectByType<QuestTracker>();
        if (!tracker) Debug.LogWarning("[QuestGiver] QuestTracker를 찾지 못했어요.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _inRange = true;
        Debug.Log($"[NPC] {_inRange} {displayName} 범위 진입");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _inRange = false;
    }

    void Update()
    {
        if (!_inRange || tracker == null) return;

        if (Input.GetKeyDown(interactKey))
        {
            // 1) 보상 수령 우선
            if (tracker.state == QuestState.CompletedUnclaimed && tracker.giver == this)
            {
                tracker.ClaimReward(this);
                return;
            }

            // 2) 진행 중이면 진행상황만 안내
            if (tracker.state == QuestState.Accepted && tracker.giver == this)
            {
                var q = tracker.currentQuest;
                if (q)
                    Debug.Log($"[NPC] 진행중: {q.title} ({tracker.progress}/{q.targetCount})");
                return;
            }

            // 3) 새 퀘스트 제공 (바쁠 땐 거절)
            if (!tracker.IsBusy && quest != null)
            {
                tracker.Accept(quest, this);
                return;
            }

            // 4) 다른 NPC 퀘스트 진행중
            if (tracker.IsBusy && tracker.giver != this)
            {
                Debug.Log("[NPC] 다른 곳에서 받은 퀘스트가 진행 중이에요.");
            }
        }
    }
}

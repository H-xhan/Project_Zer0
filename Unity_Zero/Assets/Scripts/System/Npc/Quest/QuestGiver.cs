using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class QuestGiver : MonoBehaviour
{
    [Header("설정")]
    public string displayName = "NPC";
    public NPCQuestListSO questList; // ✅ 여러 퀘스트 리스트
    public KeyCode interactKey = KeyCode.F;

    [SerializeField] private Collider interactTrigger; // 자식 SphereCollider 권장
    private QuestTracker tracker;
    private PlayerQuestLog log;  // ✅ 플레이어 퀘스트 진행 기록
    private bool _inRange;

    void Reset()
    {
        // 트리거 없으면 자동 생성
        if (interactTrigger == null)
        {
            var go = new GameObject("InteractTrigger");
            go.transform.SetParent(transform, false);

            var sphere = go.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = 1.5f;

            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            // 릴레이 연결
            var relay = go.AddComponent<TriggerRelay>();
            relay.target = this;

            interactTrigger = sphere;
        }
    }

    void Awake()
    {
        if (interactTrigger == null) Reset();

        tracker = FindFirstObjectByType<QuestTracker>();
        log = FindFirstObjectByType<PlayerQuestLog>();

        if (!tracker) Debug.LogWarning("[QuestGiver] QuestTracker를 찾지 못했어요.");
        if (!log) Debug.LogWarning("[QuestGiver] PlayerQuestLog를 찾지 못했어요.");
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

            // 2) 진행중 안내
            if (tracker.state == QuestState.Accepted && tracker.giver == this)
            {
                var q = tracker.currentQuest;
                if (q) Debug.Log($"[NPC:{displayName}] 진행중: {q.title} ({tracker.progress}/{q.targetCount})");
                return;
            }

            // 3) 새 퀘스트 제공
            if (!tracker.IsBusy)
            {
                var next = GetNextQuest(); // ✅ 자동으로 조건 맞는 퀘스트 선택
                if (next != null)
                {
                    tracker.Accept(next, this);
                    return;
                }
                else
                {
                    Debug.Log($"[NPC:{displayName}] 지금 줄 수 있는 퀘스트가 없습니다.");
                }
            }

            // 4) 다른 NPC 퀘스트 진행중
            if (tracker.IsBusy && tracker.giver != this)
            {
                Debug.Log("[NPC] 다른 곳에서 받은 퀘스트가 진행 중이에요.");
            }
        }
    }

    // ✅ NPC 퀘스트 리스트 중 다음 퀘스트 찾기
    private QuestSO GetNextQuest()
    {
        if (questList == null || log == null)
        {
            Debug.LogWarning($"[NPC:{displayName}] 퀘스트 리스트 또는 로그가 없습니다.");
            return null;
        }

        var candidates = questList.offers
            .Where(o => o.enabled && o.quest != null)
            .OrderBy(o => o.priority);

        foreach (var offer in candidates)
        {
            var q = offer.quest;
            string qid = q.questId;
            // QuestSO에 id(string) 필드가 있어야 함

            // (1) 선행 퀘스트 체크
            if (offer.requiredCompletedQuestIds != null && offer.requiredCompletedQuestIds.Length > 0)
            {
                bool allDone = offer.requiredCompletedQuestIds.All(req => log.HasCompleted(req));
                if (!allDone) continue;
            }

            // (2) 반복 / 쿨타임 체크
            if (!offer.repeatable && log.HasCompleted(qid))
                continue;
            if (offer.repeatable && log.IsOnCooldown(qid, offer.repeatCooldownSec))
                continue;

            // 조건을 모두 만족한 첫 퀘스트 반환
            return q;
        }

        return null;
    }

    // ===== 자식 트리거에서 릴레이로 호출됨 =====
    public void HandleTriggerEnter(Collider other)
    {
        var trackerFound = other.GetComponentInParent<QuestTracker>();
        if (trackerFound == null) return;

        if (!other.CompareTag("Player") && other.attachedRigidbody == null) return;

        var root = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;
        tracker = root.GetComponentInParent<QuestTracker>();
        if (tracker != null)
        {
            _inRange = true;
            tracker = trackerFound;
            //Debug.Log($"[NPC] '{displayName}' 범위 진입");
        }
    }

    public void HandleTriggerExit(Collider other)
    {
        if (!_inRange) return;

        var outFromPlayer = other.GetComponentInParent<QuestTracker>() != null;
        if (!outFromPlayer) return;

        _inRange = false;
        tracker = null;
        Debug.Log($"[NPC] '{displayName}' 범위 이탈");
    }
}

using UnityEngine;

[DisallowMultipleComponent]
public class QuestGiver : MonoBehaviour
{
    [Header("설정")]
    public string displayName = "NPC";
    public QuestSO quest;
    public KeyCode interactKey = KeyCode.F;

    [SerializeField] private Collider interactTrigger; // 자식 SphereCollider 권장
    private QuestTracker tracker;
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

            // ★ 릴레이 추가 + 타겟 연결
            var relay = go.AddComponent<TriggerRelay>();
            relay.target = this;

            interactTrigger = sphere;
        }
    }

    void Awake()
    {
        if (interactTrigger == null) Reset();
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

    // ===== 자식 트리거에서 릴레이로 호출됨 =====
    public void HandleTriggerEnter(Collider other)
    {
        // 플레이어 판정
        if (!other.CompareTag("Player") && other.attachedRigidbody == null) return;

        var root = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;
        tracker = root.GetComponentInParent<QuestTracker>();
        if (tracker != null)
        {
            _inRange = true;
            Debug.Log($"[NPC] '{displayName}' 범위 진입");
        }
    }

    public void HandleTriggerExit(Collider other)
    {
        if (!_inRange) return;
        _inRange = false;
        tracker = null;
        Debug.Log($"[NPC] '{displayName}' 범위 이탈");
    }
}

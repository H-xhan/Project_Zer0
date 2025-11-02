using UnityEngine;

public class QuestTracker : MonoBehaviour
{
    [Header("레퍼런스")]
    [SerializeField] private TimeWallet wallet;        // 보상 지급용
    [SerializeField] private TimeUI_Clean ui;          // 토스트 출력용

    [Header("상태")]
    public QuestSO currentQuest;
    public QuestGiver giver;                           // 퀘스트를 준 NPC
    public QuestState state = QuestState.None;
    public int progress;

    void Awake()
    {
        if (!wallet) wallet = FindFirstObjectByType<TimeWallet>();
        if (!ui) ui = FindFirstObjectByType<TimeUI_Clean>();
    }

    public bool IsBusy => state == QuestState.Accepted || state == QuestState.CompletedUnclaimed;

    // ──────────────────────────────────────────────────────────────
    // 수락
    public void Accept(QuestSO quest, QuestGiver from)
    {
        if (IsBusy)
        {
            ui?.ShowToast("이미 진행 중인 퀘스트가 있어요.");
            return;
        }
        if (!quest)
        {
            Debug.LogWarning("[퀘스트] 수락 실패: QuestSO가 null");
            return;
        }

        currentQuest = quest;
        giver = from;
        progress = 0;
        state = QuestState.Accepted;

        Debug.Log($"📝 [퀘스트] '{quest.title}' 수락: {quest.description}");
        ui?.ShowToast($"📝 퀘스트 시작: {quest.title}");
    }

    // ──────────────────────────────────────────────────────────────
    // 진행 알림: 도달형
    public void NotifyReachZone(string zoneName)
    {
        if (state != QuestState.Accepted || currentQuest == null) return;
        if (currentQuest.objectiveType != QuestObjectiveType.ReachZone) return;

        if (currentQuest.objectiveParam == zoneName)
        {
            progress++;
            TryComplete();
        }
    }

    // 진행 알림: 수집형 (선택)
    public void NotifyCollect(string key, int amount = 1)
    {
        if (state != QuestState.Accepted || currentQuest == null) return;
        if (currentQuest.objectiveType != QuestObjectiveType.Collect) return;

        if (currentQuest.objectiveParam == key)
        {
            progress += amount;
            TryComplete();
        }
    }

    // ──────────────────────────────────────────────────────────────
    // 목표 달성 체크 → “보상 미수령” 상태로 보류
    void TryComplete()
    {
        if (currentQuest == null) return;

        if (progress >= Mathf.Max(1, currentQuest.targetCount))
        {
            state = QuestState.CompletedUnclaimed;

            Debug.Log($"✅ [퀘스트] 완료: {currentQuest.title} → 보상은 NPC에게서 수령");
            var who = giver ? giver.displayName : "퀘스트 제공자";
            ui?.ShowToast($"✅ 완료! 보상 받으려면 [{who}]에게 돌아가세요.");
        }
        else
        {
            ui?.ShowToast($"진행 {progress}/{currentQuest.targetCount}");
        }
    }

    // ──────────────────────────────────────────────────────────────
    // NPC 상호작용 시 호출: 보상 지급
    public void ClaimReward(QuestGiver from)
    {
        // 다른 NPC에게서 수령 시도 방지
        if (state != QuestState.CompletedUnclaimed || currentQuest == null)
        {
            ui?.ShowToast("아직 보상을 받을 수 없어요.");
            return;
        }
        if (giver != null && from != giver)
        {
            ui?.ShowToast("이 퀘스트는 다른 NPC에게서 받았어요.");
            return;
        }

        float sec = currentQuest.timeRewardSeconds;
        if (wallet) wallet.AddSeconds(sec, $"퀘스트 보상: {currentQuest.title}");

        Debug.Log($"🎁 [퀘스트] 보상 수령: {currentQuest.title} → 시간 +{sec:0.#}초");
        ui?.ShowToast($"🎁 보상 수령! 시간 +{sec:0.#}초");

        state = QuestState.RewardClaimed;

        // 정리(원하면 다음 퀘스트를 위해 초기화)
        currentQuest = null;
        giver = null;
        progress = 0;
        state = QuestState.None;
    }

    // 필요하면 취소/리셋 API도 제공 가능
    public void CancelActiveQuest()
    {
        if (!IsBusy) return;
        Debug.Log("[퀘스트] 진행 중인 퀘스트 취소");
        ui?.ShowToast("퀘스트가 취소되었어요.");

        currentQuest = null;
        giver = null;
        progress = 0;
        state = QuestState.None;
    }
}

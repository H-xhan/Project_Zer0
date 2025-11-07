using UnityEngine;

[RequireComponent(typeof(Collider))]
public class QuestReachZone : MonoBehaviour
{
    [Header("이 트리거가 완료시킬 퀘스트")]
    public QuestSO targetQuest;                       // ex) Q001_ReachPoint

    [Header("조건 설정")]
    public bool requireQuestActive = true;            // true면 해당 퀘스트를 받은 상태여야만 완료
    public bool completeOnlyOnce = true;              // 한 번만 완료 처리

    bool _completed;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;                         // 존은 트리거 유지
    }

    void OnTriggerEnter(Collider other)
    {
        if (_completed && completeOnlyOnce) return;
        if (targetQuest == null) return;

        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null || pc.questLog == null) return;

        var log = pc.questLog;

        if (requireQuestActive && !log.IsQuestActive(targetQuest))
            return;

        log.CompleteQuest(targetQuest);
        _completed = true;

        Debug.Log($"[QuestReachZone] {targetQuest.title} 완료 (도착)");
    }
}

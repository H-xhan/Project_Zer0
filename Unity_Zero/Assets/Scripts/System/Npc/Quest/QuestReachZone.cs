using UnityEngine;

[RequireComponent(typeof(Collider))]
public class QuestReachZone : MonoBehaviour
{
    [Header("완료 대상 퀘스트")]
    [Tooltip("이 존에 도달하면 완료 처리할 퀘스트")]
    public QuestSO targetQuest;

    [Header("조건 설정")]
    [Tooltip("해당 퀘스트가 진행 중일 때만 완료할지 여부")]
    public bool requireQuestActive = true;

    [Tooltip("한 번만 완료 처리할지 여부")]
    public bool completeOnlyOnce = true;

    bool _completed;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_completed && completeOnlyOnce)
            return;
        if (targetQuest == null)
            return;

        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null || pc.questLog == null)
            return;

        var log = pc.questLog;

        if (requireQuestActive && !log.IsQuestActive(targetQuest))
            return;

        log.CompleteQuest(targetQuest);
        _completed = true;

        Debug.Log($"[QuestReachZone] {targetQuest.name} 목표 지점 도달, 퀘스트 완료");
    }
}

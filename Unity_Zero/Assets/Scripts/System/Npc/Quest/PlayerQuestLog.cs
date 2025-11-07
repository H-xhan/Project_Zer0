using System.Collections.Generic;
using UnityEngine;

public class PlayerQuestLog : MonoBehaviour
{
    [SerializeField] private List<QuestSO> activeQuests = new List<QuestSO>();
    [SerializeField] private List<QuestSO> completedQuests = new List<QuestSO>();
    [SerializeField] private bool cameraLock;

    public void StartQuest(QuestSO quest)
    {
        if (quest == null) return;
        if (IsQuestCompleted(quest)) return;
        if (IsQuestActive(quest)) return;

        activeQuests.Add(quest);
        Debug.Log($"[QuestLog] 퀘스트 시작: {quest.title}");
    }

    public void CompleteQuest(QuestSO quest)
    {
        if (quest == null) return;

        if (activeQuests.Contains(quest))
            activeQuests.Remove(quest);

        if (!completedQuests.Contains(quest))
        {
            completedQuests.Add(quest);
            Debug.Log($"[QuestLog] 퀘스트 완료: {quest.title}");
        }
    }

    public void SetCameraLock(bool value)
    {
        cameraLock = value;
        Debug.Log("[QuestLog] CameraLock = " + value);
    }

    public bool IsCameraLocked()
    {
        return cameraLock;
    }

    public bool IsQuestActive(QuestSO quest)
    {
        return quest != null && activeQuests.Contains(quest);
    }

    public bool IsQuestCompleted(QuestSO quest)
    {
        return quest != null && completedQuests.Contains(quest);
    }

    public bool HasActiveQuest()
    {
        return activeQuests.Count > 0;
    }

    public bool IsInTimedOrDangerQuest()
    {
        foreach (var q in activeQuests)
        {
            if (q == null) continue;
            if (q.isTimed || q.isDangerQuest)
                return true;
        }
        return false;
    }
}

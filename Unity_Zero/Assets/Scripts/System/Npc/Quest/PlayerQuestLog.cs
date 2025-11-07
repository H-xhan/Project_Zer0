using System.Collections.Generic;
using UnityEngine;

public class PlayerQuestLog : MonoBehaviour
{
    [Header("진행 중 퀘스트")]
    [Tooltip("현재 진행 중인 퀘스트 목록")]
    [SerializeField] private List<QuestSO> activeQuests = new List<QuestSO>();

    [Header("완료된 퀘스트")]
    [Tooltip("완료 처리된 퀘스트 목록")]
    [SerializeField] private List<QuestSO> completedQuests = new List<QuestSO>();

    [Header("카메라 제약 상태")]
    [Tooltip("퀘스트 진행에 따라 카메라를 잠글지 여부")]
    [SerializeField] private bool cameraLock;

    // 새로운 퀘스트 시작
    public void StartQuest(QuestSO quest)
    {
        if (quest == null)
            return;
        if (IsQuestCompleted(quest))
            return;
        if (IsQuestActive(quest))
            return;

        activeQuests.Add(quest);
    }

    // 퀘스트 완료 처리
    public void CompleteQuest(QuestSO quest)
    {
        if (quest == null)
            return;

        if (activeQuests.Contains(quest))
            activeQuests.Remove(quest);

        if (!completedQuests.Contains(quest))
            completedQuests.Add(quest);
    }

    // 카메라 락 on/off
    public void SetCameraLock(bool value)
    {
        cameraLock = value;
    }

    // 카메라 락 상태 조회
    public bool IsCameraLocked()
    {
        return cameraLock;
    }

    // 해당 퀘스트가 진행 중인지 확인
    public bool IsQuestActive(QuestSO quest)
    {
        return quest != null && activeQuests.Contains(quest);
    }

    // 해당 퀘스트가 이미 완료되었는지 확인
    public bool IsQuestCompleted(QuestSO quest)
    {
        return quest != null && completedQuests.Contains(quest);
    }

    // 진행 중인 퀘스트가 하나라도 있는지 확인
    public bool HasActiveQuest()
    {
        return activeQuests.Count > 0;
    }

    // 시간 제한 또는 위험 퀘스트가 진행 중인지 확인
    public bool IsInTimedOrDangerQuest()
    {
        foreach (var q in activeQuests)
        {
            if (q == null)
                continue;

            if (q.isTimed || q.isDangerQuest)
                return true;
        }

        return false;
    }
}

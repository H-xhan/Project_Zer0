// Assets/Scripts/System/Npc/Quest/QuestState.cs
public enum QuestState
{
    None,                 // 진행 없음
    Accepted,             // 수락 후 진행 중
    CompletedUnclaimed,   // 목표 달성했지만 보상 미수령
    RewardClaimed         // 보상 수령 완료
}

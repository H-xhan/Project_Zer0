using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectZer0/Quest/NPC Quest List", fileName = "NPCQuestList")]
public class NPCQuestListSO : ScriptableObject
{
    [Header("식별 및 설명")]
    public string npcId = "NPC_001";          // 저장/조건 체크 키
    [TextArea] public string description;

    [Header("제공 퀘스트들(위에서 아래 순서로 평가)")]
    public List<Offer> offers = new();

    [Serializable]
    public class Offer
    {
        public QuestSO quest;

        [Header("조건")]
        public string[] requiredCompletedQuestIds; // 선행 퀘스트(모두 완료 필요)
        public bool repeatable;                    // 반복 가능?
        public float repeatCooldownSec;            // 반복 쿨다운(완료 보상 수령 후 기준)

        [Header("표시/우선순위")]
        public bool enabled = true;                // 임시 비활성
        public int priority = 0;                   // 낮을수록 먼저
    }
}

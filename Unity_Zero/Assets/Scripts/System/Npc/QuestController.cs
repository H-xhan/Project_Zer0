using UnityEngine;

[RequireComponent(typeof(Collider))]
public class QuestController : MonoBehaviour
{
    [System.Serializable]
    public class QuestStep
    {
        [Header("퀘스트 데이터")]
        public QuestSO quest;

        [Header("보상 - 시간")]
        public float rewardTimeSeconds;

        [Header("보상 - 아이템 (선택)")]
        public ItemSO rewardItem;
        public int rewardItemAmount = 1;
    }

    [Header("퀘스트 체인 (순서대로)")]
    public QuestStep[] questSteps;

    [Header("상호작용 설정")]
    public KeyCode interactKey = KeyCode.F;

    [Header("연동 시스템")]
    public PlayerController playerController;       // Player 하나만 바라보는 구조
    public TimeSystemController timeSystem;         // 없으면 PlayerController에서 가져옴

    private bool _playerInRange;
    private bool[] _rewardGiven;

    void Awake()
    {
        //  더 이상 isTrigger 강제 설정 안 함 
        // 충돌 막는 콜라이더와 상호작용 트리거를 분리해서 쓰는 걸 추천.

        if (questSteps == null)
            questSteps = new QuestStep[0];

        _rewardGiven = new bool[questSteps.Length];
    }

    void Start()
    {
        if (playerController == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerController = playerObj.GetComponent<PlayerController>();
        }

        if (timeSystem == null && playerController != null)
            timeSystem = playerController.timeSystem;
    }

    void OnTriggerEnter(Collider other)
    {
        // 태그 대신 PlayerController를 부모까지 검색해서 찾기
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        _playerInRange = true;

        if (playerController == null)
            playerController = pc;
    }

    void OnTriggerExit(Collider other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        if (pc == playerController)
            _playerInRange = false;
    }

    void Update()
    {
        if (!_playerInRange) return;
        if (questSteps == null || questSteps.Length == 0) return;
        if (playerController == null || playerController.questLog == null) return;

        if (Input.GetKeyDown(interactKey))
            HandleInteraction();
    }

    private void HandleInteraction()
    {
        var log = playerController.questLog;

        int latestCompletedIndex = GetLatestCompletedIndex(log);
        int activeIndex = GetActiveIndex(log);

        // 진행 중 퀘스트가 있으면: 안내만
        if (activeIndex >= 0)
        {
            var step = questSteps[activeIndex];
            Debug.Log($"[QuestController] 진행 중: {step.quest.title}");
            return;
        }

        // 직전 퀘스트 완료 후 → 보상 + 다음 퀘
        if (latestCompletedIndex >= 0 && latestCompletedIndex < questSteps.Length)
        {
            if (!_rewardGiven[latestCompletedIndex])
            {
                GrantReward(latestCompletedIndex);
                _rewardGiven[latestCompletedIndex] = true;
            }

            int nextIndex = latestCompletedIndex + 1;
            if (nextIndex < questSteps.Length)
            {
                var nextStep = questSteps[nextIndex];
                if (nextStep.quest != null && !log.IsQuestCompleted(nextStep.quest))
                {
                    log.StartQuest(nextStep.quest);
                    Debug.Log($"[QuestController] 다음 퀘스트 시작: {nextStep.quest.title}");
                }
            }
            else
            {
                //더 이상 줄 퀘스트 없으면 체인 완전 종료 → 카메라 락 해제
                log.SetCameraLock(false);
                Debug.Log("[QuestController] 모든 퀘스트 체인 완료 / CameraLock OFF");
            }

            return;
        }

        if (latestCompletedIndex < 0 && questSteps.Length > 0)
        {
            var first = questSteps[0];
            if (first.quest != null)
            {
                log.StartQuest(first.quest);

                // 이 체인 시작하는 순간 카메라 락 켜기
                log.SetCameraLock(true);

                Debug.Log($"[QuestController] 첫 퀘스트 시작: {first.quest.title} / CameraLock ON");
            }
        }
    }

    private int GetLatestCompletedIndex(PlayerQuestLog log)
    {
        int latest = -1;
        for (int i = 0; i < questSteps.Length; i++)
        {
            var q = questSteps[i].quest;
            if (q == null) break;

            if (log.IsQuestCompleted(q))
                latest = i;
            else
                break;
        }
        return latest;
    }

    private int GetActiveIndex(PlayerQuestLog log)
    {
        for (int i = 0; i < questSteps.Length; i++)
        {
            var q = questSteps[i].quest;
            if (q != null && log.IsQuestActive(q))
                return i;
        }
        return -1;
    }

    private void GrantReward(int index)
    {
        if (index < 0 || index >= questSteps.Length) return;
        var step = questSteps[index];

        if (timeSystem != null && step.rewardTimeSeconds > 0f)
        {
            timeSystem.AddSeconds(step.rewardTimeSeconds, "QuestReward");
            Debug.Log($"[QuestController] 시간 보상 +{step.rewardTimeSeconds}초 지급");
        }

        if (playerController != null &&
            playerController.inventory != null &&
            step.rewardItem != null &&
            step.rewardItemAmount > 0)
        {
            bool added = playerController.inventory.TryAdd(step.rewardItem, step.rewardItemAmount);
            Debug.Log($"[QuestController] 아이템 보상 {step.rewardItem.name} x{step.rewardItemAmount} 지급 (성공: {added})");
        }

        Debug.Log($"[QuestController] 퀘스트 보상 지급 완료 (Step {index + 1})");
    }
}

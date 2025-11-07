using UnityEngine;

[RequireComponent(typeof(Collider))]
public class QuestController : MonoBehaviour
{
    [System.Serializable]
    public class QuestStep
    {
        [Header("퀘스트 데이터")]
        [Tooltip("이 스텝에서 시작되거나 완료를 확인할 퀘스트")]
        public QuestSO quest;

        [Header("시간 보상")]
        [Tooltip("퀘스트 완료 시 지급할 시간(초)")]
        public float rewardTimeSeconds;

        [Header("아이템 보상 (선택)")]
        [Tooltip("퀘스트 완료 시 지급할 아이템")]
        public ItemSO rewardItem;

        [Tooltip("아이템 보상 개수")]
        public int rewardItemAmount = 1;
    }

    [Header("퀘스트 체인")]
    [Tooltip("순서대로 진행될 퀘스트 스텝 목록")]
    public QuestStep[] questSteps;

    [Header("상호작용 설정")]
    [Tooltip("NPC와 상호작용하여 퀘스트를 시작/진행하는 키")]
    public KeyCode interactKey = KeyCode.F;

    [Header("연동 시스템")]
    [Tooltip("플레이어 컨트롤러 (비워두면 자동 탐색)")]
    public PlayerController playerController;

    [Tooltip("시간 시스템 (비워두면 PlayerController에서 참조)")]
    public TimeSystemController timeSystem;

    bool _playerInRange;
    bool[] _rewardGiven;

    private void Awake()
    {
        if (questSteps == null)
            questSteps = new QuestStep[0];

        _rewardGiven = new bool[questSteps.Length];

        // NPC 콜라이더 트리거 강제
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void Start()
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

    private void OnTriggerEnter(Collider other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null)
            return;

        _playerInRange = true;
        if (playerController == null)
            playerController = pc;
    }

    private void OnTriggerExit(Collider other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null)
            return;

        if (pc == playerController)
            _playerInRange = false;
    }

    private void Update()
    {
        if (!_playerInRange)
            return;
        if (questSteps == null || questSteps.Length == 0)
            return;
        if (playerController == null || playerController.questLog == null)
            return;

        if (Input.GetKeyDown(interactKey))
            HandleInteraction();
    }

    void HandleInteraction()
    {
        var log = playerController.questLog;

        int latestCompletedIndex = GetLatestCompletedIndex(log);
        int activeIndex = GetActiveIndex(log);

        // 진행 중인 퀘스트가 있을 때
        if (activeIndex >= 0)
        {
            var activeQuest = questSteps[activeIndex].quest;
            if (log.IsQuestCompleted(activeQuest))
            {
                Debug.Log($"[QuestController] {activeQuest.name} 완료 상태 (보상 대기 중)");
            }
            else
            {
                Debug.Log($"[QuestController] {activeQuest.name} 진행 중");
            }
            return;
        }

        // 직전 퀘스트 완료 후 보고 및 다음 스텝으로
        if (latestCompletedIndex >= 0 && latestCompletedIndex < questSteps.Length)
        {
            if (!_rewardGiven[latestCompletedIndex])
            {
                Debug.Log($"[QuestController] {questSteps[latestCompletedIndex].quest.name} 보고 완료 → 보상 지급");
                GrantReward(latestCompletedIndex);
                _rewardGiven[latestCompletedIndex] = true;
            }

            int nextIndex = latestCompletedIndex + 1;

            // 다음 퀘스트 시작
            if (nextIndex < questSteps.Length)
            {
                var nextStep = questSteps[nextIndex];
                if (nextStep.quest != null && !log.IsQuestCompleted(nextStep.quest))
                {
                    log.StartQuest(nextStep.quest);
                    log.SetCameraLock(true);
                    Debug.Log($"[QuestController] 다음 퀘스트 시작: {nextStep.quest.name}");
                }
            }
            else
            {
                log.SetCameraLock(false);
                Debug.Log("[QuestController] 모든 퀘스트 체인 완료");
            }

            return;
        }

        // 아무 퀘스트도 진행 중이거나 완료되지 않은 경우 → 첫 퀘스트 시작
        if (latestCompletedIndex < 0 && questSteps.Length > 0)
        {
            var first = questSteps[0];
            if (first.quest != null)
            {
                log.StartQuest(first.quest);
                log.SetCameraLock(true);
                Debug.Log($"[QuestController] 첫 퀘스트 시작: {first.quest.name}");
            }
        }
    }

    int GetLatestCompletedIndex(PlayerQuestLog log)
    {
        int latest = -1;
        for (int i = 0; i < questSteps.Length; i++)
        {
            var q = questSteps[i].quest;
            if (q == null)
                break;

            if (log.IsQuestCompleted(q))
                latest = i;
            else
                break;
        }
        return latest;
    }

    int GetActiveIndex(PlayerQuestLog log)
    {
        for (int i = 0; i < questSteps.Length; i++)
        {
            var q = questSteps[i].quest;
            if (q != null && log.IsQuestActive(q))
                return i;
        }
        return -1;
    }

    void GrantReward(int index)
    {
        if (index < 0 || index >= questSteps.Length)
            return;

        var step = questSteps[index];

        // 시간 보상
        if (timeSystem != null && step.rewardTimeSeconds > 0f)
        {
            timeSystem.AddSeconds(step.rewardTimeSeconds, "QuestReward");
            Debug.Log($"[QuestController] 시간 보상 +{step.rewardTimeSeconds:F1}초 지급");
        }

        // 아이템 보상
        if (playerController != null &&
            playerController.inventory != null &&
            step.rewardItem != null &&
            step.rewardItemAmount > 0)
        {
            bool added = playerController.inventory.TryAdd(step.rewardItem, step.rewardItemAmount);
            if (added)
                Debug.Log($"[QuestController] 아이템 보상: {step.rewardItem.name} x{step.rewardItemAmount}");
        }
    }
}

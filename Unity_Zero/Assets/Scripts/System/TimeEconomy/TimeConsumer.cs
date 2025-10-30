using UnityEngine;

public class TimeConsumer : MonoBehaviour
{
    [SerializeField] private TimeWallet wallet;
    [SerializeField] private TimeConfig config;
    [SerializeField] private TimeCostProfile costs;

    void Awake()
    {
        if (!wallet) wallet = FindFirstObjectByType<TimeWallet>();
        if (!config) config = Resources.Load<TimeConfig>("TimeConfig"); // 선택사항
    }

    // 이동 거리 기반 차감
    public void SpendForMove(float meters)
    {
        float perM = costs ? costs.moveCostPerMeter :
                    config ? config.moveCostPerMeter : 0.1f;

        float spend = Mathf.Max(0f, meters * perM);
        if (spend > 0f) wallet.SpendSeconds(spend, $"Move {meters:F2}m");
    }

    // 스프린트 중 초당 추가 차감
    public void SpendForSprintDelta(float deltaTime)
    {
        float extra = costs ? costs.sprintExtraPerSecond :
                      config ? config.sprintExtraPerSecond : 0.2f;

        float spend = Mathf.Max(0f, deltaTime * extra);
        if (spend > 0f) wallet.SpendSeconds(spend, "Sprinting");
    }

    public void SpendForJump()
    {
        float c = costs ? costs.jumpCost :
                 config ? config.jumpCost : 0.5f;
        if (c > 0f) wallet.SpendSeconds(c, "Jump");
    }

    public void SpendForAttack()
    {
        float c = costs ? costs.attackCost : 0.5f;
        if (c > 0f) wallet.SpendSeconds(c, "Attack");
    }

    public void SpendForInteract()
    {
        float c = costs ? costs.interactCost : 0.2f;
        if (c > 0f) wallet.SpendSeconds(c, "Interact");
    }

    public void SpendForInventoryOpen()
    {
        float c = costs ? costs.inventoryOpenCost : 0.1f;
        if (c > 0f) wallet.SpendSeconds(c, "Inventory Open");
    }
}

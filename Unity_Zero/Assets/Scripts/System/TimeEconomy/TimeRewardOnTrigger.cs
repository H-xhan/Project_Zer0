using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TimeRewardOnTrigger : MonoBehaviour
{
    [Tooltip("이 트리거에 들어오면 지급할 시간(초)")]
    public float rewardSeconds = 30f; // ⏱️ 예: 30초 지급
    public bool destroyAfterGive = true;        // 1회성 보상 여부

    private void OnTriggerEnter(Collider other)
    {
        var wallet = other.GetComponentInParent<TimeWallet>();
        if (!wallet) wallet = FindFirstObjectByType<TimeWallet>();
        if (!wallet) return;

        wallet.AddSeconds(rewardSeconds, $"{name} reward"); // ⏱️ 시간 지급

        if (destroyAfterGive) Destroy(gameObject);
    }
}

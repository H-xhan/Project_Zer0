using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TimeMarket : MonoBehaviour
{
    [Header("거래 설정")]
    public TimeTransactionType transactionType = TimeTransactionType.Buy; // Buy=구매, Sell=판매
    public float timeAmount = 100f;          // 거래되는 '시간(초)'
    public int creditCost = 50;              // 거래 비용(크레딧, int로 고정)
    public KeyCode interactKey = KeyCode.F;

    [Header("보조(거리 체크)")]
    public bool useDistanceFallback = true;
    public float interactDistance = 2.2f;

    [Header("참조")]
    [SerializeField] private TimeWallet wallet;
    [SerializeField] private PlayerCurrency currency;

    private bool _inRange;
    private Transform _player;

    void Reset()
    {
        // 트리거 설정 자동화
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        var rb = gameObject.GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true; rb.useGravity = false;
    }

    void Awake()
    {
        if (!wallet) wallet = FindFirstObjectByType<TimeWallet>();
        if (!currency) currency = FindFirstObjectByType<PlayerCurrency>();

        var cc = FindFirstObjectByType<CharacterController>();
        if (cc) _player = cc.transform;

        if (!wallet) Debug.LogError("⏳ [시간 거래소] TimeWallet을 찾을 수 없습니다.");
        if (!currency) Debug.LogWarning("💰 [시간 거래소] PlayerCurrency가 없어 크레딧 검증을 생략합니다(무료 거래).");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>())
        {
            _inRange = true;
            Debug.Log("🏪 [시간 거래소] 플레이어 접근 (F키로 상호작용)");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>())
        {
            _inRange = false;
            Debug.Log("🚶 [시간 거래소] 플레이어가 거래 범위를 벗어났습니다.");
        }
    }

    void Update()
    {
        bool canInteract = _inRange;

        // 트리거가 불안정할 때 거리 보조
        if (!canInteract && useDistanceFallback && _player)
            canInteract = Vector3.Distance(_player.position, transform.position) <= interactDistance;

        if (canInteract && Input.GetKeyDown(interactKey))
        {
            if (!wallet) { Debug.LogWarning("⏳ [시간 거래소] Wallet 없음"); return; }

            if (transactionType == TimeTransactionType.Buy) TryBuy();
            else TrySell();
        }
    }

    void TryBuy()
    {
        // 크레딧 체크 (currency 없으면 무료 구매)
        bool canPay = (currency == null) || currency.TrySpend(creditCost);
        if (!canPay)
        {
            Debug.Log("❌ [시간 거래소] 크레딧이 부족합니다.");
            return;
        }

        wallet.AddSeconds(timeAmount, "시간 구매");
        Debug.Log($"🕒 [시간 거래소] {creditCost} 크레딧으로 {timeAmount}초를 구매했습니다. 현재: {wallet.CurrentSeconds:0.##}초");
    }

    void TrySell()
    {
        // 판매: 시간 보유량 확인 → 차감 → 크레딧 지급
        if (wallet.CurrentSeconds < timeAmount)
        {
            Debug.Log("❌ [시간 거래소] 판매할 시간이 부족합니다.");
            return;
        }

        wallet.SpendSeconds(timeAmount, "시간 판매");
        currency?.AddCredits(creditCost);
        Debug.Log($"💸 [시간 거래소] {timeAmount}초 판매, {creditCost} 크레딧 획득. 현재: {wallet.CurrentSeconds:0.##}초");
    }
}

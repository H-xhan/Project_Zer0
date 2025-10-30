using UnityEngine;

public class TimeSinkInteractable : MonoBehaviour
{
    [Tooltip("이 장치를 사용(열기/가동)하는 데 필요한 시간 비용(초)")]
    public float costSeconds = 5f;

    [Tooltip("상호작용 키")]
    public KeyCode key = KeyCode.F;

    [Header("Targets")]
    public GameObject targetToToggle; // 문/장치 등

    private bool _playerInRange;
    private TimeWallet _wallet;

    void Awake()
    {
        _wallet = FindFirstObjectByType<TimeWallet>();
    }

    void Update()
    {
        if (_playerInRange && Input.GetKeyDown(key))
        {
            if (_wallet != null && _wallet.CurrentSeconds >= costSeconds)
            {
                _wallet.SpendSeconds(costSeconds, $"{name} use"); // ⏱️ 시간 지불
                if (targetToToggle) targetToToggle.SetActive(!targetToToggle.activeSelf);
            }
            else
            {
                Debug.Log("[TimeSink] Not enough time!");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) _playerInRange = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) _playerInRange = false;
    }
}

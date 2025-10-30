using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    public ItemSO item;           // 어떤 아이템인지
    public int amount = 1;        // 몇 개를 줄지
    public KeyCode pickupKey = KeyCode.E; // 줍기 키

    public float promptDistance = 2.0f;   // 플레이어 접근 거리
    public string promptText = "E: 줍기"; // (UI 연동은 프로젝트 규칙에 맞춰)

    private Transform _player;
    private Inventory _playerInv;
    private bool _inRange;

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;                     // 트리거로 사용 (범위 감지)
    }

    void Start()
    {
        // 간단히 태그로 플레이어 찾기 (프로젝트에 맞게 변경)
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj)
        {
            _player = playerObj.transform;
            _playerInv = playerObj.GetComponent<Inventory>();
        }
    }

    void Update()
    {
        if (_player == null || _playerInv == null) return;

        // 거리 체크로 간단한 프롬프트 처리(정교한 UI는 별도)
        _inRange = Vector3.Distance(_player.position, transform.position) <= promptDistance;

        if (_inRange && Input.GetKeyDown(pickupKey))
        {
            int left = _playerInv.AddItem(item, amount); // 남는 수량
            if (left == 0)
            {
                Destroy(gameObject);                     // 전부 들어갔으면 픽업 오브젝트 제거
            }
            else
            {
                amount = left;                           // 일부만 들어갔으면 잔량 갱신
            }
        }
    }

    // (선택) OnGUI로 초간단 프롬프트 표시 — 임시 디버그용
    void OnGUI()
    {
        if (!_inRange) return;

        // 🔒 카메라 null 방어
        if (Camera.main == null) return;

        Vector3 screen = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.0f);
        var rect = new Rect(screen.x - 60, Screen.height - screen.y - 30, 120, 20);
        GUI.Label(rect, promptText);
    }

}

using System.Collections.Generic;           // 리스트 사용
using UnityEngine;
using UnityEngine.UI;                       // uGUI
using TMPro;                                // TextMeshPro

public class InventoryUI : MonoBehaviour
{


    [Header("References")]
    public Inventory inventory;             // 플레이어 인벤토리 참조
    public GameObject slotPrefab;           // 슬롯 프리팹 (루트: Button, 자식: Icon(Image), Count(TMP))
    public Transform gridRoot;              // GridLayoutGroup 달린 부모

    [Header("Open/Close")]
    public GameObject inventoryPanel;       // 인벤토리 전체 패널(Active 토글)

    private readonly List<SlotWidget> _widgets = new(); // 슬롯 위젯 캐시

    private void Awake()
    {
        // ✅ 자동 와이어 (인스펙터 비어있을 때 안전)
        if (inventory == null) inventory = FindFirstObjectByType<Inventory>();
        if (inventoryPanel == null)
        {
            var t = transform.Find("Inventory");          // 같은 캔버스 하위에 "Inventory"가 있다면 자동 연결
            if (t) inventoryPanel = t.gameObject;
        }
        if (gridRoot == null)
        {
            var g = transform.Find("Inventory/Grid");     // 기본 경로 시도
            if (!g && inventoryPanel) g = inventoryPanel.transform.Find("Grid");
            if (g) gridRoot = g;
        }

        // 필수 참조 검사
        if (!inventory) Debug.LogError("InventoryUI: Inventory 참조가 없습니다.");
        if (!inventoryPanel) Debug.LogError("InventoryUI: Inventory Panel 참조가 없습니다.");
        if (!gridRoot) Debug.LogError("InventoryUI: Grid Root 참조가 없습니다.");
        if (!slotPrefab) Debug.LogError("InventoryUI: Slot Prefab 참조가 없습니다.");
    }

    private void Start()
    {
        // 인벤토리 이벤트 구독
        if (inventory)
        {
            inventory.OnInventoryChanged += Redraw;           // 내용 바뀌면 다시 그림
            inventory.OnInventoryOpenStateChanged += SetOpen; // 열기 상태 변경 반영
        }

        Build();                                 // 슬롯 UI 생성
        SetOpen(inventory != null && inventory.IsOpen); // 시작 상태 반영
        Redraw();                                // 첫 렌더
    }



    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= Redraw;
            inventory.OnInventoryOpenStateChanged -= SetOpen;
        }
    }

    // 👇 Inventory.cs 에서 호출할 공개 토글 API
    public void ToggleOpen()
    {
        if (!inventoryPanel)
        {
            Debug.LogError("InventoryUI.ToggleOpen: inventoryPanel 이 비었습니다.");
            return;
        }
        SetOpen(!inventoryPanel.activeSelf);     // 현재 상태 반대로
    }

    // 패널 열고/닫기 + 커서 처리
    public void SetOpen(bool open)
    {
        Debug.Log($"[InventoryUI] SetOpen({open}) panel={(inventoryPanel ? inventoryPanel.name : "NULL")} grid={(gridRoot ? gridRoot.childCount : -1)} widgets={_widgets.Count}");

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(open); // 패널 열고 닫기

            // 선택사항: 커서 표시 상태도 함께 변경하면 편리함
            Cursor.visible = open;
            Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        }
            if (!inventoryPanel) return;
        inventoryPanel.SetActive(open);

        // 프로젝트 정책에 맞춰 커서/입력 잠금
        Cursor.visible = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
    }

    // 슬롯 UI 생성
    private void Build()
    {
        if (!gridRoot || !slotPrefab || inventory == null) return;

        foreach (Transform c in gridRoot) Destroy(c.gameObject); // 기존 슬롯 제거
        _widgets.Clear();

        for (int i = 0; i < inventory.slots.Count; i++)
        {
            var go = Instantiate(slotPrefab, gridRoot);     // 프리팹 인스턴스
            var w = new SlotWidget(go);                    // 참조 바인딩

            int index = i;                                  // 클로저 캡쳐 안전
            if (w.button != null)
                w.button.onClick.AddListener(() => inventory.UseSlot(index)); // 클릭 시 사용

            _widgets.Add(w);
        }
    }

    // 슬롯 다시 그리기
    private void Redraw()
    {
        if (inventory == null || _widgets.Count != inventory.slots.Count) return;

        for (int i = 0; i < _widgets.Count; i++)
        {
            var s = inventory.slots[i];     // 데이터
            var w = _widgets[i];            // 위젯

            if (s.IsEmpty)
            {
                if (w.icon) w.icon.enabled = false;         // 아이콘 숨김
                w.SetCount("");                             // 수량 숨김
            }
            else
            {
                if (w.icon)
                {
                    w.icon.enabled = true;
                    w.icon.sprite = s.item.icon;            // 아이콘 이미지 적용
                    w.icon.preserveAspect = true;           // 비율 유지
                }

                // 스택형만 숫자 표시
                var txt = (s.item.maxStack > 1 && s.count > 1) ? s.count.ToString() : "";
                w.SetCount(txt);
            }
        }
    }

    // ----- 슬롯 헬퍼 -----
    class SlotWidget
    {
        public Button button;
        public Image icon;
        public TMP_Text countTMP;   // TMP 우선
        public Text countUGUI;      // 레거시 Text 대비

        public SlotWidget(GameObject root)
        {
            // 루트에 Button 필수
            button = root.GetComponent<Button>();
            if (!button) button = root.AddComponent<Button>();

            // Icon(Image)
            var iconTf = root.transform.Find("Icon");
            if (iconTf) icon = iconTf.GetComponent<Image>();
            if (!icon) Debug.LogError("SlotPrefab: 'Icon' (Image) 가 없습니다.");

            // Count(TMP_Text or Text)
            var countTf = root.transform.Find("Count");
            if (countTf)
            {
                countTMP = countTf.GetComponent<TMP_Text>();
                if (!countTMP) countUGUI = countTf.GetComponent<Text>();
            }
            if (!countTMP && !countUGUI)
                Debug.LogError("SlotPrefab: 'Count' 에 TMP_Text 또는 Text 컴포넌트가 필요합니다.");
        }

        public void SetCount(string t)
        {
            if (countTMP) countTMP.text = t;
            else if (countUGUI) countUGUI.text = t;
        }
    }


}

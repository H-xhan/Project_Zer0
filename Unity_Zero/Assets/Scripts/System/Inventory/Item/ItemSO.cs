using UnityEngine;

public enum ItemType
{
    Consumable,     // 포션 등 사용하면 사라짐
    Equipment,      // 무기/방어구 (착용 시스템은 v2에서)
    Material,       // 재료
    Quest           // 퀘스트 아이템
}

[CreateAssetMenu(menuName = "Game/Item", fileName = "NewItem")]
public class ItemSO : ScriptableObject
{
    [Header("Identity")]
    public string itemId;                     // 고유 ID(문자) – 저장/네트워크용
    public string displayName;                // UI 표시 이름

    [Header("Visual")]
    public Sprite icon;                       // 인벤토리 아이콘

    [Header("Stacking")]
    public int maxStack = 1;                  // 1이면 비스택(장비 등), N이면 스택 가능

    [Header("Type")]
    public ItemType type = ItemType.Material; // 아이템 분류

    [Header("Gameplay Params")]
    public float power = 0f;                  // 예: 회복량/공격력 등 범용 파라미터
    public string meta;                       // 추가 메타(설명, JSON 등)
}

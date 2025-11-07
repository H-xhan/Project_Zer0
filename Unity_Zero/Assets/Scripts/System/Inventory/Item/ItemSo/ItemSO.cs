using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item")]
public class ItemSO : ScriptableObject
{
    [Tooltip("아이템 고유 ID (데이터/저장용)")]
    public string itemId;

    [Tooltip("UI에 표시될 이름")]
    public string displayName;

    [Tooltip("인벤토리 및 UI에 표시될 아이콘")]
    public Sprite icon;

    [Tooltip("여러 개를 한 슬롯에 쌓을 수 있는지 여부")]
    public bool stackable = true;

    [Tooltip("스택 가능할 때 한 슬롯 최대 개수")]
    public int maxStack = 99;
}

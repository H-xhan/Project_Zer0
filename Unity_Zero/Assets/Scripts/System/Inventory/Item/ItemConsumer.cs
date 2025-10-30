using UnityEngine;

public class ItemConsumer : MonoBehaviour
{
    // 플레이어 컴포넌트 참조(체력/스태미너)
    private PlayerHealth _health;
    private PlayerStamina _stamina; // 네가 쓰는 스태미너 스크립트 이름에 맞춰 연결

    void Awake()
    {
        _health = GetComponent<PlayerHealth>();  // 체력 스크립트
        _stamina = GetComponent<PlayerStamina>(); // 스태미너 스크립트(이미 만들었음)
    }

    public void Consume(ItemSO item)
    {
        if (item == null) return;

        Debug.Log($"Consumed {item.displayName}");

        GetComponent<PlayerHealth>()?.Heal(item.power);

        // 간단 규칙 예시: meta에 키워드로 분기하거나, type/파워로 처리
        // 여기선 displayName/meta에 따라 나눠보자
        if (item.type == ItemType.Consumable)
        {
            // 예: "HP" 포함 → 체력 회복, "SP" 포함 → 스태미너 회복
            if (_health && (item.meta.Contains("HP") || item.displayName.Contains("체력")))
            {
                _health.Heal(item.power);        // power만큼 회복
            }
            else if (_stamina && (item.meta.Contains("SP") || item.displayName.Contains("스태미너")))
            {
                _stamina.Recover(item.power);    // 네 스태미너 스크립트에 맞춰 함수명 조정
            }
            else
            {
                // 기본은 체력 회복으로 처리
                if (_health) _health.Heal(item.power);
            }
        }
    }
}

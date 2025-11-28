using UnityEngine;

public class EnemyStealthSensorModule : MonoBehaviour
{
    [Tooltip("인식할 플레이어 스텔스 모듈")]
    [SerializeField] private PlayerStealthModule playerStealth;

    // AI가 플레이어를 인식해도 되는지 여부
    public bool CanDetectPlayer()
    {
        // 스텔스 중이면 항상 false
        if (playerStealth != null && playerStealth.IsStealthed)
            return false;

        return true;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class Table : MonoBehaviour
{
    public Text interactionText;
    public CasinoManager manager;
    void Start()
    {
        
    }

    void Update()
    {
        int playerLayer = LayerMask.GetMask("Player");

        // 플레이어가 테이블 주변 반경 안에 있는지 확인 (구체 범위로 충돌체 탐지)
        Collider[] colliders = Physics.OverlapSphere(transform.position, 2f, playerLayer);

        // 탐지된 플레이어가 하나라도 있으면 true
        bool hasPlayer = colliders.Length > 0;

        if (hasPlayer)
        {
            if(Input.GetKeyDown(KeyCode.F))
            {

            }
        }
    }
}

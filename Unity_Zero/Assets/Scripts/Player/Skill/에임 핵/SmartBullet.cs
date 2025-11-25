using UnityEngine;

public class SmartBullet : MonoBehaviour
{
    [SerializeField] private float speed = 40f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float turnSpeed = 10f; // 유도 회전 속도

    private Vector3 _direction;
    private Transform _target;
    private float _damage = 10f;

    public void Initialize(
        Vector3 origin,
        Vector3 forward,
        SmartAimController smartAim,
        float damageMultiplier)
    {
        transform.position = origin;
        transform.forward = forward;
        _direction = forward;

        _damage *= damageMultiplier;

        if (smartAim != null)
            _target = smartAim.GetBestTarget(origin, forward);

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (_target != null)
        {
            Vector3 targetDir = (_target.position - transform.position).normalized;
            _direction = Vector3.Slerp(_direction, targetDir, turnSpeed * Time.deltaTime);
            transform.forward = _direction;
        }

        transform.position += _direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. 적(Enemy)인지 확인 (태그 혹은 레이어 체크)
        // 레이어 마스크를 쓰거나 태그를 쓰거나 프로젝트 규칙에 맞게
        if (other.CompareTag("Enemy") || (1 << other.gameObject.layer & LayerMask.GetMask("Enemy")) != 0)
        {
            // 2. 데미지 주기 (인터페이스나 컴포넌트 호출)
            // 예시: IDamageable 타겟 찾기
            var targetHealth = other.GetComponent<IDamageable>(); // 혹은 EnemyHealth
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(_damage);
                Debug.Log($"[SmartBullet] 명중! 데미지: {_damage}");
            }

            // 3. 이펙트 생성 (폭발 펑!)
            // Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            // 4. 총알 삭제
            Destroy(gameObject);
        }
        // 벽이나 바닥에 닿았을 때도 삭제하고 싶다면 추가
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}

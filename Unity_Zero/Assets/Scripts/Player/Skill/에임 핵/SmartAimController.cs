using UnityEngine;

public class SmartAimController : MonoBehaviour
{
    [Tooltip("유도 대상 레이어(적)")]
    [SerializeField] private LayerMask targetLayer;

    [Tooltip("유도 가능한 최대 거리")]
    [SerializeField] private float maxLockDistance = 50f;

    [Tooltip("카메라 또는 총알 기준 방향")]
    [SerializeField] private Transform fireOrigin;

    private bool _isActive;
    private float _remainTime;
    private float _maxLockAngle;
    private float _homingStrength;

    public bool IsActive => _isActive;
    public float HomingStrength => _homingStrength;
    public float MaxLockAngle => _maxLockAngle;

    public void Activate(float duration, float maxLockAngle, float homingStrength)
    {
        _isActive = true;
        _remainTime = duration;
        _maxLockAngle = maxLockAngle;
        _homingStrength = homingStrength;
    }

    private void Update()
    {
        if (!_isActive)
            return;

        _remainTime -= Time.deltaTime;
        if (_remainTime <= 0f)
            _isActive = false;
    }

    public bool TryGetHomingDirection(Vector3 origin, Vector3 forward, out Vector3 homingDir)
    {
        homingDir = forward;

        if (!_isActive)
            return false;

        Ray ray = new Ray(origin, forward);

        if (Physics.SphereCast(ray, 1f, out RaycastHit hit, maxLockDistance, targetLayer))
        {
            Vector3 toTarget = (hit.collider.bounds.center - origin).normalized;
            float angle = Vector3.Angle(forward, toTarget);
            if (angle <= _maxLockAngle)
            {
                homingDir = toTarget;
                return true;
            }
        }

        return false;
    }

    public Transform GetBestTarget(Vector3 origin, Vector3 forward)
    {
        if (!_isActive) return null;

        Ray ray = new Ray(origin, forward);

        if (Physics.SphereCast(ray, 1f, out RaycastHit hit, maxLockDistance, targetLayer))
        {
            Vector3 toTarget = (hit.collider.bounds.center - origin).normalized;
            float angle = Vector3.Angle(forward, toTarget);
            if (angle <= _maxLockAngle)
                return hit.transform;
        }

        return null;
    }
}

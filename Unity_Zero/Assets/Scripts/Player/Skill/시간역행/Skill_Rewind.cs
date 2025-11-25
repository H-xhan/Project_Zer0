using UnityEngine;
using System.Collections.Generic;

public class Skill_Rewind : TBSSkill
{
    private readonly List<Vector3> _positionHistory = new List<Vector3>();

    private float _recordInterval = 0.05f;
    private float _recordTimer = 0f;
    private float _rewindSeconds = 10f;

    private bool _isRewinding = false;
    private int _currentIndex = -1;

    // 이동 제어
    private CharacterController _controller;
    private float _baseRewindSpeed = 50f;
    private float _easingStrength = 0.4f;

    // 비주얼 연출용
    private Renderer[] _renderers;
    private readonly Dictionary<Material, Color> _originalColors = new Dictionary<Material, Color>();
    private Color _rewindColor = new Color(0.5f, 1f, 1f, 0.7f); // 홀로그램 느낌

    // 잔상 스냅샷 컨트롤러 (TrailRenderer 대체)
    private GhostTrailController _ghostTrailController;

    public Skill_Rewind(PlayerController player,
                        TimeSystemController timeSystem,float cooldown,float timeCost, float damageMultiplier)
        : base(player, timeSystem, cooldown, timeCost, damageMultiplier)
    {
        if (player != null)
        {
            _controller = player.GetComponent<CharacterController>();
            _renderers = player.GetComponentsInChildren<Renderer>();

            // 잔상 스냅샷 컨트롤러 자동 검색 (플레이어나 자식 오브젝트에 붙어 있어야 함)
            _ghostTrailController = player.GetComponentInChildren<GhostTrailController>();
        }
    }

    public void Tick()
    {
        if (_player == null)
            return;

        if (_isRewinding)
        {
            UpdateRewind();
            return;
        }

        _recordTimer += Time.deltaTime;
        if (_recordTimer >= _recordInterval)
        {
            _recordTimer = 0f;
            RecordPosition();
        }
    }

    private void RecordPosition()
    {
        _positionHistory.Add(_player.transform.position);

        int maxCount = Mathf.CeilToInt(_rewindSeconds / _recordInterval) + 5;

        if (_positionHistory.Count > maxCount)
            _positionHistory.RemoveAt(0);
    }

    protected override void OnUse()
    {
        if (_isRewinding || _positionHistory.Count == 0)
            return;

        _isRewinding = true;
        _player.SetRewindState(true);
        _currentIndex = _positionHistory.Count - 1;

        if (_controller != null)
            _controller.enabled = false;

        // 잔상 스냅샷 시작
        if (_ghostTrailController != null)
            _ghostTrailController.SetActive(true);

        SetRewindVisuals(true);

        Debug.Log("[Skill_Rewind] 시간 역행 시작");
    }

    private void UpdateRewind()
    {
        if (_currentIndex < 0)
        {
            FinishRewind();
            return;
        }

        Vector3 target = _positionHistory[_currentIndex];
        Vector3 current = _player.transform.position;

        float distance = Vector3.Distance(current, target);

        float normalizedIndex = 1f - ((float)_currentIndex / (_positionHistory.Count - 1));
        float easeFactor = Mathf.Lerp(1f, _easingStrength, normalizedIndex);

        float speed = _baseRewindSpeed * easeFactor;
        float step = speed * Time.deltaTime;

        if (distance < 0.05f)
        {
            _currentIndex--;
            return;
        }

        // 플레이어 이동
        _player.transform.position = Vector3.MoveTowards(current, target, step);

        // 해당 "과거 프레임 위치"에 잔상 생성
        if (_ghostTrailController != null)
        {
            _ghostTrailController.SpawnSnapshotAt(
                target,                // 과거 위치
                _player.transform.rotation // 현재 회전 or 저장한 회전
            );
        }
    }

    private void FinishRewind()
    {
        _isRewinding = false;
        _player.SetRewindState(false);
        _positionHistory.Clear();

        if (_controller != null)
        {
            Physics.SyncTransforms();
            _controller.enabled = true;
        }

        // 잔상 스냅샷 종료
        if (_ghostTrailController != null)
            _ghostTrailController.SetActive(false);

        SetRewindVisuals(false);

        Debug.Log("[Skill_Rewind] 시간 역행 완료");
    }

    private void SetRewindVisuals(bool active)
    {
        if (_renderers != null)
        {
            foreach (var rend in _renderers)
            {
                if (rend == null) continue;

                var mats = rend.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var mat = mats[i];
                    if (mat == null) continue;

                    if (active)
                    {
                        if (!_originalColors.ContainsKey(mat))
                            _originalColors[mat] = mat.color;

                        mat.color = _rewindColor;
                    }
                    else
                    {
                        if (_originalColors.TryGetValue(mat, out var orig))
                            mat.color = orig;
                    }
                }
            }

            if (!active)
                _originalColors.Clear();
        }
    }
}

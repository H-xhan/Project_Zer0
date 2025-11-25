using UnityEngine;
using System.Collections.Generic;

public class Skill_Rewind : TBSSkill
{
    private readonly List<Vector3> _positionHistory = new List<Vector3>();

    private float _recordInterval = 0.05f;
    private float _recordTimer = 0f;
    private float _rewindSeconds = 3f;

    private bool _isRewinding = false;
    private int _currentIndex = -1;

    // Trail
    private TrailRenderer _trail;
    private Transform _trailPivot;

    // 이동 제어
    private CharacterController _controller;
    private float _baseRewindSpeed = 12f;
    private float _easingStrength = 0.4f;

    // 비주얼 연출용
    private Renderer[] _renderers;
    private readonly Dictionary<Material, Color> _originalColors = new Dictionary<Material, Color>();
    private Color _rewindColor = new Color(0.5f, 1f, 1f, 0.7f); // 홀로그램 느낌

    public Skill_Rewind(PlayerController player,
                        TimeSystemController timeSystem,
                        float cooldown,
                        float timeCost)
        : base(player, timeSystem, cooldown, timeCost)
    {
        if (player != null)
        {
            _controller = player.GetComponent<CharacterController>();
            _renderers = player.GetComponentsInChildren<Renderer>();

            // 플레이어나 자식에 붙어 있는 TrailRenderer 자동 탐색
            _trailPivot = _player.transform.Find("TrailPivot");         // TrailPivot 찾기
            _trail = player.GetComponentInChildren<TrailRenderer>();
            if (_trail != null)
            {
                _trail.emitting = false;   // 기본은 꺼두기
                _trail.Clear();
            }
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

        if (_trail != null)
            _trail.emitting = true;

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

        _player.transform.position = Vector3.MoveTowards(current, target, step);
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

        if (_trail != null)
            _trail.emitting = false;

        SetRewindVisuals(false);

        Debug.Log("[Skill_Rewind] 시간 역행 완료");
    }

    private void SetRewindVisuals(bool active)
    {
        // 색 변경
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

        // [핵심 추가] Trail on/off
        if (_trail != null)
        {
            if (active)
            {
                _trail.Clear();      // 이전 잔상 지우고
                _trail.emitting = true;
            }
            else
            {
                _trail.emitting = false;
                _trail.Clear();      // 끝날 때도 한번 정리
            }
        }
    }
}

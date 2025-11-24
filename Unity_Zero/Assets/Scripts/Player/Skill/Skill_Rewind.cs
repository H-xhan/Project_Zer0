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

    private CharacterController _controller;

    // --------------------------
    //     튜닝 가능한 값들
    // --------------------------

    private float _baseRewindSpeed = 12f;   // 기본 되감기 속도
    private float _easingStrength = 0.4f;   // 감속 비율 (0.1 ~ 0.7 추천)
                                            // 값이 클수록 끝부분에서 감속이 강해짐

    public Skill_Rewind(PlayerController player,
                        TimeSystemController timeSystem,
                        float cooldown,
                        float timeCost)
        : base(player, timeSystem, cooldown, timeCost)
    {
        if (player != null)
            _controller = player.GetComponent<CharacterController>();
    }

    public void Tick()
    {
        if (_player == null) return;

        if (_isRewinding)
        {
            UpdateRewind();
            return;
        }

        // -------------------------
        //      평상시 위치 기록
        // -------------------------
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
        {
            _positionHistory.RemoveAt(0);
        }
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

        Debug.Log("[Skill_Rewind] 시간 역행 시작!");
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

        // -------------------------
        //     Easing 기반 감속
        // -------------------------
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

        Debug.Log("[Skill_Rewind] 시간 역행 완료!");
    }
}

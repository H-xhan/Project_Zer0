using System.Collections.Generic;
using UnityEngine;

public class Skill_Rewind : TBSSkill
{
    private struct Snapshot
    {
        public Vector3 position;
        public Quaternion rotation;
        public float planarSpeed;
        public float verticalVelocity;
        public bool grounded;
        public bool isSprinting;
    }

    private readonly List<Snapshot> _buffer = new List<Snapshot>();

    // 기록 관련
    private readonly float _recordInterval = 0.05f;   // 약 20FPS
    private float _recordTimer;
    private int _maxSnapshotCount;

    // 상태
    private bool _isRewinding;
    private int _rewindIndex;

    // 되감기 속도 및 잔상 구간
    private int _stepsPerFrame = 3;      // 한 프레임에 몇 개의 스냅샷을 소비할지 (높을수록 더 빠름)
    private int _ghostLastFrames = 8;    // 마지막 몇 개 스냅샷 구간에서만 잔상 생성
    private int _ghostStartIndex;        // 잔상을 찍기 시작할 인덱스

    // 캐시
    private readonly Transform _tf;
    private readonly CharacterController _cc;
    private readonly MovementModule _movement;
    private readonly PlayerAnimModule _animModule;
    private readonly GhostTrailController _ghostTrail;

    public Skill_Rewind(
        PlayerController player,
        TimeSystemController timeSystem,
        float cooldown,
        float timeCost,
        float damageMultiplier = 1f
    ) : base(player, timeSystem, cooldown, timeCost, damageMultiplier)
    {
        if (player != null)
        {
            _tf = player.transform;
            _cc = player.GetComponent<CharacterController>();
            _movement = player.movement;
            _animModule = player.animModule;

            // 플레이어 자식에서 잔상 컨트롤러 찾기
            _ghostTrail = player.GetComponentInChildren<GhostTrailController>();
        }

        // 최근 N초만 기록 (예: 3초)
        float recordWindow = 3.0f;
        _maxSnapshotCount = Mathf.Max(10, Mathf.RoundToInt(recordWindow / _recordInterval));
    }

    // TBSDeviceController.Update() -> ActiveSkillModule.Tick() 에서 매 프레임 호출됨
    public void Tick()
    {
        if (_player == null || _tf == null)
            return;

        if (_isRewinding)
        {
            TickRewind();
        }
        else
        {
            TickRecord();
        }
    }

    // 1) 평소에 스냅샷 기록
    private void TickRecord()
    {
        if (_movement == null)
            return;

        _recordTimer += Time.deltaTime;
        if (_recordTimer < _recordInterval)
            return;

        _recordTimer -= _recordInterval;

        Snapshot snap = new Snapshot
        {
            position = _tf.position,
            rotation = _tf.rotation,
            planarSpeed = _movement.GetPlanarSpeed(),
            verticalVelocity = _movement.GetVerticalVelocity(),
            grounded = _movement.IsGrounded(),
            isSprinting = _movement.IsSprinting()
        };

        if (_buffer.Count >= _maxSnapshotCount)
            _buffer.RemoveAt(0);

        _buffer.Add(snap);
    }

    // 2) 되감기 실행 중
    private void TickRewind()
    {
        if (_buffer.Count == 0 || _rewindIndex < 0)
        {
            StopRewind();
            return;
        }

        // 한 프레임에 여러 스냅샷을 소비해서 빠르게 되감기
        for (int i = 0; i < _stepsPerFrame; i++)
        {
            if (_rewindIndex < 0)
            {
                StopRewind();
                return;
            }

            Snapshot snap = _buffer[_rewindIndex];

            bool ccWasEnabled = false;
            if (_cc != null)
            {
                ccWasEnabled = _cc.enabled;
                _cc.enabled = false;
            }

            _tf.SetPositionAndRotation(snap.position, snap.rotation);

            if (_cc != null)
                _cc.enabled = ccWasEnabled;

            // 애니메이션 상태 갱신 (그때와 비슷한 상태로만 맞춰줌)
            if (_animModule != null)
            {
                _animModule.Tick(
                    Time.deltaTime,
                    snap.planarSpeed,
                    snap.grounded,
                    snap.verticalVelocity,
                    false,              // 되감기 중에는 Jump 트리거는 사용 안 함
                    snap.isSprinting,
                    0f                  // 되감기 중엔 전후 입력 없음 → 0으로 고정
                );

                _animModule.UpdateTurn(0f, snap.planarSpeed, snap.grounded);
            }

            // 잔상: 마지막 구간에서만 생성
            if (_ghostTrail != null && _rewindIndex <= _ghostStartIndex)
            {
                _ghostTrail.SpawnSnapshotAt(snap.position, snap.rotation);
            }

            _rewindIndex--;
        }
    }

    // Q 스킬 실제 발동 시점
    protected override void OnUse()
    {
        if (_isRewinding)
            return;

        if (_buffer.Count == 0)
            return;

        StartRewind();
    }

    private void StartRewind()
    {
        _isRewinding = true;

        if (_player != null)
            _player.SetRewindState(true);

        _rewindIndex = _buffer.Count - 1;

        // 잔상을 보여줄 구간의 시작 인덱스 (마지막 _ghostLastFrames 만큼)
        _ghostStartIndex = Mathf.Max(0, _buffer.Count - _ghostLastFrames);
    }

    private void StopRewind()
    {
        _isRewinding = false;

        if (_player != null)
            _player.SetRewindState(false);
    }
}

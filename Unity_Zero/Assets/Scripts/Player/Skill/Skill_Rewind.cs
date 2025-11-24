using System.Collections.Generic;
using UnityEngine;

public class Skill_Rewind : TBSSkill
{
    private struct RewindFrame
    {
        public Vector3 position;
        public float time;

        public RewindFrame(Vector3 pos, float t)
        {
            position = pos;
            time = t;
        }
    }

    private readonly Queue<RewindFrame> _history = new Queue<RewindFrame>();

    private float _recordInterval = 0.1f;   // 기록 간격
    private float _recordTimer = 0f;
    private float _rewindSeconds = 3f;      // 몇 초 전까지 되돌릴지

    public Skill_Rewind(PlayerController player,
                        TimeSystemController timeSystem,
                        float cooldown,
                        float timeCost)
        : base(player, timeSystem, cooldown, timeCost)
    {
    }

    // ActiveSkillModule.Tick()에서 매 프레임 호출됨
    public void Tick()
    {
        if (_player == null)
            return;

        _recordTimer += Time.deltaTime;
        if (_recordTimer >= _recordInterval)
        {
            _recordTimer = 0f;

            _history.Enqueue(new RewindFrame(_player.transform.position, Time.time));

            // 너무 오래된 데이터는 제거
            float cutoff = Time.time - (_rewindSeconds + 1f);
            while (_history.Count > 0 && _history.Peek().time < cutoff)
                _history.Dequeue();
        }
    }

    protected override void OnUse()
    {
        if (_player == null)
            return;

        if (_history.Count == 0)
        {
            Debug.Log("[Skill_Rewind] 되돌릴 기록이 없습니다.");
            return;
        }

        float targetTime = Time.time - _rewindSeconds;
        Vector3 targetPos = _player.transform.position;
        bool found = false;

        // 큐에서 타임라인에 가장 가까운 위치 찾기
        foreach (var frame in _history)
        {
            if (frame.time <= targetTime)
            {
                targetPos = frame.position;
                found = true;
            }
            else
            {
                break;
            }
        }

        if (!found)
        {
            // 3초 전이 없으면 가장 오래된 기록으로 이동
            targetPos = _history.Peek().position;
        }

        var cc = _player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            _player.transform.position = targetPos;
            Physics.SyncTransforms();
            cc.enabled = true;
        }
        else
        {
            _player.transform.position = targetPos;
        }

        Debug.Log("[Skill_Rewind] 시간 역행 완료");
    }
}

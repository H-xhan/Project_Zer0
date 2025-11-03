using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 퀘스트 완료/보상수령/쿨다운 시간 등을 관리.
/// ▶ 현재 버전은 "Play 시작 시 자동 초기화" (테스트/프로토타입용)
/// ▶ 최종 버전에서는 SaveSystem으로 교체 가능.
/// </summary>
public class PlayerQuestLog : MonoBehaviour
{
    const string PREF_COMPLETED = "Q_COMPLETED_";    // + questId
    const string PREF_LASTCLAIM = "Q_LASTCLAIM_TS_"; // + questId

    private HashSet<string> _completed = new();

    [Header("테스트 모드 옵션")]
    [Tooltip("Play 시작 시 퀘스트 기록(PlayerPrefs)을 모두 삭제")]
    public bool resetOnStart = true;

    [Tooltip("쿨다운 정보는 유지할지 여부 (테스트용)")]
    public bool keepCooldown = false;

    void Awake()
    {
        if (resetOnStart)
        {
            Debug.Log("[PlayerQuestLog] ▶ 테스트용 초기화: PlayerPrefs 퀘스트 기록 삭제");

            // PlayerPrefs 전체 삭제 대신 접두사 필터로 부분 삭제
            foreach (var key in PlayerPrefsKeys())
            {
                if (key.StartsWith(PREF_COMPLETED))
                    PlayerPrefs.DeleteKey(key);
                if (!keepCooldown && key.StartsWith(PREF_LASTCLAIM))
                    PlayerPrefs.DeleteKey(key);
            }

            PlayerPrefs.Save();
            _completed.Clear();
        }
    }

    // 🔹 PlayerPrefs 전체 키 가져오기 (Unity에 직접 API가 없어서 구현)
    IEnumerable<string> PlayerPrefsKeys()
    {
#if UNITY_EDITOR
        var fi = typeof(PlayerPrefs).GetField("s_PlayerPrefs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (fi?.GetValue(null) is Dictionary<string, object> dict)
            return dict.Keys;
#endif
        // Editor 외에는 전체 삭제가 불가능하므로, 테스트 용도로만 동작
        return Array.Empty<string>();
    }

    string KeyCompleted(string questId) => PREF_COMPLETED + questId;
    string KeyLastClaim(string questId) => PREF_LASTCLAIM + questId;

    // ──────────────────────────────────────────────────────────────
    public bool HasCompleted(string questId)
    {
        if (_completed.Contains(questId)) return true;
        if (PlayerPrefs.GetInt(KeyCompleted(questId), 0) == 1)
        {
            _completed.Add(questId);
            return true;
        }
        return false;
    }

    public void MarkCompleted(string questId)
    {
        _completed.Add(questId);
        PlayerPrefs.SetInt(KeyCompleted(questId), 1);
    }

    public void MarkRewardClaimed(string questId)
    {
        long unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PlayerPrefs.SetString(KeyLastClaim(questId), unix.ToString());
        PlayerPrefs.Save();
    }

    public bool IsOnCooldown(string questId, float cooldownSec)
    {
        if (cooldownSec <= 0f) return false;
        string s = PlayerPrefs.GetString(KeyLastClaim(questId), "");
        if (string.IsNullOrEmpty(s)) return false;
        if (!long.TryParse(s, out long last)) return false;
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return (now - last) < cooldownSec;
    }
}

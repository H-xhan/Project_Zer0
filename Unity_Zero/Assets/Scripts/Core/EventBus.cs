using System;
using System.Collections.Generic;

/// 전역 이벤트 전달용 이벤트 버스
public static class EventBus
{
    // 퍼즐 클리어 알림 (퍼즐 ID 전달)
    public static event Action<string> OnPuzzleCleared;

    // 모든 필수 퍼즐 클리어 알림
    public static event Action OnAllRequiredPuzzlesCleared;

    public static void RaisePuzzleCleared(string puzzleId)
    {
        OnPuzzleCleared?.Invoke(puzzleId);
        Publish(new PuzzleClearedEvent { PuzzleId = puzzleId });
    }

    public static void RaiseAllRequiredPuzzlesCleared()
    {
        OnAllRequiredPuzzlesCleared?.Invoke();
        Publish(new AllRequiredPuzzlesClearedEvent());
    }

    public interface IEventData { }

    public struct PuzzleClearedEvent : IEventData
    {
        public string PuzzleId;
    }

    public struct AllRequiredPuzzlesClearedEvent : IEventData { }

    private static readonly Dictionary<Type, List<Delegate>> _subscribers
        = new Dictionary<Type, List<Delegate>>();

    public static void Subscribe<T>(Action<T> handler) where T : IEventData
    {
        var type = typeof(T);

        if (!_subscribers.TryGetValue(type, out var list))
        {
            list = new List<Delegate>();
            _subscribers[type] = list;
        }

        if (!list.Contains(handler))
        {
            list.Add(handler);
        }
    }

    public static void Unsubscribe<T>(Action<T> handler) where T : IEventData
    {
        var type = typeof(T);

        if (_subscribers.TryGetValue(type, out var list))
        {
            list.Remove(handler);
            if (list.Count == 0)
            {
                _subscribers.Remove(type);
            }
        }
    }

    public static void Publish<T>(T eventData) where T : IEventData
    {
        var type = typeof(T);

        if (!_subscribers.TryGetValue(type, out var list))
            return;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] is Action<T> handler)
            {
                try
                {
                    handler(eventData);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"[EventBus] Error in {type.Name}: {e}");
                }
            }
        }
    }
}

using System;

/// 전역 이벤트 전달용 간단한 이벤트 버스
public static class EventBus
{
    // 퍼즐 클리어 알림 (퍼즐 ID 전달)
    public static event Action<string> OnPuzzleCleared;

    // 모든 필수 퍼즐 클리어 알림
    public static event Action OnAllRequiredPuzzlesCleared;

    // 퍼즐 클리어 발생 시 호출
    public static void RaisePuzzleCleared(string puzzleId)
    {
        OnPuzzleCleared?.Invoke(puzzleId);
    }

    // 모든 필수 퍼즐 클리어 발생 시 호출
    public static void RaiseAllRequiredPuzzlesCleared()
    {
        OnAllRequiredPuzzlesCleared?.Invoke();
    }
}

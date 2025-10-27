using System;

// ※ GameManager가 네임스페이스가 없다면 이 파일도 네임스페이스를 두지 마세요.
// 둘 다 네임스페이스가 있으면 "같은 네임스페이스"로 맞추면 됩니다.

public static class EventBus
{
    // 퍼즐 클리어시 퍼즐ID를 브로드캐스트
    public static event Action<string> OnPuzzleCleared;

    // 모든 필수 퍼즐 클리어시 알림
    public static event Action OnAllRequiredPuzzlesCleared;

    // 퍼즐 1개 클리어 알림 메서드
    public static void RaisePuzzleCleared(string puzzleId)
    {
        OnPuzzleCleared?.Invoke(puzzleId);
    }

    // 모든 필수 퍼즐 클리어 알림 메서드
    public static void RaiseAllRequiredPuzzlesCleared()
    {
        OnAllRequiredPuzzlesCleared?.Invoke();
    }
}

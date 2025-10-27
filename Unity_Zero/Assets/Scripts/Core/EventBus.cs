using System;

// �� GameManager�� ���ӽ����̽��� ���ٸ� �� ���ϵ� ���ӽ����̽��� ���� ������.
// �� �� ���ӽ����̽��� ������ "���� ���ӽ����̽�"�� ���߸� �˴ϴ�.

public static class EventBus
{
    // ���� Ŭ����� ����ID�� ��ε�ĳ��Ʈ
    public static event Action<string> OnPuzzleCleared;

    // ��� �ʼ� ���� Ŭ����� �˸�
    public static event Action OnAllRequiredPuzzlesCleared;

    // ���� 1�� Ŭ���� �˸� �޼���
    public static void RaisePuzzleCleared(string puzzleId)
    {
        OnPuzzleCleared?.Invoke(puzzleId);
    }

    // ��� �ʼ� ���� Ŭ���� �˸� �޼���
    public static void RaiseAllRequiredPuzzlesCleared()
    {
        OnAllRequiredPuzzlesCleared?.Invoke();
    }
}

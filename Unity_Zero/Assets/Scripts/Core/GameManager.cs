using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // �̱���: ��𼭵� GameManager.Instance �� ����
    public static GameManager Instance { get; private set; }

    // �������� ��ü ���� �ð�(��)
    [SerializeField] private float stageTimeLimit = 300f;

    // ������ �б� ���� Ÿ�̸�(���� �ð� UI���� ǥ��)
    public float RemainingTime { get; private set; }

    // ���� ���� ���� ����(��: ��� �ʼ� ���� �Ϸ� �� true)
    public bool AllRequiredPuzzlesCleared { get; private set; }

    private void Awake()
    {
        // �̱��� ����
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        RemainingTime = stageTimeLimit;
    }

    private void OnEnable()
    {
        // ���� �Ϸ� �̺�Ʈ ����
        EventBus.OnPuzzleCleared += HandlePuzzleCleared;
    }

    private void OnDisable()
    {
        EventBus.OnPuzzleCleared -= HandlePuzzleCleared;
    }

    private void Update()
    {
        // �ð� ����
        RemainingTime -= Time.deltaTime;
        if (RemainingTime <= 0f)
        {
            RemainingTime = 0f;
            // �ð� ���� �� ó��(���� ����, ����� ��)
            Debug.Log("[GameManager] Time Up! Stage Failed.");
            // �ʿ� �� �� ���ε� or ��� UI ȣ��
        }
    }

    private void HandlePuzzleCleared(string puzzleId)
    {
        Debug.Log($"[GameManager] Puzzle Cleared: {puzzleId}");
        // ���� ����: �ʼ� ������ 1�� ���̶�� ����(Ȯ�� ����)
        AllRequiredPuzzlesCleared = true;
        // �������� "�� ����" ���� �̺�Ʈ ��� ������ ���⼭ ����
        EventBus.RaiseAllRequiredPuzzlesCleared();
    }
}

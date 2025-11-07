using UnityEngine;
using UnityEngine.UI;

public class CasinoManager : MonoBehaviour
{
    [Header("GamePanelUI")]
    public GameObject gamePanel;
    public Text dealerDialogue;
    public Button batting;
    public Button allIn;
    public Button TimerStart;
    public Button TimerStop;

    [Header("isPlayGame")]
    public Button Yes;
    public Button No;

    void Start()
    {
        gamePanel.SetActive(false);
        dealerDialogue.enabled = false;

        // IsPlayGame 버튼
        Yes.onClick.AddListener(OnYesClicked); // 'Yes' 버튼 클릭 시 OnYesClicked 함수 실행
        No.onClick.AddListener(OnNoClicked);   // 'No' 버튼 클릭 시 OnNoClicked 함수 실행

        // 게임 패널 버튼
        batting.onClick.AddListener(OnBattingClicked);
        allIn.onClick.AddListener(OnAllInClicked);

        // 타이머 버튼 (람다식을 사용한 간단한 예시)
        // 실행할 코드가 짧다면 람다식 '() => { ... }'을 사용하는 것이 편리합니다.
        TimerStart.onClick.AddListener(() => {
            Debug.Log("타이머 시작 버튼 클릭됨!");
            // 여기에 타이머 시작 로직 구현
        });

        TimerStop.onClick.AddListener(() => {
            Debug.Log("타이머 중지 버튼 클릭됨!");
            // 여기에 타이머 중지 로직 구현
        });
    }

    void Update()
    {
    }

    // "게임 한판 하시겠습니까?" 메시지를 띄우는 함수
    public void IsPlayGame()
    {
        dealerDialogue.enabled = true;
        dealerDialogue.text = "게임 한판 하시겠습니까?";

        // Yes/No 버튼을 활성화 (필요하다면)
        Yes.gameObject.SetActive(true);
        No.gameObject.SetActive(true);
    }

    // 'Yes' 버튼을 클릭했을 때 실행될 함수
    public void OnYesClicked()
    {
        Debug.Log("Yes 버튼 클릭!");
        StartGame(); // StartGame 함수 호출
    }

    // 'No' 버튼을 클릭했을 때 실행될 함수
    public void OnNoClicked()
    {
        Debug.Log("No 버튼 클릭!");

        // 대화창과 Yes/No 버튼을 다시 비활성화
        dealerDialogue.enabled = false;
        Yes.gameObject.SetActive(false);
        No.gameObject.SetActive(false);
    }

    // 'Batting' 버튼을 클릭했을 때 실행될 함수
    public void OnBattingClicked()
    {
        Debug.Log("배팅 버튼 클릭!");
        // 배팅 관련 로직...
    }

    // 'AllIn' 버튼을 클릭했을 때 실행될 함수
    public void OnAllInClicked()
    {
        Debug.Log("올인 버튼 클릭!");
        // 올인 관련 로직...
    }


    // 실제 게임을 시작하는 함수 (OnYesClicked 에서 호출됨)
    public void StartGame()
    {
        // "게임 한판?" 대화창 및 Yes/No 버튼 숨기기
        dealerDialogue.enabled = false;
        Yes.gameObject.SetActive(false);
        No.gameObject.SetActive(false);

        // 게임 패널(배팅, 올인 버튼 등) 활성화
        if (gamePanel != null)
        {
            gamePanel.SetActive(true);
            Debug.Log("게임을 시작합니다.");
        }
    }
}
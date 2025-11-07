using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EfficiencyUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("효율 정보를 제공하는 플레이어 컨트롤러")]
    public PlayerController player;

    [Tooltip("0~1 범위를 표시하는 슬라이더")]
    public Slider slider;

    [Tooltip("퍼센트를 표시하는 텍스트 (선택 사항)")]
    public TMP_Text percentText;

    [Header("Visual Settings")]
    [Tooltip("게이지 값 보간 속도")]
    public float lerpSpeed = 12f;

    [Tooltip("효율이 낮을 때 색상")]
    public Color lowColor = new Color(0.85f, 0.35f, 0.25f);

    [Tooltip("효율이 높을 때 색상")]
    public Color highColor = new Color(0.25f, 0.85f, 0.55f);

    [Tooltip("채워지는 영역의 이미지 (선택 사항)")]
    public Image fillImage;

    // 화면에 표현되는 현재 값 (실제 값과 보간)
    private float _visual = 1f;

    private void Awake()
    {
        // 플레이어 자동 참조 보정
        if (!player)
            player = FindFirstObjectByType<PlayerController>();

        // 슬라이더 초기 설정
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = _visual;
        }
    }

    private void Update()
    {
        // 필수 참조가 없으면 동작하지 않음
        if (player == null || slider == null)
            return;

        // 효율 모듈에서 0~1 정규화 값 가져오기
        float target = player.efficiency.Normalized();

        // 표시 값 보간
        _visual = Mathf.Lerp(_visual, target, Time.deltaTime * lerpSpeed);
        slider.value = _visual;

        // 텍스트 표시
        if (percentText != null)
        {
            int pct = Mathf.RoundToInt(_visual * 100f);
            percentText.text = pct + "%";
        }

        // 색상 보간
        if (fillImage != null)
            fillImage.color = Color.Lerp(lowColor, highColor, _visual);
    }
}

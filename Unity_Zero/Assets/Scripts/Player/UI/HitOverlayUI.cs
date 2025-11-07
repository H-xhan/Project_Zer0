using UnityEngine;
using UnityEngine.UI;

public class HitOverlayUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("전체 화면을 덮는 UI Image")]
    public Image image;

    [Header("Flash Settings")]
    [Tooltip("피격 시 기본 색상 (알파는 코드에서 제어)")]
    public Color flashColor = new Color(1f, 0f, 0f, 0f);

    [Tooltip("피격 시 최대 알파 값")]
    public float maxAlpha = 0.6f;

    [Tooltip("피격 1회당 추가되는 알파 값")]
    public float addPerHit = 0.35f;

    [Tooltip("강하게 유지되는 시간")]
    public float holdTime = 0.06f;

    [Tooltip("서서히 사라지는 속도")]
    public float fadeOutSpeed = 2.0f;

    // 유지 타이머 (0 이상일 때는 유지)
    private float _holdTimer;

    private void Awake()
    {
        // Image 자동 참조
        if (image == null)
            image = GetComponent<Image>();

        if (image != null)
        {
            // 화면 전체를 덮도록 설정
            var rt = image.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;

            // 오버레이는 입력을 막지 않는다
            image.raycastTarget = false;

            // 초기 색상 알파 0
            var c = flashColor;
            c.a = 0f;
            image.color = c;
        }
    }

    // 외부에서 피해 발생 시 호출
    public void Flash(float damage)
    {
        if (image == null)
            return;

        // 기존 알파에 누적, 최대값 제한
        var c = flashColor;
        c.a = Mathf.Min(maxAlpha, image.color.a + addPerHit);
        image.color = c;

        // 유지 타이머 초기화
        _holdTimer = holdTime;
    }

    private void Update()
    {
        if (image == null)
            return;

        // 유지 시간 동안은 알파 감소하지 않음
        if (_holdTimer > 0f)
        {
            _holdTimer -= Time.deltaTime;
            return;
        }

        // 알파를 서서히 0으로 감소
        if (image.color.a > 0f)
        {
            var c = image.color;
            c.a = Mathf.MoveTowards(c.a, 0f, fadeOutSpeed * Time.deltaTime);
            image.color = c;
        }
    }
}

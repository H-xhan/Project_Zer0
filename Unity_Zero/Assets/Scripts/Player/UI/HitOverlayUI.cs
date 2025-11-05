using UnityEngine;
using UnityEngine.UI;

public class HitOverlayUI : MonoBehaviour
{
    [Header("Refs")]
    public Image image;                // 풀스크린 Image

    [Header("Flash")]
    public Color flashColor = new Color(1f, 0f, 0f, 0.0f); // R, alpha는 무시하고 아래에서 설정
    public float maxAlpha = 0.6f;      // 최대 투명도
    public float addPerHit = 0.35f;    // 한 번 피격 시 추가되는 알파
    public float holdTime = 0.06f;     // 유지 시간
    public float fadeOutSpeed = 2.0f;  // 초당 알파 감소량

    float _holdTimer = 0f;

    void Awake()
    {
        if (image == null) image = GetComponent<Image>();

        //강제로 화면 전체를 덮도록 고정
        if (image != null)
        {
            var rt = image.rectTransform;
            rt.anchorMin = Vector2.zero;     // (0,0)
            rt.anchorMax = Vector2.one;      // (1,1)
            rt.offsetMin = Vector2.zero;     // 좌하단 여백 0
            rt.offsetMax = Vector2.zero;     // 우상단 여백 0
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
            image.raycastTarget = false;

            // 시작은 투명
            var c = flashColor;
            c.a = 0f;
            image.color = c;
        }
    }

    public void Flash(float damage)
    {
        if (image == null) return;

        // 기존 알파에 누적, maxAlpha 제한
        var c = image.color;
        c = flashColor;                                      // 색 고정
        c.a = Mathf.Min(maxAlpha, image.color.a + addPerHit); // 알파 누적
        image.color = c;

        _holdTimer = holdTime; // 잠깐 유지
    }

    void Update()
    {
        if (image == null) return;

        if (_holdTimer > 0f)
        {
            _holdTimer -= Time.deltaTime; // 유지 타이머 소진
            return;
        }

        if (image.color.a > 0f)
        {
            var c = image.color;
            c.a = Mathf.MoveTowards(c.a, 0f, fadeOutSpeed * Time.deltaTime); // 서서히 사라짐
            image.color = c;
        }
    }
}

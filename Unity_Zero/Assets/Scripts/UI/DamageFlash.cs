using UnityEngine;
using UnityEngine.UI;

public class DamageFlash : MonoBehaviour
{
    [Header("References")]
    public Image overlayImage;                                   // 전체 화면을 덮는 Image (빨간색)

    [Header("Flash Settings")]
    public float baseMaxAlpha = 0.35f;                           // 기본 최대 알파 (가시성)
    public float criticalExtraAlpha = 0.25f;                     // 위험 구간일 때 추가 알파
    public float flashInTime = 0.05f;                            // 번쩍 올라오는 시간(초)
    public float fadeOutTime = 0.25f;                            // 서서히 사라지는 시간(초)
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 페이드 커브

    [Header("Stacking")]
    public bool stackIntensity = true;                           // 연속 피격 시 세기 누적 여부
    public float maxStackedAlpha = 0.8f;                         // 누적 상한

    float currentAlpha = 0f;                                     // 현재 알파 (상태값)
    Coroutine running;                                           // 현재 코루틴 핸들

    void Reset()
    {
        // 에디터에서 추가 시 자동 참조 시도
        if (!overlayImage) overlayImage = GetComponent<Image>(); // 같은 오브젝트에 붙였으면 자동할당
    }

    void Awake()
    {
        if (!overlayImage) overlayImage = GetComponent<Image>(); // 런타임 보정
        SetAlpha(0f);                                            // 시작은 투명
    }

    // 외부에서 호출: 데미지를 받았을 때 번쩍임
    public void Flash(float damageAmount, float currentHealthRatio, float criticalThreshold = 0.3f)
    {
        if (!overlayImage) return;                               // 참조 없으면 무시

        // 1) 데미지 비례 강도 계산 (과하지 않게 정규화)
        //    - damageAmount가 클수록 강하게
        //    - currentHealthRatio가 낮아 위험할수록 더 강하게
        float dmgFactor = Mathf.Clamp01(damageAmount / 50f);     // 50 데미지를 기준으로 0~1 스케일
        float dangerBonus = (currentHealthRatio <= criticalThreshold) ? criticalExtraAlpha : 0f;

        float target = baseMaxAlpha * (0.5f + 0.5f * dmgFactor)  // 0.5~1.0 배 스케일
                       + dangerBonus;                            // 위험 보너스 더하기

        // 2) 누적 모드면 현재 알파보다 높게 덮어씀 (상한 제한)
        if (stackIntensity) target = Mathf.Clamp(Mathf.Max(target, currentAlpha), 0f, maxStackedAlpha);

        // 3) 코루틴 재시작
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(FlashRoutine(target));
    }

    // 알파 변경 루틴: 짧게 올렸다가 부드럽게 페이드아웃
    System.Collections.IEnumerator FlashRoutine(float peakAlpha)
    {
        // 짧게 상승
        float t = 0f;
        float start = currentAlpha;
        while (t < flashInTime)
        {
            t += Time.unscaledDeltaTime;                         // 타임스케일 무시 (히트스톱에도 보이게)
            float k = Mathf.Clamp01(t / flashInTime);
            SetAlpha(Mathf.Lerp(start, peakAlpha, k));
            yield return null;
        }
        SetAlpha(peakAlpha);

        // 곡선 기반 페이드아웃
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeOutTime);
            float eased = 1f - fadeCurve.Evaluate(k);           // 1→0으로 감소
            SetAlpha(peakAlpha * eased);
            yield return null;
        }
        SetAlpha(0f);
        running = null;
    }

    void SetAlpha(float a)
    {
        currentAlpha = a;                                       // 상태 저장
        if (overlayImage)
        {
            Color c = overlayImage.color;                       // 기존 색 유지
            c.a = a;                                           // 알파만 조절
            overlayImage.color = c;
        }
    }
}

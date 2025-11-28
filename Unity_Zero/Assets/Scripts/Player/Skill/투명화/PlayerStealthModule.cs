using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStealthModule : MonoBehaviour
{
    [Tooltip("투명 처리할 스키닝 메쉬 렌더러들(몸, 머리, 장비 등)")]
    [SerializeField] private SkinnedMeshRenderer[] targetRenderers;

    [Tooltip("투명화 시 목표 알파 값")]
    [Range(0f, 1f)]
    [SerializeField] private float stealthAlpha = 0.3f;

    [Tooltip("투명해지는데 걸리는 시간(초)")]
    [SerializeField] private float fadeOutDuration = 0.25f;

    [Tooltip("복구되는데 걸리는 시간(초)")]
    [SerializeField] private float fadeInDuration = 0.2f;

    private readonly List<MaterialPropertyBlock> _mpbList = new List<MaterialPropertyBlock>();
    private readonly List<Color> _baseColorList = new List<Color>();
    private readonly List<bool> _useBaseColorProperty = new List<bool>();

    private Coroutine _fadeRoutine;
    public bool IsStealthActive { get; private set; }

    // PlayerController에서 쓰는 기존 API 호환용
    public bool IsInvisible => IsStealthActive;

    private void Awake()
    {
        CacheRenderers();
    }

    private void CacheRenderers()
    {
        _mpbList.Clear();
        _baseColorList.Clear();
        _useBaseColorProperty.Clear();

        if (targetRenderers == null) return;

        foreach (var rend in targetRenderers)
        {
            if (rend == null) continue;

            var mpb = new MaterialPropertyBlock();
            rend.GetPropertyBlock(mpb);

            var mat = rend.sharedMaterial;
            if (mat == null)
            {
                _mpbList.Add(mpb);
                _baseColorList.Add(Color.white);
                _useBaseColorProperty.Add(true);
                continue;
            }

            bool hasBaseColor = mat.HasProperty("_BaseColor");
            Color baseColor = hasBaseColor ? mat.GetColor("_BaseColor") : mat.color;

            _mpbList.Add(mpb);
            _baseColorList.Add(baseColor);
            _useBaseColorProperty.Add(hasBaseColor);
        }
    }

    public void EnableStealth()
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeRoutine(1f, stealthAlpha, fadeOutDuration));
        IsStealthActive = true;
    }

    public void DisableStealth()
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeRoutine(stealthAlpha, 1f, fadeInDuration));
        IsStealthActive = false;
    }

    private IEnumerator FadeRoutine(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            ApplyAlpha(to);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(from, to, t);
            ApplyAlpha(alpha);
            yield return null;
        }

        ApplyAlpha(to);
    }

    private void ApplyAlpha(float alpha)
    {
        if (targetRenderers == null) return;

        int count = Mathf.Min(targetRenderers.Length, _mpbList.Count);
        for (int i = 0; i < count; i++)
        {
            var rend = targetRenderers[i];
            if (rend == null) continue;

            var mpb = _mpbList[i];
            var baseColor = _baseColorList[i];
            bool useBaseColor = _useBaseColorProperty[i];

            var c = baseColor;
            c.a = alpha;

            if (useBaseColor)
                mpb.SetColor("_BaseColor", c);
            else
                mpb.SetColor("_Color", c);

            rend.SetPropertyBlock(mpb);
        }
    }
}

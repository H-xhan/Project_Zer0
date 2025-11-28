using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStealthModule : MonoBehaviour
{
    [Tooltip("스텔스 적용 대상 SkinnedMeshRenderer. 비워두면 자식에서 자동 검색")]
    [SerializeField] private SkinnedMeshRenderer[] targetRenderers;

    [Tooltip("Target Renderers가 비어 있을 때 자식에서 자동 검색 여부")]
    [SerializeField] private bool autoFindRenderersIfEmpty = true;

    [Tooltip("스텔스 상태에서 사용할 홀로그램/투명 머티리얼")]
    [SerializeField] private Material stealthMaterial;

    [Tooltip("페이드 인/아웃 소요 시간")]
    [SerializeField] private float fadeDuration = 0.35f;

    // 원본 머티리얼 저장
    private readonly Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();

    // 페이드용 MaterialPropertyBlock
    private readonly List<Renderer> _fadeRenderers = new List<Renderer>();

    private Coroutine _fadeRoutine;

    public bool IsStealthActive { get; private set; }

    public bool IsStealthed => IsStealthActive;


    private void Awake()
    {
        CacheRenderersIfNeeded();
    }

    private void CacheRenderersIfNeeded()
    {
        if (targetRenderers != null && targetRenderers.Length > 0)
            return;

        if (!autoFindRenderersIfEmpty)
            return;

        targetRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
    }

    public void EnableStealth()
    {
        if (IsStealthActive)
            return;

        CacheRenderersIfNeeded();
        SwapToStealth();

        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeTo(1f)); // 1 = 완전 스텔스
        IsStealthActive = true;
    }

    public void DisableStealth()
    {
        if (!IsStealthActive)
            return;

        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeTo(0f)); // 0 = 원래 모습
        IsStealthActive = false;
    }

    private void SwapToStealth()
    {
        var renderers = GetComponentsInChildren<Renderer>(true);
        _fadeRenderers.Clear();

        foreach (var rend in renderers)
        {
            if (!_originalMaterials.ContainsKey(rend))
                _originalMaterials.Add(rend, rend.sharedMaterials);

            // 스텔스 머티리얼로 교체
            Material[] src = rend.sharedMaterials;
            Material[] newMats = new Material[src.Length];
            for (int i = 0; i < newMats.Length; i++)
            {
                newMats[i] = stealthMaterial;
            }

            rend.materials = newMats;
            _fadeRenderers.Add(rend);
        }
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float time = 0f;
        float startAlpha = 1f - targetAlpha; // enable: 0->1, disable: 1->0

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            ApplyAlpha(alpha);

            yield return null;
        }

        ApplyAlpha(targetAlpha);

        if (targetAlpha == 0f)
            RestoreOriginal();
    }

    private void ApplyAlpha(float alpha)
    {
        foreach (var rend in _fadeRenderers)
        {
            if (rend == null) continue;

            var mpb = new MaterialPropertyBlock();
            rend.GetPropertyBlock(mpb);

            mpb.SetColor("_BaseColor", new Color(1f, 1f, 1f, alpha));
            rend.SetPropertyBlock(mpb);
        }
    }

    private void RestoreOriginal()
    {
        foreach (var kvp in _originalMaterials)
        {
            if (kvp.Key != null)
                kvp.Key.materials = kvp.Value;
        }
    }
}

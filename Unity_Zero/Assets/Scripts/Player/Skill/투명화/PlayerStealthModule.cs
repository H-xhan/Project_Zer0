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

    // 스텔스 전 원래 머티리얼 저장
    private readonly Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();

    public bool IsStealthActive { get; private set; }
    public bool IsInvisible => IsStealthActive;

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
        Debug.Log($"[Stealth] SkinnedMeshRenderer 자동 검색: {targetRenderers.Length}개");
    }

    public void EnableStealth()
    {
        if (IsStealthActive)
            return;

        if (stealthMaterial == null)
        {
            Debug.LogWarning("[Stealth] StealthMaterial이 비어 있습니다.");
            return;
        }

        CacheRenderersIfNeeded();

        // 자식의 모든 Renderer 기준으로 처리 (장비/머리 등 포함)
        var renderers = GetComponentsInChildren<Renderer>(true);

        foreach (var rend in renderers)
        {
            if (rend == null)
                continue;

            if (!_originalMaterials.ContainsKey(rend))
                _originalMaterials.Add(rend, rend.sharedMaterials);

            var src = rend.sharedMaterials;
            if (src == null || src.Length == 0)
                continue;

            var newMats = new Material[src.Length];
            for (int i = 0; i < newMats.Length; i++)
                newMats[i] = stealthMaterial;

            // materials 사용해서 인스턴스 생성
            rend.materials = newMats;
        }

        IsStealthActive = true;
        Debug.Log("[Stealth] Stealth 활성화 (Material Swap)");
    }

    public void DisableStealth()
    {
        if (!IsStealthActive)
            return;

        foreach (var kvp in _originalMaterials)
        {
            var rend = kvp.Key;
            var mats = kvp.Value;

            if (rend == null || mats == null)
                continue;

            rend.materials = mats;
        }

        IsStealthActive = false;
        Debug.Log("[Stealth] Stealth 비활성화 (Original Material 복구)");
    }
}

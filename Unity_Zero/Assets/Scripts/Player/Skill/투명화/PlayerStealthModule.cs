using System;
using UnityEngine;

public class PlayerStealthModule : MonoBehaviour
{
    [SerializeField] private Renderer[] targetRenderers;

    [SerializeField] private MovementModule movementModule;
    [SerializeField] private float stealthSpeedMultiplier = 0.7f; // 70% 속도

    [Range(0f, 1f)][SerializeField] private float visibleAlpha = 1f;
    [Range(0f, 1f)][SerializeField] private float invisibleAlpha = 0.15f;
    [SerializeField] private float fadeSpeed = 3f;

    [SerializeField] private bool isInvisible;

    public bool IsInvisible => isInvisible;

    private Material[][] _cachedMaterials;
    private Color[][] _originalColors;

    private float _currentAlpha;
    private float _targetAlpha;

    [SerializeField] private GhostTrailEffect ghostTrailEffect;

    private void Awake()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
            targetRenderers = GetComponentsInChildren<Renderer>();

        CacheMaterialsAndColors();

        isInvisible = false;
        _currentAlpha = visibleAlpha;
        _targetAlpha = visibleAlpha;
    }

    private void CacheMaterialsAndColors()
    {
        int count = targetRenderers.Length;

        _cachedMaterials = new Material[count][];
        _originalColors = new Color[count][];

        for (int i = 0; i < count; i++)
        {
            var rend = targetRenderers[i];
            if (rend == null) continue;

            // materials 호출은 여기서 단 1번만
            _cachedMaterials[i] = rend.materials;

            int matCount = _cachedMaterials[i].Length;
            _originalColors[i] = new Color[matCount];

            for (int j = 0; j < matCount; j++)
            {
                var mat = _cachedMaterials[i][j];

                if (mat.HasProperty("_BaseColor"))
                    _originalColors[i][j] = mat.GetColor("_BaseColor");
                else
                    _originalColors[i][j] = mat.color;
            }
        }
    }

    private void Update()
    {
        if (Mathf.Approximately(_currentAlpha, _targetAlpha))
            return;

        _currentAlpha = Mathf.MoveTowards(_currentAlpha, _targetAlpha, fadeSpeed * Time.deltaTime);
        ApplyAlpha(_currentAlpha);
    }

    public void SetInvisible(bool value)
    {
        if (isInvisible == value)
            return;

        isInvisible = value;
        _targetAlpha = isInvisible ? invisibleAlpha : visibleAlpha;

        // 스텔스 켤 때 속도 패널티 적용
        if (movementModule != null)
        {
            movementModule.SetSpeedMultiplier(
                isInvisible ? stealthSpeedMultiplier : 1f
            );
        }

        // 켤 때만 잔상
        if (isInvisible && ghostTrailEffect != null)
        {
            ghostTrailEffect.PlayOneShotTrail();
        }

        // 해제 직전에 한 번 더 터뜨리고 싶으면 아래처럼:
        
        if (!isInvisible && ghostTrailEffect != null)
        {
            ghostTrailEffect.PlayOneShotTrail();
        }
        
    }

    private void ApplyAlpha(float alpha)
    {
        for (int i = 0; i < _cachedMaterials.Length; i++)
        {
            var mats = _cachedMaterials[i];
            var origColors = _originalColors[i];

            for (int j = 0; j < mats.Length; j++)
            {
                var mat = mats[j];
                Color targetColor = new Color(
                    origColors[j].r,
                    origColors[j].g,
                    origColors[j].b,
                    alpha);

                mat.color = targetColor;

                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", targetColor);
            }
        }
    }
}

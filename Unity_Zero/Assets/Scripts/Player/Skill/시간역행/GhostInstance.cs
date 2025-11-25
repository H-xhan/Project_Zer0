using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GhostInstance : MonoBehaviour
{
    [Tooltip("잔상이 유지되는 시간(초)")]
    [SerializeField] private float lifetime = 0.4f;

    private float _elapsed;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    private MaterialPropertyBlock _mpb;
    private Color _baseColor;
    private Action<GhostInstance> _onReturnToPool;

    private static readonly int ColorID = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();

        if (_meshFilter.sharedMesh == null)
            _meshFilter.sharedMesh = new Mesh();

        _mpb = new MaterialPropertyBlock();
    }

    public void Initialize(Material sharedMaterial, Color baseColor, float lifeTime, Action<GhostInstance> onReturnToPool)
    {
        _meshRenderer.sharedMaterial = sharedMaterial;

        _baseColor = baseColor;
        lifetime = lifeTime;
        _onReturnToPool = onReturnToPool;

        _elapsed = 0f;

        ApplyColor(_baseColor);
    }

    public void BakeFromSkinnedMesh(SkinnedMeshRenderer skinnedMeshRenderer)
    {
        if (_meshFilter.sharedMesh == null)
            _meshFilter.sharedMesh = new Mesh();

        skinnedMeshRenderer.BakeMesh(_meshFilter.sharedMesh);
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;

        if (_elapsed >= lifetime)
        {
            ReturnToPool();
            return;
        }

        float t = _elapsed / lifetime;
        float alpha = Mathf.Lerp(_baseColor.a, 0f, t);

        Color current = _baseColor;
        current.a = alpha;
        ApplyColor(current);
    }

    private void ApplyColor(Color color)
    {
        _meshRenderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(ColorID, color);
        _meshRenderer.SetPropertyBlock(_mpb);
    }

    private void ReturnToPool()
    {
        _onReturnToPool?.Invoke(this);
        gameObject.SetActive(false);
    }
}

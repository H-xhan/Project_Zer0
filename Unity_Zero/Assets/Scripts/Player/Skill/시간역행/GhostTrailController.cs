using System.Collections.Generic;
using UnityEngine;

public class GhostTrailController : MonoBehaviour
{
    [Tooltip("잔상을 만들 기준 스킨 메쉬 렌더러 (캐릭터 바디)")]
    [SerializeField] private SkinnedMeshRenderer targetSkinnedMesh;

    [Tooltip("잔상에 사용할 머티리얼 (Unlit Transparent 권장)")]
    [SerializeField] private Material ghostMaterial;

    [Tooltip("잔상의 기본 색상 및 초기 투명도")]
    [SerializeField] private Color ghostColor = new Color(0f, 1f, 1f, 0.6f);

    [Tooltip("스냅샷 생성 간격 (초)")]
    [SerializeField] private float snapshotInterval = 0.05f;

    [Tooltip("각 잔상이 유지되는 시간 (초)")]
    [SerializeField] private float ghostLifetime = 5f;

    [Tooltip("미리 만들어둘 잔상 개수 (풀 크기)")]
    [SerializeField] private int poolSize = 120;

    private readonly Queue<GhostInstance> _pool = new Queue<GhostInstance>();
    private float _timer;
    private bool _isActive;

    private void Awake()
    {
        InitializePool();
    }

    private void Update()
    {
        if (!_isActive) return;
        if (targetSkinnedMesh == null) return;

        _timer += Time.deltaTime;

        if (_timer >= snapshotInterval)
        {
            _timer = 0f;
            SpawnSnapshot();
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GhostInstance instance = CreateGhostInstance();
            instance.gameObject.SetActive(false);
            _pool.Enqueue(instance);
        }
    }

    private GhostInstance CreateGhostInstance()
    {
        GameObject go = new GameObject("GhostInstance");
        go.transform.SetParent(transform);

        GhostInstance ghost = go.AddComponent<GhostInstance>();

        MeshFilter meshFilter = go.GetComponent<MeshFilter>();
        if (meshFilter.sharedMesh == null)
            meshFilter.sharedMesh = new Mesh();

        return ghost;
    }

    private GhostInstance GetFromPool()
    {
        if (_pool.Count > 0)
            return _pool.Dequeue();

        GhostInstance extra = CreateGhostInstance();
        extra.gameObject.SetActive(false);
        return extra;
    }

    private void ReturnToPool(GhostInstance instance)
    {
        _pool.Enqueue(instance);
    }

    public void SpawnSnapshotAt(Vector3 pos, Quaternion rot)
    {
        GhostInstance ghost = GetFromPool();
        if (ghost == null) return;

        Transform ghostTransform = ghost.transform;
        ghostTransform.position = pos;
        ghostTransform.rotation = rot;
        ghostTransform.localScale = Vector3.one;

        ghost.Initialize(ghostMaterial, ghostColor, ghostLifetime, ReturnToPool);
        ghost.BakeFromSkinnedMesh(targetSkinnedMesh);

        ghost.gameObject.SetActive(true);
    }

    private void SpawnSnapshot()
    {
        GhostInstance ghost = GetFromPool();
        if (ghost == null) return;

        Transform targetTransform = targetSkinnedMesh.transform;

        Transform ghostTransform = ghost.transform;
        ghostTransform.position = targetTransform.position;
        ghostTransform.rotation = targetTransform.rotation;
        ghostTransform.localScale = targetTransform.lossyScale;

        ghost.Initialize(ghostMaterial, ghostColor, ghostLifetime, ReturnToPool);
        ghost.BakeFromSkinnedMesh(targetSkinnedMesh);

        ghost.gameObject.SetActive(true);
    }

    public void SetActive(bool active)
    {
        _isActive = active;

        if (!active)
            _timer = 0f;
    }
}

using System.Collections;
using UnityEngine;

public class GhostTrailController : MonoBehaviour
{
    [Tooltip("잔상을 만들 기준 스키닝 메쉬 렌더러(몸 전체)")]
    [SerializeField] private SkinnedMeshRenderer sourceRenderer;

    [Tooltip("잔상에 사용할 머티리얼(Transparent/Fade 계열)")]
    [SerializeField] private Material ghostMaterial;

    [Tooltip("잔상 유지 시간(초)")]
    [SerializeField] private float ghostLifetime = 0.5f;

    [Tooltip("연속 잔상 생성 주기(초) - R 스킬용")]
    [SerializeField] private float trailSpawnInterval = 0.08f;

    private bool _continuousActive;
    private float _trailTimer;

    private void Update()
    {
        if (!_continuousActive)
            return;

        _trailTimer += Time.deltaTime;
        if (_trailTimer >= trailSpawnInterval)
        {
            _trailTimer = 0f;
            // 현재 위치 기준으로 잔상 생성
            SpawnSnapshotAt(transform.position, transform.rotation);
        }
    }

    // R 스킬용: 연속 잔상 on/off
    public void SetContinuousTrail(bool active)
    {
        _continuousActive = active;
        _trailTimer = 0f;
    }

    // Q 스킬용: 특정 위치/회전에 스냅샷 잔상 생성
    public void SpawnSnapshotAt(Vector3 position, Quaternion rotation)
    {
        if (sourceRenderer == null || ghostMaterial == null)
            return;

        var mesh = new Mesh();
        sourceRenderer.BakeMesh(mesh);

        var ghostObj = new GameObject("GhostTrail_Snapshot");
        var t = ghostObj.transform;
        t.position = position;
        t.rotation = rotation;
        t.localScale = transform.lossyScale;

        var mf = ghostObj.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        var mr = ghostObj.AddComponent<MeshRenderer>();
        mr.sharedMaterial = ghostMaterial;

        StartCoroutine(FadeAndDestroy(ghostObj, mesh, mr));
    }

    private IEnumerator FadeAndDestroy(GameObject ghostObj, Mesh mesh, MeshRenderer mr)
    {
        var mpb = new MaterialPropertyBlock();

        Color baseColor;
        bool useBaseColor = ghostMaterial.HasProperty("_BaseColor");
        if (useBaseColor)
            baseColor = ghostMaterial.GetColor("_BaseColor");
        else
            baseColor = ghostMaterial.color;

        float elapsed = 0f;

        while (elapsed < ghostLifetime)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Clamp01(elapsed / ghostLifetime);

            var c = baseColor;
            c.a *= t;

            if (useBaseColor)
                mpb.SetColor("_BaseColor", c);
            else
                mpb.SetColor("_Color", c);

            mr.SetPropertyBlock(mpb);
            yield return null;
        }

        Destroy(mesh);
        Destroy(ghostObj);
    }
}

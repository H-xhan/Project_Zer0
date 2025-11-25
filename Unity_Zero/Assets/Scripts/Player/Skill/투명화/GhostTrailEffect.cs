using System.Collections;
using UnityEngine;

public class GhostTrailEffect : MonoBehaviour
{
    [Tooltip("스냅샷을 뽑아낼 스킨드 메쉬 렌더러 (비워두면 자동 검색)")]
    [SerializeField] private SkinnedMeshRenderer targetRenderer;

    [Tooltip("잔상에 사용할 원본 머티리얼")]
    [SerializeField] private Material ghostMaterial;

    [SerializeField] private float spawnInterval = 0.03f;
    [SerializeField] private float ghostLifetime = 0.6f;

    [Range(0f, 1f)][SerializeField] private float startAlpha = 0.45f;
    [Range(0f, 1f)][SerializeField] private float endAlpha = 0.0f;

    [SerializeField] private int spawnCount = 10;

    private bool _isPlaying;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        if (ghostMaterial == null)
            Debug.LogWarning("[GhostTrail] Ghost Material이 비어 있습니다.");
    }

    public void PlayOneShotTrail()
    {
        if (_isPlaying)
            return;

        if (targetRenderer == null || ghostMaterial == null)
        {
            Debug.LogWarning("[GhostTrail] 재생 불가: targetRenderer 또는 ghostMaterial 누락.");
            return;
        }

        StartCoroutine(SpawnTrailRoutine());
    }

    private IEnumerator SpawnTrailRoutine()
    {
        _isPlaying = true;

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnGhost();
            yield return new WaitForSeconds(spawnInterval);
        }

        _isPlaying = false;
    }

    private void SpawnGhost()
    {
        // 1. 메쉬 생성 및 베이크
        var mesh = new Mesh();
        targetRenderer.BakeMesh(mesh);

        // 2. 잔상 오브젝트 생성
        var ghostObj = new GameObject("GhostTrail");
        ghostObj.transform.SetPositionAndRotation(targetRenderer.transform.position, targetRenderer.transform.rotation);
        ghostObj.transform.localScale = targetRenderer.transform.lossyScale * 1.05f;

        var mf = ghostObj.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        var mr = ghostObj.AddComponent<MeshRenderer>();
        mr.sharedMaterial = ghostMaterial; // 머티리얼 인스턴스 복제 없음

        // 3. MPB로 초기 알파 세팅
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mr.GetPropertyBlock(mpb);

        Color baseColor = ghostMaterial.HasProperty("_BaseColor")
            ? ghostMaterial.GetColor("_BaseColor")
            : ghostMaterial.color;

        baseColor.a = startAlpha;

        if (ghostMaterial.HasProperty("_BaseColor"))
            mpb.SetColor("_BaseColor", baseColor);

        mpb.SetColor("_Color", baseColor);
        mr.SetPropertyBlock(mpb);

        // 4. 페이드 + 메쉬/오브젝트 정리
        StartCoroutine(FadeAndDestroy(ghostObj, mr, baseColor, mesh));
    }

    // meshToDestroy 추가 + MPB 재사용
    private IEnumerator FadeAndDestroy(GameObject ghostObj, MeshRenderer mr, Color baseColor, Mesh meshToDestroy)
    {
        float elapsed = 0f;
        var mpb = new MaterialPropertyBlock();

        while (elapsed < ghostLifetime)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(0f, 1f, elapsed / ghostLifetime);
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            Color c = baseColor;
            c.a = alpha;

            mr.GetPropertyBlock(mpb);

            if (ghostMaterial.HasProperty("_BaseColor"))
                mpb.SetColor("_BaseColor", c);

            mpb.SetColor("_Color", c);
            mr.SetPropertyBlock(mpb);

            yield return null;
        }

        // 메쉬 먼저 정리 → 그 다음 오브젝트 삭제
        if (meshToDestroy != null)
            Destroy(meshToDestroy);

        Destroy(ghostObj);
    }
}

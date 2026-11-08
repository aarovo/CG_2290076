// DiamondMesh_Faceted.cs
// 씬: S19의 모든 씬 / 오브젝트: 면을 나눈 다이아몬드
// S19: DiamondMesh_Rewinded와 같은 정점, 같은 삼각형에서 출발해
//      면마다 정점을 따로 두어(6 → 24) normal과 UV 좌표를 면마다 갖게 한 버전.
// 이 스크립트는 메시를 만들고 보관하는 역할만 함 — 이동/회전/스케일 계산은 별도 스크립트가 담당.
// 머티리얼이 비어 있거나 분홍색(셰이더 오류)이면 URP Lit 머티리얼을 자동으로 붙임.
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class S19_PyramidMeshAbs : MonoBehaviour
{
    [Header("다이아몬드 원본 정점 (0=아래 꼭짓점, 1~4=중간 사각형, 5=위 꼭짓점)")]
    [SerializeField]
    Vector3[] baseVertices =
    {
        new Vector3(-0.5f, -0.5f, -0.5f), // 0 밑면 왼쪽 앞
        new Vector3( 0.5f, -0.5f, -0.5f), // 1 밑면 오른쪽 앞
        new Vector3( 0.5f, -0.5f,  0.5f), // 2 밑면 오른쪽 뒤
        new Vector3(-0.5f, -0.5f,  0.5f), // 3 밑면 왼쪽 뒤
        new Vector3( 0f,    0.5f,  0f),   // 4 꼭대기
    };

    // DiamondMesh_Rewinded와 같은 삼각형 (면마다 정점 번호 3개)
    // 바깥에서 봤을 때 시계 방향 = 앞면 (Unity 규칙)
    static readonly int[] triangles =
    {
        0, 1, 2,  0, 2, 3,                     // 밑면 (아래에서 볼 때 시계 방향)
        4, 1, 0,  4, 2, 1,  4, 3, 2,  4, 0, 3, // 옆면 4개 (앞, 오른쪽, 뒤, 왼쪽)
    };

    Mesh mesh;

    public Vector3[] BaseVertices => baseVertices;

    void OnEnable()
    {
        EnsureLitMaterial();

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        SetVertices(baseVertices);
    }

    public void SetVertices(Vector3[] v)
    {
        Vector2 uvTopCenter = new Vector2(0.5f, 1); // 텍스처의 위 가운데
        Vector2 uvBottomLeft = new Vector2(0, 0);    // 텍스처의 왼쪽 아래
        Vector2 uvBottomRight = new Vector2(1, 0);    // 텍스처의 오른쪽 아래

        Vector3[] facetedVertices =
        {
            v[0], v[1], v[2],  v[0], v[2], v[3],          // 밑면 2개
            v[4], v[1], v[0],  v[4], v[2], v[1],          // 옆면: 앞, 오른쪽
            v[4], v[3], v[2],  v[4], v[0], v[3],          // 옆면: 뒤, 왼쪽
        };

        int[] facetedTriangles =
        {
            0, 1, 2, 3, 4, 5,                             // 밑면
            6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17,   // 옆면
        };

        Vector2[] uv =
        {
            // 밑면: 정사각형 그대로 대응
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1),
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1),
            // 옆면 4개: (꼭대기, 오른쪽, 왼쪽) → (위 가운데, 오른쪽 아래, 왼쪽 아래)
            uvTopCenter, uvBottomRight, uvBottomLeft,
            uvTopCenter, uvBottomRight, uvBottomLeft,
            uvTopCenter, uvBottomRight, uvBottomLeft,
            uvTopCenter, uvBottomRight, uvBottomLeft,
        };

        mesh.Clear();
        mesh.vertices = facetedVertices;     // 다름: 6개 대신 24개
        mesh.triangles = facetedTriangles;   // 다름: 적은 순서대로
        mesh.uv = uv;                        // 다름: UV 좌표 추가
        mesh.RecalculateNormals();           // 같은 줄이지만, 이제 면마다 normal이 따로 계산됨
        mesh.RecalculateBounds();
    }

    // 머티리얼이 없거나 셰이더가 깨져 있으면(분홍색) Lit 머티리얼을 새로 만들어 붙임
    void EnsureLitMaterial()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        Material current = meshRenderer.sharedMaterial;

        bool needsMaterial = current == null
                          || current.shader == null
                          || !current.shader.isSupported
                          || current.shader.name == "Hidden/InternalErrorShader";
        if (!needsMaterial) return;

        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        if (lit == null) lit = Shader.Find("Standard");   // URP가 아닌 프로젝트용 대비
        if (lit == null) return;

        meshRenderer.sharedMaterial = new Material(lit) { name = "Diamond_Lit (자동 생성)" };
    }
}
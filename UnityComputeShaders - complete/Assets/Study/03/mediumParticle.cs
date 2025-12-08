using UnityEngine;

public class mediumParticle : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public ComputeShader shader;
    public Material material;

    // [데이터 1: 계산용] 입자의 중심점 (영혼)
    struct Particle
    {
        public Vector3 position;
    }

    // [데이터 2: 그리기용] 삼각형의 꼭짓점 (껍데기)
    struct Vertex
    {
        public Vector3 position;
    }

    ComputeBuffer particleBuffer;
    ComputeBuffer vertexBuffer;

    const int COUNT = 1000;

    void Start()
    {
        // 1. 파티클(영혼) 초기화
        Particle[] particles = new Particle[COUNT];

        for (int i = 0; i < COUNT; i++)
        {
            // 랜덤 위치에 생성
            particles[i].position = Random.insideUnitSphere * 5.0f;
        }

        // 2. 버퍼 생성
        // 파티클 버퍼: 1,000개 (stride: float 3개 = 12byte)
        particleBuffer = new ComputeBuffer(COUNT, 12);
        particleBuffer.SetData(particles);

        // ★핵심★ 버텍스 버퍼: 파티클 1개당 점 3개니까 -> 3,000개 생성!
        vertexBuffer = new ComputeBuffer(COUNT * 3, 12);

        int kernel = shader.FindKernel("CSMain");
        shader.SetBuffer(kernel, "particleBuffer", particleBuffer);
        shader.SetBuffer(kernel, "vertexBuffer", vertexBuffer);

        // 렌더링 쉐이더에도 연결
        material.SetBuffer("vertexBuffer", vertexBuffer);
    }

    // Update is called once per frame
    void Update()
    {
        shader.SetFloat("deltaTime", Time.deltaTime);
        // 그룹 계산: 1000 / 64
        shader.Dispatch(0, Mathf.CeilToInt(COUNT / 64f), 1, 1);
        
    }

    void OnRenderObject()
    {
        material.SetPass(0);
        // ★핵심★ 그리기 명령이 달라집니다.
        // MeshTopology.Triangles: 점 3개를 이어 삼각형을 그려라
        // 3: 점 3개를 한 세트로 봐라 (삼각형 1개)
        // COUNT: 그 세트를 총 1,000번 그려라
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 3, COUNT);
    }

    void OnDestroy()
    {
        if (particleBuffer != null) particleBuffer.Release();
        if (vertexBuffer != null) vertexBuffer.Release();
    }
}

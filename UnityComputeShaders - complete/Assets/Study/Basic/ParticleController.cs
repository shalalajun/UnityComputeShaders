using UnityEngine;

public class ParticleController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("설정")]
    public ComputeShader computeShader;
    public Material particleMaterial;
    public Mesh particleMesh;

    ComputeBuffer buffer;
    int count = 10000;
    int stride = 24;

    struct Particle
    {
        public Vector3 position;
        public Vector3 color;
    }

    void Start()
    {
        // 1. 택배 상자 만들기 (1만 개 자리)
        buffer = new ComputeBuffer(count,stride);

        // ★ 중요: 상자에 초기 데이터 채워넣기 (초기화)
        // 이걸 안 하면 쓰레기값(Garbage)이 들어있어서 화면에 안 보일 수 있습니다.
        Particle[] particles = new Particle[count];
        for (int i=0; i<count; i++)
        {
            // X, Z는 넓게 퍼뜨리고, Y는 바닥부터 천장까지 골고루 배치
            particles[i].position = new Vector3(
                Random.Range(-5.0f, 5.0f), // X
                Random.Range(0.0f, 10.0f), // Y
                Random.Range(-5.0f, 5.0f)  // Z
            );
            particles[i].color = Vector3.one;
        }
        buffer.SetData(particles);// 상자에 데이터 넣기

        // 2. 쉐이더들에게 상자 위치 알려주기
        // (Compute Shader에게)
        int kernel = computeShader.FindKernel("CSMain");
        computeShader.SetBuffer(kernel, "particleBuffer", buffer);

        // (Render Shader에게 - 마테리얼에 연결)
        particleMaterial.SetBuffer("_ParticleBuffer", buffer);
    }

    // Update is called once per frame
    void Update()
    {
        // 3. 계산 시키기 (Compute Shader)
        int kernel = computeShader.FindKernel("CSMain");

        // 시간 값 보내주기
        computeShader.SetFloat("time", Time.time);

        // 일꾼 투입! (10000개 / 64명 = 약 157개 그룹)
        computeShader.Dispatch(kernel, Mathf.CeilToInt(count / 64.0f), 1, 1);

        // 4. 그리기! (Draw)
        // URP에서는 Update에서 그리는 게 가장 확실합니다.
        Graphics.DrawMeshInstancedProcedural(particleMesh, 0, particleMaterial, 
            new Bounds(Vector3.zero, Vector3.one * 1000), count);
        
    }

    void OnDestroy()
    {
        // 5. 정리 (메모리 누수 방지)
        if (buffer != null) buffer.Release();
    }
}

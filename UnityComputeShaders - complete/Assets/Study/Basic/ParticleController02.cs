using UnityEngine;

public class ParticleController02 : MonoBehaviour
{
    [Header("필수 설정")]
    public ComputeShader computeShader;
    public Material particleMaterial;
    public Mesh particleMesh;

    ComputeBuffer buffer;
    int count = 40000;
    
    // ★ 변경 1: Stride를 32로 변경 (float 8개 * 4byte)
    // float4 position (x,y,z, life) + float4 color (r,g,b, a)
    int stride = 32; 

    struct Particle
    {
        public Vector4 position; // w에 '수명'을 저장할 겁니다.
        public Vector4 color;
    }

    void Start()
    {
        buffer = new ComputeBuffer(count, stride);
        Particle[] particles = new Particle[count];
        
        for (int i = 0; i < count; i++)
        {
            // ★ 변경 2: 처음부터 아주 넓게 퍼뜨림 (뭉침 방지)
            particles[i].position = new Vector4(
                Random.Range(-10.0f, 10.0f), 
                Random.Range(-5.0f, 5.0f),   
                Random.Range(-5.0f, 5.0f),
                Random.Range(0.5f, 1.0f) // w: 초기 수명 (0.5~1.0초)
            );
            particles[i].color = Vector4.one;
        }
        buffer.SetData(particles);

        int kernel = computeShader.FindKernel("CSMain");
        computeShader.SetBuffer(kernel, "particleBuffer", buffer);
        particleMaterial.SetBuffer("_ParticleBuffer", buffer);
    }

    void Update()
    {
        int kernel = computeShader.FindKernel("CSMain");
        computeShader.SetFloat("time", Time.time);
        
        // ★ 중요: 델타타임(프레임 간 시간)을 보내줘야 수명을 깎을 수 있습니다.
        computeShader.SetFloat("deltaTime", Time.deltaTime); 

        int groupCount = Mathf.CeilToInt(count / 64.0f);
        computeShader.Dispatch(kernel, groupCount, 1, 1);

        Graphics.DrawMeshInstancedProcedural(particleMesh, 0, particleMaterial, 
            new Bounds(Vector3.zero, new Vector3(50.0f, 50.0f, 50.0f)), count);
    }

    void OnDestroy()
    {
        if (buffer != null) buffer.Release();
    }
}
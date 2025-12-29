using UnityEngine;
using System.Runtime.InteropServices;//컴퓨트쉐이더에는 무조건 씀

public class GPUFlocking : MonoBehaviour
{
    public Transform target;
    
    // private Vector3 acceleration;    가속도는 지피유연산에서는 쓰고 지우는 값이라 나중에 적용한다ㅏ.

    [System.Serializable]
    public struct GPUBoidData
    {
        public Vector3 position;
        public Vector3 velocity;
    }

    [Range(0f, 20f)] public float maxSpeed = 5.0f;
    [Range(0f, 10f)] public float maxSteerForce = 0.5f;

    [Range(256, 32768)]
    public int boidCount = 8192;

    public ComputeShader computeFlockingShader;
    public ComputeBuffer computeFlockingBuffer;


   // [New 1] 렌더링에 필요한 변수들 추가
    public Mesh boidMesh;          // 화면에 그릴 모양 (예: 삼각형, 새 모양)
    public Material boidMaterial;  // 아까 만든 쉐이더(BoidRenderURP)를 적용한 재질

   // "몇 개를 어떻게 그릴지" 정보를 담는 특수 버퍼 (Args Buffer)
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    void Start()
    {
        computeFlockingBuffer = new ComputeBuffer(boidCount, Marshal.SizeOf(typeof(GPUBoidData)));

        GPUBoidData[] gpuBoids = new GPUBoidData[boidCount];//시샵에서는 이렇게 명확하게 자리부터 만들어줘야한다.

        for(int i=0; i<boidCount; i++)
        {
            gpuBoids[i].position = Random.insideUnitSphere * 10.0f;
            gpuBoids[i].velocity = Random.insideUnitSphere * 1.0f;

        }

        computeFlockingBuffer.SetData(gpuBoids);


        // 인자가 5개라서 크기는 5 * uint크기(4byte), 타입은 IndirectArguments
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        
        if (boidMesh != null)
        {
            args[0] = (uint)boidMesh.GetIndexCount(0); // 메쉬의 점 개수
            args[1] = (uint)boidCount;                 // 그릴 개수 (8192개)
            args[2] = (uint)boidMesh.GetIndexStart(0); // 시작점
            args[3] = (uint)boidMesh.GetBaseVertex(0); // 베이스 정점
            args[4] = 0;                               // 인스턴스 시작 위치
        }
        
        // 명령서 내용을 버퍼에 저장
        argsBuffer.SetData(args);
    }

    // Update is called once per frame
    void Update()
    {
      int kernelHandle = computeFlockingShader.FindKernel("CSMain");

      computeFlockingShader.SetBuffer(kernelHandle, "computeFlockingBuffer", computeFlockingBuffer);

      computeFlockingShader.SetFloat("deltaTime",Time.deltaTime);

      computeFlockingShader.Dispatch(kernelHandle, boidCount/256, 1, 1);

      RenderInstancedMesh();

    }

    // [New 4] 실제 그리기 함수
    void RenderInstancedMesh()
    {
        if (boidMaterial == null || boidMesh == null || !SystemInfo.supportsInstancing) return;

        // A. 마테리얼에게 "데이터는 저기(버퍼)에 있어"라고 알려줌
        // 쉐이더 코드에 적힌 변수명("computeFlockingBuffer")과 같아야 함!
        boidMaterial.SetBuffer("computeFlockingBuffer", computeFlockingBuffer);

        // B. 화면 밖으로 나가면 안 그리도록 경계 설정 (일단 엄청 크게 잡음)
        Bounds bounds = new Bounds(Vector3.zero, new Vector3(1000.0f, 1000.0f, 1000.0f));

        // C. [핵심] GPU야 그려라!! (GameObject 없이 그리기)
        Graphics.DrawMeshInstancedIndirect(
            boidMesh,           // 이 모양으로
            0,                  // 첫 번째 서브메쉬를
            boidMaterial,       // 이 재질을 입혀서
            bounds,             // 이 범위 안에서
            argsBuffer          // 이 명령서(개수 등)대로 그려라!
        );
    }

    void OnDestroy()
    {
        if (computeFlockingBuffer != null)
        {
            computeFlockingBuffer.Release(); // GPU 메모리 해제
            computeFlockingBuffer = null;
        }       
    }
}

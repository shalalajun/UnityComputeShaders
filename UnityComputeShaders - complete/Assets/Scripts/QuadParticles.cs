using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public class QuadParticles : MonoBehaviour
{

    private Vector2 cursorPos; // 마우스위치 저장용

    // struct
    // [데이터 설계도 1: 정점(껍데기)] 
    // 화면에 그려질 사각형의 모서리 점들입니다. 
    // 일꾼이 그림을 그릴 때 필요한 정보입니다.
    struct Vertex
    {
        public Vector3 position;
        public Vector2 uv;
        public float life;
    }


    // [데이터 설계도 2: 파티클(영혼)]
    // 눈에 보이진 않지만, 실제로 날아다니는 입자의 물리 정보입니다.
    // 일꾼이 위치를 계산할 때 쓰는 핵심 데이터입니다.
    struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;
        public float life;
    }

    // [택배 상자 무게 측정]
    // GPU한테 보낼 데이터 1개의 크기(byte)를 미리 계산합니다.
    // float 하나당 4byte입니다.
    const int SIZE_VERTEX = 6 * sizeof(float); // Vector3(3) + Vector2(2) + float(1) = 6개
    const int SIZE_PARTICLE = 7 * sizeof(float); // Vector3(3) + Vector3(3) + float(1) = 7개

    public int particleCount = 10000; // 파티클 개수 (만 개의 영혼)
    public Material material;         // 그림 그릴 붓 (쉐이더가 포함된 재질)
    public ComputeShader shader;      // 계산 전문 일꾼 (컴퓨트 쉐이더)
    [Range(0.01f, 1.0f)]
    public float quadSize = 0.1f;    // 사각형 크기

    int numParticles;  // 실제 계산된 파티클 개수
    int kernelID;      // 일꾼 작업반 ID
    ComputeBuffer particleBuffer;  // 파티클(영혼) 담을 컨테이너 트럭
    ComputeBuffer vertexBuffer;    // 버텍스(껍데기) 담을 컨테이너 트럭

    int groupSizeX;   // 일꾼 그룹 수
    
    // Use this for initialization
    void Start()
    {
        Init();  // 준비 시작!
    }

    void Init()
    {
        // 1. 일꾼 작업반(Kernel) 찾기
        // find the id of the kernel
        kernelID = shader.FindKernel("CSMain");


        // 2. 일꾼 배치 계획 세우기
        // GPU의 스레드 수에 맞춰서 그룹을 몇 개 만들지 계산합니다.
        uint threadsX;
        shader.GetKernelThreadGroupSizes(kernelID, out threadsX, out _, out _);
        groupSizeX = Mathf.CeilToInt((float)particleCount / (float)threadsX);
        numParticles = groupSizeX * (int)threadsX; // 그룹 단위로 딱 떨어지게 개수 조정


// 3. CPU에서 데이터 준비하기 (포장 작업)

        // (1) 파티클(영혼) 배열 생성
        // initialize the particles
        Particle[] particleArray = new Particle[numParticles];


        // (2) 버텍스(껍데기) 배열 생성 
        // ★중요★: 파티클 1개당 사각형 1개를 만듭니다.
        // 사각형 1개는 삼각형 2개로 이루어지고, 삼각형 1개는 점 3개니까
        // 파티클 1개 = 점 6개 (3개 + 3개)가 필요합니다.
        int numVertices = numParticles * 6;
        Vertex[] vertexArray = new Vertex[numVertices];

        Vector3 pos = new Vector3();
        
        int index;

        // 초기값 세팅 루프
        for (int i = 0; i < numParticles; i++)
        {

            // 랜덤한 위치 생성 (공중에 흩뿌리기)
            pos.Set(Random.value * 2 - 1.0f, Random.value * 2 - 1.0f, Random.value * 2 - 1.0f);
            pos.Normalize();
            pos *= Random.value;
            pos *= 0.5f;


            // 파티클(영혼) 정보 채우기
            particleArray[i].position.Set(pos.x, pos.y, pos.z + 3);
            particleArray[i].velocity.Set(0,0,0);
          
            // Initial life value
            particleArray[i].life = Random.value * 5.0f + 1.0f;
            

            // 버텍스(껍데기) 정보 채우기
            // 파티클 1개(i)당 버텍스 6개(index ~ index+5)를 설정합니다.
            index = i*6;


            // 사각형을 만들기 위한 UV(텍스처 좌표)를 미리 지정합니다.
            // (0,0)은 왼쪽 아래, (1,1)은 오른쪽 위입니다.
            
            // 첫 번째 삼각형 (좌하, 좌상, 우상)
            //Triangle 1 - bottom-left, top-left, top-right
            vertexArray[index].uv.Set(0,0);
            vertexArray[index+1].uv.Set(0,1);
            vertexArray[index+2].uv.Set(1,1);


            // 두 번째 삼각형 (좌하, 우상, 우하)
            //Triangle 2 - bottom-left, top-right, bottom-right  // // 
			vertexArray[index+3].uv.Set(0,0);
            vertexArray[index+4].uv.Set(1,1);
            vertexArray[index+5].uv.Set(1,0);


            // 주의: 여기서 vertexArray의 position(위치)은 설정하지 않았습니다.
            // 왜냐하면 위치는 매 프레임 GPU 일꾼이 계산해서 채워넣을 것이기 때문입니다!
        }

        // create compute buffers
        // 4. 택배 상자(Buffer) 만들기 및 데이터 전송
        particleBuffer = new ComputeBuffer(numParticles, SIZE_PARTICLE);
        particleBuffer.SetData(particleArray);
        vertexBuffer = new ComputeBuffer(numVertices, SIZE_VERTEX);
        vertexBuffer.SetData(vertexArray);


        // 5. 일꾼(Compute Shader)에게 장비 지급
        // bind the compute buffers to the shader and the compute shader
        shader.SetBuffer(kernelID, "particleBuffer", particleBuffer);
        shader.SetBuffer(kernelID, "vertexBuffer", vertexBuffer);
        shader.SetFloat("halfSize", quadSize*0.5f);
        
        // 6. 붓(Material)에게 물감통 연결
        // 렌더링할 때(그릴 때) 위치 정보가 담긴 vertexBuffer를 쓰라고 알려줍니다.
        material.SetBuffer("vertexBuffer", vertexBuffer);
    }


    // [화면 그리기 단계]
    // 유니티가 렌더링할 때 호출하는 함수입니다.
    void OnRenderObject()
    {
        // 1. 붓 설정 (재질 활성화)
        material.SetPass(0);


        // 2. 그리기 명령 (DrawProceduralNow)
        // MeshTopology.Triangles: "점 3개씩 묶어서 삼각형으로 그려라"
        // 6: "점 6개를 한 세트로 봐라" (이 부분이 조금 헷갈릴 수 있는 부분, 뒤에서 설명)
        // numParticles: "그 세트를 총 numParticles 개수만큼 그려라"
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, numParticles);
    }


    // [뒷정리]
    void OnDestroy()
    {

        // 게임이 끝나면 트럭(버퍼)을 폐기처분해야 메모리가 안 샙니다.
        if (particleBuffer != null)
            particleBuffer.Release();
        if (vertexBuffer != null)
            vertexBuffer.Release();
    }

    // Update is called once per frame
    // [매 프레임 업데이트]
    void Update()
    {
        float[] mousePosition2D = { cursorPos.x, cursorPos.y };

        // Send datas to the compute shader
        // 1. 일꾼에게 현재 시간과 마우스 위치 알려주기
        shader.SetFloat("deltaTime", Time.deltaTime);
        shader.SetFloats("mousePosition", mousePosition2D);

        // Update the Particles
        // 2. "작업 시작!" 명령 내리기 (Dispatch)
        // groupSizeX 만큼의 작업반을 투입합니다.
        shader.Dispatch(kernelID, groupSizeX, 1, 1);
    }


    // [마우스 위치 계산 - GUI용]
    void OnGUI()
    {

        // 마우스 위치를 월드 좌표로 변환하는 구식 방법입니다.
        // 크게 중요하지 않으니 패스!
        Vector3 p = new Vector3();
        Camera c = Camera.main;
        Event e = Event.current;
        Vector2 mousePos = new Vector2();

        // Get the mouse position from Event.
        // Note that the y position from Event is inverted.
        mousePos.x = e.mousePosition.x;
        mousePos.y = c.pixelHeight - e.mousePosition.y;

        p = c.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, c.nearClipPlane + 14));

        cursorPos.x = p.x;
        cursorPos.y = p.y;
        
    }
}

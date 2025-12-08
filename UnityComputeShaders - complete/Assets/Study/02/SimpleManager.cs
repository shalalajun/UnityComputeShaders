using UnityEngine;

public class SimpleManager : MonoBehaviour
{
    public ComputeShader shader;
    public Material material; // 점을 그릴 재질
    
    // [간소화 1] 데이터 1개 = float 3개 (x,y,z) = 12 byte
    ComputeBuffer positionBuffer;
    const int COUNT = 1000; // 입자 1000개만 쓰자

    void Start()
    {
        // 1. 데이터 담을 통(Buffer) 만들기
        positionBuffer = new ComputeBuffer(COUNT, 12); // 개수, 크기(stride)

        // 2. 초기 데이터 넣기 (처음엔 다 0,0,0에 있으면 재미없으니 x만 펼쳐놓자)
        Vector3[] initialPositions = new Vector3[COUNT];
        for (int i = 0; i < COUNT; i++)
        {
            // x는 -5 ~ 5 사이로 퍼뜨리고, y는 바닥(-5)에 둠
            float xPos = Mathf.Lerp(-5f, 5f, (float)i / COUNT);
            initialPositions[i] = new Vector3(xPos, -5f, 0);
        }
        positionBuffer.SetData(initialPositions);

        // 3. 쉐이더랑 재질에 버퍼 연결해주기 (장비 지급)
        int kernel = shader.FindKernel("CSMain");
        shader.SetBuffer(kernel, "positionBuffer", positionBuffer);
        material.SetBuffer("positionBuffer", positionBuffer);
    }

    void Update()
    {
        // 1. 시간 정보 알려주기
        shader.SetFloat("deltaTime", Time.deltaTime);

        // 2. 일꾼 투입! (1000개 / 64명 = 그룹 수 계산)
        int groupCount = Mathf.CeilToInt(COUNT / 64f);
        shader.Dispatch(0, groupCount, 1, 1);
    }

    void OnRenderObject()
    {
        // 1. 재질 세팅
        material.SetPass(0);

        // 2. 그리기! (점 1000개 그려라)
        // Topology.Points: 점으로 그려라 (삼각형 조립 X)
        Graphics.DrawProceduralNow(MeshTopology.Points, COUNT);
    }

    void OnDestroy()
    {
        // 뒷정리 (필수)
        if (positionBuffer != null) positionBuffer.Release();
    }
}
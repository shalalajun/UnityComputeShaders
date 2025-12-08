using UnityEngine;
using System.Collections;

public class AssignTextures : MonoBehaviour
{
    // 1. 작업 지시서 (우리가 짠 .compute 파일)
    public ComputeShader shader;

    // 2. 도화지 크기 (가로세로 256픽셀)
    public int texResolution = 256;

    // 3. 화면에 보여줄 액자 (큐브나 쿼드 같은 물체)
    Renderer rend;

    // 4. ★ 특수 도화지 (그림이 그려질 텍스처)
    RenderTexture outputTexture;

    // 5. 작업 지시서의 몇 페이지를 펼칠지 (커널 번호)유니티가 쉐이더 함수들을 **배열(0번, 1번, 2번...)**로 관리하기 때문이고
    int kernelHandle;

    public string kenelName = "solidYellow";


    // Use this for initialization
    void Start()
    {
        outputTexture = new RenderTexture(texResolution, texResolution, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();

        //"내 발 밑(이 스크립트가 붙은 GameObject)을 뒤져서, Renderer라는 이름표를 단 부품을 찾아내! 그리고 그걸 이제부터 **rend**라고 부르겠어."
        rend = GetComponent<Renderer>();

        //방금 찾아온 그 렌더러(rend)의 스위치를 켜라(ON)
        rend.enabled = true;

        InitShader();
    }

    private void InitShader()
    {
        // 1. 작업 팀(커널) 찾기
        // 쉐이더 파일 안에 있는 "CSMain"이라는 함수 번호를 알아옵니다.
        Debug.Log("커널 이름: " + kenelName);
        kernelHandle = shader.FindKernel(kenelName);
        Debug.Log("커널 핸들: " + kernelHandle);

        // 2. 일꾼에게 도화지 쥐여주기 (Compute Shader 연결)
        // "자, 커널(kernelHandle) 팀! 너희 쉐이더 코드 안에 'Result'라는 변수 있지?
        // 거기다가 방금 만든 'outputTexture'를 끼워 넣어라."

        shader.SetInt("texResolution", texResolution);

        shader.SetTexture(kernelHandle, "Result", outputTexture);


        // 3. 액자에도 도화지 끼우기 (Material 연결)
        // "화면(rend)에 보여질 재질(material)의 메인 텍스처(_MainTex) 자리에
        // 방금 그 'outputTexture'를 끼워 넣어라."
        // 👉 결과: 일꾼이 칠하면 -> 액자에 바로 보임 (같은 종이를 공유하니까!)
        rend.material.SetTexture("_MainTex", outputTexture);


        // 4. 작업 시작 명령! (Dispatch)
        // 도화지 크기(256)를 8로 나눠서 팀을 꾸립니다.
        DispatchShader(texResolution / 8, texResolution / 8);
    }

    private void DispatchShader(int x, int y)
    {

        // texResolution이 256이라면?
        // x = 256 / 8 = 32
        // y = 256 / 8 = 32
        
        // "가로로 32개 팀, 세로로 32개 팀 출동해라!"
        // (한 팀이 8x8칸을 칠하니까, 32팀 x 8칸 = 256칸 딱 맞음)
        shader.Dispatch(kernelHandle, x, y, 1);
    }

    void Update()
    {
        // 'U' 키를 누르면 다시 그리기
        if (Input.GetKeyUp(KeyCode.U))
        {
            DispatchShader(texResolution / 8, texResolution / 8);
        }
    }
}


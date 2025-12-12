using UnityEngine;

public class GPUGraph : MonoBehaviour
{
 
    [SerializeField]
    ComputeShader computeShader;

    [SerializeField]
    Material material;

    [SerializeField]
    Mesh mesh;

    [SerializeField, Range(10, 1000)]
    int resolution = 10;

    [SerializeField]
    FunctionLibrary.FunctionName function;

    static readonly int 
    positionsId = Shader.PropertyToID("_Positions"),
    resolutionId = Shader.PropertyToID("_Resolution"),
    stepId = Shader.PropertyToID("_Step"),
    timeId = Shader.PropertyToID("_Time");

 
    // [추가 1] 모드 선택용 열거형(Enum) 정의
    public enum TransitionMode { Cycle, Random }

    // [추가 2] 인스펙터에서 모드를 선택할 변수
    [SerializeField]
    TransitionMode transitionMode;

    [SerializeField, Min(0f)]
    float functionDuration = 1f;

    [SerializeField, Min(0f)]
    float transitionDuration = 1f;

    float duration;
    bool transitioning;
    FunctionLibrary.FunctionName transitionFunction;



    ComputeBuffer positionsBuffer;


    void UpdateFunctionOnGPU () {
        float step = 2f / resolution;
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);
        computeShader.SetBuffer(0, positionsId, positionsBuffer);
        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(0, groups, groups, 1);
        material.SetBuffer(positionsId, positionsBuffer);
        material.SetFloat(stepId, step);
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, positionsBuffer.count);
    }

    
    // Awake 대신 OnEnable을 씁니다.
    void OnEnable()
    {
        positionsBuffer = new ComputeBuffer(resolution * resolution, 3 * 4);

    }

    void OnDisable () {
        positionsBuffer.Release();
        positionsBuffer = null;
    }


    void Update () {

       duration += Time.deltaTime;
        if (transitioning) {
            if (duration >= transitionDuration) {
                duration -= transitionDuration;
                transitioning = false;
            }
        }
        else if (duration >= functionDuration) {
            duration -= functionDuration;
            transitioning = true;
            transitionFunction = function;
            PickNextFunction();
        }

        UpdateFunctionOnGPU();

    }

    void PickNextFunction () {
        // 사이클 모드면 Next, 아니면 Random 함수 호출
        function = transitionMode == TransitionMode.Cycle ?
            FunctionLibrary.GetNextFunctionName(function) :
            FunctionLibrary.GetRandomFunctionNameOtherThan(function);
    }
}

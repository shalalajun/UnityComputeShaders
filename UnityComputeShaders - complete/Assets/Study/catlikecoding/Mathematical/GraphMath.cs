using UnityEngine;

public class GraphMath : MonoBehaviour
{
    [SerializeField]
	Transform pointPrefab;
    

    [SerializeField, Range(10, 100)]
    int resolution = 10;

    [SerializeField]
    FunctionLibrary.FunctionName function;


    // [추가 1] 모드 선택용 열거형(Enum) 정의
    public enum TransitionMode { Cycle, Random }

    // [추가 2] 인스펙터에서 모드를 선택할 변수
    [SerializeField]
    TransitionMode transitionMode;


    // [추가 1] 함수가 유지될 시간 (기본 1초)
    [SerializeField, Min(0f)]
    float functionDuration = 1f;


    // [추가 1] 변신에 걸리는 시간 (기본 1초)
    [SerializeField, Min(0f)]
    float transitionDuration = 1f;

    // [추가 2] 현재 변신 중인가요?
    bool transitioning;

    // [추가 3] 변신 전 함수 이름 (어디로부터 변신하는지)
    FunctionLibrary.FunctionName transitionFunction;

    // [추가 2] 흐른 시간을 기록할 변수
    float duration;

    Transform[] points;

    void Awake()
    {
        float step = 2f / resolution;
        var scale = Vector3.one * step;

        points = new Transform[resolution * resolution];

        for (int i = 0; i < points.Length; i++) {
            Transform point = points[i] = Instantiate(pointPrefab);
            
            // 위치 설정 코드 삭제됨 (Update에서 함)
            
            point.localScale = scale;
            point.SetParent(transform, false);
        }
    }

   void Update () {
        duration += Time.deltaTime;

        // [상태 1] 변신 중이라면?
        if (transitioning) {
            if (duration >= transitionDuration) {
                duration -= transitionDuration;
                transitioning = false; // 변신 끝!
            }
        }
        // [상태 2] 일반 상태라면?
        else if (duration >= functionDuration) {
            duration -= functionDuration;
            transitioning = true; // 변신 시작!
            transitionFunction = function; // 현재 함수를 '이전 함수'로 기록
            PickNextFunction(); // 다음 함수 고르기
        }

        // 상태에 따라 다른 그리기 함수 호출
        if (transitioning) {
            UpdateFunctionTransition();
        }
        else {
            UpdateFunction();
        }
    }

    void PickNextFunction () {
        // 사이클 모드면 Next, 아니면 Random 함수 호출
        function = transitionMode == TransitionMode.Cycle ?
            FunctionLibrary.GetNextFunctionName(function) :
            FunctionLibrary.GetRandomFunctionNameOtherThan(function);
    }

   void UpdateFunction () {
        FunctionLibrary.Function f = FunctionLibrary.GetFunction(function);
        float time = Time.time;
        float step = 2f / resolution;
        float v = 0.5f * step - 1f;

        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++) {
            // [복구된 내용 1] 한 줄(Row)이 다 찼으면 다음 줄로 넘김
            if (x == resolution) {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }

            // [복구된 내용 2] u(가로) 좌표 계산
            float u = (x + 0.5f) * step - 1f;

            // [복구된 내용 3] 함수로 계산된 3D 위치를 점에 적용
            points[i].localPosition = f(u, v, time);
        }
    }

    void UpdateFunctionTransition () {
        // 1. 시작 함수(from)와 목표 함수(to)를 가져옵니다.
        FunctionLibrary.Function
            from = FunctionLibrary.GetFunction(transitionFunction),
            to = FunctionLibrary.GetFunction(function);
        
        // 2. 진행률(progress) 계산: 현재 시간 / 전체 전환 시간
        float progress = duration / transitionDuration;
        
        float time = Time.time;
        float step = 2f / resolution;
        float v = 0.5f * step - 1f;

        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++) {
            if (x == resolution) {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }
            float u = (x + 0.5f) * step - 1f;

            // 3. Morph 함수를 호출하여 섞인 위치를 적용합니다.
            points[i].localPosition = FunctionLibrary.Morph(
                u, v, time, from, to, progress
            );
        }
    }
}

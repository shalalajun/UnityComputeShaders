using UnityEngine;

public class GraphMath : MonoBehaviour
{
    [SerializeField]
	Transform pointPrefab;
    

    [SerializeField, Range(10, 100)]
    int resolution = 10;

    [SerializeField]
    FunctionLibrary.FunctionName function;

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
    FunctionLibrary.Function f = FunctionLibrary.GetFunction(function);
    float time = Time.time;
    float step = 2f / resolution;

    // v 변수를 루프 밖에서 미리 선언
    float v = 0.5f * step - 1f;

    for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++) {
            if (x == resolution) {
                x = 0;
                z += 1;
                // 줄(행)이 바뀔 때만 v를 갱신 (최적화)
                v = (z + 0.5f) * step - 1f;
            }
            float u = (x + 0.5f) * step - 1f;
            
            // 함수가 Vector3를 반환하므로 localPosition에 통째로 대입 가능!
            points[i].localPosition = f(u, v, time);
        }
        
    }
}

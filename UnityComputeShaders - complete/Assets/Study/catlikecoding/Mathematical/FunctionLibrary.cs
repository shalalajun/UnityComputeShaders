using UnityEngine;
using static UnityEngine.Mathf;

// : MonoBehaviour 가 없습니다!
// 이 클래스는 단순히 수학 공식들을 모아놓은 '도구상자' 역할을 할 것이기 때문입니다.
public static class FunctionLibrary
{

    public delegate Vector3 Function(float u, float v, float t);

    // 메뉴에 표시될 이름들
    public enum FunctionName { Wave, MultiWave, Ripple, Sphere, Torus }

    // 함수들을 순서대로 담은 배열 생성
    static Function[] functions = { Wave, MultiWave, Ripple, Sphere, Torus };

    public static Function GetFunction (FunctionName name) {
        // if문 없이 배열 인덱스로 바로 찾아서 반환
        return functions[(int)name];
    }

    public static Vector3 Sphere (float u, float v, float t) {
        // 1. 반지름(r)을 요동치게 만듭니다. (비틀린 패턴)
        float r = 0.9f + 0.1f * Sin(PI * (6f * u + 4f * v + t));
        
        // 2. 구의 높이 비율(s)을 계산합니다.
        float s = r * Cos(0.5f * PI * v);
        
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r * Sin(0.5f * PI * v);
        p.z = s * Cos(PI * u);
        
        return p;
    }

    public static Vector3 Torus (float u, float v, float t) {
        // r1: 도넛 링 자체가 7각형 별 모양처럼 꿈틀거림
        float r1 = 0.7f + 0.1f * Sin(PI * (6f * u + 0.5f * t));
        
        // r2: 튜브 표면이 꽈배기처럼 꼬임
        float r2 = 0.15f + 0.05f * Sin(PI * (8f * u + 4f * v + 2f * t));
        
        float s = r1 + r2 * Cos(PI * v);
        
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r2 * Sin(PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }


    public static Vector3 Wave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (u + v + t));
        p.z = v;
        return p;
    }

    public static Vector3 MultiWave (float u, float v, float t) {

        Vector3 p;
        p.x = u;
		// 큰 파도: 시간(t)에 0.5를 곱해서 속도를 절반으로 늦춤 -> 천천히 움직임
		p.y = Sin(PI * (u + 0.5f * t));
		
		// 작은 파도: 원래 속도(t) 유지 -> 빠르게 움직이며 큰 파도 위를 훑고 지나감
		p.y += 0.5f * Sin(2f * PI * (v + t));

        // 세 번째 파도 추가 (대각선 방향, 시간은 0.25배로 아주 느리게)
        p.y += Sin(PI * (u + v + 0.25f * t));
		
		// 합계(2.5)로 나누어 정규화
        p.y *= 1f / 2.5f;

        p.z = v;
        
        return p;
	}

    public static Vector3 Ripple(float u, float v, float t)
    {
        // Abs(x) 대신 피타고라스 정리로 거리 d 계산
        float d = Sqrt(u * u + v * v);

        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (4f * d - t));
        p.y /= 1f + 10f * d;
        p.z = v;
        return p;
    }
}



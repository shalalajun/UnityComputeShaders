using UnityEngine;
using System.Collections.Generic;

public class Vehicle : MonoBehaviour
{
    [Header("Settings")]
    public float maxSpeed;
    public float maxForce;

    [Header("Weights")]
    [Range(0f, 5f)] public float separationWeight = 4.0f; // 분리하려는 힘의 중요도
    [Range(0f, 5f)] public float seekWeight = 0.5f;       // 타겟을 쫓는 힘의 중요도

    [Header("References")]
    public Transform target;        // 쫓아갈 목표
    public List<Vehicle> flockList; // 전체 친구들 명단 (Spawner가 넣어줌)
    
    // 나중을 위해 남겨둔 변수
    // public FlowField flowField; 

    private Vector3 position;
    private Vector3 velocity;
    private Vector3 acceleration;

    void Start()
    {
        position = transform.position;
        velocity = new Vector3(0, 0, 2); // 초기 속도
        acceleration = Vector3.zero;

        // 개성 부여 (랜덤성)
        maxSpeed = Random.Range(8.0f, 12.0f);
        maxForce = Random.Range(5.0f, 10.0f); // 힘이 너무 세면 튕겨 나갈 수 있으니 적당히 조절
    }

    void Update()
    {
        // ------------------------------------------------------------------
        // 1. 힘 계산 (Calculate)
        // ------------------------------------------------------------------
        
        Vector3 sepForce = Vector3.zero;
        Vector3 seekForce = Vector3.zero;

        // 분리 (Separation) 힘 계산
        if (flockList != null)
        {
            sepForce = Separate(flockList);
        }

        // 추적 (Seek) 힘 계산
        if (target != null)
        {
            seekForce = Seek(target.position);
        }

        // ------------------------------------------------------------------
        // 2. 가중치 적용 (Weighting)
        // ------------------------------------------------------------------
        
        // "친구랑 부딪히는 게 타겟 쫓는 것보다 2배 더 싫어!"
        sepForce *= separationWeight;
        seekForce *= seekWeight;

        // ------------------------------------------------------------------
        // 3. 힘 적용 (Apply)
        // ------------------------------------------------------------------
        
        applyForce(sepForce);
        applyForce(seekForce);

        // 4. 물리 업데이트
        UpdateForce();
    }

    void applyForce(Vector3 force)
    {
        acceleration += force;
    }

    void UpdateForce()
    {
        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        position += velocity * Time.deltaTime;
        transform.position = position;

        // 이동 방향 바라보기
        if (velocity != Vector3.zero)
        {
            // 부드러운 회전을 위해 Slerp 사용 (선택 사항)
            Quaternion lookRotation = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        acceleration = Vector3.zero;
    }

    // ======================================================================
    //  힘 계산 함수들 (이제 Vector3를 반환합니다!)
    // ======================================================================

    // 분리 (Separation)
    Vector3 Separate(List<Vehicle> boids)
    {
        float desiredSeparation = 2.5f; // 이 거리 안으로 들어오면 밀어냄
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (Vehicle other in boids)
        {
            if (other == this || other == null) continue;

            float d = Vector3.Distance(transform.position, other.transform.position);

            if (d < desiredSeparation)
            {
                Vector3 diff = transform.position - other.transform.position;
                diff.Normalize();

                if (d > 0)
                    diff /= d; // 가까울수록 더 강하게

                sum += diff;
                count++;
            }
        }

        if (count > 0)
        {
            sum /= count;
            sum.Normalize();
            sum *= maxSpeed;

            Vector3 steer = sum - velocity;
            steer = Vector3.ClampMagnitude(steer, maxForce);
            return steer; // [변경] 힘을 적용하지 않고 리턴함
        }
        
        return Vector3.zero; // 아무도 없으면 힘 0
    }

    // 추적 (Seek)
    Vector3 Seek(Vector3 TargetPos)
    {
        Vector3 desired = TargetPos - transform.position;
        desired.Normalize();
        desired *= maxSpeed;

        Vector3 steer = desired - velocity;
        steer = Vector3.ClampMagnitude(steer, maxForce);

        return steer; // [변경] 힘을 적용하지 않고 리턴함
    }

    // --- (나중을 위해 남겨둔 코드: Arrive, Follow 등은 필요할 때 위처럼 Vector3 리턴형으로 바꾸면 됩니다) ---
    /*
    void arrive(Vector3 TargetPos) { ... }
    void follow(FlowField flow) { ... }
    */
}
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public Transform target;
    public float maxForce;
    public float maxSpeed;

    public FlowField flowField;

    private Vector3 position;
    private Vector3 velocity;
    private Vector3 acceleration; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        position = transform.position;
        velocity = new Vector3(0,0,2);
        acceleration = new Vector3(0,0,0);

        maxSpeed = Random.Range(8.0f, 12.0f);
        maxForce = Random.Range(2.0f, 8.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if(target != null){
            Seek(target.position);
        }

        UpdateForce();
    }

    void applyForce(Vector3 force){
        acceleration += force;
    }

    void UpdateForce(){

        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        position += velocity *Time.deltaTime;
        transform.position = position;

        if (velocity != Vector3.zero) {
            transform.rotation = Quaternion.LookRotation(velocity);
        }

        acceleration = Vector3.zero;
    }

    void Seek(Vector3 TargetPos){

        Vector3 desired = TargetPos - transform.position;
        desired = desired.normalized * maxSpeed;

        Vector3 steer = desired - velocity;

        steer = Vector3.ClampMagnitude(steer,maxForce);

        applyForce(steer);
    }

    void arrive(Vector3 TargetPos){

        Vector3 desired = TargetPos - transform.position;
    

        float d = desired.magnitude;

        desired = desired.normalized;

        float slowingRadius = 20.0f;

        // [추가] 멈춤 기준 거리 (0.1m 이내면 그냥 멈춤)
        float stopRadius = 0.08f; 

        if (d < stopRadius)
        {
            // 1. 위치를 목표에 강제로 맞춤 (깔끔하게)
            transform.position = TargetPos;
            // 2. 속도와 가속도를 0으로 죽임
            velocity = Vector3.zero;
            acceleration = Vector3.zero;
            return; // 여기서 함수 끝! 더 이상 힘 계산 안 함
        }

        if(d < slowingRadius)
        {
            float m = (d / slowingRadius) * maxSpeed;

            desired = desired.normalized * m;
        }
        else
        {
            // 멀리 있으면 그냥 최고 속도로 달림
            desired = desired.normalized * maxSpeed;
        }

        Vector3 steer = desired - velocity;

        steer = Vector3.ClampMagnitude(steer,maxForce);

        applyForce(steer);

    }

    void follow(FlowField flow){
    // 1. FlowField에게 내 위치를 주고 방향(Vector2)을 받아옵니다.
        // 받아온 건 2D지만 우리는 3D 공간을 쓰므로 Vector3로 형변환합니다.
        Vector3 desired = (Vector3)flow.Lookup(position);

        // 2. 그 방향으로 최고 속도로 달리고 싶음
        desired = desired.normalized * maxSpeed;

        // 3. 조향력 공식 (목표 속도 - 현재 속도)
        Vector3 steer = desired - velocity;

        // 4. 힘을 너무 세게 주지 못하게 제한
        steer = Vector3.ClampMagnitude(steer, maxForce);

        // 5. 힘 적용
        applyForce(steer);
            
    }

    void Borders(FlowField flow)
    {
        // [수정된 부분] 실제 FlowField의 크기를 가져와서 계산합니다.
        // 전체 너비 = 칸 개수(cols) * 한 칸의 크기(resolution)
        float width = flow.cols * flow.resolution;
        float height = flow.rows * flow.resolution;

        // 팩맨 로직: 오른쪽 끝으로 나가면(width보다 커지면), 왼쪽 끝(0)으로 보냄
        if (position.x < 0) position.x = width;
        if (position.y < 0) position.y = height;
        if (position.x > width) position.x = 0;
        if (position.y > height) position.y = 0;
    }

    public void Separate(List<Vehicle> boids)
    {
        float desiredSeparation = 2.0f;
        Vector3 sum = Vector3.zero;     // 도망갈 방향들의 합
        int count = 0;

    }

}

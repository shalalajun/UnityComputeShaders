using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public Transform target;
    public float maxForce = 0.2f;
    public float maxSpeed = 8.0f;

    private Vector3 position;
    private Vector3 velocity;
    private Vector3 acceleration; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        position = transform.position;
        velocity = new Vector3(0,0,2);
        acceleration = new Vector3(0,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        if(target != null){
            arrive(target.position);
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

    void follow(Vector3 flow){

        Vector3 desired = TargetPos - transform.position;
        desired = desired.normalized * maxSpeed;

        Vector3 steer = desired - velocity;

        steer = Vector3.ClampMagnitude(steer,maxForce);

        applyForce(steer);
    }
}

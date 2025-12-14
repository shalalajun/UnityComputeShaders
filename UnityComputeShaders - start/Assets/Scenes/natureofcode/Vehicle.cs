using UnityEngine;

public class Vehicle
{
    public GameObject model;
    public Transform transform;

    public Vector3 position;
    public Vector3 velocity;
    public Vector3 acceleration;

    float r;
    float maxspeed;
    float maxforce;

    public Vehicle(GameObject obj, float ms, float mf)
    {
        this.model = obj;
        this.transform = obj.transform;

        this.position = transform.position;
        this.velocity = Vector3.zero;
        this.acceleration = Vector3.zero;

        this.r = 6;
        this.maxspeed = ms;
        this.maxforce = mf;
    }

    public void applyforce(Vector3 force)
    {
        acceleration += force;
    }

    public void seek(Vector3 target)
    {
        Vector3 desired = target - position;

        desired = desired.normalized * maxspeed;

        Vector3 steer = desired - velocity;  

        steer = Vector3.ClampMagnitude(steer,maxforce);

        applyforce(steer);      
    }

    public void update()
    {
        velocity += acceleration;
        velocity = Vector3.ClampMagnitude(velocity, maxspeed); // 속도 제한
        position += velocity * Time.deltaTime; // 위치 이동
        acceleration = Vector3.zero; // 가속도 리셋

        // 2. 실제 유니티 오브젝트 이동
        transform.position = position;

        // 3. 회전 (진행 방향 바라보기)
        if (velocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(velocity);
        }
    }
}

using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    [Header("Settings")]
    public float maxSpeed = 8f;
    public float maxForce = 0.2f;

    [Header("Connection")]
    public GameObject myModel; // 여기에 큐브/스피어 연결
    public Transform target;   // 목표물 연결

    // 순수 로직 클래스인 Vehicle을 멤버로 가짐
    private Vehicle vehicle;

    void Start()
    {
        // Vehicle 객체 생성 (모델과 설정값 전달)
        if (myModel != null)
        {
            vehicle = new Vehicle(myModel, maxSpeed, maxForce);
        }
    }

    void Update()
    {
        if (vehicle != null && target != null)
        {
            // 1. 타겟 추적 계산 (Seek)
            vehicle.seek(target.position);

            // 2. 물리 업데이트 및 이동 적용 (Update)
            vehicle.update();
        }
    }
}

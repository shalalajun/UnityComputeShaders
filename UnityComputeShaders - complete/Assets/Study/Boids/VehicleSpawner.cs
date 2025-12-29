using UnityEngine;
using System.Collections.Generic;

public class VehicleSpawner : MonoBehaviour
{
    public GameObject vehiclePrefab; 
    
    // [추가됨 1] 인스펙터에서 빨간 공(목표물)을 여기에 넣어줘야 합니다.
    public Transform targetObj; 

    // public FlowField flowField; // 안 쓰니까 지워도 됩니다.

    [Range(10, 500)]
    public int spawnCount = 100;

    // 전체 비히클 명단
    public List<Vehicle> vehicles = new List<Vehicle>();

    void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnVehicle();
        }
    }

    void Update()
    {
    }

    void SpawnVehicle()
    {
        // 랜덤 위치 생성
        Vector3 randomPos = Random.insideUnitSphere * 50.0f;
        randomPos.z = 0; 

        SpawnVehicleAtPosition(randomPos);
    }

    void SpawnVehicleAtPosition(Vector3 spawnPos)
    {
        GameObject clone = Instantiate(vehiclePrefab, spawnPos, Quaternion.identity);
        Vehicle v = clone.GetComponent<Vehicle>();

        // [추가됨 2] 생성된 비히클에게 "이게 네 목표야"라고 알려줍니다.
        if (targetObj != null)
        {
            v.target = targetObj;
        }
        else
        {
            Debug.LogWarning("스포너에 Target Obj가 연결되지 않았습니다!");
        }

        // [중요] 비히클에게 전체 명단을 넘겨줍니다.
        v.flockList = this.vehicles; 

        vehicles.Add(v);
        clone.transform.parent = this.transform;
    }
}
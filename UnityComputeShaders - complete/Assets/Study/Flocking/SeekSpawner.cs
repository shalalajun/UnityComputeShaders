using UnityEngine;
using System.Collections.Generic;

public class SeekSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject vehiclePrefab; // 생성할 비히클 프리팹
    public Transform targetObj;      // 쫓아다닐 목표물 (빨간 공)

    [Range(10, 500)]
    public int spawnCount = 100;     // 생성할 마리 수
    public float spawnRadius = 50.0f; // 생성 범위 (반지름)

    // 전체 비히클 명단 (Separation 계산을 위해 필요)
    // 인스펙터에서 확인하기 편하게 public으로 둡니다.
    public List<Vehicle> vehicles = new List<Vehicle>();

    void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnVehicle();
        }
    }

    void SpawnVehicle()
    {
        // 1. 랜덤 위치 생성 (너무 뭉치지 않게 넓게)
        Vector3 randomPos = Random.insideUnitSphere * spawnRadius;
        randomPos.z = 0; // 2D 평면 느낌을 위해 Z축 고정

        SpawnVehicleAtPosition(randomPos);
    }

    void SpawnVehicleAtPosition(Vector3 spawnPos)
    {
        // 2. 프리팹 생성
        GameObject clone = Instantiate(vehiclePrefab, spawnPos, Quaternion.identity);
        Vehicle v = clone.GetComponent<Vehicle>();

        // [중요 1] 타겟 연결해주기 ("저걸 쫓아가!")
        if (targetObj != null)
        {
            v.target = targetObj;
        }
        else
        {
            Debug.LogWarning("SeekSpawner에 Target Obj가 연결되지 않았습니다! 인스펙터를 확인하세요.");
        }

        // [중요 2] 친구들 명단 넘겨주기 ("얘네랑 부딪히지 마!")
        v.flockList = this.vehicles; 

        // 3. 리스트에 등록하고 정리
        vehicles.Add(v);
        clone.transform.parent = this.transform;
    }
}
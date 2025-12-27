using UnityEngine;
using System.Collections.Generic;

public class VehicleSpawner : MonoBehaviour
{
    public GameObject vehiclePrefab; // 비히클 프리팹 (도장)
    public FlowField flowField;      // 플로우 필드 (길 정보)
    
    [Range(10, 1000)]
    public int spawnCount = 100;     // 처음에 몇 마리 생성할지 (인스펙터에서 조절 가능)

    private List<Vehicle> vehicles = new List<Vehicle>();

    void Start()
    {
        // 시작하자마자 spawnCount만큼 반복해서 생성
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnVehicle();
        }
    }

    // Update는 이제 필요 없습니다 (클릭 안 받을 거니까요)
    // void Update() { } 

    // 랜덤 위치에 생성하는 함수
    void SpawnVehicle()
    {
        // 1. 맵 전체 크기 계산 (랜덤 범위 잡기 위해)
        // FlowField가 초기화될 때까지 기다려야 할 수도 있으니 안전하게 계산
        float mapWidth = flowField.cols * flowField.resolution;
        float mapHeight = flowField.rows * flowField.resolution;

        // 2. 맵 안의 랜덤한 좌표 뽑기
        float x = Random.Range(0, mapWidth);
        float y = Random.Range(0, mapHeight);

        // 3. 생성!
        SpawnVehicleAtPosition(new Vector3(x, y, 0));
    }

    // 실제 생성을 담당하는 내부 함수
    void SpawnVehicleAtPosition(Vector3 spawnPos)
    {
        // 프리팹 복제
        GameObject clone = Instantiate(vehiclePrefab, spawnPos, Quaternion.identity);
        
        // Vehicle 스크립트 가져오기
        Vehicle v = clone.GetComponent<Vehicle>();

        // FlowField 정보 알려주기
        v.flowField = flowField;

        // 리스트에 추가 (관리용)
        vehicles.Add(v);
        
        // (선택 사항) 하이어라키 창이 지저분해지지 않게 Spawner 하위로 넣기
        clone.transform.parent = this.transform; 
    }
}
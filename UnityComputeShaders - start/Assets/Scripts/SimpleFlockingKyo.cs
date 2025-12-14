using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFlockingKyo : MonoBehaviour
{
   public struct Boid
    {
        public Vector3 position;
        public Vector3 direction;

        public Boid(Vector3 pos)
        {
            position.x = pos.x;
            position.y = pos.y;
            position.z = pos.z;

            direction.x = 0;
            direction.y = 0;
            direction.z = 0;
        }
    }

    public ComputeShader shader;

    public float rotationSpeed = 1f;
    public float boidSpeed = 1f;
    public float neighbourDistance = 1f;
    public float boidSpeedVariation = 1f;

    public GameObject boidPrefab;
    public int boidsCount;
    public float spawnRadius;
    public Transform target;

    // GPU와 통신할 때 쓸 ID
    int kernelHandle;

    // GPU 메모리 그릇
    ComputeBuffer boidsBuffer;

    Boid[] boidsArray;
    GameObject[] boids;

    int groupSizeX; // 쓰레드 그룹 개수
    int numOfBoids; // 실제 계산될 파티클 개수 (패딩 포함)

    void Start()
    {
        kernelHandle = shader.FindKernel("CSMain");

        uint x;
        shader.GetKernelThreadGroupSizes(kernelHandle, out x, out _, out _);
        groupSizeX = Mathf.CeilToInt((float)boidsCount/(float)x);
        numOfBoids = groupSizeX * (int)x;


        initBoids();  // 물고기들을 생성하고 (배열 만들기)
        initShader(); // 쉐이더 가방을 싸야 합니다 (버퍼 만들기)


    }

    private void initBoids()
    {
        boids = new GameObject[numOfBoids];
        boidsArray = new Boid[numOfBoids];

        for (int i=0; i<numOfBoids; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;

            boidsArray[i] = new Boid(pos);

            boids[i] = Instantiate(boidPrefab, pos, Quaternion.identity) as GameObject;

            boidsArray[i].direction = boids[i].transform.forward;
        }
    }

    void initShader()
    {
            // [1단계] GPU 전용 메모리(가방) 확보
        boidsBuffer = new ComputeBuffer(numOfBoids, 6 * sizeof(float));
        
        // [2단계] CPU에 있던 데이터를 가방에 담기
        boidsBuffer.SetData(boidsArray);

        // [3단계] 쉐이더와 연결하고 설정값 맞추기
        shader.SetBuffer(kernelHandle, "boidsBuffer", boidsBuffer);
        shader.SetFloat("rotationSpeed", rotationSpeed);
        shader.SetFloat("boidSpeed", boidSpeed);
        shader.SetFloat("boidSpeedVariation", boidSpeedVariation);
        shader.SetVector("flockPosition", target.transform.position);
        shader.SetFloat("neighbourDistance", neighbourDistance);
        shader.SetInt("boidsCount", boidsCount);
    }

    void Update()
    {
        shader.SetFloat("time", Time.time);
        shader.SetFloat("deltaTime", Time.deltaTime);

        shader.Dispatch(kernelHandle, groupSizeX, 1, 1);

        // ★ GPU: "계산 다 했어!" -> CPU: "그럼 데이터 내놔." cpu 가 gpu로 부터 데이터를 받는 부분
        boidsBuffer.GetData(boidsArray);

        for (int i = 0; i < boidsArray.Length; i++)
        {
            boids[i].transform.localPosition = boidsArray[i].position;

            if (!boidsArray[i].direction.Equals(Vector3.zero))
            {
                boids[i].transform.rotation = Quaternion.LookRotation(boidsArray[i].direction);
            }

        }

    }

     void OnDestroy()
    {
        if (boidsBuffer!=null)
        {
            boidsBuffer.Dispose();
        }
    }

}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstancedFlocking : MonoBehaviour
{
    public struct Boid
    {
        public Vector3 position;
        public Vector3 direction;
        public float noise_offset;

        public Boid(Vector3 pos, Vector3 dir, float offset)
        {
            position.x = pos.x;
            position.y = pos.y;
            position.z = pos.z;
            direction.x = dir.x;
            direction.y = dir.y;
            direction.z = dir.z;
            noise_offset = offset;
        }
    }

    public ComputeShader shader;

    public float rotationSpeed = 1f;
    public float boidSpeed = 1f;
    public float neighbourDistance = 1f;
    public float boidSpeedVariation = 1f;
    public int boidsCount;
    public float spawnRadius;
    public Transform target;


    int kernelHandle;
    ComputeBuffer boidsBuffer;
    Boid[] boidsArray;
    int groupSizeX;
    int numOfBoids;

    // ===== 새로 추가된 부분 ===== GPU 연산을 위해서 게임오브젝트 대신 사용
    public Mesh boidMesh;
    public Material boidMaterial;

    ComputeBuffer argsBuffer;
    uint[] args = new uint[5] { 0, 0, 0, 0, 0 }; // 일단 주문서같은거라 생각만 해두자

    Bounds bounds;

    // =======================================================

    void Start()
    {
        kernelHandle = shader.FindKernel("CSMain");

        uint x;
        shader.GetKernelThreadGroupSizes(kernelHandle, out x, out _, out _);
        groupSizeX = Mathf.CeilToInt((float)boidsCount / (float)x);
        numOfBoids = groupSizeX * (int)x;

        bounds = new Bounds(Vector3.zero, Vector3.one * 1000);

        InitBoids();
        InitShader();
    }

    private void InitBoids()
    {
        boidsArray = new Boid[numOfBoids];

        for (int i = 0; i < numOfBoids; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            /***
            Lerp = Linear Interpolation (선형 보간)
            Slerp = Spherical Linear Interpolation (구면 선형 보간) 
            transform.rotation = 이 오브젝트의 현재 방향 (기준 방향)
            Random.rotation = 완전 랜덤 방향
            0.3f = 30% 지점
            기준 방향에서 랜덤 방향 쪽으로 30%만 간 회전
            기준 방향         랜덤 방향
            ●───────────────●
                ↑
            여기! (30% 지점)
            모든 보이드가 "대충 비슷한 방향"이지만 "조금씩 다름"
            ***/
            Quaternion rot = Quaternion.Slerp(transform.rotation, Random.rotation, 0.3f);
            float offset = Random.value * 1000.0f;
            boidsArray[i] = new Boid(pos, rot.eulerAngles, offset);
        }
    }

    void InitShader()
    {
        boidsBuffer = new ComputeBuffer(numOfBoids, 7 * sizeof(float));
        boidsBuffer.SetData(boidsArray);

        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        if (boidMesh != null)
        {
            args[0] = (uint)boidMesh.GetIndexCount(0);
            args[1] = (uint)numOfBoids;
        }
        argsBuffer.SetData(args);

        shader.SetBuffer(this.kernelHandle, "boidsBuffer", boidsBuffer);
        shader.SetFloat("rotationSpeed", rotationSpeed);
        shader.SetFloat("boidSpeed", boidSpeed);
        shader.SetFloat("boidSpeedVariation", boidSpeedVariation);
        shader.SetVector("flockPosition", target.transform.position);
        shader.SetFloat("neighbourDistance", neighbourDistance);
        shader.SetInt("boidsCount", numOfBoids);

        /***
        GPU가 계산한 위치를 누가 읽어서 그리나요?
        → 렌더링 셰이더 (Material 안의 셰이더)가 직접 읽음!
        ```
        ```
        ┌─────────────────┐
        │ Compute Shader  │ ← 위치 계산
        └────────┬────────┘
                │
                ▼
        [boidsBuffer]  ← GPU 메모리에 위치 데이터
                │
                ▼
        ┌─────────────────┐
        │ Instanced Shader│ ← 위치 읽어서 그리기
        │ (in Material)   │
        └─────────────────┘
        ***/

        boidMaterial.SetBuffer("boidsBuffer", boidsBuffer);
    }

    void Update()
    {
        shader.SetFloat("time", Time.time);
        shader.SetFloat("deltaTime", Time.deltaTime);

        shader.Dispatch(this.kernelHandle, groupSizeX, 1, 1);

        Graphics.DrawMeshInstancedIndirect(boidMesh, 0, boidMaterial, bounds, argsBuffer);
    }

    void OnDestroy()
    {
        if (boidsBuffer != null)
        {
            boidsBuffer.Dispose();
        }

        if (argsBuffer != null)
        {
            argsBuffer.Dispose();
        }
    }
}


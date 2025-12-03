Shader "Custom/ParticleRenderURP"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // ★핵심: 인스턴싱과 절차적 드로잉 설정
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup

            // URP 핵심 라이브러리 포함
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Particle
            {
                float3 position;
                float3 color;
            };

            // 읽기 전용 버퍼
            StructuredBuffer<Particle> _ParticleBuffer;

            struct Attributes
            {
                float4 positionOS : POSITION;
                uint instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
            };

            void setup()
            {
                // 여기선 비워둬도 됩니다.
                // 위치 계산을 vertex 쉐이더 안에서 직접 하니까요.
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                // 1. 버퍼에서 내 번호(instanceID) 데이터 꺼내기
                float3 dataPos = _ParticleBuffer[input.instanceID].position;
                float3 dataCol = _ParticleBuffer[input.instanceID].color;

                // 2. 오브젝트 공간(Object Space) 위치 계산
                // 원래 큐브 위치(input.positionOS)에 파티클 위치(dataPos) 더하기
                // * 0.1은 큐브 크기를 작게 줄이는 역할
                float3 worldPos = dataPos + input.positionOS.xyz * 0.1;

                // 3. 월드 좌표를 화면 좌표(Clip Space)로 변환 (URP 전용 함수)
                // TransformObjectToHClip 대신 직접 월드 좌표를 넣어서 변환
                output.positionCS = TransformWorldToHClip(worldPos);
                
                output.color = float4(dataCol, 1.0);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                return input.color;
            }
            ENDHLSL
        }
    }
}
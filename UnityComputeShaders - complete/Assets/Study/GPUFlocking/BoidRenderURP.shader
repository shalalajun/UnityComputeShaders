Shader "Custom/BoidRenderURP"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        // 3D 모델(Mesh)의 크기를 조절할 변수 추가
        _Scale ("Scale", Vector) = (1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
       

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _Color;
            float3 _Scale;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            struct GPUBoidData
            {
                float3 position;
                float3 velocity;
            };

            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                StructuredBuffer<GPUBoidData> computeFlockingBuffer;
            #endif

           struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // [핵심 함수] setup: GPU가 물체를 그리기 직전에 위치를 강제로 잡아주는 곳
            void setup()
            {
                #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                    // 1. 내 ID(순서) 알아내기
                    GPUBoidData boid = computeFlockingBuffer[unity_InstanceID];

                    // 2. 위치, 회전, 크기 행렬 만들기
                    float3 pos = boid.position;
                    float3 scale = _Scale;

                    // 변환 행렬 초기화 (0으로 채움)
                    unity_ObjectToWorld = 0;
                    
                    // 크기(Scale) 적용 (대각선 행렬)
                    unity_ObjectToWorld._11_22_33_44 = float4(scale.x, scale.y, scale.z, 1.0);

                    // 위치(Position) 적용 (4번째 열)
                    unity_ObjectToWorld._14_24_34 = pos;
                    
                    // (회전은 일단 무시하고 정면만 보게 둠 - 나중에 추가 가능)
                #endif
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                // 이미 setup()에서 unity_ObjectToWorld가 설정되었으므로
                // TransformObjectToHClip 함수가 알아서 잘 변환해줍니다.
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                
                return output;
            }

            // 프래그먼트 쉐이더 (색칠하기)
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return _Color;
            }
            ENDHLSL
          
        }
    }
}

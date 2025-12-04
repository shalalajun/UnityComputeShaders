Shader "Custom/ParticleRenderStandard"
{
    SubShader
    {
        // 기본 렌더링 설정
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM // HLSLPROGRAM 대신 CGPROGRAM을 씁니다
            #pragma vertex vert
            #pragma fragment frag
            
            // ★핵심: 인스턴싱과 절차적 드로잉 설정
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup

            #include "UnityCG.cginc" // URP 라이브러리 대신 이걸 씁니다

            struct Particle
            {
                float4 position;
                float4 color;
            };

            // 읽기 전용 버퍼
            StructuredBuffer<Particle> _ParticleBuffer;

            struct appdata
            {
                float4 vertex : POSITION;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 col : COLOR;
            };

            void setup()
            {
                // 위치 계산을 vertex 안에서 하므로 비워둠
            }

            v2f vert (appdata v)
            {
                v2f o;

                // 1. 내 번호(instanceID)에 맞는 데이터를 꺼냅니다.
                // (appdata에 SV_InstanceID를 선언해야 UNITY_GET_INSTANCE_ID로 가져올 수 있음)
                // 하지만 여기선 v.instanceID를 직접 씁니다.
                float3 dataPos = _ParticleBuffer[v.instanceID].position;
                float3 dataCol = _ParticleBuffer[v.instanceID].color;

                // 2. 오브젝트 공간 위치 계산
                // 원래 큐브 위치(v.vertex) + 파티클 위치(dataPos)
                float3 worldPos = dataPos + v.vertex.xyz * 1.0; // 크기 1.0

                // 3. 화면 좌표 변환 (Built-in 전용 함수)
                // mul(UNITY_MATRIX_VP, float4(worldPos, 1.0)) 과 같습니다.
                o.pos = UnityObjectToClipPos(float4(worldPos, 1.0));
                
                o.col = float4(dataCol, 1.0);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return i.col;
            }
            ENDCG
        }
    }
}
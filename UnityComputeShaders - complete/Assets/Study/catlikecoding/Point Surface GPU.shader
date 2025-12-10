Shader "Graph/Point Surface GPU"
{
     Properties {
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
    }

    SubShader {
        CGPROGRAM
        #pragma surface ConfigureSurface Standard fullforwardshadows addshadow
        #pragma instancing_options procedural:ConfigureProcedural
        #pragma target 4.5

        struct Input {
            float3 worldPos;
        };

        float _Smoothness;

        // 전처리기 안에 버퍼 선언
        #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
            StructuredBuffer<float3> _Positions;
        #endif

        void ConfigureProcedural(){
            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
                // 여기에 위치 가져오는 코드가 들어갈 예정
                float3 position = _Positions[unity_InstanceID];
            #endif
        }

        void ConfigureSurface (Input input, inout SurfaceOutputStandard surface) {
            surface.Albedo.rg = input.worldPos.xy * 0.5 + 0.5;
            surface.Smoothness = _Smoothness;
        }

        ENDCG
    }
    FallBack "Diffuse"
}

Shader "Graph / Point Surface GPU"
{
    Properties {
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
    }

    SubShader {
        CGPROGRAM
        #pragma surface ConfigureSurface Standard fullforwardshadows addshadow
        // [수정 1] 중복된 줄 삭제하고 아래 옵션 하나만 남김
        #pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
        #pragma target 4.5

        struct Input {
            float3 worldPos;
        };

        float _Smoothness;
        
        // [수정 2] _Step 변수 선언 추가 (이게 없으면 컴파일 에러!)
        float _Step;

        #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
            StructuredBuffer<float3> _Positions;
        #endif

        void ConfigureProcedural(){
            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
                float3 position = _Positions[unity_InstanceID];
                
                // [수정 3] 행렬 초기화 코드 추가 (이게 있어야 깔끔하게 변환됨)
                unity_ObjectToWorld = 0.0;
                unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0);
                unity_ObjectToWorld._m00_m11_m22 = _Step;
            #endif
        }

        void ConfigureSurface (Input input, inout SurfaceOutputStandard surface) {
            surface.Albedo.rg = saturate(input.worldPos.xy * 0.5 + 0.5);
            surface.Smoothness = _Smoothness;
        }

        ENDCG
    }
    FallBack "Diffuse"
}
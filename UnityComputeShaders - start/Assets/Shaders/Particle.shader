Shader "Custom/Particle" {
    Properties {         
        _PointSize("Point size", Float) = 5.0     
    }  
    SubShader {
        Pass {
            Tags{ "RenderType" = "Opaque" }
            LOD 200
            
            // [수정] 점이 겹칠 때 예쁘게 보이도록
            Blend SrcAlpha One 
            ZWrite Off 

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #include "UnityCG.cginc"

            uniform float _PointSize;

            struct Particle{
                float3 position;
                float3 velocity;
                float life;
            };

            StructuredBuffer<Particle> particleBuffer;
        
            struct v2f{
                float4 position : SV_POSITION;
                float4 color : COLOR;
                float size: PSIZE;
            };
        
            // [중요] Metal에서는 uint로 타입을 명확히!
            v2f vert(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
            {
                v2f o = (v2f)0;

                // C#에서 (Points, 1, count)로 호출했으므로 
                // instance_id가 0 ~ 999999 입니다.
                // vertex_id는 항상 0입니다.
                Particle p = particleBuffer[instance_id];

                o.position = UnityObjectToClipPos(float4(p.position, 1));
                o.color = float4(1, 0.5, 0.2, 1); // 잘 보이는 주황색
                o.size = _PointSize;

                return o;
            }

            float4 frag(v2f i) : COLOR
            {
                return i.color;
            }
            ENDCG
        }
    }
    FallBack Off
}
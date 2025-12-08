Shader "Custom/SimpleParticle"
{
    Properties     
    {
        _Color("Main Color", Color) = (1, 1, 0, 1)
        _PointSize("Point Size", Float) = 5.0
    }  

    SubShader 
    {
        // [수정] Tags는 보통 여기서 선언하는 것이 안전합니다.
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }

        Pass 
        {
            Blend SrcAlpha One
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            // DX11 등 최신 그래픽스 API 사용
            #pragma target 5.0

            // C#에서 보낸 데이터 (위치값 x,y,z)
            StructuredBuffer<float3> positionBuffer;
            
            fixed4 _Color;
            float _PointSize;

            struct v2f{
                float4 position : SV_POSITION;
                float4 color : COLOR;
                float size : PSIZE; // 점 크기
            };

            v2f vert(uint id : SV_VertexID)
            {
                v2f o;

                // 버퍼에서 위치 가져오기
                float3 worldPos = positionBuffer[id];

                // 월드 좌표 -> 화면 좌표 변환
                o.position = UnityWorldToClipPos(float4(worldPos, 1.0f));

                // 색상 (높이에 따라 밝기 변화)
                float brightness = (worldPos.y + 5.0) * 0.1f; 
                o.color = _Color * brightness; 
                
                // 점 크기 설정
                o.size = _PointSize;

                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                return i.color;
            }

            ENDCG
        }
    }
    FallBack Off
}
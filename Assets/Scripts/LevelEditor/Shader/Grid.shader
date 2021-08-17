Shader "Unlit/Grid"
{
    Properties
    {
        _LineWidth ("Line Width", Float) = 1
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            float _LineWidth;
            float3 _GridSize;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul (unity_ObjectToWorld, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 pos = frac(i.worldPos.xyz/_GridSize);
                pos = saturate((_LineWidth-pos)*10);
                
                float l = saturate(pos.x + pos.y);
                
                if(l <= 0.1)
                    discard;
                
                fixed4 col = float4(l,l,l, 1);
                
                return col*_Color;
            }
            ENDCG
        }
    }
}

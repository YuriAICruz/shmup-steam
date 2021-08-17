Shader "Unlit/Roomba"
{
    Properties
    {
        _Alpha ("Alpha", Range(0, 1)) = 0.4
        
        _LineColor ("Line Color", Color) = (1,1,1,1)
		_Border ("Border", Float) = 1
		_Scale ("Scale", Float) = 1
		
		
        // Colors
        [HDR]
        _Color ("Color", Color) = (1, 1, 1, 1)
        _HColor ("Highlight Color", Color) = (0.8, 0.8, 0.8, 1.0)
        _SColor ("Shadow Color", Color) = (0.2, 0.2, 0.2, 1.0)
        
        // texture
        _MainTex ("Main Texture", 2D) = "white" { }

        // ramp
        _RampThreshold ("Ramp Threshold", Range(0.1, 1)) = 0.5
        _RampSmooth ("Ramp Smooth", Range(0, 1)) = 0.1
        
        // specular
        _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
        _SpecSmooth ("Specular Smooth", Range(0, 1)) = 0.1
        
        [KeywordEnum(OFF, ON)] SPECULAR_DEF ("Specular", float) = 0
        _Shininess ("Shininess", Range(0.001, 10)) = 0.2
        
        // rim light
        _RimColor ("Rim Color", Color) = (0.8, 0.8, 0.8, 0.6)
        _RimThreshold ("Rim Threshold", Range(0, 1)) = 0.5
        _RimSmooth ("Rim Smooth", Range(0, 1)) = 0.1
        
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode Enum", Float) = 2
    }
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
                
        Cull [_CullMode]
        
        CGPROGRAM
        #pragma multi_compile SPECULAR_DEF_ON SPECULAR_DEF_OFF
        
        #pragma surface surf Toon addshadow fullforwardshadows exclude_path:deferred exclude_path:prepass
        #pragma target 3.0
        
        fixed4 _Color;
        fixed4 _HColor;
        fixed4 _SColor;
        
        sampler2D _MainTex;
        
        float _RampThreshold;
        float _RampSmooth;
        float _Alpha;
        
        float _SpecSmooth;
        fixed _Shininess;
        
        fixed4 _RimColor;
        fixed _RimThreshold;
        float _RimSmooth;
        
        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            float4 screenPos;
        };

        inline fixed4 LightingToon(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            half3 normalDir = normalize(s.Normal);
            half3 halfDir = normalize(lightDir + viewDir);
            
            float ndl = max(0, dot(normalDir, lightDir));
            float ndh = max(0, dot(normalDir, halfDir));
            float ndv = max(0, dot(normalDir, viewDir));
            
            // two steps
            fixed3 ramp = smoothstep(_RampThreshold - _RampSmooth * 0.5, _RampThreshold + _RampSmooth * 0.5, ndl);
            ramp *= atten;
            
            _SColor = lerp(_HColor, _SColor, _SColor.a);
            float3 rampColor = lerp(_SColor.rgb, _HColor.rgb, ramp);
            
            // specular
            float spec = pow(ndh, s.Specular * 128.0) * s.Gloss;
            spec *= atten;
            spec = smoothstep(0.5 - _SpecSmooth * 0.5, 0.5 + _SpecSmooth * 0.5, spec);
            
            // rim
            float rim = (1.0 - ndv) * ndl;
            rim *= atten;
            rim = smoothstep(_RimThreshold - _RimSmooth * 0.5, _RimThreshold + _RimSmooth * 0.5, rim);
            
            fixed3 lightColor = _LightColor0.rgb;
            
            fixed4 color;
            fixed3 diffuse = s.Albedo * lightColor * rampColor;
            fixed3 specular = _SpecColor.rgb * lightColor * spec;
            fixed3 rimColor = _RimColor.rgb * lightColor * _RimColor.a * rim;
            
            color.rgb = diffuse + specular + rimColor;
            color.a = s.Alpha;
            
            return color;
        }
        
        void surf(Input IN, inout SurfaceOutput o)
        {   
            const float4x4 thresholdMatrix = {
                1,9,3,11,
                13,5,15,7,
                4,12,2,10,
                16,8,14,6
            };
            
            float2 pixelPos = IN.screenPos.xy / IN.screenPos.w * _ScreenParams;
            float threshold = thresholdMatrix[pixelPos.x%4][pixelPos.y%4] / 17;
            
            clip(_Alpha - threshold);  
                 
            fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = mainTex.rgb * _Color.rgb;
          
            o.Alpha = mainTex.a * _Color.a;
            
            #if SPECULAR_DEF_ON
                o.Specular =_Shininess;
            #else
                o.Specular = pow(2,64);
            #endif
            o.Gloss = mainTex.a;
        }
        
        ENDCG        
    }
    FallBack "Diffuse"
}


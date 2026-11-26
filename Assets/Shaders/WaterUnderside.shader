// Draws the water surface as seen from below: a soft, slowly rippling ceiling with a bright horizon rim.
Shader "Fishing/Water Underside"
{
    Properties
    {
        _DeepColor ("Deep Color", Color) = (0.11, 0.26, 0.29, 0.82)
        _RippleColor ("Ripple Color", Color) = (0.5, 0.76, 0.76, 1)
        _RippleScale ("Ripple Scale", Float) = 0.5
        _RippleSpeed ("Ripple Speed", Float) = 0.6
        _RippleStrength ("Ripple Strength", Range(0, 1)) = 0.6
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "WaterUnderside"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor;
                half4 _RippleColor;
                float _RippleScale;
                float _RippleSpeed;
                float _RippleStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }

            // Three crossing waves; the analytic derivative doubles as a fake surface normal.
            float3 RippleGradient(float2 p, float t)
            {
                float2 a = float2(0.86, 0.50);
                float2 b = float2(-0.42, 0.91);
                float2 c = float2(0.31, -0.95);

                float waveA = cos(dot(p, a) * 1.1 + t * 1.1) * 1.1;
                float waveB = cos(dot(p, b) * 1.7 + t * 1.6) * 1.02;
                float waveC = cos(dot(p, c) * 2.6 + t * 2.3) * 0.91;

                return float3(a.x * waveA + b.x * waveB + c.x * waveC, 0, a.y * waveA + b.y * waveB + c.y * waveC);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 p = input.positionWS.xz * _RippleScale;
                float t = _Time.y * _RippleSpeed;

                float3 gradient = RippleGradient(p, t);

                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - input.positionWS);
                float lookingUp = saturate(abs(viewDir.y));
                float grazing = 1.0 - lookingUp;

                float ripple = saturate(dot(gradient, float3(0.35, 0, 0.35)) * _RippleStrength + 0.5);
                float3 color = lerp(_DeepColor.rgb, _RippleColor.rgb, ripple * 0.5 + grazing * 0.22);
                float alpha = saturate(_DeepColor.a + (1.0 - _DeepColor.a) * grazing * 0.7);

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}

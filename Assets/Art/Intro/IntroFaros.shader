Shader "FrecuenciaCero/Intro/FarosSuaves2D"
{
    Properties
    {
        _Color("Fade tint", Color) = (1,1,1,1)
        _LightColor("Headlight color", Color) = (0.99,0.98,0.94,0.075)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half4 _LightColor;
            CBUFFER_END
            struct Attributes { float3 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float distance = saturate(input.uv.x);
                float width = lerp(0.055, 0.34, distance);
                float lateral = (input.uv.y - 0.5) / width;
                float softEdge = exp(-5.0 * lateral * lateral);
                float attenuation = exp(-2.4 * distance);
                float start = smoothstep(0.0, 0.025, distance);
                float end = 1.0 - smoothstep(0.65, 1.0, distance);
                return half4(_LightColor.rgb * _Color.rgb,
                    _LightColor.a * _Color.a * softEdge * attenuation * start * end);
            }
            ENDHLSL
        }
    }
}

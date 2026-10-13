Shader "FrecuenciaCero/ContornoInteraccion2D"
{
    Properties { _MainTex("Silhouette",2D)="white"{} }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
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
            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            float4 _UVRect;
            float4 _OutlineStep;
            struct Attributes { float3 positionOS:POSITION;float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0; };
            Varyings Vert(Attributes i){Varyings o;o.positionCS=TransformObjectToHClip(i.positionOS);o.uv=i.uv;return o;}
            half Alpha(float2 uv)
            {
                if(any(uv<_UVRect.xy)||any(uv>_UVRect.zw))return 0;
                return SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv).a;
            }
            half4 Frag(Varyings i):SV_Target
            {
                float2 s=_OutlineStep.xy;half around=0;
                around=max(around,Alpha(i.uv+float2(s.x,0)));around=max(around,Alpha(i.uv-float2(s.x,0)));
                around=max(around,Alpha(i.uv+float2(0,s.y)));around=max(around,Alpha(i.uv-float2(0,s.y)));
                around=max(around,Alpha(i.uv+s));around=max(around,Alpha(i.uv-s));
                around=max(around,Alpha(i.uv+float2(s.x,-s.y)));around=max(around,Alpha(i.uv+float2(-s.x,s.y)));
                return half4(.94,.96,.97,saturate(around-Alpha(i.uv))*.9);
            }
            ENDHLSL
        }
    }
}

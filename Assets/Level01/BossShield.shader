Shader "Castle/BossShield"
{
    Properties
    {
        [HDR] _BaseColor("Shield Color", Color) = (0.1, 0.7, 1.5, 0.65)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float3 normalWS : TEXCOORD1; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half rim = pow(1.0 - saturate(dot(normalize(input.normalWS), normalize(GetWorldSpaceViewDir(input.positionWS)))), 2.0);
                half bands = pow(saturate(sin(input.positionWS.y * 18.0 - _Time.y * 2.0)), 18.0);
                half pulse = 0.85 + 0.15 * sin(_Time.y * 3.0);
                return half4(_BaseColor.rgb * pulse, _BaseColor.a * (0.06 + rim * 0.85 + bands * 0.09));
            }
            ENDHLSL
        }
    }
}

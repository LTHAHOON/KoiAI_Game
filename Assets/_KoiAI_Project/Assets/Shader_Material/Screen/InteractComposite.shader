Shader "Hidden/KoiAI/InteractComposite"
{
    Properties
    {
        [HDR] _InteractOutlineColor ("Interact Outline Color", Color) = (1, 0.8, 0.1, 1)
        _InteractOutlineWidth ("Interact Outline Width", Range(1, 8)) = 2
        _InteractOutlineAlpha ("Interact Outline Alpha", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "InteractComposite"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D_X(_InteractMaskTexture);
            float4 _InteractOutlineColor;
            float _InteractOutlineWidth;
            float _InteractOutlineAlpha;

            float SampleMask(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_InteractMaskTexture, sampler_PointClamp, uv).r;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float inside = smoothstep(0.35, 0.65, SampleMask(uv));
                float2 offset = _InteractOutlineWidth / _ScreenParams.xy;
                float neighbor = max(
                    max(SampleMask(uv + float2(offset.x, 0)), SampleMask(uv - float2(offset.x, 0))),
                    max(SampleMask(uv + float2(0, offset.y)), SampleMask(uv - float2(0, offset.y))));
                float outline = saturate(neighbor - inside);

                half4 scene = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                float outlineAlpha = outline * _InteractOutlineColor.a * _InteractOutlineAlpha;
                half3 color = lerp(scene.rgb, _InteractOutlineColor.rgb, outlineAlpha);
                return half4(color, scene.a);
            }
            ENDHLSL
        }
    }
}

Shader "Hidden/KoiAI/DetectComposite"
{
    Properties
    {
        _DetectProgress ("Detect Progress", Range(0, 1)) = 0
        _SilhouetteColor ("Silhouette Color", Color) = (0.05, 0.9, 0.8, 1)
        _OutlineColor ("Outline Color", Color) = (0.15, 1, 0.9, 1)
        _SilhouetteOpacity ("Silhouette Opacity", Range(0, 1)) = 0.28
        _OutlineWidth ("Outline Width", Range(1, 8)) = 2
        _DissolveEdgeWidth ("Dissolve Edge Width", Range(0.005, 0.1)) = 0.025
        _ScreenDissolve ("Screen Dissolve", Range(-1, 1)) = 1
        _ScreenAlpha ("Screen Alpha", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "DetectComposite"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D_X(_DetectMaskTexture);

            float _DetectProgress;
            float4 _SilhouetteColor;
            float4 _OutlineColor;
            float _SilhouetteOpacity;
            float _OutlineWidth;
            float _DissolveEdgeWidth;
            float _ScreenDissolve;
            float _ScreenAlpha;

            float SampleMask(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_DetectMaskTexture, sampler_PointClamp, uv).r;
            }

            float Hash(float2 pixel)
            {
                return frac(sin(dot(pixel, float2(12.9898, 78.233))) * 43758.5453);
            }

            float RevealAt(float2 uv)
            {
                float mask = smoothstep(0.35, 0.65, SampleMask(uv));
                float noise = Hash(floor(uv * _ScreenParams.xy));
                float dissolve = smoothstep(noise - 0.015, noise + 0.015, _DetectProgress);
                return mask * dissolve;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                float screenAlphaMask = pow(saturate(uv.y - _ScreenDissolve), 2.0) * _ScreenAlpha;
                float mask = SampleMask(uv);
                float2 pixel = floor(uv * _ScreenParams.xy);
                float noise = Hash(pixel);
                float dissolve = smoothstep(noise - 0.015, noise + 0.015, _DetectProgress);
                float inside = smoothstep(0.35, 0.65, mask) * dissolve;

                float2 offset = _OutlineWidth / _ScreenParams.xy;
                float neighbor = max(
                    max(RevealAt(uv + float2(offset.x, 0)), RevealAt(uv - float2(offset.x, 0))),
                    max(RevealAt(uv + float2(0, offset.y)), RevealAt(uv - float2(0, offset.y))));
                float outline = saturate(neighbor - inside);

                float rimDistance = abs(noise - _DetectProgress);
                float dissolveRim = (1.0 - smoothstep(_DissolveEdgeWidth, _DissolveEdgeWidth * 2.0, rimDistance))
                    * smoothstep(0.35, 0.65, mask)
                    * step(0.001, _DetectProgress)
                    * step(_DetectProgress, 0.999);

                half4 scene = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                half3 color = lerp(scene.rgb, _SilhouetteColor.rgb, _SilhouetteOpacity * inside);
                float outlineAlpha = saturate(outline + dissolveRim * 0.8) * _OutlineColor.a * screenAlphaMask;
                color = lerp(color, _OutlineColor.rgb, outlineAlpha);
                return half4(color, scene.a);
            }
            ENDHLSL
        }
    }
}

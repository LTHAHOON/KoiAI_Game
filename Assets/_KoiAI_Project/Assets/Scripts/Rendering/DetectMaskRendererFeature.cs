using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace KoiAI.Rendering
{
    public class DetectMaskRendererFeature : ScriptableRendererFeature
    {
        private static readonly int DetectProgressId = Shader.PropertyToID("_DetectProgress");
        private static readonly int SilhouetteColorId = Shader.PropertyToID("_SilhouetteColor");
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int SilhouetteOpacityId = Shader.PropertyToID("_SilhouetteOpacity");
        private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
        private static readonly int DissolveEdgeWidthId = Shader.PropertyToID("_DissolveEdgeWidth");
        private static readonly int ScreenDissolveId = Shader.PropertyToID("_ScreenDissolve");
        private static readonly int ScreenAlphaId = Shader.PropertyToID("_ScreenAlpha");

        private static float _detectionProgress;
        private static float _screenDissolve = 1f;
        private static float _screenAlpha = 1f;

        [SerializeField]
        private LayerMask _detectableLayers;
        [SerializeField]
        private Color _silhouetteColor = new(0.05f, 0.9f, 0.8f, 1f);
        [SerializeField]
        private Color _outlineColor = new(0.15f, 1f, 0.9f, 1f);
        [SerializeField, Range(0f, 1f)]
        private float _silhouetteOpacity = 0.28f;
        [SerializeField, Range(1f, 8f)]
        private float _outlineWidth = 2f;
        [SerializeField, Range(0.005f, 0.1f)]
        private float _dissolveEdgeWidth = 0.025f;

        [SerializeField]
        private Material _maskMaterial;
        [SerializeField]
        private Material _compositeMaterial;
        
        private DetectMaskPass _maskPass;
        private DetectCompositePass _compositePass;

        public static void SetDetectionProgress(float progress)
        {
            _detectionProgress = Mathf.Clamp01(progress);
        }

        public static void SetScreenDissolve(float dissolve, float alpha)
        {
            _screenDissolve = dissolve;
            _screenAlpha = Mathf.Clamp01(alpha);
        }

        public override void Create()
        {
            if (_detectableLayers.value == 0)
            {
                _detectableLayers = LayerMask.GetMask("Detectable");
            }

            _maskPass = new DetectMaskPass();
            _compositePass = new DetectCompositePass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game
                || renderingData.cameraData.renderType != CameraRenderType.Base
                || _detectionProgress <= 0f
                || _maskMaterial == null
                || _compositeMaterial == null)
            {
                return;
            }

            _compositeMaterial.SetFloat(DetectProgressId, _detectionProgress);
            _compositeMaterial.SetColor(SilhouetteColorId, _silhouetteColor);
            _compositeMaterial.SetColor(OutlineColorId, _outlineColor);
            _compositeMaterial.SetFloat(SilhouetteOpacityId, _silhouetteOpacity);
            _compositeMaterial.SetFloat(OutlineWidthId, _outlineWidth);
            _compositeMaterial.SetFloat(DissolveEdgeWidthId, _dissolveEdgeWidth);
            _compositeMaterial.SetFloat(ScreenDissolveId, _screenDissolve);
            _compositeMaterial.SetFloat(ScreenAlphaId, _screenAlpha);

            _maskPass.Setup(_detectableLayers, _maskMaterial);
            _compositePass.Setup(_maskPass, _compositeMaterial);
            renderer.EnqueuePass(_maskPass);
            renderer.EnqueuePass(_compositePass);
        }

        private class DetectMaskPass : ScriptableRenderPass
        {
            private static readonly List<ShaderTagId> ShaderTags = new()
            {
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("SRPDefaultUnlit")
            };

            private LayerMask _layerMask;
            private Material _material;

            public TextureHandle MaskTexture { get; private set; }

            public DetectMaskPass()
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            }

            public void Setup(LayerMask layerMask, Material material)
            {
                _layerMask = layerMask;
                _material = material;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                MaskTexture = TextureHandle.nullHandle;

                UniversalResourceData resources = frameData.Get<UniversalResourceData>();
                UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalLightData lightData = frameData.Get<UniversalLightData>();

                TextureDesc textureDesc = renderGraph.GetTextureDesc(resources.activeColorTexture);
                textureDesc.name = "_DetectSilhouetteMask";
                textureDesc.colorFormat = GraphicsFormat.R8_UNorm;
                textureDesc.depthBufferBits = DepthBits.None;
                textureDesc.msaaSamples = MSAASamples.None;
                textureDesc.bindTextureMS = false;
                textureDesc.clearBuffer = true;
                textureDesc.clearColor = Color.clear;
                textureDesc.useMipMap = false;
                textureDesc.autoGenerateMips = false;
                MaskTexture = renderGraph.CreateTexture(textureDesc);

                DrawingSettings drawingSettings = CreateDrawingSettings(
                    ShaderTags,
                    renderingData,
                    cameraData,
                    lightData,
                    SortingCriteria.CommonOpaque);
                drawingSettings.overrideMaterial = _material;
                drawingSettings.overrideMaterialPassIndex = 0;

                FilteringSettings filteringSettings = new(RenderQueueRange.all, _layerMask);
                RendererListParams rendererListParams = new(renderingData.cullResults, drawingSettings, filteringSettings);
                RendererListHandle rendererList = renderGraph.CreateRendererList(rendererListParams);

                using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass<MaskPassData>(
                           "Render Detectable Silhouette Mask",
                           out MaskPassData passData,
                           profilingSampler))
                {
                    passData.rendererList = rendererList;
                    builder.SetRenderAttachment(MaskTexture, 0, AccessFlags.Write);
                    builder.UseRendererList(rendererList);
                    builder.SetRenderFunc(static (MaskPassData data, RasterGraphContext context) =>
                    {
                        context.cmd.ClearRenderTarget(false, true, Color.clear);
                        context.cmd.DrawRendererList(data.rendererList);
                    });
                }
            }

            private class MaskPassData
            {
                public RendererListHandle rendererList;
            }
        }

        private class DetectCompositePass : ScriptableRenderPass
        {
            private static readonly int DetectMaskTextureId = Shader.PropertyToID("_DetectMaskTexture");

            private DetectMaskPass _maskPass;
            private Material _material;

            public DetectCompositePass()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            }

            public void Setup(DetectMaskPass maskPass, Material material)
            {
                _maskPass = maskPass;
                _material = material;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                TextureHandle maskTexture = _maskPass.MaskTexture;
                if (!maskTexture.IsValid())
                {
                    return;
                }

                UniversalResourceData resources = frameData.Get<UniversalResourceData>();
                TextureHandle source = resources.activeColorTexture;
                TextureDesc sourceDesc = renderGraph.GetTextureDesc(source);
                sourceDesc.name = "_DetectCompositeSource";
                sourceDesc.depthBufferBits = DepthBits.None;
                sourceDesc.msaaSamples = MSAASamples.None;
                sourceDesc.bindTextureMS = false;
                sourceDesc.clearBuffer = false;
                TextureHandle sourceCopy = renderGraph.CreateTexture(sourceDesc);
                renderGraph.AddBlitPass(source, sourceCopy, Vector2.one, Vector2.zero, passName: "Copy Detect Composite Source");

                using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass<CompositePassData>(
                           "Composite Detect Silhouette",
                           out CompositePassData passData,
                           profilingSampler))
                {
                    passData.source = sourceCopy;
                    passData.mask = maskTexture;
                    passData.material = _material;
                    builder.UseTexture(sourceCopy, AccessFlags.Read);
                    builder.UseTexture(maskTexture, AccessFlags.Read);
                    builder.SetRenderAttachment(resources.activeColorTexture, 0, AccessFlags.Write);
                    builder.AllowGlobalStateModification(true);
                    builder.SetRenderFunc(static (CompositePassData data, RasterGraphContext context) =>
                    {
                        context.cmd.SetGlobalTexture(DetectMaskTextureId, data.mask);
                        Blitter.BlitTexture(context.cmd, data.source, new Vector4(1f, 1f, 0f, 0f), data.material, 0);
                    });
                }
            }

            private class CompositePassData
            {
                public TextureHandle source;
                public TextureHandle mask;
                public Material material;
            }
        }
    }
}

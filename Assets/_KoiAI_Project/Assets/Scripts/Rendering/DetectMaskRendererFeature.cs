using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using KoiAI.Utilities;

namespace KoiAI.Rendering
{
    public class DetectMaskRendererFeature : ScriptableRendererFeature
    {
        private static readonly int DetectProgressId = Shader.PropertyToID("_DetectProgress");
        private static readonly int SilhouetteColorId = Shader.PropertyToID("_SilhouetteColor");
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int SilhouetteOpacityId = Shader.PropertyToID("_SilhouetteOpacity");
        private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
        private static readonly int InteractOutlineAlphaId = Shader.PropertyToID("_InteractOutlineAlpha");
        private static readonly int DissolveEdgeWidthId = Shader.PropertyToID("_DissolveEdgeWidth");
        private static readonly int ScreenDissolveId = Shader.PropertyToID("_ScreenDissolve");
        private static readonly int ScreenAlphaId = Shader.PropertyToID("_ScreenAlpha");

        private static float _detectionProgress;
        private static float _screenDissolve = 1f;
        private static float _screenAlpha = 1f;
        private static List<Renderer> _interactableRenderers = new();
      //  private static readonly HashSet<Renderer> InteractableRenderers = new();

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
        [SerializeField, Min(0f)]
        private float _fadeInOutDuration = 0.2f;

        [SerializeField]
        private Material _maskMaterial;
        [SerializeField]
        private Material _compositeMaterial;
        [SerializeField]
        private Material _interactMaskMaterial;
        [SerializeField]
        private Material _interactCompositeMaterial;

        private static float _targetOutlineAlpha;        
        private float _currentOutlineAlpha;
        private DetectMaskPass _maskPass;
        private DetectCompositePass _compositePass;
        private InteractMaskPass _interactMaskPass;
        private InteractCompositePass _interactCompositePass;

        public static void SetInteractableRenderers(IEnumerable<Renderer> renderers)
        {
            _interactableRenderers = renderers.ToList();
        }

        public static void ClearInteractableRenderers()
        {
            _targetOutlineAlpha = 0f;
        }

        public static void SetInteractOutlineAlpha(float alpha)
        {
            _targetOutlineAlpha = alpha;
        }

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
            _interactMaskPass = new InteractMaskPass();
            _interactCompositePass = new InteractCompositePass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game
                || renderingData.cameraData.renderType != CameraRenderType.Base)
            {
                return;
            }

            UpdateInteractOutlineAlpha();


            if (_detectionProgress > 0f && _maskMaterial != null && _compositeMaterial != null)
            {
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

            if (_interactableRenderers.Count > 0 && _interactMaskMaterial != null && _interactCompositeMaterial != null)
            {
                _interactCompositeMaterial.SetFloat(InteractOutlineAlphaId, _currentOutlineAlpha);
                _interactMaskPass.Setup(_interactableRenderers, _interactMaskMaterial);
                _interactCompositePass.Setup(_interactMaskPass, _interactCompositeMaterial);
                renderer.EnqueuePass(_interactMaskPass);
                renderer.EnqueuePass(_interactCompositePass);
            }
        }

        private void UpdateInteractOutlineAlpha()
        {
            float targetAlpha = Mathf.Clamp01(_targetOutlineAlpha);
            if (_fadeInOutDuration <= 0f)
            {
                _currentOutlineAlpha = targetAlpha;
                return;
            }

            _currentOutlineAlpha = Mathf.MoveTowards(
                _currentOutlineAlpha,
                targetAlpha,
                Time.deltaTime / _fadeInOutDuration);
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

        private class InteractMaskPass : ScriptableRenderPass
        {
            private readonly List<Renderer> _renderers = new();
            private Material _material;

            public TextureHandle MaskTexture { get; private set; }

            public InteractMaskPass()
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            }

            public void Setup(IEnumerable<Renderer> renderers, Material material)
            {
                _renderers.Clear();
                _renderers.AddRange(renderers);
                _material = material;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                MaskTexture = TextureHandle.nullHandle;

                UniversalResourceData resources = frameData.Get<UniversalResourceData>();
                TextureDesc textureDesc = renderGraph.GetTextureDesc(resources.activeColorTexture);
                textureDesc.name = "_InteractMaskTexture";
                textureDesc.colorFormat = GraphicsFormat.R8_UNorm;
                textureDesc.depthBufferBits = DepthBits.None;
                textureDesc.msaaSamples = MSAASamples.None;
                textureDesc.bindTextureMS = false;
                textureDesc.clearBuffer = true;
                textureDesc.clearColor = Color.clear;
                textureDesc.useMipMap = false;
                textureDesc.autoGenerateMips = false;
                MaskTexture = renderGraph.CreateTexture(textureDesc);

                using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass<InteractMaskPassData>(
                           "Render Interactable Depth Tested Mask",
                           out InteractMaskPassData passData,
                           profilingSampler))
                {
                    passData.renderers = _renderers.ToArray();
                    passData.material = _material;
                    builder.SetRenderAttachment(MaskTexture, 0, AccessFlags.Write);
                    builder.SetRenderAttachmentDepth(resources.activeDepthTexture, AccessFlags.Read);
                    builder.SetRenderFunc(static (InteractMaskPassData data, RasterGraphContext context) =>
                    {
                        context.cmd.ClearRenderTarget(false, true, Color.clear);
                        foreach (Renderer renderer in data.renderers)
                        {
                            if (renderer != null && renderer.enabled)
                            {
                                context.cmd.DrawRenderer(renderer, data.material);
                            }
                        }
                    });
                }
            }

            private class InteractMaskPassData
            {
                public Renderer[] renderers;
                public Material material;
            }
        }

        private class InteractCompositePass : ScriptableRenderPass
        {
            private static readonly int InteractMaskTextureId = Shader.PropertyToID("_InteractMaskTexture");

            private InteractMaskPass _maskPass;
            private Material _material;

            public InteractCompositePass()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            }

            public void Setup(InteractMaskPass maskPass, Material material)
            {
                _maskPass = maskPass;
                _material = material;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (!_maskPass.MaskTexture.IsValid())
                {
                    return;
                }

                UniversalResourceData resources = frameData.Get<UniversalResourceData>();
                TextureHandle source = resources.activeColorTexture;
                TextureDesc sourceDesc = renderGraph.GetTextureDesc(source);
                sourceDesc.name = "_InteractCompositeSource";
                sourceDesc.depthBufferBits = DepthBits.None;
                sourceDesc.msaaSamples = MSAASamples.None;
                sourceDesc.bindTextureMS = false;
                sourceDesc.clearBuffer = false;
                TextureHandle sourceCopy = renderGraph.CreateTexture(sourceDesc);
                renderGraph.AddBlitPass(source, sourceCopy, Vector2.one, Vector2.zero, passName: "Copy Interact Composite Source");

                using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass<InteractCompositePassData>(
                           "Composite Interactable Outline",
                           out InteractCompositePassData passData,
                           profilingSampler))
                {
                    passData.source = sourceCopy;
                    passData.mask = _maskPass.MaskTexture;
                    passData.material = _material;
                    builder.UseTexture(sourceCopy, AccessFlags.Read);
                    builder.UseTexture(_maskPass.MaskTexture, AccessFlags.Read);
                    builder.SetRenderAttachment(resources.activeColorTexture, 0, AccessFlags.Write);
                    builder.AllowGlobalStateModification(true);
                    builder.SetRenderFunc(static (InteractCompositePassData data, RasterGraphContext context) =>
                    {
                        context.cmd.SetGlobalTexture(InteractMaskTextureId, data.mask);
                        Blitter.BlitTexture(context.cmd, data.source, new Vector4(1f, 1f, 0f, 0f), data.material, 0);
                    });
                }
            }

            private class InteractCompositePassData
            {
                public TextureHandle source;
                public TextureHandle mask;
                public Material material;
            }
        }
    }
}

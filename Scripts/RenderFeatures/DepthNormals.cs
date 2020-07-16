using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace M8.URP.RenderFeatures {
    /// <summary>
    /// Render normals to _CameraDepthNormalsTexture.
    /// </summary>
    public class DepthNormals : ScriptableRendererFeature {
        const string depthNormalTextureName = "_CameraDepthNormalsTexture";

        class DepthNormalsPass : ScriptableRenderPass {
            const int depthBufferBits = 32;
            const string profilerTag = "M8.URP.Depth-Normals Prepass";

            public RenderTextureDescriptor descriptor { get; private set; }

            private RenderTargetHandle mDepthAttachmentHandle;

            private Material mDepthNormalsMat = null;
            private FilteringSettings mFilterSettings;
            private ShaderTagId mShaderTagID = new ShaderTagId("DepthOnly");

            public DepthNormalsPass(RenderQueueRange renderQueueRange, LayerMask layerMask, Material material) {
                mFilterSettings = new FilteringSettings(renderQueueRange, layerMask);
                mDepthNormalsMat = material;
            }

            public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle depthAttachmentHandle) {
                mDepthAttachmentHandle = depthAttachmentHandle;

                baseDescriptor.colorFormat = RenderTextureFormat.ARGB32;
                baseDescriptor.depthBufferBits = depthBufferBits;

                descriptor = baseDescriptor;
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
                cmd.GetTemporaryRT(mDepthAttachmentHandle.id, descriptor, FilterMode.Point);

                ConfigureTarget(mDepthAttachmentHandle.Identifier());
                ConfigureClear(ClearFlag.All, Color.black);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
                var cmd = CommandBufferPool.Get(profilerTag);

                using(new ProfilingScope(cmd, new ProfilingSampler(profilerTag))) {
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();

                    var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                    var drawSettings = CreateDrawingSettings(mShaderTagID, ref renderingData, sortFlags);
                    drawSettings.perObjectData = PerObjectData.None;

                    ref var camData = ref renderingData.cameraData;
                    var cam = camData.camera;
                    if(camData.isStereoEnabled)
                        context.StartMultiEye(cam);

                    drawSettings.overrideMaterial = mDepthNormalsMat;

                    context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref mFilterSettings);

                    cmd.SetGlobalTexture(depthNormalTextureName, mDepthAttachmentHandle.id);
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public override void FrameCleanup(CommandBuffer cmd) {
                if(mDepthAttachmentHandle != RenderTargetHandle.CameraTarget) {
                    cmd.ReleaseTemporaryRT(mDepthAttachmentHandle.id);
                    mDepthAttachmentHandle = RenderTargetHandle.CameraTarget;
                }
            }
        }

        private Material mDepthNormalsMat;
        private DepthNormalsPass mDepthNormalsPass;
        private RenderTargetHandle mDepthNormalsTexture;

        public override void Create() {
            mDepthNormalsMat = CoreUtils.CreateEngineMaterial("Hidden/Internal-DepthNormalsTexture");

            mDepthNormalsPass = new DepthNormalsPass(RenderQueueRange.opaque, -1, mDepthNormalsMat);
            mDepthNormalsPass.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;

            mDepthNormalsTexture.Init(depthNormalTextureName);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            mDepthNormalsPass.Setup(renderingData.cameraData.cameraTargetDescriptor, mDepthNormalsTexture);
            renderer.EnqueuePass(mDepthNormalsPass);
        }
    }
}
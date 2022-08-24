using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomRenderFeature : ScriptableRendererFeature
{

    #region Feature Object

    public FeatureSettings settings = new FeatureSettings();
    private RenderTargetHandle renderTextureHandle;
    private CustomRenderPass customRenderPass;

    public override void Create()
    {
        customRenderPass = new CustomRenderPass("Custom_RenderPass", settings.passEvent, settings.blitMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!settings.isEnabled) return;

        RenderTargetIdentifier targetIdentifier = renderer.cameraColorTarget;
        customRenderPass.Setup(targetIdentifier);

        renderer.EnqueuePass(customRenderPass);
    }

    #endregion

    #region Render Pass Object

    private class CustomRenderPass : ScriptableRenderPass
    {
        private string profilerTag;
        private Material blitMaterial;
        private RenderTargetIdentifier cameraColorTargetIdentifier;
        private RenderTargetHandle tempTexture;

        public CustomRenderPass(string profilerTag, RenderPassEvent renderPassEvent, Material blitMaterial) 
        {
            this.blitMaterial = blitMaterial;
            this.renderPassEvent = renderPassEvent;
            this.profilerTag = profilerTag;
        }

        public void Setup(RenderTargetIdentifier identifier)
        {
            cameraColorTargetIdentifier = identifier;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            cmd.GetTemporaryRT(tempTexture.id, renderingData.cameraData.cameraTargetDescriptor);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }

        //public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        //{
        //    cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor);
        //}

        //public override void FrameCleanup(CommandBuffer cmd)
        //{
        //    cmd.ReleaseTemporaryRT(tempTexture.id);
        //}

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
            cmd.Clear();

            cmd.Blit(cameraColorTargetIdentifier, tempTexture.Identifier(), blitMaterial, 0);
            cmd.Blit(tempTexture.Identifier(), cameraColorTargetIdentifier);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

    }

    #endregion

    #region Property Class

    [System.Serializable]
    public class FeatureSettings
    {
        public bool isEnabled = true;
        public RenderPassEvent passEvent = RenderPassEvent.AfterRendering;
        public Material blitMaterial;
    }


    #endregion
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms;

[RequireComponent(typeof(Camera))]
public class DepthPeeling : MonoBehaviour
{
    public enum RT
    {
        Depth = 0,
        Color = 1,
    }

    [Range(1, 6)] public int depthMax = 3;
    public RT rt;
    public Shader MRTShader;
    public Shader finalClipsShader;

    private Camera sourceCamera;
    private Camera tempCamera;
 
    public RenderTexture[] rts;
    public RenderTexture rtTemp;
    private RenderBuffer[] colorBuffers;
    private RenderTexture depthBuffer;
    public RenderTexture finalClips; // 这里显存占用比较大 如果要优化 可以考虑 从前往后叠加 在一张rt上做累积  具体算法在说明里
    public bool showFinal;
    private Material finalClipsMat;

    void Start()
    {
        this.sourceCamera = this.GetComponent<Camera>();
        tempCamera = new GameObject().AddComponent<Camera>();
        tempCamera.enabled = false;
 
        finalClipsMat = new Material(finalClipsShader);
        this.rts = new RenderTexture[2]
        {
            new RenderTexture(sourceCamera.pixelWidth, sourceCamera.pixelHeight, 0, RenderTextureFormat.RFloat),
            new RenderTexture(sourceCamera.pixelWidth, sourceCamera.pixelHeight, 0, RenderTextureFormat.Default)
        };
         
        rts[0].Create();
        rts[1].Create();
        finalClips = new RenderTexture(sourceCamera.pixelWidth, sourceCamera.pixelHeight, 0,
            RenderTextureFormat.Default);

        finalClips.dimension = TextureDimension.Tex2DArray;
        finalClips.volumeDepth = 6;
        finalClips.Create();

        Shader.SetGlobalTexture("FinalClips", finalClips);
        rtTemp = new RenderTexture(sourceCamera.pixelWidth, sourceCamera.pixelHeight, 0, RenderTextureFormat.RFloat);
        rtTemp.Create();

        Shader.SetGlobalTexture("DepthRendered", rtTemp);
        colorBuffers = new RenderBuffer[2] {rts[0].colorBuffer, rts[1].colorBuffer};

        depthBuffer = new RenderTexture(sourceCamera.pixelWidth, sourceCamera.pixelHeight, 16,
            RenderTextureFormat.Depth);
        depthBuffer.Create();
    }


    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        tempCamera.CopyFrom(sourceCamera);
        tempCamera.clearFlags = CameraClearFlags.SolidColor;
        tempCamera.backgroundColor = Color.clear;
        tempCamera.SetTargetBuffers(colorBuffers, depthBuffer.depthBuffer);
        tempCamera.cullingMask = 1 << LayerMask.NameToLayer("clipRender");


        for (int i = 0; i < depthMax; i++)
        {
            Graphics.Blit(rts[0], rtTemp);// 这里不知道为什么需要复制出来 不能直接用rts【0】 当时我判断是不可同时读写所以复制一份就可以了
            Shader.SetGlobalInt("DepthRenderedIndex", i);
            tempCamera.RenderWithShader(MRTShader, "");
            Graphics.CopyTexture(rts[1], 0, 0, finalClips, i, 0);
        }

        if (showFinal == false)
        {
            Graphics.Blit(rts[rt.GetHashCode()], destination);
        }
        else
        {
            Graphics.Blit(null, destination, finalClipsMat);
        }
    }

    void OnDestroy()
    {
        rts[0].Release();
        rts[1].Release();
        finalClips.Release();
        rtTemp.Release();

        depthBuffer.Release();
    }
}
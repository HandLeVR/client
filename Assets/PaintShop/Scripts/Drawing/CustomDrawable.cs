using Height2NormalMap;
using UnityEngine;

/// <summary>
/// Custom version of the Drawable class which allows to set additional parameter. This class is responsible for
/// displaying the paint on the workpiece. 
/// </summary>
public class CustomDrawable : MonoBehaviour
{
    [Header("General Settings")] public int textureSize = 2048;

    public bool isTestPaper;

    // the box in which the user is allowed to spray (the offset is added to the the box)
    public BoxCollider sprayBox;
    public float sprayBoxOffset = 0.5f;

    [Header("Shader")] public Material cookieToHeightmapShader;
    public Material heightmapToColorShader;
    public Material heightmapToMetallicShader;
    public Material heightmapToOcclusionShader;
    public Material heightmapToHeatmapShader;
    public Material heightmapToRoughnessShader;
    public Material metallicMaskShader;
    public Material fillCrack;
    public Material drawNormalsShader;
    public Material drawTangentsShader;
    public ComputeShader computeShader;

    [Header("Calculated Textures (Debug)")]
    public RenderTexture heightmapOutput;

    public RenderTexture mainOutput;
    public RenderTexture metallicOutput;
    public RenderTexture cookieOutput;
    public RenderTexture normalsMap;
    public RenderTexture tangentsMap;
    public RenderTexture greyscaleMap;
    public RenderTexture normalMapRuns;
    public RenderTexture normalMapAll;
    public RenderTexture occlusionMap;

    [Header("Debug")] public bool stopAlphaAtMinThickness;

    // the copy created when the magnifying glass is used
    [HideInInspector] public CustomDrawable currentCopy;

    // temp textures
    private RenderTexture _heightmapTmp1;
    private RenderTexture _heightmapTmp2;
    private RenderTexture _heightmapOld;
    private RenderTexture _mainOld;
    private RenderTexture _mainEmpty;
    private RenderTexture _cookieBlank;
    private RenderTexture _metallicTmp;
    private RenderTexture _metallicEmpty;
    private RenderTexture _heightmapEmpty;
    private RenderTexture _cookieTmp;
    private RenderTexture _normalMapEmpty;

    // misc
    private Color _metallicInitColor;
    private Color _metallicColor;
    private NormalMapGenerator _normalMapGenerator;
    private GaussianBlurFilter _filter;
    private Mesh _mesh;

    // is set if the workpiece surface is loaded from a recording
    private Texture2D _presetHeightmap;

    // shader ids
    private int _simulatePaintRunKernelHandle;
    private int _simulatePaintReductionKernelHandle;
    private int _smoothKernelHandle;
    private int _heightmapToGreyscaleKernelHandle;
    private static readonly int MainTexId = Shader.PropertyToID("_BaseMap");
    private static readonly int MetallicGlossMapId = Shader.PropertyToID("_MetallicGlossMap");
    private static readonly int BumpMapId = Shader.PropertyToID("_BumpMap");
    private static readonly int CookieTexId = Shader.PropertyToID("_CookieTex");
    private static readonly int RoughnessTexId = Shader.PropertyToID("_RoughnessTex");
    private static readonly int MaxSmoothnessId = Shader.PropertyToID("_MaxSmoothness");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int DetailNormalMapId = Shader.PropertyToID("_DetailNormalMap");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorTexId = Shader.PropertyToID("_ColorTex");
    private static readonly int HeightTexId = Shader.PropertyToID("_HeightTex");
    private static readonly int MetalTexId = Shader.PropertyToID("_MetalTex");

    private static readonly int MaxSmoothnessHeightmapThresholdId =
        Shader.PropertyToID("_MaxSmoothnessHeightmapThreshold");

    private static readonly int MinFlowHeightmapThresholdId = Shader.PropertyToID("_MinFlowHeightmapThreshold");
    private static readonly int RoughnessStrengthId = Shader.PropertyToID("_RoughnessStrength");
    private static readonly int PowerValueId = Shader.PropertyToID("_PowerValue");
    private static readonly int OcclusionMapId = Shader.PropertyToID("_OcclusionMap");
    private static readonly int MaxAlphaId = Shader.PropertyToID("_MaxAlpha");
    private static readonly int InvertColorsId = Shader.PropertyToID("_InvertColors");

    private static readonly Color BlankNormalMapColor = new(128 / 255f, 128 / 255f, 255 / 255f);

    private void Start()
    {
        _metallicInitColor = Color.Lerp(Color.black, Color.white, PaintController.Instance.actualInitMetallic);
        _metallicInitColor.a = PaintController.Instance.actualInitSmoothness;
        _metallicColor = Color.Lerp(Color.black, Color.white, PaintController.Instance.maxMetallic);

        _mesh = GetComponent<MeshFilter>().mesh;
        if (!sprayBox)
        {
            sprayBox = GetComponentInChildren<BoxCollider>();
            sprayBox.size = _mesh.bounds.size + new Vector3(sprayBoxOffset, sprayBoxOffset, sprayBoxOffset);
        }

        heightmapOutput = GenerateRenderTexture(enableRandomWrite: true);
        _heightmapTmp1 = GenerateRenderTexture(enableRandomWrite: true);
        _heightmapTmp2 = GenerateRenderTexture(enableRandomWrite: true);
        _heightmapOld = GenerateRenderTexture(enableRandomWrite: true);
        _heightmapEmpty = GenerateRenderTexture();
        greyscaleMap = GenerateRenderTexture(enableRandomWrite: true);
        normalMapRuns = GenerateRenderTexture(BlankNormalMapColor);
        normalMapAll = GenerateRenderTexture(BlankNormalMapColor);
        _normalMapEmpty = GenerateRenderTexture(BlankNormalMapColor);
        cookieOutput = GenerateRenderTexture();
        _cookieTmp = GenerateRenderTexture();
        metallicOutput = GenerateRenderTexture(!isTestPaper ? _metallicInitColor : Color.clear);
        _metallicTmp = GenerateRenderTexture(!isTestPaper ? _metallicInitColor : Color.clear);
        _metallicEmpty = GenerateRenderTexture(!isTestPaper ? _metallicInitColor : Color.clear);
        mainOutput =
            GenerateRenderTexture(!isTestPaper ? PaintController.Instance.actualDrawableInitColor : Color.white);
        _mainOld = GenerateRenderTexture(!isTestPaper ? PaintController.Instance.actualDrawableInitColor : Color.white);
        _mainEmpty =
            GenerateRenderTexture(!isTestPaper ? PaintController.Instance.actualDrawableInitColor : Color.white);
        _cookieBlank = GenerateRenderTexture(Color.clear);
        normalsMap = GenerateRenderTexture(enableRandomWrite: true);
        tangentsMap = GenerateRenderTexture(enableRandomWrite: true);
        occlusionMap = GenerateRenderTexture(Color.white);

        var r = GetComponent<Renderer>();
        var mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);
        mpb.SetTexture(MainTexId, mainOutput);
        mpb.SetTexture(MetallicGlossMapId, metallicOutput);
        if (!isTestPaper)
            mpb.SetTexture(DetailNormalMapId, normalMapAll);
        if (computeShader != null)
            mpb.SetTexture(BumpMapId, normalMapAll);
        mpb.SetTexture(OcclusionMapId, occlusionMap);
        r.SetPropertyBlock(mpb);

        Reset();
        LoadHeightmaps();

        _normalMapGenerator = GetNormalMapGenerator();
        _filter = new GaussianBlurFilter
        {
            iteration = 1,
            sampleFactor = 1f
        };
    }

    /// <summary>
    /// Resets the drawable to the initial state
    /// </summary>
    public void Reset()
    {
        // don't reset workpiece if it was created in the same frame
        if (mainOutput == null)
            return;

        SetRenderTextureColor(_mainEmpty,
            !isTestPaper ? PaintController.Instance.actualDrawableInitColor : Color.white);
        _metallicInitColor = Color.Lerp(Color.black, Color.white, PaintController.Instance.actualInitMetallic);
        _metallicInitColor.a = PaintController.Instance.actualInitSmoothness;
        SetRenderTextureColor(_metallicEmpty, !isTestPaper ? _metallicInitColor : Color.clear);

        Graphics.CopyTexture(_mainEmpty, mainOutput);
        Graphics.CopyTexture(_mainEmpty, _mainOld);
        Graphics.CopyTexture(_metallicEmpty, metallicOutput);
        Graphics.CopyTexture(_heightmapEmpty, heightmapOutput);
        Graphics.CopyTexture(_heightmapEmpty, _heightmapTmp1);
        Graphics.CopyTexture(_heightmapEmpty, _heightmapOld);
        Graphics.CopyTexture(_normalMapEmpty, normalMapAll);

        if (computeShader != null)
        {
            _simulatePaintRunKernelHandle = computeShader.FindKernel("SimulatePaintRun");
            _simulatePaintReductionKernelHandle = computeShader.FindKernel("SimulatePaintReduction");
            _smoothKernelHandle = computeShader.FindKernel("Smooth");
            _heightmapToGreyscaleKernelHandle = computeShader.FindKernel("HeightmapToGreyscale");
        }

        heightmapToMetallicShader.SetColor(ColorId, _metallicColor);
        heightmapToMetallicShader.SetFloat(MaxSmoothnessId, PaintController.Instance.maxSmoothness);
        heightmapToMetallicShader.SetFloat(MaxSmoothnessHeightmapThresholdId,
            PaintController.Instance.maxSmoothnessHeightmapThreshold);
        heightmapToMetallicShader.SetFloat(PowerValueId, PaintController.Instance.glossPowerValue);
    }

    /// <summary>
    /// Sets the heightmaps which can be loaded whenever LoadHeightmaps() is called
    /// </summary>
    public void LoadHeightmaps(Texture2D heightmap)
    {
        _presetHeightmap = heightmap;
        LoadHeightmaps();
    }

    /// <summary>
    /// Loads the given heightmap.
    /// </summary>
    public void LoadHeightmaps(RenderTexture heightmap)
    {
        Graphics.Blit(heightmap, heightmapOutput);
        Graphics.Blit(heightmap, _heightmapOld);
    }

    /// <summary>
    /// Loads the heightmaps from the presets if set.
    /// </summary>
    private void LoadHeightmaps()
    {
        if (_presetHeightmap == null)
            return;
        Graphics.Blit(_presetHeightmap, heightmapOutput);
        Graphics.Blit(_presetHeightmap, _heightmapOld);
    }

    /// <summary>
    /// Uses the heightmap to calculate the visible color and metallic/smoothness strength.
    /// </summary>
    private void FixedUpdate()
    {
        // don't update if you are a copy of the real workpiece (in evaluation mode)
        if (gameObject.name == "Workpiece(Clone)")
            return;

        // Actually we only need to recalculate the normals and tangents maps if the workpiece is rotated but if we
        // don't call one of the following functions every fixed update painting is not possible after some specific
        // events (e.g. teleporting or switching to the scene view and back to the game view in the editor). That
        // doesn't make sense as shortly hovering over the inspector also solves the problem...
        GenerateRenderTextureWithShader(normalsMap, drawNormalsShader);
        GenerateRenderTextureWithShader(tangentsMap, drawTangentsShader);

        // simulate paint run of the shader is set
        if (computeShader != null)
            SimulatePaintRun();

        UpdateWorkpiece();
    }

    /// <summary>
    /// Updates the appearance of the workpiece in dependence of the heightmap. The heightmap represents the amount of
    /// color for each pixel on the workpiece.
    /// </summary>
    public void UpdateWorkpiece()
    {
        // apply color
        heightmapToColorShader.SetColor(BaseColorId,
            !isTestPaper ? PaintController.Instance.actualDrawableInitColor : Color.white);
        Graphics.Blit(heightmapOutput, mainOutput, heightmapToColorShader);
        Graphics.CopyTexture(mainOutput, _mainOld);

        // apply metallic 
        heightmapToMetallicShader.SetColor(BaseColorId, !isTestPaper ? _metallicInitColor : Color.clear);
        Graphics.Blit(heightmapOutput, metallicOutput, heightmapToMetallicShader);
        _filter.Apply(metallicOutput, metallicOutput);

        // apply occlusion
        if (heightmapToOcclusionShader)
            Graphics.Blit(heightmapOutput, occlusionMap, heightmapToOcclusionShader);

        // apply roughness 
        if (heightmapToRoughnessShader)
        {
            // takes the normal map containing the runs and adds roughness
            heightmapToRoughnessShader.SetFloat(RoughnessStrengthId, PaintController.Instance.actualRoughnessStrength);
            heightmapToRoughnessShader.SetFloat(MinFlowHeightmapThresholdId,
                PaintController.Instance.minFlowHeightmapThreshold);
            heightmapToRoughnessShader.SetTexture(RoughnessTexId, PaintController.Instance.roughnessTexture);
            heightmapToRoughnessShader.SetTexture(BumpMapId, normalMapRuns);
            Graphics.Blit(heightmapOutput, normalMapAll, heightmapToRoughnessShader);
        }

        // update copy of the workpiece when the magnifying glass is active to (e.g. if the paint still runs)
        if (currentCopy)
        {
            Graphics.CopyTexture(mainOutput, currentCopy.mainOutput);
            Graphics.CopyTexture(metallicOutput, currentCopy.metallicOutput);
            Graphics.CopyTexture(normalMapAll, currentCopy.normalMapAll);
            Graphics.CopyTexture(occlusionMap, currentCopy.occlusionMap);
        }

        if (HeatmapActive())
        {
            // draw heatmap
            heightmapToHeatmapShader.SetTexture(ColorTexId, _mainOld);
            heightmapToHeatmapShader.SetTexture(HeightTexId, heightmapOutput);

            // inverts the mask for flashlight evaluation
            if (!ApplicationController.Instance.showHeatmapOnWorkpiece &&
                ApplicationController.Instance.currentTool == SprayGun.Tool.Flashlight)
            {
                heightmapToHeatmapShader.SetFloat(InvertColorsId, 1);
                metallicMaskShader.SetFloat(InvertColorsId, 1);
            }
            else
            {
                heightmapToHeatmapShader.SetFloat(InvertColorsId, 0);
                metallicMaskShader.SetFloat(InvertColorsId, 0);
            }

            // show heatmap on the workpiece if the flashlight is not used
            Graphics.Blit(
                ApplicationController.Instance.currentTool == SprayGun.Tool.SprayGun ? _cookieBlank : _cookieTmp,
                mainOutput, heightmapToHeatmapShader);
            _filter.Apply(mainOutput, mainOutput);

            // mask off the metallic map in heatmap mode
            Graphics.CopyTexture(metallicOutput, _metallicTmp);
            metallicMaskShader.SetTexture(MetalTexId, _metallicTmp);
            Graphics.Blit(
                ApplicationController.Instance.currentTool == SprayGun.Tool.SprayGun ? _cookieBlank : _cookieTmp,
                metallicOutput, metallicMaskShader);

            // cookie disappears if its not reset
            Graphics.CopyTexture(_cookieBlank, _cookieTmp);
        }

        Graphics.CopyTexture(heightmapOutput, _heightmapOld);
    }


    /// <summary>
    /// Use drawing material which was modified by the spot drawer to create the cookie. The cookie then is used to
    /// modify the heightmap which determines the added color in this frame.
    /// </summary>
    public void Draw(Material drawingMat)
    {
        Graphics.Blit(_cookieTmp, cookieOutput, drawingMat, 0);
        Graphics.DrawMeshNow(_mesh, transform.localToWorldMatrix);
        Graphics.CopyTexture(cookieOutput, _cookieTmp);

        // cookie must not change heightmap if flashlight (evaluation mode) is active
        if (ApplicationController.Instance.currentTool == SprayGun.Tool.Flashlight)
            return;

        if (fillCrack != null)
        {
            // fill gaps and the edges of continuous surfaces in the uv map
            Graphics.Blit(_cookieTmp, cookieOutput, fillCrack);
            Graphics.CopyTexture(cookieOutput, _cookieTmp);
        }

        cookieToHeightmapShader.SetTexture(CookieTexId, cookieOutput);
        cookieToHeightmapShader.SetFloat(MaxAlphaId,
            stopAlphaAtMinThickness ? PaintController.Instance.maxSmoothnessHeightmapThreshold : 1);
        Graphics.Blit(_heightmapOld, heightmapOutput, cookieToHeightmapShader);
        Graphics.CopyTexture(heightmapOutput, _heightmapOld);
    }

    /// <summary>
    /// Sets the properties of the compute shade which handles the paint flow.
    /// </summary>
    private void SimulatePaintRun()
    {
        computeShader.SetTexture(_simulatePaintRunKernelHandle, "input", _heightmapOld);
        computeShader.SetTexture(_simulatePaintRunKernelHandle, "normals", normalsMap);
        computeShader.SetTexture(_simulatePaintRunKernelHandle, "tangents", tangentsMap);
        computeShader.SetTexture(_simulatePaintRunKernelHandle, "result", heightmapOutput);
        computeShader.SetInt("texSize", textureSize);
        computeShader.SetFloat("minFlowThickness", PaintController.Instance.minFlowHeightmapThreshold);
        computeShader.SetFloat("viscosity", PaintController.Instance.viscosity);
        computeShader.Dispatch(_simulatePaintRunKernelHandle, mainOutput.width / 64, mainOutput.height / 1, 1);

        Graphics.CopyTexture(heightmapOutput, _heightmapTmp2);
        computeShader.SetTexture(_simulatePaintReductionKernelHandle, "input", _heightmapOld);
        computeShader.SetTexture(_simulatePaintReductionKernelHandle, "normals", normalsMap);
        // lastResult is needed because on some GPUs it is not possible to read and write from/to the same texture
        computeShader.SetTexture(_simulatePaintReductionKernelHandle, "lastResult", _heightmapTmp2);
        computeShader.SetTexture(_simulatePaintReductionKernelHandle, "result", heightmapOutput);
        computeShader.SetInt("texSize", textureSize);
        computeShader.SetFloat("minFlowThickness", PaintController.Instance.minFlowHeightmapThreshold);
        computeShader.SetFloat("viscosity", PaintController.Instance.viscosity);
        computeShader.Dispatch(_simulatePaintReductionKernelHandle, mainOutput.width / 64, mainOutput.height / 1, 1);

        Graphics.CopyTexture(heightmapOutput, _heightmapOld);
        computeShader.SetTexture(_smoothKernelHandle, "input", _heightmapOld);
        computeShader.SetTexture(_smoothKernelHandle, "result", heightmapOutput);
        computeShader.SetInt("texSize", textureSize);
        computeShader.Dispatch(_smoothKernelHandle, mainOutput.width / 64, mainOutput.height / 1, 1);

        computeShader.SetTexture(_heightmapToGreyscaleKernelHandle, "input", heightmapOutput);
        computeShader.SetTexture(_heightmapToGreyscaleKernelHandle, "result", greyscaleMap);
        computeShader.SetInt("texSize", textureSize);
        computeShader.Dispatch(_heightmapToGreyscaleKernelHandle, mainOutput.width / 64, mainOutput.height / 1, 1);

        _normalMapGenerator.Apply(normalMapRuns);

        Graphics.CopyTexture(heightmapOutput, _heightmapOld);
    }

    /// <summary>
    /// Sets all necessary properties for a RenderTexture.
    /// 
    /// Theoretically enableRandomWrite do not need to be set true for all textures, but it
    /// caused problems in the built version if it was not set to true for one of the empty textures.
    /// To avoid these problems enableRandomWrite is set to true for all textures.
    /// </summary>
    private RenderTexture GenerateRenderTexture(Color? color = null, bool enableRandomWrite = false)
    {
        var outputRt = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGBHalf);
        outputRt.enableRandomWrite = enableRandomWrite;
        outputRt.Create();
        var currentActive = RenderTexture.active;
        RenderTexture.active = outputRt;
        GL.Clear(true, true, color ?? Color.clear);
        RenderTexture.active = currentActive;

        return outputRt;
    }

    private void GenerateRenderTextureWithShader(RenderTexture renderTexture, Material shader)
    {
        var currentActive = RenderTexture.active;
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, Color.clear);
        shader.SetPass(0);
        Graphics.DrawMeshNow(_mesh, transform.localToWorldMatrix);
        RenderTexture.active = currentActive;
    }

    private void SetRenderTextureColor(RenderTexture rt, Color color)
    {
        var currentActive = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, color);
        RenderTexture.active = currentActive;
    }

    private bool HeatmapActive()
    {
        return heightmapToHeatmapShader != null && metallicMaskShader != null &&
               ApplicationController.Instance.showHeatMap;
    }

    private NormalMapGenerator GetNormalMapGenerator()
    {
        NormalMapGenerator result = new NormalMapGenerator()
        {
            heightMap = new BlurHeightMapFilter()
            {
                preBlur = new GaussianBlurFilter()
                {
                    iteration = 1,
                    sampleFactor = 0.5f
                },
                postBlur = new GaussianBlurFilter()
                {
                    iteration = 0,
                    sampleFactor = 0.5f
                },
                heigthMap = new HeightMapFilter()
                {
                    factor = 1f
                }
            },
            normalMap = new BlurNormalMapFilter()
            {
                normalMap = new SobelNormalMapFilter()
                {
                    bumpEffect = 0.5f
                },
                preBlur = new GaussianBlurFilter()
                {
                    iteration = 0,
                    sampleFactor = 0.5f
                },
                postBlur = new GaussianBlurFilter()
                {
                    iteration = 1,
                    sampleFactor = 1
                }
            }
        };
        result.baseHeightMap = greyscaleMap;
        return result;
    }

    private void OnDestroy()
    {
        // needed to avoid warnings when releasing render textures
        RenderTexture.active = null;
        
        if (heightmapOutput == null)
            return;
        heightmapOutput.Release();
        _heightmapTmp1.Release();
        _heightmapTmp2.Release();
        _heightmapOld.Release();
        greyscaleMap.Release();
        normalMapRuns.Release();
        normalMapAll.Release();
        _normalMapEmpty.Release();
        _heightmapEmpty.Release();
        cookieOutput.Release();
        _cookieTmp.Release();
        metallicOutput.Release();
        _metallicEmpty.Release();
        mainOutput.Release();
        _mainOld.Release();
        _mainEmpty.Release();
        normalsMap.Release();
        tangentsMap.Release();
        occlusionMap.Release();
    }
}
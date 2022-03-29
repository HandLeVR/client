using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages everything related the visuals of the coat application.
/// </summary>
public class PaintController : Singleton<PaintController>
{
    [Header("Paint Properties")] public CustomDrawable[] drawables;
    public Color drawableInitColor = Color.white;
    public float maxSpotDrawerIntensity = 25f;
    [Range(0.01f, 180f)] public float minSpotDrawerAngle = 70f;

    [Header("Coat Properties")]
    [Tooltip(
        "The maximum viscosity considered in paint run and roughness calculation. This value will be used if a higher value is set in the coat property file.")]
    public float maxViscosity = 40;

    [Tooltip(
        "The minimum viscosity considered in paint run and roughness calculation. This value will be used if a lower value is set in the coat property file.")]
    public float minViscosity = 10;

    [Header("Metallic Effect")]
    [Tooltip("The initial metallic value for all drawables. Should be less than or equal the max metallic value.")]
    [Range(0f, 1f)]
    public float initMetallic = 0f;

    [Tooltip(
        "The maximum metallic value for all drawables. Was moved out of the coat class because real painter understand something different under the term 'metallic'.")]
    [Range(0f, 1f)]
    public float maxMetallic = 0.25f;

    // smoothness must not be 0 to ensure the the fill crack shader works
    [Tooltip("The initial smoothness for all drawables. Should be less than or equal the max smoothness value.")]
    [Range(0.01f, 1f)]
    public float initSmoothness = 0.01f;

    [Tooltip(
        "Takes the gloss to th y-th power where y=glossPowerValue. For example y=1 means a linear increasement of the gloss with increasing coat thickness and y=2 means a quadratic increasement of the gloss.")]
    public float glossPowerValue = 2;

    [Header("Shader Properties")] public Material heightmapToColorShader;
    public Material cookieToHeightmapShader;
    public Material spotDrawerShader;
    public Material heightmapToHeatmapShader;
    public Material heightmapToRoughnessShader;

    [Tooltip(
        "This value ist mapped to the targetMinThicknessWet property of the selected coat. The coat appearance related properties a adapted in relation to this value. This ensures that the application of (speed and distance of the spray gun) is independent of the coat.")]
    public float targetMinThicknessWetAlpha = 0.16f;

    [Tooltip("This property can be used to modify how fast the coat thickness increases while painting.")]
    public float thicknessModifier = 0.02f;

    [Tooltip(
        "Modifies how fast the thickness is increased in the distance heightmap if the distance to the workpiece is under the min distance.")]
    public float closeDistanceThicknessModifier = 0.75f;

    [Tooltip(
        "Modifies how fast the thickness is increased in the distance heightmap if the distance to the workpiece is over the max distance.")]
    public float farDistanceThicknessModifier = 0.25f;

    [Tooltip("The increase of the coat thickness is slowed down by this factor after the min thickness is reached.")]
    public float dampFactorAfterMinThickness = 0.2f;

    [Tooltip("Smooths the gradient in the heatmap in dependence of the optimal coat thickness range.")] [Range(0f, 1f)]
    public float heatmapGradientSmoothness = 0.1f;

    public bool roughnessOnDistance = true;

    [Header("Roughness")]
    [Tooltip(
        "The maximum scale of the detail normal map which is used for the roughness effect. This controls the overall strength of the roughness effect.")]
    public float maxDetailNormalMapScale = 0.005f;

    [Tooltip("Texture used to create roughness.")]
    public Texture roughnessTexture;

    [Header("Cookie Scaling")]
    [Tooltip(
        "The original cookie which is used as the spray pattern. The cookie is scaled in dependence of the velocity and the direction of the spray gun movements.")]
    public Texture cookie;

    [Tooltip(
        "If the height of the scaled cookie is greater than the width the field of view of the spot drawer needs to be adopted. The max field of view is controlled by this value.")]
    public float spotDrawerMaxAngle = 160f;

    [Tooltip("Controls how much the height and the width of the original cookie texture can be increased.")]
    public float maxCookieSizeIncrease = 1024f;

    [Tooltip(
        "If the spray gun moves further between two frames the intensity is reduced to zero. Between zero and maxDistanceIntensity the intensity is reduced linearly.")]
    public float maxDistanceIntensity = 0.25f;

    [Tooltip(
        "The moved distance on an axis between two frames multiplied by this value results in the width and height by which the original cookie texture is extended.")]
    public float distanceToCookieScale = 15000;

    [Tooltip("The shader used to scale the cookie.")]
    public Material cookieScaleShader;

    public Material wideStreamCookieShader;

    [Header("Heatmap Flashlight Settings")] [Tooltip("The cookie used by the flashlight.")]
    public Texture flashlightCookie;

    [Tooltip("Field of view of the camera used to draw the flashlight cookie. Influences the size of the cookie.")]
    [Range(0.01f, 180f)]
    public float flashlightSpotDrawerAngle = 50f;
    
    // coat management
    public Coat chosenCoat;
    public Coat chosenBaseCoat;

    // values which are set when the coat is loaded
    [HideInInspector] public float minSprayDistance;
    [HideInInspector] public float maxSprayDistance;
    [HideInInspector] public float maxSmoothness;
    [HideInInspector] public float maxSmoothnessHeightmapThreshold;
    [HideInInspector] public float minFlowHeightmapThreshold;
    [HideInInspector] public float viscosity;
    [HideInInspector] public Color sprayColor;

    // actual used values to allow reset to predefined values
    [HideInInspector] public float actualInitMetallic;
    [HideInInspector] public float actualInitSmoothness;
    [HideInInspector] public Color actualDrawableInitColor;
    [HideInInspector] public float actualRoughnessStrength;

    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private static readonly int FullColorHeightmapThresholdID = Shader.PropertyToID("_FullColorHeightmapThreshold");
    private static readonly int AlphaToHeightmapMultiplierID = Shader.PropertyToID("_AlphaToHeightmapMultiplier");
    private static readonly int GoodHeightID = Shader.PropertyToID("_GoodHeight");
    private static readonly int ExcessiveHeightID = Shader.PropertyToID("_ExcessiveHeight");
    private static readonly int RunningHeightID = Shader.PropertyToID("_RunningHeight");

    private static readonly int StartRoughnessHeightmapThresholdID =
        Shader.PropertyToID("_StartRoughnessHeightmapThreshold");

    private static readonly int FullRoughnessHeightmapThresholdID =
        Shader.PropertyToID("_FullRoughnessHeightmapThreshold");

    private static readonly int GradientSmoothnessID = Shader.PropertyToID("_GradientSmoothness");
    private static readonly int DampFactorAfterMinThicknessID = Shader.PropertyToID("_DampFactorAfterMinThickness");

    private static readonly int CloseDistanceThicknessModifierID =
        Shader.PropertyToID("_CloseDistanceThicknessModifier");

    private static readonly int FarDistanceThicknessModifierID = Shader.PropertyToID("_FarDistanceThicknessModifier");
    private static readonly int DistanceRoughnessStrengthID = Shader.PropertyToID("_DistanceRoughnessStrength");

    // Prevent non-singleton constructor use.
    protected PaintController()
    {
    }

    public void LoadCoat(TMP_Dropdown dropdown)
    {
        LoadCoat(dropdown.options[dropdown.value].text, true);
    }

    public void LoadCoat(long id)
    {
        LoadCoat(DataController.Instance.coats[id], true);
    }

    public void LoadCoat(string coatName, bool resetDrawables)
    {
        LoadCoat(DataController.Instance.coats.First(coat => coat.Value.name == coatName).Value, resetDrawables);
    }

    /// <summary>
    /// Loads the given coat and sets all properties accordingly.
    /// </summary>
    public void LoadCoat(Coat coat, bool resetDrawables)
    {
        chosenCoat = coat;
        sprayColor = chosenCoat.color;
        heightmapToColorShader.SetColor(ColorID,
            chosenCoat.type != CoatType.Clearcoat ? sprayColor : actualDrawableInitColor);

        minSprayDistance = chosenCoat.minSprayDistance / 100;
        maxSprayDistance = chosenCoat.maxSprayDistance / 100;

        // defines maximal amount of smoothness of the painted areas
        maxSmoothness = Mathf.Clamp(chosenCoat.glossWet / 100, initSmoothness, 1f);

        // a greater value for viscosity means slower flowing
        viscosity = Mathf.Sqrt(Mathf.Clamp(chosenCoat.viscosity, minViscosity, maxViscosity) - minViscosity) /
                    Mathf.Sqrt(maxViscosity - minViscosity);

        // set shader values
        float fullColorHeightmapThreshold = targetMinThicknessWetAlpha *
                                            (chosenCoat.fullOpacityMinThicknessWet / chosenCoat.targetMinThicknessWet);
        heightmapToColorShader.SetFloat(FullColorHeightmapThresholdID, fullColorHeightmapThreshold);
        cookieToHeightmapShader.SetFloat(FullColorHeightmapThresholdID, fullColorHeightmapThreshold);
        cookieToHeightmapShader.SetFloat(DampFactorAfterMinThicknessID, dampFactorAfterMinThickness);
        spotDrawerShader.SetFloat(AlphaToHeightmapMultiplierID, thicknessModifier);
        spotDrawerShader.SetFloat(CloseDistanceThicknessModifierID, closeDistanceThicknessModifier);
        spotDrawerShader.SetFloat(FarDistanceThicknessModifierID, farDistanceThicknessModifier);
        heightmapToHeatmapShader.SetFloat(GoodHeightID, targetMinThicknessWetAlpha);
        heightmapToHeatmapShader.SetFloat(ExcessiveHeightID,
            targetMinThicknessWetAlpha * (chosenCoat.targetMaxThicknessWet / chosenCoat.targetMinThicknessWet));
        heightmapToHeatmapShader.SetFloat(GradientSmoothnessID, heatmapGradientSmoothness);
        maxSmoothnessHeightmapThreshold = targetMinThicknessWetAlpha *
                                          (chosenCoat.fullGlossMinThicknessWet / chosenCoat.targetMinThicknessWet);
        minFlowHeightmapThreshold = targetMinThicknessWetAlpha *
                                    (chosenCoat.runsStartThicknessWet / chosenCoat.targetMinThicknessWet);
        heightmapToHeatmapShader.SetFloat(RunningHeightID, minFlowHeightmapThreshold);
        actualRoughnessStrength = maxDetailNormalMapScale * (chosenCoat.roughness / 100) * viscosity;
        heightmapToRoughnessShader.SetFloat(StartRoughnessHeightmapThresholdID,
            targetMinThicknessWetAlpha *
            ((chosenCoat.fullGlossMinThicknessWet / 4) / chosenCoat.targetMinThicknessWet));
        heightmapToRoughnessShader.SetFloat(FullRoughnessHeightmapThresholdID,
            targetMinThicknessWetAlpha * (chosenCoat.fullGlossMinThicknessWet / chosenCoat.targetMinThicknessWet));
        heightmapToRoughnessShader.SetFloat(DistanceRoughnessStrengthID, roughnessOnDistance ? 1 : 0);

        if (resetDrawables)
            ResetDrawables();
    }

    public void LoadBaseCoat(string baseCoatName, bool dry = true)
    {
        LoadBaseCoat(
            baseCoatName == null ? null : DataController.Instance.coats.Values.First(coat => coat.name == baseCoatName),
            dry);
    }

    /// <summary>
    /// Needed to be callable from inspector.
    /// </summary>
    public void LoadBaseCoat(TMP_Dropdown dropdown)
    {
        LoadBaseCoat(dropdown.value == 0 ? null : dropdown.options[dropdown.value].text);
    }

    public void LoadBaseCoat(long id, bool dry = true)
    {
        LoadBaseCoat(id == 0 ? null : DataController.Instance.coats[id], dry);
    }

    /// <summary>
    /// Modifies the surface of the workpiece to make it look like it was painted with a base coat.
    /// </summary>
    public void LoadBaseCoat(Coat coat, bool dry = true)
    {
        if (coat == null)
        {
            actualInitMetallic = initMetallic;
            actualInitSmoothness = initSmoothness;
            actualDrawableInitColor = drawableInitColor;
            chosenBaseCoat = null;
        }
        else
        {
            chosenBaseCoat = coat;
            actualInitMetallic = 0.15f;
            actualInitSmoothness = (dry ? chosenBaseCoat.glossDry : chosenBaseCoat.glossWet) / 100;
            actualDrawableInitColor = chosenBaseCoat.color;
        }

        heightmapToColorShader.SetColor(ColorID,
            chosenCoat.type != CoatType.Clearcoat ? sprayColor : actualDrawableInitColor);
        ResetDrawables();
    }

    public void Awake()
    {
        actualInitMetallic = initMetallic;
        actualInitSmoothness = initSmoothness;
        actualDrawableInitColor = drawableInitColor;
        if (DataController.Instance.coats.Count > 0)
            LoadCoat(DataController.Instance.coats.Values.ToList()[0], false);
    }

    public void ResetDrawables()
    {
        drawables.ToList().ForEach(d => d.Reset());
        EvaluationController.Instance.Reset();
        PaintStandHitController.Instance.ClearHits();
        PlayRecordingController.Instance.ClearHits();
    }

    public void SetDrawable(CustomDrawable customDrawable)
    {
        // save spray test paper 
        CustomDrawable sprayTestPaper = drawables[0];
        drawables = new[] { sprayTestPaper, customDrawable };
    }
}
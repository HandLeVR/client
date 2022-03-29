using System.Linq;
using UnityEngine;

/// <summary>
/// A beaker that can be used to estimate the amount of a fluid.
/// </summary>
public class Beaker : EstimationObject
{
    public SkinnedMeshRenderer fluidRenderer;
    public Renderer rangeDisplayRenderer;
    [Range(0.0f, 100.0f)] public float percentageLevel;
    public Color fluidColor;

    [HideInInspector] public bool beakerSet;
    [HideInInspector] public float minPercentage;
    [HideInInspector] public float maxPercentage;
    [HideInInspector] public float estimationRangeZeroPosition; // = 0.5042f
    [HideInInspector] public float estimationRangeMaxPosition; // = 0.5447f

    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void OnEnable()
    {
        beakerSet = false;
        SetPercentagesRangeDisplay(minPercentage, maxPercentage);
        fluidRenderer.sharedMaterial.SetColor(ColorId, fluidColor);
        rangeDisplayRenderer.sharedMaterial.SetColor(ColorId, Color.clear);
    }

    void Update()
    {
        SetPercentagesRangeDisplay(minPercentage, maxPercentage);
        SetPercentageFluidLevel();
    }

    /// <summary>
    /// Set the fluid level based on a percentage value.
    /// </summary>
    void SetPercentageFluidLevel()
    {
        fluidRenderer.SetBlendShapeWeight(0, 100 - Mathf.Clamp(percentageLevel, 0, 100));
    }

    /// <summary>
    /// Locks the beaker when continue coin was selected and displays correct range
    /// </summary>
    /// <returns>result of the task</returns>
    public override bool ShowSolution()
    {
        beakerSet = true;
        bool correct = percentageLevel > minPercentage && percentageLevel < maxPercentage;

        StartCoroutine(Lerp.ColorShared(rangeDisplayRenderer, rangeDisplayRenderer.sharedMaterial.color,
            new Color(0, 1, 0, 0.5f), 0.5f));
        if (correct)
        {
            StartCoroutine(Lerp.ColorShared(fluidRenderer, fluidRenderer.sharedMaterial.color, Color.green, 0.5f));
        }
        else
        {
            StartCoroutine(Lerp.ColorShared(fluidRenderer, fluidRenderer.sharedMaterial.color, Color.red, 0.5f));
            StartCoroutine(WaitFor.Seconds(0.5f,
                () => StartCoroutine(Lerp.Float(f => percentageLevel = f, percentageLevel,
                    (maxPercentage - minPercentage) / 2f + minPercentage, 3))));
            StartCoroutine(WaitFor.Seconds(2.5f,
                () => StartCoroutine(Lerp.ColorShared(fluidRenderer, fluidRenderer.sharedMaterial.color, Color.green,
                    0.5f))));
        }

        return correct;
    }

    public override void Reset()
    {
        beakerSet = false;
        percentageLevel = 0;
        rangeDisplayRenderer.sharedMaterial.SetColor(ColorId, Color.clear);
    }

    /// <summary>
    /// Calculates and passes the new upper and lower limit for the estimation range shader
    /// </summary>
    /// <param name="min">min value passed from task</param>
    /// <param name="max">max value passed from task</param>
    void SetPercentagesRangeDisplay(float min, float max)
    {
        float newLowerLimit = Mathf.Lerp(estimationRangeZeroPosition, estimationRangeMaxPosition, min / 100);
        float newUpperLimit = Mathf.Lerp(estimationRangeZeroPosition, estimationRangeMaxPosition, max / 100);

        rangeDisplayRenderer.sharedMaterial.SetFloat("_ClipRangeLowerLimit", newLowerLimit);
        rangeDisplayRenderer.sharedMaterial.SetFloat("_ClipRangeUpperLimit", newUpperLimit);
    }

    /// <summary>
    /// Method used to change the level of the fluid by controller interaction from beaker.cs
    /// </summary>
    /// <param name="value"></param>
    public void ChangeFluidLevelValue(float value)
    {
        percentageLevel = percentageLevel + value;
    }

    public override void FadeIn()
    {
        GetComponentsInChildren<Renderer>().ToList().ForEach(r => r.FadeMaterialsToOriginalAlpha(0.5f));
    }

    public override void FadeOut()
    {
        GetComponentsInChildren<Renderer>().ToList()
            .FadeOutMaterialsAndSetOriginalAlpha(0.5f, () => gameObject.SetActive(false));
    }
}
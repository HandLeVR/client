using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the slider component (Background gradient, slider handle, sliders range).
/// </summary>
public class TimePeriodColorBlock : MonoBehaviour
{
    public Transform Background;
    
    [HideInInspector] public bool doNotUnfreezeMe;

    private Slider Slider;
    private Image[] Segments;

    /// <summary>
    /// Find the GUI-Elements
    /// </summary>
    private void Awake()
    {
        Slider = GetComponentInChildren<Slider>();
        Segments = Background.GetComponentsInChildren<Image>();
    }

    public void SetUpSegments(EvaluationParameterValues values, bool stayFrozen = false)
    {
        float border1 = values.optimalValue - values.threshold3;
        float border2 = values.optimalValue - values.threshold2;
        float border3 = values.optimalValue - values.threshold1;
        float border4 = values.optimalValue + values.threshold1;
        float border5 = values.optimalValue + values.threshold2;
        float border6 = values.optimalValue + values.threshold3;
        SetSize(Segments[0], values.lowerBound, values.upperBound, values.lowerBound, border1);
        SetSize(Segments[1], values.lowerBound, values.upperBound, border1, border2);
        SetSize(Segments[2], values.lowerBound, values.upperBound, border2, border3);
        SetSize(Segments[3], values.lowerBound, values.upperBound, border3, border4);
        SetSize(Segments[4], values.lowerBound, values.upperBound, border4, border5);
        SetSize(Segments[5], values.lowerBound, values.upperBound, border5, border6);
        SetSize(Segments[6], values.lowerBound, values.upperBound, border6, values.upperBound);
        doNotUnfreezeMe = stayFrozen;
        Slider.minValue = values.lowerBound;
        Slider.maxValue = values.upperBound;
    }

    /// <summary>
    /// Sets the size of the given segment.
    /// </summary>
    private void SetSize(Image segment, float lowerBound, float upperBound, float segmentLowerBound, float segmentUpperBound)
    {
        float newLowerBound = Math.Max(lowerBound, segmentLowerBound);
        float newUpperBound = Math.Min(upperBound, segmentUpperBound);
        float range = upperBound - lowerBound;
        float newRange = newUpperBound - newLowerBound;
        segment.GetComponent<LayoutElement>().flexibleWidth = segmentLowerBound > segmentUpperBound ? 0 : newRange / range;
    }

    /// <summary>
    /// Set the sliders value to a given value.
    /// </summary>
    public void SetSliderValue(float value)
    {
        Slider.value = value;
    }

    /// <summary>
    /// Show the sliders background frozen in gray. Hide the handle as it has no functionality rn.
    /// </summary>
    public void FreezeSlider()
    {
        Slider.handleRect.gameObject.SetActive(false);
        Segments.ToList().ForEach(image => image.material = ReflectionToolUIController.Instance.grayscaleMaterial);
    }

    /// <summary>
    /// Show the sliders background in nice colorful mode. Show the sliders handle.
    /// </summary>
    public void UnfreezeSlider()
    {
        if (doNotUnfreezeMe) return;
        Slider.handleRect.gameObject.SetActive(true);
        Segments.ToList().ForEach(image => image.material = null);
    }
}
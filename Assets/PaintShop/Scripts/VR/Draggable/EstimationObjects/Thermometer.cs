using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A thermometer that can be used to estimate temperatures.
/// </summary>
public class Thermometer : EstimationObject
{
    private float minValue = 0.111f;
    private float maxValue = 0.9965f;
    [Range(0, 100)] public float currentFillPercentage;
    public TextMeshProUGUI currentValue;
    public CanvasGroup canvasGroup;
    public Image liquid;
    public Image correctRangeIndicator;

    [HideInInspector] public float correctPercentageMin;
    [HideInInspector] public float correctPercentageMax;
    [HideInInspector] public bool thermometerSet;

    private const float _correctRangeMaxIndicatorPosition = 0.265f;
    private const float _correctRangeMinIndicatorPosition = -0.4267f;

    private void OnEnable()
    {
        thermometerSet = false;
        correctRangeIndicator.color = new Color(0, 1, 0, 0f);
        canvasGroup.alpha = 0;
        SetCorrectRange();
    }

    void Update()
    {
        SetPercentage();
    }

    /// <summary>
    /// Sets size and position of the correct range indicator
    /// </summary>
    private void SetCorrectRange()
    {
        float range = correctPercentageMax - correctPercentageMin;
        Vector3 rangeVector = new Vector3(1, range / 100, 1);
        correctRangeIndicator.transform.localScale = rangeVector;

        float y_pos = Mathf.Lerp(_correctRangeMinIndicatorPosition, _correctRangeMaxIndicatorPosition,
            ((correctPercentageMin + correctPercentageMax) / 2) / 100);
        Vector3 posVector = new Vector3(correctRangeIndicator.transform.localPosition.x, y_pos,
            correctRangeIndicator.transform.localPosition.z);
        correctRangeIndicator.transform.localPosition = posVector;
    }

    /// <summary>
    /// Sets the fill value of the thermometer liquid
    /// </summary>
    private void SetPercentage()
    {
        float value = Mathf.Lerp(minValue, maxValue, currentFillPercentage / 100);
        liquid.fillAmount = value;
        currentValue.text = Convert.ToInt32(Mathf.Clamp(currentFillPercentage, 0, 100)) + "°C";
    }

    private void FadeInCorrectRangeIndicator()
    {
        StartCoroutine(Lerp.Color(correctRangeIndicator, correctRangeIndicator.color, new Color(0, 1, 0, 0.5f), 0.2f));
    }

    public override bool ShowSolution()
    {
        thermometerSet = true;
        bool correct = currentFillPercentage >= correctPercentageMin && currentFillPercentage <= correctPercentageMax;

        if (correct)
        {
            StartCoroutine(Lerp.Color(liquid, liquid.color, Color.green, 0.5f));
        }
        else
        {
            StartCoroutine(Lerp.Color(liquid, liquid.color, Color.red, 0.5f));
            StartCoroutine(WaitFor.Seconds(0.5f, () => StartCoroutine(Lerp.Float(f => currentFillPercentage = f,
                currentFillPercentage, (correctPercentageMax - correctPercentageMin) / 2f + correctPercentageMin, 3))));
            StartCoroutine(WaitFor.Seconds(2.5f, () => StartCoroutine(Lerp.Color(liquid, liquid.color, new Color(0, 1, 0, 0.5f), 0.5f))));
        }

        FadeInCorrectRangeIndicator();

        return correct;
    }

    public override void Reset()
    {
        thermometerSet = false;
        currentFillPercentage = 0;
        liquid.color = Color.red;
        correctRangeIndicator.color = new Color(0, 1, 0, 0f);
        canvasGroup.alpha = 0;
    }

    public void ChangeLevelValue(float value)
    {
        currentFillPercentage = currentFillPercentage + value;
    }

    public override void FadeIn()
    {
        GetComponentsInChildren<Renderer>().ToList().ForEach(r => r.FadeMaterialsToOriginalAlpha(0.5f));
        StartCoroutine(Lerp.Alpha(canvasGroup, 1, 0.5f));
    }

    public override void FadeOut()
    {
        GetComponentsInChildren<Renderer>().ToList()
            .FadeOutMaterialsAndSetOriginalAlpha(0.5f, () => gameObject.SetActive(false));
        StartCoroutine(Lerp.Alpha(canvasGroup, 0, 0.5f));
    }
}
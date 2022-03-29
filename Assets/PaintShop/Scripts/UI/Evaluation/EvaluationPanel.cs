using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EvaluationPanel : MonoBehaviour
{
    public TextMeshProUGUI correctDistanceText;
    public TextMeshProUGUI correctAngleText;
    public TextMeshProUGUI colorConsumptionText;
    public TextMeshProUGUI colorWastageText;
    public TextMeshProUGUI colorUsageText;
    public TextMeshProUGUI fullyPressedText;
    public TextMeshProUGUI averageSpeedText;
    public TextMeshProUGUI averageCoatThicknessText;

    public Toggle toolMagnifyingGlassToggle;
    public Toggle toolFlashlightToggle;
    public Toggle coatApplicationToggle;
    public Toggle coatThicknessToggle;

    [Header("Evaluation Rating Settings")] public EvaluationRating correctDistanceRating;
    public EvaluationRating correctAngleRating;
    public EvaluationRating colorConsumptionRating;
    public EvaluationRating colorWastageRating;
    public EvaluationRating colorUsageRating;
    public EvaluationRating fullyPressedRating;
    public EvaluationRating averageSpeedRating;
    public EvaluationRating averageCoatThicknessRating;
    
    private readonly string CorrectDistanceString = "{0} %";
    private readonly string CorrectAngleString = "{0} %";
    private readonly string ColorConsumptionString = "{0} ml";
    private readonly string ColorWastageString = "{0} ml";
    private readonly string ColorUsageString = "{0} ml";
    private readonly string FullyPressedString = "{0} %";
    private readonly string AverageSpeedString = "{0} m/s";
    private readonly string AverageCoatThicknessString = "{0} µm";

    void OnEnable()
    {
        ApplicationController.Instance.SetEvaluationModeActive(true);
        toolFlashlightToggle.isOn = false;
        toolMagnifyingGlassToggle.isOn = true;

        correctDistanceText.text = String.Format(CorrectDistanceString,
            $"{EvaluationController.Instance.GetCorrectDistancePercentage():0.00}");
        correctDistanceRating.UpdateSprites(EvaluationController.Instance.GetCorrectDistancePercentage(),
            EvaluationParameterUtil.GetCorrectDistanceValues());

        correctAngleText.text = String.Format(CorrectAngleString,
            $"{EvaluationController.Instance.GetCorrectAnglePercentage():0.00}");
        correctAngleRating.UpdateSprites(EvaluationController.Instance.GetCorrectAnglePercentage(),
            EvaluationParameterUtil.GetCorrectAngleValues());

        colorConsumptionText.text = String.Format(ColorConsumptionString,
            $"{EvaluationController.Instance.GetColorConsumption():0.00}");
        colorConsumptionRating.UpdateSprites(EvaluationController.Instance.GetColorConsumption(),
            EvaluationParameterUtil.GetColorConsumptionValues());

        colorWastageText.text =
            String.Format(ColorWastageString, $"{EvaluationController.Instance.GetColorWastage():0.00}");
        colorWastageRating.UpdateSprites(EvaluationController.Instance.GetColorWastage(),
            EvaluationParameterUtil.GetColorWastageValues());

        colorUsageText.text = String.Format(ColorUsageString, $"{EvaluationController.Instance.GetColorUsage():0.00}");
        colorUsageRating.UpdateSprites(EvaluationController.Instance.GetColorUsage(),
            EvaluationParameterUtil.GetColorUsageValues());

        fullyPressedText.text = String.Format(FullyPressedString,
            $"{EvaluationController.Instance.GetFullyPressedPercentage():0.00}");
        fullyPressedRating.UpdateSprites(EvaluationController.Instance.GetFullyPressedPercentage(),
            EvaluationParameterUtil.GetFullyPressedValues());

        averageSpeedText.text =
            String.Format(AverageSpeedString, $"{EvaluationController.Instance.GetAverageSpeed():0.00}");
        averageSpeedRating.UpdateSprites(EvaluationController.Instance.GetAverageSpeed(),
            EvaluationParameterUtil.GetAverageSpeedValues());

        averageCoatThicknessText.text = String.Format(AverageCoatThicknessString,
            $"{EvaluationController.Instance.GetCurrentCoatThickness():0.00}");
        averageCoatThicknessRating.UpdateSprites(EvaluationController.Instance.GetCurrentCoatThickness(),
            EvaluationParameterUtil.GetAverageCoatThicknessValues());

        coatThicknessToggle.isOn = !ApplicationController.Instance.showHeatmapOnWorkpiece;
        coatApplicationToggle.isOn = ApplicationController.Instance.showHeatmapOnWorkpiece;
    }

    void OnDisable()
    {
        // need to check because this throws an error message on scene change otherwise
        if (ApplicationController.Instance)
            ApplicationController.Instance.SetEvaluationModeActive(false);
    }

    public void ToggleEvaluationTool()
    {
        if (ApplicationController.Instance.currentTool == SprayGun.Tool.Flashlight)
        {
            ApplicationController.Instance.currentTool = SprayGun.Tool.MagnifyingGlass;
            toolFlashlightToggle.isOn = false;
            toolMagnifyingGlassToggle.isOn = true;
        }
        else
        {
            ApplicationController.Instance.currentTool = SprayGun.Tool.Flashlight;
            toolMagnifyingGlassToggle.isOn = false;
            toolFlashlightToggle.isOn = true;
        }
    }

    public void ToggleCoatApplication()
    {
        ApplicationController.Instance.InvertHeatmapVisibility();
        coatThicknessToggle.isOn = !ApplicationController.Instance.showHeatmapOnWorkpiece;
        coatApplicationToggle.isOn = ApplicationController.Instance.showHeatmapOnWorkpiece;
    }
}
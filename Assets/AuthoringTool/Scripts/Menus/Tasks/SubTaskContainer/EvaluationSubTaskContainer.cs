using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a evaluation sub task.
/// </summary>
public class EvaluationSubTaskContainer : BaseSubTaskContainer
{
    public Toggle heatMapToggle;
    public GameObject paramsContainer;
    public Toggle correctDistanceToggle;
    public Toggle correctAngleToggle;
    public Toggle colorConsumptionToggle;
    public Toggle wastedColorToggle;
    public Toggle colorUsageToggle;
    public Toggle fullyPressedToggle;
    public Toggle averageSpeedToggle;
    public Toggle coatThicknessToggle;
    public TMP_InputField audioInputField;
    public TMP_InputField speechBubbleInputField;
    public Toggle automaticAudioToggle;
    public TMP_InputField skippableInputField;


    protected override void SetUpByProperties()
    {
        SetSkippableInputField(skippableInputField);
        SetToggle(heatMapToggle, "heatMap",true);
        SetToggle(correctDistanceToggle, "correctDistance");
        SetToggle(correctAngleToggle, "correctAngle");
        SetToggle(colorConsumptionToggle, "colorConsumption");
        SetToggle(wastedColorToggle, "colorWastage");
        SetToggle(colorUsageToggle, "colorUsage");
        SetToggle(fullyPressedToggle, "fullyPressed");
        SetToggle(averageSpeedToggle, "averageSpeed");
        SetToggle(coatThicknessToggle, "coatThickness");
        bool parameterActive = correctDistanceToggle.isOn || correctAngleToggle.isOn ||
                               colorConsumptionToggle.isOn || wastedColorToggle.isOn || colorUsageToggle.isOn ||
                               fullyPressedToggle.isOn || averageSpeedToggle.isOn || averageSpeedToggle.isOn;
        paramsContainer.gameObject.SetActive(parameterActive);
        SetInstructorSettings(audioInputField, speechBubbleInputField, automaticAudioToggle);
    }

    public override bool ValuesMissing()
    {
        return !(correctDistanceToggle.isOn || correctAngleToggle.isOn ||
                 colorConsumptionToggle.isOn || wastedColorToggle.isOn || colorUsageToggle.isOn ||
                 fullyPressedToggle.isOn|| averageSpeedToggle.isOn || averageSpeedToggle.isOn);
    }
}


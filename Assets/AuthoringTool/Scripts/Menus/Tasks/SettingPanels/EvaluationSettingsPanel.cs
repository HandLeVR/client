using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents the settings panel for the evaluation task.
/// </summary>
public class EvaluationSettingsPanel : BaseSettingsPanel
{
    public Toggle heatmapToggle;
    public Toggle correctDistanceToggle;
    public Toggle correctAngleToggle;
    public Toggle colorConsumptionToggle;
    public Toggle wastedPaintToggle;
    public Toggle appliedColorToggle;
    public Toggle pressedTriggerToggle;
    public Toggle speedToggle;
    public Toggle coatThicknessToggle;
    public TMP_InputField audioInput;
    public TMP_InputField speechBubbleInput;
    public Toggle automaticAudioToggle;
    public TMP_Dropdown skippableDropdown;

    public void Awake()
    {
        InitSkippableDropdown(skippableDropdown, false);
        heatmapToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        correctDistanceToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        correctAngleToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        colorConsumptionToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        wastedPaintToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        appliedColorToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        pressedTriggerToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        speedToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        coatThicknessToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        InitInstructorSettings(audioInput, speechBubbleInput, automaticAudioToggle);
    }

    protected override void SetUpByProperties()
    {
        SetSkippableDropdown(skippableDropdown);
        SetToggle(heatmapToggle, "heatMap", true);
        SetToggle(correctDistanceToggle, "correctDistance", true);
        SetToggle(correctAngleToggle, "correctAngle", true);
        SetToggle(colorConsumptionToggle, "colorConsumption", true);
        SetToggle(wastedPaintToggle, "colorWastage", true);
        SetToggle(appliedColorToggle, "colorUsage", true);
        SetToggle(pressedTriggerToggle, "fullyPressed", true);
        SetToggle(speedToggle, "averageSpeed", true);
        SetToggle(coatThicknessToggle, "coatThickness", true);
        SetInstructorSettings(audioInput, speechBubbleInput, automaticAudioToggle);
    }

    protected override void SetJSON()
    {
        SubTask subTask = relatedSubTaskContainer.SubTaskData;
        JObject json = new JObject
        {
            {"heatMap", heatmapToggle.isOn},
            {"correctDistance", correctDistanceToggle.isOn},
            {"correctAngle", correctAngleToggle.isOn},
            {"colorConsumption", colorConsumptionToggle.isOn},
            {"colorWastage", wastedPaintToggle.isOn},
            {"colorUsage", appliedColorToggle.isOn},
            {"fullyPressed", pressedTriggerToggle.isOn},
            {"averageSpeed", speedToggle.isOn},
            {"coatThickness", coatThicknessToggle.isOn}
        };
        SetSkippableDropdownJSON(json, skippableDropdown);
        SetInstructorJSON(json, audioInput, speechBubbleInput, automaticAudioToggle);
        subTask.properties = json.ToString();
    }
}

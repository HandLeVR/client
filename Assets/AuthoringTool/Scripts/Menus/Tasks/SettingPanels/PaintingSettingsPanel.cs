using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents the settings panel for the painting task.
/// </summary>
public class PaintingSettingsPanel : BaseSettingsPanel
{
    public TMP_InputField monitorInput;
    public TMP_Dropdown workpieceDropdown;
    public TMP_InputField baseCoatInput;
    public TMP_InputField coatInput;
    public Toggle distanceRayToggle;
    public Toggle distanceMarkerToggle;
    public Toggle angleRayToggle;
    public Toggle optionDistanceRayToggle;
    public Toggle optionDistanceMarkerToggle;
    public Toggle optionAngleRayToggle;
    public TMP_InputField minTimeInput;
    public TMP_InputField audioInput;
    public TMP_InputField speechBubbleInput;
    public Toggle automaticAudioToggle;
    public TMP_Dropdown skippableDropdown;
    public TMP_InputField helpDurationInput;

    public void Awake()
    {
        monitorInput.onEndEdit.AddListener(delegate { SaveSettings(); });
        distanceRayToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        distanceMarkerToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        angleRayToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        optionDistanceRayToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        optionDistanceMarkerToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        optionAngleRayToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        minTimeInput.onEndEdit.AddListener(delegate { SaveSettings(); });
        helpDurationInput.onEndEdit.AddListener(delegate { SaveSettings(); });
        InitSkippableDropdown(skippableDropdown, false);
        InitCoatInput(baseCoatInput, true, noCoatEntry: true, noClearCoat: true);
        InitCoatInput(coatInput, true);
        InitWorkpieceDropdown(workpieceDropdown);
        InitInstructorSettings(audioInput, speechBubbleInput, automaticAudioToggle);
    }

    protected override void SetUpByProperties()
    {
        SetTextInputField(monitorInput, "textMonitor");
        SetWorkpieceDropdown(workpieceDropdown, "workpieceId");
        SetCoatInput(baseCoatInput, "baseCoatId");
        SetCoatInput(coatInput, "coatId");
        SetToggle(distanceRayToggle, "distanceRay");
        SetToggle(distanceMarkerToggle, "distanceMarker");
        SetToggle(angleRayToggle, "angleRay");
        SetToggle(optionDistanceRayToggle, "optionDistanceRay");
        SetToggle(optionDistanceMarkerToggle, "optionDistanceMarker");
        SetToggle(optionAngleRayToggle, "optionAngleRay");
        SetTextInputField(minTimeInput, "minSprayTime", "0");
        SetTextInputField(helpDurationInput, "helpDuration");
        SetSkippableDropdown(skippableDropdown);
        SetInstructorSettings(audioInput, speechBubbleInput, automaticAudioToggle);
    }

    protected override void SetJSON()
    {
        SubTask subTask = relatedSubTaskContainer.SubTaskData;
        CheckValidInput(minTimeInput, 0, 10, 10);
        CheckValidInput(helpDurationInput, 0, 99, 99);
        JObject json = new JObject
        {
            { "textMonitor", monitorInput.text },
            { "distanceRay", distanceRayToggle.isOn },
            { "distanceMarker", distanceMarkerToggle.isOn },
            { "angleRay", angleRayToggle.isOn },
            { "optionDistanceRay", optionDistanceRayToggle.isOn },
            { "optionDistanceMarker", optionDistanceMarkerToggle.isOn },
            { "optionAngleRay", optionAngleRayToggle.isOn },
            { "minSprayTime", minTimeInput.text },
            { "helpDuration", helpDurationInput.text }
        };
        SetSkippableDropdownJSON(json, skippableDropdown);
        SetWorkpieceDropdownJSON(json, workpieceDropdown, "workpieceId");
        SetCoatInputJSON(json, baseCoatInput, "baseCoatId");
        SetCoatInputJSON(json, coatInput, "coatId");
        SetInstructorJSON(json, audioInput, speechBubbleInput, automaticAudioToggle);
        subTask.properties = json.ToString();
    }
}
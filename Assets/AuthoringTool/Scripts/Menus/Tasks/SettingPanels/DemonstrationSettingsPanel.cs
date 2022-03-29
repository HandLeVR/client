using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents the settings panel for the demonstration task.
/// </summary>
public class DemonstrationSettingsPanel : BaseSettingsPanel
{
    public TMP_InputField monitorInput;
    public TMP_InputField recordInput;
    public TMP_InputField baseCoatInput;
    public TMP_InputField coatInput;
    public Toggle distanceRayToggle;
    public Toggle distanceMarkerToggle;
    public Toggle angleRayToggle;
    public TMP_InputField audioInput;
    public TMP_InputField speechBubbleInput;
    public Toggle automaticAudioToggle;
    public TMP_Dropdown skippableDropdown;

    public void Awake()
    {
        monitorInput.onEndEdit.AddListener(delegate { SaveSettings(); });
        distanceRayToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        distanceMarkerToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        angleRayToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        InitSkippableDropdown(skippableDropdown);
        InitRecordingInput(recordInput);
        InitCoatInput(baseCoatInput, true, true, true, true);
        InitCoatInput(coatInput, true, true);
        InitInstructorSettings(audioInput, speechBubbleInput, automaticAudioToggle);
    }

    protected override void SetUpByProperties()
    {
        SetTextInputField(monitorInput, "textMonitor");
        SetRecordingInput(recordInput, "recordingId");
        SetCoatInput(baseCoatInput, "baseCoatId");
        SetCoatInput(coatInput, "coatId");
        SetToggle(distanceRayToggle, "distanceRay");
        SetToggle(distanceMarkerToggle, "distanceMarker");
        SetToggle(angleRayToggle, "angleRay");
        SetSkippableDropdown(skippableDropdown);
        SetInstructorSettings(audioInput, speechBubbleInput, automaticAudioToggle);
    }

    protected override void SetJSON()
    {
        SubTask subTask = relatedSubTaskContainer.SubTaskData;
        JObject json = new JObject
        {
            {"textMonitor", monitorInput.text},
            {"distanceRay", distanceRayToggle.isOn},
            {"distanceMarker", distanceMarkerToggle.isOn},
            {"angleRay", angleRayToggle.isOn},
        };
        SetRecordingInputJSON(json, recordInput, "recordingId");
        SetCoatInputJSON(json, baseCoatInput, "baseCoatId");
        SetCoatInputJSON(json, coatInput, "coatId");
        SetInstructorJSON(json, audioInput, speechBubbleInput, automaticAudioToggle);
        SetSkippableDropdownJSON(json, skippableDropdown);
        subTask.properties = json.ToString();
    }
}
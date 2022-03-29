using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents the settings panel for the spray test task.
/// </summary>
public class SprayTestSettingsPanel : BaseSettingsPanel
{
    public TMP_InputField monitorInput;
    public TMP_InputField errorProbabilityInput;
    public Toggle splitToggle;
    public Toggle middleMaterialToggle;
    public Toggle curvedToggle;
    public Toggle shiftedToggle;
    public Toggle sShapedToggle;
    public Toggle flutteringToggle;
    public Toggle distanceRayToggle;
    public Toggle distanceMarkerToggle;
    public Toggle angleRayToggle;
    public TMP_InputField coatInput;
    public TMP_Dropdown skippableDropdown;
    public TMP_InputField audioInput;
    public TMP_InputField speechBubbleInput;
    public Toggle automaticAudioToggle;
    public TMP_InputField finalAudioInput;

    public void Awake()
    {
        monitorInput.onEndEdit.AddListener(delegate { SaveSettings(); });
        errorProbabilityInput.onEndEdit.AddListener(delegate { SaveSettings(); });
        splitToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        middleMaterialToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        curvedToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        shiftedToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        sShapedToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        flutteringToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        distanceRayToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        distanceMarkerToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        angleRayToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        InitSkippableDropdown(skippableDropdown);
        InitCoatInput(coatInput, true);
        InitAudioInput(finalAudioInput);
        InitInstructorSettings(audioInput, speechBubbleInput, automaticAudioToggle);
    }

    protected override void SetUpByProperties()
    {
        SetTextInputField(monitorInput, "textMonitor");
        SetCoatInput(coatInput, "coatId");
        SetTextInputField(errorProbabilityInput, "errorRate", "0");
        SetToggle(splitToggle, "splittedSpray");
        SetToggle(middleMaterialToggle, "excessiveMaterial");
        SetToggle(curvedToggle, "oneSidedCurved");
        SetToggle(shiftedToggle, "oneSidedDisplaced");
        SetToggle(sShapedToggle, "sShaped");
        SetToggle(flutteringToggle, "flutteringSpray");
        SetToggle(distanceRayToggle, "distanceRay");
        SetToggle(distanceMarkerToggle, "distanceMarker");
        SetToggle(angleRayToggle, "angleRay");
        SetSkippableDropdown(skippableDropdown);
        SetAudioInput(finalAudioInput, "finalAudioId");
        SetInstructorSettings(audioInput, speechBubbleInput, automaticAudioToggle);
    }

    protected override void SetJSON()
    {
        SubTask subTask = relatedSubTaskContainer.SubTaskData;
        CheckValidInput(errorProbabilityInput, 0, 100, 100);
        JObject json = new JObject
        {
            {"textMonitor", monitorInput.text},
            {"errorRate", errorProbabilityInput.text},
            {"splittedSpray", splitToggle.isOn},
            {"excessiveMaterial", middleMaterialToggle.isOn},
            {"oneSidedCurved", curvedToggle.isOn},
            {"oneSidedDisplaced", shiftedToggle.isOn},
            {"sShaped", sShapedToggle.isOn},
            {"flutteringSpray", flutteringToggle.isOn},
            {"distanceRay", distanceRayToggle.isOn},
            {"distanceMarker", distanceMarkerToggle.isOn},
            {"angleRay", angleRayToggle.isOn}
        };
        SetSkippableDropdownJSON(json, skippableDropdown);
        SetAudioInputJSON(json, finalAudioInput, "finalAudioId");
        SetCoatInputJSON(json, coatInput, "coatId");
        SetInstructorJSON(json, audioInput, speechBubbleInput, automaticAudioToggle);
        subTask.properties = json.ToString();
    }
}
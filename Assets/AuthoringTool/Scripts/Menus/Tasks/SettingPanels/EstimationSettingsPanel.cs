using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents the settings panel for the estimation task.
/// </summary>
public class EstimationSettingsPanel : BaseSettingsPanel
{
    public TMP_InputField monitorInput;
    public TMP_InputField audioInput;
    public Toggle automaticAudioToggle;
    public TMP_Dropdown skippableDropdown;
    public TMP_InputField speechBubbleInput;
    public TMP_InputField finalAudioInput;
    public TMP_Dropdown objectDropdown;
    public TMP_InputField minimumInputField;
    public TextMeshProUGUI minimumUnitLabel;
    public TMP_InputField maximumInputField;
    public TextMeshProUGUI maximumUnitLabel;

    public void Awake()
    {
        monitorInput.onEndEdit.AddListener(delegate { SaveSettings(); });
        minimumInputField.onEndEdit.AddListener(delegate { SaveSettings(); });
        maximumInputField.onEndEdit.AddListener(delegate { SaveSettings(); });
        objectDropdown.onValueChanged.AddListener(delegate { SaveSettings(); });
        InitSkippableDropdown(skippableDropdown, addAllSkippableEntry:false);
        InitAudioInput(finalAudioInput);
        InitInstructorSettings(audioInput, speechBubbleInput, automaticAudioToggle);
    }

    protected override void SetUpByProperties()
    {
        SetTextInputField(monitorInput, "textMonitor");
        SetObjectDropdown(objectDropdown, "interactiveObject");
        SetTextInputField(minimumInputField, "minimum");
        SetTextInputField(maximumInputField, "maximum");
        SetAudioInput(finalAudioInput, "finalAudioId");
        SetSkippableDropdown(skippableDropdown);
        SetInstructorSettings(audioInput, speechBubbleInput, automaticAudioToggle);
        UpdateLabels();
    }

    protected override void SetJSON()
    {
        SubTask subTask = relatedSubTaskContainer.SubTaskData;
        int maxValue = objectDropdown.value == 0 ? 720 : objectDropdown.value == 1 ? 1000 : 100;
        CheckValidInput(minimumInputField, 0, maxValue, maxValue);
        CheckValidInput(maximumInputField, 0, maxValue, maxValue);
        JObject json = new JObject
        {
            {"textMonitor", monitorInput.text},
            {"minimum", minimumInputField.text},
            {"maximum", maximumInputField.text}
        };
        SetSkippableDropdownJSON(json, skippableDropdown);
        SetObjectDropdownJSON(json, objectDropdown, "interactiveObject");
        SetAudioInputJSON(json, finalAudioInput, "finalAudioId");
        SetInstructorJSON(json, audioInput, speechBubbleInput, automaticAudioToggle);
        UpdateLabels();
        subTask.properties = json.ToString();
    }

    /// <summary>
    /// Updates the unit labels in dependence of the selected estimation object.
    /// </summary>
    private void UpdateLabels()
    {
        string unit = "min";
        if (objectDropdown.value == 1)
            unit = "ml";
        else if (objectDropdown.value == 2)
            unit = "°C";
        minimumUnitLabel.text = maximumUnitLabel.text = unit;
    }

    private void SetObjectDropdown(TMP_Dropdown dropdown, string propertyName)
    {
        if (!properties.TryGetValue(propertyName, out JToken objectName)) return;
        if ((string) objectName == "clock")
            dropdown.value = 0;
        else if ((string) objectName == "beaker")
            dropdown.value = 1;
        else
            dropdown.value = 2;
    }

    private void SetObjectDropdownJSON(JObject json, TMP_Dropdown dropdown, string propertyName)
    {
        if (dropdown.value == 0)
            json.Add(propertyName,"clock");
        else if (dropdown.value == 1)
            json.Add(propertyName, "beaker");
        else
            json.Add(propertyName, "thermometer");
    }
}
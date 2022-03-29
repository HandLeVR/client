﻿using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents a settings panel for a self assessment task.
/// </summary>
public class SelfAssessmentSettingsPanel : BaseSettingsPanel
{
    public TMP_InputField monitorInput;
    public TMP_InputField audioInput;
    public TMP_InputField speechBubbleInput;
    public Toggle automaticAudioToggle;
    public TMP_Dropdown skippableDropdown;

    public void Awake()
    {
        monitorInput.onEndEdit.AddListener(delegate { SaveSettings(); });
        InitSkippableDropdown(skippableDropdown, addAllSkippableEntry: false);
        InitInstructorSettings(audioInput, speechBubbleInput, automaticAudioToggle);
    }

    protected override void SetUpByProperties()
    {
        SetTextInputField(monitorInput, "textMonitor");
        SetSkippableDropdown(skippableDropdown);
        SetInstructorSettings(audioInput, speechBubbleInput, automaticAudioToggle);
    }

    protected override void SetJSON()
    {
        SubTask subTask = relatedSubTaskContainer.SubTaskData;
        JObject json = new JObject
        {
            { "textMonitor", monitorInput.text }
        };
        SetSkippableDropdownJSON(json, skippableDropdown);
        SetInstructorJSON(json, audioInput, speechBubbleInput, automaticAudioToggle);
        subTask.properties = json.ToString();
    }
}
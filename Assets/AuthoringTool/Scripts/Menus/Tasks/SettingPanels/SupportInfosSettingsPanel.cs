using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents the settings panel for the support info task.
/// </summary>
public class SupportInfosSettingsPanel : BaseSettingsPanel
{
    public TMP_InputField monitorInput;
    public GameObject supportInfoDropArea;
    public TMP_InputField minNumberInput;
    public TMP_InputField reminderAudioInput;
    public TMP_InputField finalReminderAudioInput;
    public TMP_InputField audioInput;
    public TMP_InputField speechBubbleInput;
    public Toggle automaticAudioToggle;
    public Toggle sequenceToggle;
    public TMP_Dropdown skippableDropdown;

    private int _maxNumSupportiveInfos = 6;
    public Transform warningContainer;
    public TextMeshProUGUI warningMessage;

    public void Awake()
    {
        monitorInput.onEndEdit.AddListener(delegate { SaveSettings(); });
        minNumberInput.onEndEdit.AddListener(delegate { SaveSettings(); });
        sequenceToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        InitSkippableDropdown(skippableDropdown, addAllSkippableEntry:false);
        InitAudioInput(reminderAudioInput);
        InitAudioInput(finalReminderAudioInput);
        InitInstructorSettings(audioInput, speechBubbleInput, automaticAudioToggle);
        warningContainer.gameObject.SetActive(false);
        warningMessage.text = "Maximale Anzahl von " + _maxNumSupportiveInfos + " unterstützenden Informationen erreicht.";
    }

    private void Update()
    {
        warningContainer.gameObject.SetActive(supportInfoDropArea.transform.childCount > _maxNumSupportiveInfos);
    }

    public void SupportInfoContainerChanged()
    {
        SaveSettings();
    }

    protected override void SetUpByProperties()
    {
        SetSkippableDropdown(skippableDropdown);
        SetTextInputField(monitorInput, "textMonitor");
        List<SupportInfo> supportInfos = GetSupportInfos("supportInfos");
        supportInfoDropArea.transform.DestroyImmediateAllChildren("Preview");
        foreach (SupportInfo supportInfo in supportInfos)
            SupportInfoController.Instance.CreateSupportInfoSettings(supportInfo, supportInfoDropArea.transform, false);
        SetTextInputField(minNumberInput, "minSupportInfos");
        SetAudioInput(reminderAudioInput, "reminderAudioId");
        SetAudioInput(finalReminderAudioInput, "finalReminderAudioId");
        SetToggle(sequenceToggle, "sequence");
        SetInstructorSettings(audioInput, speechBubbleInput, automaticAudioToggle);
    }
    
    protected override void SetJSON()
    {
        SubTask subTask = relatedSubTaskContainer.SubTaskData;
        int tmp = supportInfoDropArea.transform.childCount;
        CheckValidInput(minNumberInput, 0, tmp > 0 ? tmp : _maxNumSupportiveInfos, 1);
        JObject json = new JObject
        {
            {"textMonitor", monitorInput.text},
            {"supportInfos", JArray.FromObject(GetSupportInfos())},
            {"minSupportInfos", minNumberInput.text},
            {"sequence", sequenceToggle.isOn}
        };
        SetSkippableDropdownJSON(json, skippableDropdown);
        SetAudioInputJSON(json, reminderAudioInput, "reminderAudioId");
        SetAudioInputJSON(json, finalReminderAudioInput, "finalReminderAudioId");
        SetInstructorJSON(json, audioInput, speechBubbleInput, automaticAudioToggle);
        subTask.properties = json.ToString();
    }

    private List<SupportInfo> GetSupportInfos()
    {
        List<SupportInfo> supportInfos = new List<SupportInfo>();
        for (int i = 0; i < supportInfoDropArea.transform.childCount; i++)
        {
            BaseSupportInfoContainer container =
                supportInfoDropArea.transform.GetChild(i).GetComponent<BaseSupportInfoContainer>();
            if (container != null)
            {
                supportInfos.Add(container.supportInfoData);
                container.SaveData();
            }
        }
        return supportInfos;
    }

}

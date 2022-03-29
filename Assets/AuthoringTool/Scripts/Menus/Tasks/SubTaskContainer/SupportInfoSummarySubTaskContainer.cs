using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Represents a support info summary sub task.
/// </summary>
public class SupportInfoSummarySubTaskContainer : BaseSubTaskContainer
{
    public TMP_InputField monitorTextInputField;
    public GameObject supportInfosDropArea;
    public Transform suppInfosContainer;
    public TMP_InputField minSupportInfos;
    public TMP_InputField reminderAudioInputField;
    public TMP_InputField finalReminderAudioInputField;
    public TMP_InputField audioInputField;
    public TMP_InputField speechBubbleInputField;
    public Toggle automaticAudioToggle;
    public Toggle sequenceToggle;
    public TMP_InputField skippableInputField;

    private List<BaseSupportInfoContainer> supportInfoContainers;

    protected override void SetUpByProperties()
    {
        SetTextInputField(monitorTextInputField, "textMonitor");
        SetSkippableInputField(skippableInputField);
        SetTextInputField(minSupportInfos, "minSupportInfos");
        List<SupportInfo> supportInfos = GetSupportInfos("supportInfos");
        supportInfosDropArea.transform.DestroyAllChildren();
        supportInfoContainers = new List<BaseSupportInfoContainer>();
        foreach (SupportInfo supportInfo in supportInfos)
            supportInfoContainers.Add(
                SupportInfoController.Instance.CreateSupportInfoDisplaying(supportInfo, supportInfosDropArea.transform));
        suppInfosContainer.transform.parent.gameObject.SetActive(supportInfos.Count > 0);
        SetToggle(sequenceToggle, "sequence", true);
        SetAudioInputField(reminderAudioInputField, "reminderAudioId");
        SetAudioInputField(finalReminderAudioInputField, "finalReminderAudioId");
        SetInstructorSettings(audioInputField, speechBubbleInputField, automaticAudioToggle);
    }

    public override bool ValuesMissing()
    {
        return minSupportInfos.text == "" || SupportInfoIsValueMissing();
    }

    private bool SupportInfoIsValueMissing()
    {
        if (supportInfosDropArea.transform.childCount == 0)
            return true;
        return supportInfoContainers.Any(container => container.ValuesMissing());
    }

    private List<SupportInfo> GetSupportInfos(string propertyName)
    {
        if (!properties.TryGetValue(propertyName, out JToken jToken))
            return new List<SupportInfo>();
        return jToken.ToObject<List<SupportInfo>>();
    }
}
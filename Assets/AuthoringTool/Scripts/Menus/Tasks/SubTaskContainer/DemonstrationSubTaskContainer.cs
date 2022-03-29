using TMPro;
using UnityEngine.UI;

/// <summary>
/// Represents a demonstration sub task.
/// </summary>
public class DemonstrationSubTaskContainer : BaseSubTaskContainer
{
    public TMP_InputField monitorTextInputField;
    public TMP_InputField recordInputField;
    public TMP_InputField baseCoatInputField;
    public TMP_InputField coatInputField;
    public Toggle distanceRayToggle;
    public Toggle distanceMarkerToggle;
    public Toggle angleRayToggle;
    public TMP_InputField audioInputField;
    public TMP_InputField speechBubbleInputField;
    public Toggle automaticAudioToggle;
    public TMP_InputField skippableInputField;

    protected override void SetUpByProperties()
    {
        SetTextInputField(monitorTextInputField, "textMonitor");
        SetRecordingTextInputField(recordInputField, "recordingId");
        SetCoatInputField(baseCoatInputField, "baseCoatId");
        SetCoatInputField(coatInputField, "coatId");
        SetToggle(distanceRayToggle, "distanceRay");
        SetToggle(distanceMarkerToggle, "distanceMarker");
        SetToggle(angleRayToggle, "angleRay");
        distanceRayToggle.transform.parent.parent.gameObject.SetActive(
            distanceRayToggle.isOn || distanceMarkerToggle.isOn || angleRayToggle.isOn);
        SetSkippableInputField(skippableInputField);
        SetInstructorSettings(audioInputField, speechBubbleInputField, automaticAudioToggle);
    }

    public override bool ValuesMissing()
    {
        return recordInputField.text.Equals("") || baseCoatInputField.text.Equals("") || coatInputField.text.Equals("");
    }
}
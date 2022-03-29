using TMPro;
using UnityEngine.UI;

/// <summary>
/// Represents a painting sub task.
/// </summary>
public class PaintingSubTaskContainer : BaseSubTaskContainer
{
    public TMP_InputField monitorTextInputField;
    public TMP_InputField workpieceInputField;
    public TMP_InputField baseCoatInputField;
    public TMP_InputField coatInputField;
    public Toggle distanceRayToggle;
    public Toggle distanceMarkerToggle;
    public Toggle angleRayToggle;
    public Toggle optionDistanceRayToggle;
    public Toggle optionDistanceMarkerToggle;
    public Toggle optionAngleRayToggle;
    public TMP_InputField helpDurationInputField;
    public TMP_InputField minTimeInputField;
    public TMP_InputField audioInputField;
    public TMP_InputField speechBubbleInputField;
    public Toggle automaticAudioToggle;
    public TMP_InputField skippableInputField;

    protected override void SetUpByProperties()
    {
        SetTextInputField(monitorTextInputField, "textMonitor");
        SetSkippableInputField(skippableInputField);
        SetWorkpieceInputField(workpieceInputField);
        SetCoatInputField(baseCoatInputField, "baseCoatId");
        SetCoatInputField(coatInputField, "coatId");
        SetToggle(distanceRayToggle, "distanceRay");
        SetToggle(distanceMarkerToggle, "distanceMarker");
        SetToggle(angleRayToggle, "angleRay");
        distanceRayToggle.transform.parent.parent.gameObject.SetActive(
            distanceRayToggle.isOn || distanceMarkerToggle.isOn || angleRayToggle.isOn);
        SetToggle(optionDistanceRayToggle, "optionDistanceRay");
        SetToggle(optionDistanceMarkerToggle, "optionDistanceMarker");
        SetToggle(optionAngleRayToggle, "optionAngleRay");
        optionDistanceRayToggle.transform.parent.parent.gameObject.SetActive(
            optionDistanceRayToggle.isOn || optionDistanceMarkerToggle.isOn || optionAngleRayToggle.isOn);
        SetTextInputField(helpDurationInputField, "helpDuration");
        SetTextInputField(minTimeInputField, "minSprayTime");
        SetInstructorSettings(audioInputField, speechBubbleInputField, automaticAudioToggle);
    }

    public override bool ValuesMissing()
    {
        return baseCoatInputField.text == "" || coatInputField.text == "" || workpieceInputField.text == "" || minTimeInputField.text == "";
    }
}


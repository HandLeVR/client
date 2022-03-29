using TMPro;
using UnityEngine.UI;

/// <summary>
/// Represents a introduction sub task
/// </summary>
public class IntroductionSubTaskContainer : BaseSubTaskContainer
{
    public TMP_InputField monitorTextInputField;
    public TMP_InputField audioInputField;
    public TMP_InputField speechBubbleInputField;
    public Toggle automaticAudioToggle;
    public TMP_InputField skippableInputField;

    protected override void SetUpByProperties()
    {
        SetTextInputField(monitorTextInputField, "textMonitor");
        SetSkippableInputField(skippableInputField);
        SetInstructorSettings(audioInputField, speechBubbleInputField, automaticAudioToggle);
    }

    public override bool ValuesMissing()
    {
        return false;
    }
}

using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents a estimation sub task.
/// </summary>
public class EstimationSubTaskContainer : BaseSubTaskContainer
{
    public TMP_InputField monitorTextInputField;
    public TMP_InputField audioInputField;
    public Toggle automaticAudioToggle;
    public TMP_InputField skippableInputField;
    public TMP_InputField speechBubbleInputField;
    public TMP_InputField finalAudioInputField;
    public TMP_InputField objectInputField;
    public TMP_InputField minimumInputField;
    public TMP_InputField maximumInputField;
    public TextMeshProUGUI minimumUnitLabel;
    public TextMeshProUGUI maximumUnitLabel;

    protected override void SetUpByProperties()
    {
        SetTextInputField(monitorTextInputField, "textMonitor");
        SetAmountTextInputField(minimumInputField, minimumUnitLabel, "minimum");
        SetAmountTextInputField(maximumInputField, maximumUnitLabel, "maximum");
        SetObjectInputField(objectInputField, "interactiveObject");
        SetAudioInputField(finalAudioInputField, "finalAudioId");
        SetSkippableInputField(skippableInputField);
        SetInstructorSettings(audioInputField, speechBubbleInputField, automaticAudioToggle);
    }

    private void SetObjectInputField(TMP_InputField inputField, string propertyName)
    {
        if (properties.TryGetValue(propertyName, out JToken text))
        {
            if ((string) text == "clock")
                inputField.text = "Uhr";
            else if ((string) text == "beaker")
                inputField.text = "Messbecher";
            else
                inputField.text = "Thermometer";
        }
        inputField.transform.parent.gameObject.SetActive(inputField.text != "");
    }

    private void SetAmountTextInputField(TMP_InputField inputField, TextMeshProUGUI unitLabel, string propertyName)
    {
        SetTextInputField(inputField, propertyName);
        if (properties.TryGetValue("interactiveObject", out JToken text))
        {
            if ((string) text == "clock")
                unitLabel.text = "Minuten";
            else if ((string) text == "beaker")
                unitLabel.text = "Milliliter";
            else
                unitLabel.text = "°C";
        }
    }

    public override bool ValuesMissing()
    {
        return minimumInputField.text == "" || maximumInputField.text == "";
    }
}
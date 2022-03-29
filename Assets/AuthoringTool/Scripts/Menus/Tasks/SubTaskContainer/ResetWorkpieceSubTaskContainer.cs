using TMPro;

/// <summary>
/// Represents a reset workpiece sub task.
/// </summary>
public class ResetWorkpieceSubTaskContainer : BaseSubTaskContainer
{
    public TMP_InputField typeInputField;
    public TMP_InputField recordInputField;
    public TMP_InputField workpieceInputField;
    public TMP_InputField coatInputField;
    public TMP_InputField coatConditionInputField;

    protected override void SetUpByProperties()
    {
        SetTextInputField(typeInputField, "type", s => s == "custom" ? "Eigene Angaben" : "Aufnahme");
        SetRecordingTextInputField(recordInputField, "recordingId");
        SetWorkpieceInputField(workpieceInputField);
        SetCoatInputField(coatInputField, "coatId");
        SetTextInputField(coatConditionInputField, "coatCondition", s => s == "wet" ? "Nass" : "Trocken");
    }

    public override bool ValuesMissing()
    {
        return typeInputField.text.Equals("") ||
               typeInputField.text.Equals("Aufnahme") && recordInputField.text.Equals("") ||
               typeInputField.text.Equals("Eigene Angaben") && (workpieceInputField.text.Equals("") ||
                                                                coatInputField.text.Equals("") ||
                                                                coatConditionInputField.text.Equals(""));
    }
}

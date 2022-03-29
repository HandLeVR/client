using TMPro;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents the settings panel for the reset workpiece task.
/// </summary>
public class ResetWorkpieceSettingsPanel : BaseSettingsPanel
{
    public TMP_Dropdown typeDropdown;
    public TMP_InputField recordInput;
    public TMP_Dropdown workpieceDropdown;
    public TMP_InputField coatInput;
    public TMP_Dropdown coatConditionDropdown;

    public void Awake()
    {
        typeDropdown.onValueChanged.AddListener(delegate { SaveSettings(); });
        InitRecordingInput(recordInput);
        InitCoatInput(coatInput, true, noCoatEntry: true);
        InitWorkpieceDropdown(workpieceDropdown);
        workpieceDropdown.onValueChanged.AddListener(delegate { SaveSettings(); });
        coatConditionDropdown.onValueChanged.AddListener(delegate { SaveSettings(); });
        OnTypeDropdownChange(typeDropdown.value);
    }

    /// <summary>
    /// (De)activates elements in the settings panel on dependence of the selected type.
    /// </summary>
    public void OnTypeDropdownChange(int i)
    {
        if (i == 0)
        {
            recordInput.transform.parent.gameObject.SetActive(true);
            workpieceDropdown.transform.parent.gameObject.SetActive(false);
            coatInput.transform.parent.gameObject.SetActive(false);
            coatConditionDropdown.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            recordInput.transform.parent.gameObject.SetActive(false);
            workpieceDropdown.transform.parent.gameObject.SetActive(true);
            coatInput.transform.parent.gameObject.SetActive(true);
            coatConditionDropdown.transform.parent.gameObject.SetActive(true);
        }
    }

    protected override void SetUpByProperties()
    {
        SetTypeDropdown();
        OnTypeDropdownChange(typeDropdown.value);
        if (typeDropdown.value == 0)
        {
            SetRecordingInput(recordInput, "recordingId");
        }
        else
        {
            SetWorkpieceDropdown(workpieceDropdown, "workpieceId");
            SetRecordingInput(recordInput, "recordingId");
            SetCoatInput(coatInput, "coatId");
            SetWorkpieceConditionDropdown();
        }
    }

    protected override void SetJSON()
    {
        SubTask subTask = relatedSubTaskContainer.SubTaskData;
        JObject json = new JObject();
        SetTypeDropdownJSON(json);
        if (typeDropdown.value == 0)
        {
            SetRecordingInputJSON(json, recordInput, "recordingId");
        }
        else
        {
            SetWorkpieceDropdownJSON(json, workpieceDropdown, "workpieceId");
            SetCoatInputJSON(json, coatInput, "coatId");
            SetCoatConditionDropdownJSON(json);
        }
        subTask.properties = json.ToString();
    }

    private void SetTypeDropdown()
    {
        if (properties.TryGetValue("type", out JToken type))
            typeDropdown.value = (string) type == "recording" ? 0 : 1;
    }

    private void SetTypeDropdownJSON(JObject json)
    {
        json.Add("type", typeDropdown.value == 0 ? "recording" : "custom");
    }

    private void SetWorkpieceConditionDropdown()
    {
        if (properties.TryGetValue("coatCondition", out JToken coatCondition))
            coatConditionDropdown.value = (string) coatCondition == "wet" ? 0 : 1;
    }

    private void SetCoatConditionDropdownJSON(JObject json)
    {
        json.Add("coatCondition", coatConditionDropdown.value == 0 ? "wet" : "dry");
    }
}
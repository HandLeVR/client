using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using translator;

/// <summary>
/// Base class for all setting panels. Provides methods used by multiple setting panels.
/// </summary>
public abstract class BaseSettingsPanel : MonoBehaviour
{
    // allows to connect the panel to a task type
    public string subTaskUnityId;

    // the sub task container which is currently selected
    [HideInInspector] public BaseSubTaskContainer relatedSubTaskContainer;

    // allows to get the id of the set element
    protected JObject properties;
    private BaseDropArea dropAreaHandler;

    // needed to avoid calling SaveSettings during setup by properties
    protected bool isSettingUp;

    protected void SaveSettings()
    {
        if (isSettingUp)
            return;

        TaskController.Instance.unsavedChanges = true;
        SubTask subTask = relatedSubTaskContainer.SubTaskData;
        SetJSON();
        relatedSubTaskContainer.SetUpBySubTask(subTask);
        if (dropAreaHandler == null)
            dropAreaHandler = FindObjectOfType<BaseDropArea>();
        dropAreaHandler.ScrollTo(relatedSubTaskContainer.GetComponent<RectTransform>());
    }

    /// <summary>
    /// Creates a json from the settings in the settings panel. Depends on the sub task.
    /// </summary>
    protected abstract void SetJSON();

    /// <summary>
    /// Sets up the settings panel by the current properties. Depends on the sub task.
    /// </summary>
    protected abstract void SetUpByProperties();

    /// <summary>
    /// Sets up the settings panel by the sub task data.
    /// </summary>
    public void SetUpFromSubTask()
    {
        BaseSubTaskContainer container = relatedSubTaskContainer;

        // save empty json in sub task if the sub task is new
        if (string.IsNullOrEmpty(container.SubTaskData.properties))
        {
            properties = new JObject();
            SetUpByProperties();
            SaveSettings();
        }

        // set up the setting panel by the sub task data
        SubTask subTask = container.SubTaskData;
        properties = JObject.Parse(subTask.properties);
        isSettingUp = true;
        SetUpByProperties();
        isSettingUp = false;
    }

    public SubTask GetRelatedSubTask()
    {
        return relatedSubTaskContainer.SubTaskData;
    }

    protected void SetToggle(Toggle toggle, string propertyName, bool defaultValue = false)
    {
        if (properties.TryGetValue(propertyName, out JToken jToken))
            toggle.isOn = (bool)jToken;
        else
            toggle.isOn = defaultValue;
    }

    protected void SetTextInputField(TMP_InputField inputField, string propertyName, string defaultValue = "")
    {
        inputField.text = properties.TryGetValue(propertyName, out JToken text)
            ? (string)text
            : defaultValue;
    }

    protected void SetWorkpieceDropdown(TMP_Dropdown dropdown, string propertyName)
    {
        if (properties.TryGetValue(propertyName, out JToken workpieceId))
        {
            Workpiece workpiece = DataController.Instance.workpieces[(long)workpieceId];
            dropdown.value = dropdown.options.FindIndex(i => i.text.Equals(TranslationController.Instance.Translate(workpiece.name)));
        }
        else
        {
            dropdown.value = 0;
        }
    }

    protected void InitWorkpieceDropdown(TMP_Dropdown dropdown)
    {
        foreach (Workpiece workpiece in DataController.Instance.workpieces.Values)
            dropdown.options.Add(new TMP_Dropdown.OptionData(TranslationController.Instance.Translate(workpiece.name)));
        dropdown.onValueChanged.AddListener(delegate { SaveSettings(); });
    }

    protected void SetSkippableDropdown(TMP_Dropdown dropdown)
    {
        if (properties.TryGetValue("skippable", out JToken skippableJson))
            dropdown.value = (int)skippableJson;
        else
            dropdown.value = 0;
    }

    protected void InitSkippableDropdown(TMP_Dropdown dropdown, bool addSpeechSkippableEntry = true,
        bool addAllSkippableEntry = true)
    {
        if (addSpeechSkippableEntry)
            dropdown.options.Add(new TMP_Dropdown.OptionData(Statics.SPEECH_SKIPPABLE));
        if (addAllSkippableEntry)
            dropdown.options.Add(new TMP_Dropdown.OptionData(Statics.All_SKIPPABLE));
        dropdown.onValueChanged.AddListener(delegate { SaveSettings(); });
    }

    protected void SetInstructorSettings(TMP_InputField inputAudio, TMP_InputField inputField, Toggle toggle)
    {
        SetAudioInput(inputAudio, "audioId");
        SetTextInputField(inputField, "textSpeechBubble");
        SetToggle(toggle, "automaticAudio", true);
    }

    protected void InitInstructorSettings(TMP_InputField inputAudio, TMP_InputField inputField, Toggle toggle)
    {
        InitAudioInput(inputAudio);
        inputField.onEndEdit.AddListener(delegate { SaveSettings(); });
        inputField.characterLimit = 50;
        toggle.onValueChanged.AddListener(delegate { SaveSettings(); });
    }

    protected void SetInstructorJSON(JObject json, TMP_InputField inputAudio, TMP_InputField inputField, Toggle toggle)
    {
        SetAudioInputJSON(json, inputAudio, "audioId");
        json.Add("textSpeechBubble", inputField.text);
        json.Add("automaticAudio", toggle.isOn);
    }

    protected void SetSkippableDropdownJSON(JObject json, TMP_Dropdown dropdown)
    {
        if (dropdown.options[dropdown.value].text == Statics.NOT_SKIPPABLE)
            json.Add("skippable", 0);
        else if (dropdown.options[dropdown.value].text == Statics.SPEECH_SKIPPABLE)
            json.Add("skippable", 1);
        else
            json.Add("skippable", 2);
    }

    protected void SetWorkpieceDropdownJSON(JObject json, TMP_Dropdown dropdown, string propertyName)
    {
        if (dropdown.value != 0)
            json.Add(propertyName, DataController.Instance.workpieces.Values.ToList()[dropdown.value - 1].id);
    }

    protected void SetAudioInput(TMP_InputField inputAudio, string propertyName)
    {
        if (properties.TryGetValue(propertyName, out JToken mediaJson))
        {
            Media media = DataController.Instance.availableAudios[(long)mediaJson];
            inputAudio.placeholder.GetComponent<TextMeshProUGUI>().text = media.id.ToString();
            inputAudio.text = media.name;
        }
        else
        {
            inputAudio.placeholder.GetComponent<TextMeshProUGUI>().text = "Klicken zur Auswahl einer Audiodatei";
            inputAudio.text = "";
        }
    }

    protected void SetAudioInputJSON(JObject json, TMP_InputField inputAudio, string propertyName)
    {
        if (!inputAudio.text.Equals(string.Empty))
            json.Add(propertyName, inputAudio.placeholder.GetComponent<TextMeshProUGUI>().text);
    }

    protected void InitAudioInput(TMP_InputField inputAudio)
    {
        inputAudio.placeholder.GetComponent<TextMeshProUGUI>().text = "Klicken zur Auswahl einer Audiodatei";
        inputAudio.onSelect.AddListener(_ =>
            SelectionPopup.Instance.Init(Media.MediaType.Audio, () => SelectAudio(inputAudio)));
        inputAudio.onValueChanged.AddListener(delegate { SaveSettings(); });
    }

    private void SelectAudio(TMP_InputField inputAudio)
    {
        if (SelectionPopup.Instance.IsValidSelection())
        {
            Media media = DataController.Instance.media[SelectionPopup.Instance.GetSelectedId()];
            // the id of the media file is saved in the placeholder of the input field to be able to find the media file when the json created
            inputAudio.placeholder.GetComponent<TextMeshProUGUI>().text = media.id.ToString();
            inputAudio.text = media.name;
        }
        else
        {
            inputAudio.placeholder.GetComponent<TextMeshProUGUI>().text = "Klicken zur Auswahl einer Audiodatei";
            inputAudio.text = "";
        }
    }

    protected void SetRecordingInput(TMP_InputField inputRecord, string propertyName)
    {
        // try to find the corresponding dropdown entry with the recording on the server then with the recording in the json
        if (properties.TryGetValue(propertyName, out JToken recordJson))
        {
            Recording record = DataController.Instance.remoteRecordings[(long)recordJson];
            inputRecord.placeholder.GetComponent<TextMeshProUGUI>().text = record.id.ToString();
            inputRecord.text = record.name;
        }
        else
        {
            inputRecord.placeholder.GetComponent<TextMeshProUGUI>().text = "Klicken zur Auswahl einer Aufnahme";
            inputRecord.text = "";
        }
    }

    protected void SetRecordingInputJSON(JObject json, TMP_InputField inputRecord, string propertyName)
    {
        if (!inputRecord.text.Equals(string.Empty))
            json.Add(propertyName, inputRecord.placeholder.GetComponent<TextMeshProUGUI>().text);
    }

    protected void InitRecordingInput(TMP_InputField inputRecord)
    {
        inputRecord.placeholder.GetComponent<TextMeshProUGUI>().text = "Klicken zur Auswahl einer Aufnahme";
        inputRecord.onSelect.AddListener(_ =>
            SelectionPopup.Instance.Init(typeof(Recording), () => SelectRecording(inputRecord)));
        inputRecord.onValueChanged.AddListener(delegate { SaveSettings(); });
    }

    private void SelectRecording(TMP_InputField inputRecord)
    {
        if (SelectionPopup.Instance.IsValidSelection())
        {
            Recording record = DataController.Instance.remoteRecordings[SelectionPopup.Instance.GetSelectedId()];
            inputRecord.placeholder.GetComponent<TextMeshProUGUI>().text = record.id.ToString();
            inputRecord.text = record.name;
        }
        else
        {
            inputRecord.placeholder.GetComponent<TextMeshProUGUI>().text = "Klicken zur Auswahl einer Aufnahme";
            inputRecord.text = "";
        }
    }

    protected void SetCoatInput(TMP_InputField inputCoat, string propertyName)
    {
        InputFieldUtil.SetCoatInputFieldSettings(inputCoat, properties, propertyName);
    }

    protected void SetCoatInputJSON(JObject json, TMP_InputField inputCoat, string propertyName)
    {
        if (inputCoat.text == Statics.COAT_FROM_COAT_SELECTION)
            json.Add(propertyName, -1);
        else if (inputCoat.text == Statics.COAT_FROM_RECORDING)
            json.Add(propertyName, -2);
        else if (inputCoat.text == Statics.NO_COAT)
            json.Add(propertyName, -3);
        else if (inputCoat.text.Length > 0)
            json.Add(propertyName, inputCoat.placeholder.GetComponent<TextMeshProUGUI>().text);
    }

    protected void InitCoatInput(TMP_InputField inputCoat, bool addFromSelectionEntry = false,
        bool addFromRecordingEntry = false, bool noCoatEntry = false, bool noClearCoat = false)
    {
        inputCoat.onSelect.AddListener(_ => SelectionPopup.Instance.InitForCoat(() => SelectCoat(inputCoat),
            addFromSelectionEntry, addFromRecordingEntry, noCoatEntry, noClearCoat));
        inputCoat.onValueChanged.AddListener(delegate { SaveSettings(); });
    }

    private void SelectCoat(TMP_InputField inputCoat)
    {
        if (SelectionPopup.Instance.IsValidSelection())
        {
            long coatId = SelectionPopup.Instance.GetSelectedId();
            string coatName;
            if (coatId == -1)
                coatName = Statics.COAT_FROM_COAT_SELECTION;
            else if (coatId == -2)
                coatName = Statics.COAT_FROM_RECORDING;
            else if (coatId == -3)
                coatName = Statics.NO_COAT;
            else
                coatName = DataController.Instance.coats[coatId].name;
            inputCoat.placeholder.GetComponent<TextMeshProUGUI>().text = coatId.ToString();
            inputCoat.text = coatName;
        }
        else
        {
            inputCoat.placeholder.GetComponent<TextMeshProUGUI>().text = "Klicken zur Auswahl eines Lackes";
            inputCoat.text = "";
        }
    }

    protected void SetAnswer(Transform transform, SimpleChoice simpleChoice)
    {
        transform.GetComponentInChildren<TMP_InputField>().text = simpleChoice.label;
        transform.GetComponentInChildren<Toggle>().isOn = simpleChoice.correct;
    }

    protected List<SimpleChoice> GetSimpleChoices(string propertyAnswer, string propertyCorrect)
    {
        List<string> answers = GetAnswerTexts(propertyAnswer);
        List<bool> correctnesses = GetAnswerCorrectness(propertyCorrect);
        List<SimpleChoice> simpleChoices = new List<SimpleChoice>();
        if (answers.Count == correctnesses.Count)
        {
            for (int i = 0; i < answers.Count; i++)
            {
                SimpleChoice simpleChoice = new SimpleChoice(answers[i], correctnesses[i]);
                simpleChoices.Add(simpleChoice);
            }

            return simpleChoices;
        }

        return null;
    }

    private List<string> GetAnswerTexts(string propertyName)
    {
        List<string> answers = new List<string>();
        if (properties.TryGetValue(propertyName, out JToken jToken))
            foreach (JToken text in jToken)
                answers.Add((string)text);
        return answers;
    }

    private List<bool> GetAnswerCorrectness(string propertyName)
    {
        List<bool> correctness = new List<bool>();
        if (properties.TryGetValue(propertyName, out JToken jToken))
            foreach (JToken correct in jToken)
                correctness.Add((bool)correct);
        return correctness;
    }

    protected List<SupportInfo> GetSupportInfos(string propertyName)
    {
        if (!properties.TryGetValue(propertyName, out JToken jToken))
            return new List<SupportInfo>();
        return jToken.ToObject<List<SupportInfo>>();
    }

    protected List<JObject> GetSortableObjects(string propertyName)
    {
        List<JObject> objects = new List<JObject>();
        if (properties.TryGetValue(propertyName, out JToken jToken))
            foreach (JToken text in jToken)
                objects.Add((JObject)text);
        return objects;
    }

    protected void CheckValidInput(TMP_InputField input, int min, int max, int replaceValue)
    {
        if (int.TryParse(input.text, out int f))
            input.text = (f >= min && f <= max) ? f.ToString() : replaceValue.ToString();
    }
}
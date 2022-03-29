using System;
using System.Linq;
using TMPro;
using translator;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

/// <summary>
/// Base class for a sub task container. Sub task container represent an element in the middle panel of the
/// task creation menu. This base class provides methods used by multiple child classes.
/// </summary>
public class BaseSubTaskContainer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string type;
    public TextMeshProUGUI missingValuesWarning;
    public TextMeshProUGUI numberText;
    public TextMeshProUGUI titleText;

    protected JObject properties;

    private SubTask _subTaskData;

    /// <summary>
    /// The index of the container. Changes the corresponding label if changed.
    /// </summary>
    public int ID
    {
        set
        {
            if (numberText != null)
                numberText.text = value.ToString();
        }
    }

    /// <summary>
    /// The sub task related to this container. If the sub task is set the title text is changed accordingly. 
    /// </summary>
    public SubTask SubTaskData
    {
        get => _subTaskData;
        set
        {
            _subTaskData = value;
            titleText.text = TranslationController.Instance.TranslateSubTaskType(_subTaskData.type);
        }
    }

    private void Start()
    {
        Transform deleteButton = transform.FindDeepChild("Button Delete");
        if (deleteButton)
            deleteButton.GetComponent<Button>().onClick.AddListener(() => PopupScreenHandler.Instance.ShowConfirmation(
                "popup-remove-sub-task", "popup-remove-sub-task-confirm", DeleteSubTask));
    }

    /// <summary>
    /// Sets the sub task and the properties so they can be used by the setup method of the specialized container class.
    /// </summary>
    public void SetUpBySubTask(SubTask subTask)
    {
        SubTaskData = subTask;
        properties = GetProperties(subTask.properties);
        SetUpByProperties();
        missingValuesWarning.gameObject.SetActive(ValuesMissing() && missingValuesWarning != null);
    }

    /// <summary>
    /// Sets the fields in dependence of the task is container represents.
    /// </summary>
    protected virtual void SetUpByProperties()
    {
    }

    public virtual bool ValuesMissing()
    {
        return false;
    }

    protected void SetToggle(Toggle toggle, string propertyName, bool setParentActive = false)
    {
        toggle.isOn = properties.TryGetValue(propertyName, out JToken jToken) && (bool)jToken;
        if (setParentActive)
            toggle.transform.parent.gameObject.SetActive(toggle.isOn);
        else
            toggle.gameObject.SetActive(toggle.isOn);
    }

    protected void SetWorkpieceInputField(TMP_InputField inputField)
    {
        inputField.text = properties.TryGetValue("workpieceId", out JToken workpieceId)
            ? TranslationController.Instance.Translate(DataController.Instance.workpieces[(long)workpieceId].name)
            : "";
        inputField.transform.parent.gameObject.SetActive(inputField.text != "");
    }

    protected void SetCoatInputField(TMP_InputField inputField, string propertyName)
    {
        InputFieldUtil.SetCoatInputFieldDisplaying(inputField, properties, propertyName);
    }

    protected void SetSkippableInputField(TMP_InputField inputField)
    {
        if (properties.TryGetValue("skippable", out JToken skippableJson))
        {
            int skippable = (int)skippableJson;
            if (skippable == 0)
                inputField.text = Statics.NOT_SKIPPABLE;
            else if (skippable == 1)
                inputField.text = Statics.SPEECH_SKIPPABLE;
            else if (skippable == 2)
                inputField.text = Statics.All_SKIPPABLE;
        }
        else
            inputField.text = Statics.NOT_SKIPPABLE;
    }

    protected void SetTextInputField(TMP_InputField inputField, string propertyName, Func<string, string> func = null)
    {
        inputField.text = properties.TryGetValue(propertyName, out JToken text)
            ? func != null ? func((string)text) : (string)text
            : "";
        inputField.transform.parent.gameObject.SetActive(inputField.text != "");
    }

    protected void SetRecordingTextInputField(TMP_InputField inputField, string propertyName)
    {
        inputField.text = "";
        if (properties.TryGetValue(propertyName, out JToken recordJson))
        {
            Recording serverRecording =
                DataController.Instance.remoteRecordings.Values.FirstOrDefault(r => r.id == (long)recordJson);
            inputField.text = serverRecording != null ? serverRecording.name : "";
        }

        inputField.transform.parent.gameObject.SetActive(inputField.text != "");
    }

    protected void SetAudioInputField(TMP_InputField inputField, string propertyName)
    {
        inputField.text = properties.TryGetValue(propertyName, out JToken mediaJson)
            ? DataController.Instance.media[(long)mediaJson].name
            : "";
        inputField.transform.parent.gameObject.SetActive(inputField.text != "");
    }

    protected void SetInstructorSettings(TMP_InputField audioInputField, TMP_InputField speechBubbleInputField,
        Toggle automaticAudioToggle)
    {
        SetAudioInputField(audioInputField, "audioId");
        SetTextInputField(speechBubbleInputField, "textSpeechBubble");
        SetToggle(automaticAudioToggle, "automaticAudio", true);
    }

    private JObject GetProperties(string propertiesString)
    {
        return string.IsNullOrEmpty(propertiesString) ? new JObject() : JObject.Parse(propertiesString);
    }

    private void DeleteSubTask()
    {
        // hide the corresponding settings panel if it is active for the sub task we want to delete
        if (IsCurrentSubTask())
            TaskController.Instance.HideAllPanel();

        DestroyImmediate(gameObject);
        SubTaskController.Instance.UpdateSubTaskNumbers();
    }

    /// <summary>
    /// Returns true if the currently active settings panel is connected to the sub task represented by this container.
    /// </summary>
    /// <returns></returns>
    public bool IsCurrentSubTask()
    {
        BaseSettingsPanel panel = TaskController.Instance.GetActiveSettingsPanel();
        if (panel == null)
            return false;

        SubTask subtask = panel.GetRelatedSubTask();
        if (subtask == null)
            return false;

        if (subtask != SubTaskData)
            return false;

        return true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.dragging)
            transform.GetComponent<Button>().enabled = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.GetComponent<Button>().enabled = true;
    }
}
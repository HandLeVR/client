using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using translator;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller for the recording menu.
/// </summary>
public class RecordingMenuController : BaseMenuController
{
    public Transform recordingsList;
    public TMP_InputField nameInputField;
    public TMP_InputField descriptionInputField;
    public TMP_InputField dateInputField;
    public TMP_InputField durationInputField;
    public TMP_InputField workpieceInputField;
    public TMP_InputField topCoatInputField;
    public TMP_InputField baseCoatInputField;
    public TextMeshProUGUI pathTMP;
    public TextMeshProUGUI saveButtonText;
    public Image previewImage;
    public RecordingTableElement recordingTableElementPrefab;

    private RecordingTableElement _currentContainer;
    private string _missingValuesString;

    private void Awake()
    {
        saveButton.onClick.AddListener(SaveRecording);
        nameInputField.onValueChanged.AddListener(_ => SetUnsavedChanges(true));
        descriptionInputField.onValueChanged.AddListener(_ => SetUnsavedChanges(true));
    }

    private void OnEnable()
    {
        recordingsList.DestroyImmediateAllChildren();
        MainScreenController.Instance.LoadData(DataController.RequestType.Recordings, InstantiateContainer);
    }

    /// <summary>
    /// Instantiate a list entry for a recording element.
    /// </summary>
    private void InstantiateContainer()
    {
        recordingsList.DestroyImmediateAllChildren();
        List<Recording> synchronizedRecordings = new List<Recording>();
        foreach (var recording in DataController.Instance.remoteRecordings.Values)
        {
            RecordingTableElement recordingTableElement =
                Instantiate(recordingTableElementPrefab, recordingsList);
            if (DataController.Instance.localRecordings.Exists(r => r.id == recording.id))
            {
                recordingTableElement.Init(recording, RecordingLocation.LocalAndServer, SetUpByRecording,
                    DeleteRecording, DownloadRecording);
                synchronizedRecordings.Add(recording);
            }
            else
                recordingTableElement.Init(recording, RecordingLocation.Server, SetUpByRecording,
                    DeleteRecording, DownloadRecording);
        }

        foreach (var recording in DataController.Instance.localRecordings.Where(r1 =>
            !synchronizedRecordings.Exists(r2 => r1.id == r2.id)))
        {
            RecordingTableElement recordingTableElement =
                Instantiate(recordingTableElementPrefab, recordingsList);
            recordingTableElement.Init(recording, RecordingLocation.Local, SetUpByRecording, DeleteRecording,
                DownloadRecording);
        }

        SortRecordings();
        ResetFields();
    }

    private void ResetFields()
    {
        nameInputField.text = String.Empty;
        nameInputField.interactable = false;
        descriptionInputField.text = String.Empty;
        descriptionInputField.interactable = false;
        pathTMP.text = String.Empty;
        dateInputField.text = String.Empty;
        durationInputField.text = String.Empty;
        workpieceInputField.text = String.Empty;
        topCoatInputField.text = String.Empty;
        baseCoatInputField.text = String.Empty;
        previewImage.gameObject.SetActive(false);
        SetUnsavedChanges(false);
    }

    /// <summary>
    /// Initializes all fields on the right side by the given recording.
    /// </summary>
    private void SetUpByRecording(RecordingTableElement container, bool confirmed)
    {
        if (HasUnsavedChanges() && !confirmed)
        {
            PopupScreenHandler.Instance.ShowUnsavedChanges(() => SetUpByRecording(container, true));
            return;
        }

        nameInputField.interactable = true;
        descriptionInputField.interactable = true;
        _currentContainer = container;
        nameInputField.text = container.recording.name;
        descriptionInputField.text = container.recording.description;
        pathTMP.text = container.recording.data;
        dateInputField.text = container.recording.date.ToString("dd.MM.yyyy HH:mm:ss");
        TimeSpan timeSpan = TimeSpan.FromSeconds(container.recording.neededTime);
        durationInputField.text = timeSpan.ToString("hh':'mm':'ss");
        workpieceInputField.text = TranslationController.Instance.Translate(container.recording.workpiece.name);
        topCoatInputField.text = container.recording.coat.name;
        baseCoatInputField.text = container.recording.baseCoat != null
            ? container.recording.baseCoat.name
            : TranslationController.Instance.Translate("recording-none");
        saveButtonText.text = TranslationController.Instance.Translate(
            container.recordingLocation == RecordingLocation.Local ? "recording-upload" : "recording-save");

        SetUnsavedChanges(false);
        if (container.recordingLocation == RecordingLocation.Local)
            saveButton.interactable = true;

        if (File.Exists(container.recording.GetPreviewFilePath()))
            LoadPreview(container);
        else if (container.recordingLocation != RecordingLocation.Local)
            DownloadAndLoadRecordingPreview(container);
        else
            previewImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// Downloads the recording preview image and displays it.
    /// </summary>
    private void DownloadAndLoadRecordingPreview(RecordingTableElement container)
    {
        PopupScreenHandler.Instance.ShowLoadingData();

        StartCoroutine(RestConnector.DownloadFile("/recordings/" + container.recording.id + "/file/preview",
            container.recording.GetPreviewFilePath(),
            () =>
            {
                LoadPreview(container);
                PopupScreenHandler.Instance.Close();
            }, () =>
            {
                previewImage.gameObject.SetActive(false);
                PopupScreenHandler.Instance.ShowConnectionError();
            }));
    }

    private void LoadPreview(RecordingTableElement container)
    {
        if (DataController.Instance.LoadRecordingScreenshot(container.recording, out Texture2D texture))
        {
            previewImage.gameObject.SetActive(true);
            previewImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            previewImage.preserveAspect = true;
        }
        else
            previewImage.gameObject.SetActive(false);
    }

    private void DownloadRecording(RecordingTableElement container)
    {
        PopupScreenHandler.Instance.ShowLoadingScreen("popup-download-recording",
            "popup-downloading-recording");

        StartCoroutine(DataController.Instance.DownloadRecording(container.recording, () =>
        {
            container.Init(container.recording, RecordingLocation.LocalAndServer, SetUpByRecording,
                DeleteRecording,
                DownloadRecording);
            PopupScreenHandler.Instance.ShowMessage("popup-download-recording",
                "popup-downloaded-recording");
        }, () => PopupScreenHandler.Instance.ShowConnectionError()));
    }

    private void DeleteRecording(RecordingTableElement container, bool confirmed, RecordingLocation location)
    {
        if (!confirmed)
        {
            if (container.recordingLocation == RecordingLocation.LocalAndServer)
                PopupScreenHandler.Instance.ShowRemoveRecordingConfirmation(
                    () => DeleteRecording(container, true, RecordingLocation.Server),
                    () => DeleteRecording(container, true, RecordingLocation.Local),
                    () => DeleteRecording(container, true, RecordingLocation.LocalAndServer));
            else
                PopupScreenHandler.Instance.ShowConfirmation("popup-remove-recording",
                    container.recordingLocation == RecordingLocation.Local
                        ? "popup-remove-recording-confirm-local"
                        : "popup-remove-recording-confirm-remote",
                    () => DeleteRecording(container, true, container.recordingLocation));
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-remove-recording", "popup-removing-recording");
        DataController.Instance.DeleteRecording(container.recording, location, () =>
        {
            // delete container of the recorder is deleted in both locations or in the only possible location
            if (container.recordingLocation != RecordingLocation.LocalAndServer ||
                location == RecordingLocation.LocalAndServer)
            {
                if (_currentContainer != null && container.recording.data == _currentContainer.recording.data)
                    ResetFields();
                Destroy(container.gameObject);
            }
            // update location of the recording was saved in both locations beforehand and is only removed in one location
            else
            {
                container.Init(container.recording,
                    location == RecordingLocation.Local ? RecordingLocation.Server : RecordingLocation.Local,
                    SetUpByRecording, DeleteRecording, DownloadRecording);
            }

            PopupScreenHandler.Instance.ShowMessage("popup-remove-recording", "popup-removed-recording");
        }, OnConflict, PopupScreenHandler.Instance.ShowConnectionError);
    }

    private void SaveRecording()
    {
        if (ValuesMissing())
        {
            PopupScreenHandler.Instance.ShowMissingValues("popup-recording-missing-values", _missingValuesString);
            return;
        }

        if (!ValidationUtil.ValidateName(nameInputField.text))
            return;

        PopupScreenHandler.Instance.ShowLoadingScreen(
            _currentContainer.recordingLocation == RecordingLocation.Local
                ? "popup-upload-recording"
                : "popup-update-recording", "popup-recording-uploading");
        Recording currentRecording = _currentContainer.recording.ShallowCopy();
        currentRecording.name = nameInputField.text;
        currentRecording.description = descriptionInputField.text;
        currentRecording.data = currentRecording.name;

        if (currentRecording.id >= 0)
        {
            RestConnector.Put(currentRecording, "/recordings", SuccessfulSave,
                PopupScreenHandler.Instance.ShowConnectionError, OnConflict);
        }
        else
        {
            DataController.Instance.UploadRecording(currentRecording.GetZipFile(),
                _currentContainer.recording.GetRecordingDirectory(), currentRecording,
                SuccessfulSave, PopupScreenHandler.Instance.ShowConnectionError, OnConflict, () =>
                    PopupScreenHandler.Instance.ShowMessage("popup-file-too-big",
                        TranslationController.Instance.Translate("popup-file-too-big-text",
                            ConfigController.Instance.GetMaxFileSize())));
        }
    }

    private void SuccessfulSave(Recording updatedRecording)
    {
        if (_currentContainer.recordingLocation == RecordingLocation.Local)
        {
            _currentContainer.recording.Update(updatedRecording);
            PopupScreenHandler.Instance.ShowMessage("popup-upload-recording", "popup-uploaded-recording");
            _currentContainer.Init(updatedRecording, RecordingLocation.LocalAndServer, SetUpByRecording,
                DeleteRecording, DownloadRecording);
        }
        else
        {
            if (_currentContainer.recordingLocation != RecordingLocation.Server)
                _currentContainer.recording.Update(updatedRecording);
            PopupScreenHandler.Instance.ShowMessage("popup-update-recording", "popup-updated-recording");
            _currentContainer.Init(updatedRecording, _currentContainer.recordingLocation, SetUpByRecording,
                DeleteRecording, DownloadRecording);
        }

        SetUpByRecording(_currentContainer, true);
        _currentContainer.containerButton.Select();
        SetUnsavedChanges(false);
    }

    private void OnConflict(string message)
    {
        if (message == "NAME")
            PopupScreenHandler.Instance.ShowMessage("popup-recording-exists", "popup-recording-exists-text");
        else if (message == "COAT")
            PopupScreenHandler.Instance.ShowMessage("popup-recording-invalid", "popup-recording-invalid-text");
        else if (message == "TASK")
            PopupScreenHandler.Instance.ShowMessage("popup-remove-recording-error",
                "popup-remove-recording-error-text");
        else
            PopupScreenHandler.Instance.ShowConnectionError();
    }

    private bool ValuesMissing()
    {
        _missingValuesString = "";
        if (nameInputField.text == "")
            _missingValuesString += " " + TranslationController.Instance.Translate("recording-name-missing") + "\n";
        return nameInputField.text == "";
    }

    private void SortRecordings()
    {
        SortListElements<RecordingTableElement>(recordingsList,
            (e1, e2) => String.Compare(e1.recording.name, e2.recording.name,
                StringComparison.CurrentCultureIgnoreCase));
    }
}
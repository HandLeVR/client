using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Provides a preview für recordings.
/// </summary>
public class RecordingSelectionPreview : MonoBehaviour
{
    public TMP_InputField recordingNameInputField;
    public TMP_InputField descriptionInputField;
    public TMP_InputField filepathInputField;
    public TMP_InputField dateInputField;
    public TMP_InputField durationInputField;
    public TMP_InputField coatInputField;
    public TMP_InputField baseCoatInputField;
    public Image previewImage;

    private void OnEnable()
    {
        ClearPreview();
    }

    public void SetUpPreviewPanel(Recording recording)
    {
        recordingNameInputField.text = recording.name;
        descriptionInputField.gameObject.SetActive(recording.description.Length > 0);
        descriptionInputField.text = recording.description;
        descriptionInputField.textComponent.enableWordWrapping = true;
        filepathInputField.text = recording.data;
        dateInputField.text = recording.date.ToString("dd.MM.yyyy HH:mm:ss");
        TimeSpan timeSpan = TimeSpan.FromSeconds(recording.neededTime);
        durationInputField.text = timeSpan.ToString("hh':'mm':'ss");
        coatInputField.text = recording.coat != null ? recording.coat.name : "Keiner";
        baseCoatInputField.text = recording.baseCoat != null ? recording.baseCoat.name : "Keiner";
        
        if (File.Exists(recording.GetPreviewFilePath()))
            LoadPreview(recording);
        else 
            DownloadAndLoadRecordingPreview(recording);
    }

    private void ClearPreview()
    {
        recordingNameInputField.text = string.Empty;
        descriptionInputField.text = string.Empty;
        filepathInputField.text = string.Empty;
        dateInputField.text = string.Empty;
        durationInputField.text = string.Empty;
        coatInputField.text = string.Empty;
        baseCoatInputField.text = string.Empty;
        previewImage.sprite = null;
    }

    private void DownloadAndLoadRecordingPreview(Recording recording)
    {
        PopupScreenHandler.Instance.ShowLoadingData();

        StartCoroutine(RestConnector.DownloadFile("/recordings/" + recording.id + "/file/preview",
            recording.GetPreviewFilePath(),
            () =>
            {
                LoadPreview(recording);
                PopupScreenHandler.Instance.Close();
            }, () =>
            {
                previewImage.gameObject.SetActive(false);
                PopupScreenHandler.Instance.ShowConnectionError();
            }));
    }

    private void LoadPreview(Recording recording)
    {
        if (DataController.Instance.LoadRecordingScreenshot(recording, out Texture2D texture))
        {
            previewImage.gameObject.SetActive(true);
            previewImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            previewImage.preserveAspect = true;
        }
        else
            previewImage.gameObject.SetActive(false);
    }
}
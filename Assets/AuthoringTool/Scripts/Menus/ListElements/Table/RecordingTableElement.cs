using translator;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Represents a recording entry in the recording menu.
/// </summary>
public class RecordingTableElement : BasicTableElement
{
    public Button downloadButton;
    public Recording recording;
    public RecordingLocation recordingLocation;
    
    public void Init(Recording recording, RecordingLocation recordingLocation, UnityAction<RecordingTableElement, bool> onClick,
        UnityAction<RecordingTableElement, bool, RecordingLocation> onDelete, UnityAction<RecordingTableElement> onDownload)
    {
        this.recording = recording;
        this.recordingLocation = recordingLocation;
        text1.text = recording.name;
        text2.text = recording.date.ToString("dd.MM.yyyy HH:mm:ss");
        text3.text = TranslationController.Instance.Translate(recording.workpiece.name);
        text4.text = GetRecordingLocationString();
        containerButton.onClick.RemoveAllListeners();
        containerButton.onClick.AddListener(() => onClick(this, false));
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => onDelete(this, false, recordingLocation));
        deleteButton.gameObject.SetActive(recordingLocation != RecordingLocation.Server || recording.permission.editable);
        downloadButton.gameObject.SetActive(recordingLocation == RecordingLocation.Server);
        downloadButton.onClick.RemoveAllListeners();
        downloadButton.onClick.AddListener(() => onDownload(this));
        if (recordingLocation != RecordingLocation.Local && recording.permission != null)
        {
            infoButton.gameObject.SetActive(true);
            infoButton.onClick.RemoveAllListeners();
            infoButton.onClick.AddListener(() => PopupScreenHandler.Instance.ShowInfos(recording.permission, true));
        }
        else
            infoButton.gameObject.SetActive(false);
    }

    private string GetRecordingLocationString()
    {
        switch (recordingLocation)
        {
            case RecordingLocation.Local:
                return "Lokal";
            case RecordingLocation.Server:
                return "Server";
            default:
                return "Server und Lokal";
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Events;
using SFB;
using translator;

/// <summary>
/// Controls the import and export of tasks.
/// </summary>
public class ImportExportController : Singleton<ImportExportController>
{
    /// <summary>
    /// Allows the user to choose an archive containing tasks to import. The archive is send to the server
    /// and the server handles the import process.
    /// </summary>
    public void ImportTask(UnityAction onSuccess)
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Lernaufgabe importieren", "", "zip", false);

        if (paths.Length == 0)
            return;

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-import-tasks", "popup-importing-tasks");
        RestConnector.UploadImportFile("/tasks/import", new FileInfo(paths[0]), _ => onSuccess.Invoke(),
            () => PopupScreenHandler.Instance.ShowMessage("popup-import-file-error", "popup-import-file-error-text"),
            () => PopupScreenHandler.Instance.ShowMessage("popup-file-too-big",
                TranslationController.Instance.Translate("popup-file-too-big-text",
                    ConfigController.Instance.GetMaxFileSize())),
            PopupScreenHandler.Instance.ShowConnectionError);
    }

    /// <summary>
    /// Exports the selected tasks with all used elements.
    /// </summary>
    public void ExportTasks()
    {
        PopupScreenHandler.Instance.ShowLoadingScreen("popup-export-tasks", "popup-exporting-tasks");
        List<Task> taskList = SelectionPopup.Instance.GetSelectedIds().Select(id => DataController.Instance.tasks[id])
            .ToList();

        var json = JsonConvert.SerializeObject(taskList);
        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        string archiveName = "Task-" + timestamp;

        if (!Directory.Exists(DataController.Instance.exportPath))
            Directory.CreateDirectory(DataController.Instance.exportPath);
        string archiveParentPath = Path.Combine(DataController.Instance.exportPath, archiveName);
        Directory.CreateDirectory(archiveParentPath);
        string usedMediaPath = Path.Combine(archiveParentPath, "usedMedia");
        Directory.CreateDirectory(usedMediaPath);
        string usedRecordingsPath = Path.Combine(archiveParentPath, "usedRecordings");
        Directory.CreateDirectory(usedRecordingsPath);

        File.WriteAllText(Path.Combine(archiveParentPath, archiveName + ".json"), json);

        StartCoroutine(LoadFiles(taskList, archiveParentPath, usedMediaPath, usedRecordingsPath, () =>
        {
            Directory.Delete(archiveParentPath, true);
            PopupScreenHandler.Instance.ShowMessage("popup-export-tasks",
                String.Format(TranslationController.Instance.Translate("popup-exported-task-result"),
                    Path.Combine(archiveParentPath + ".zip")));
        }));
    }

    /// <summary>
    /// Downloads all media files and recordings used in the tasks from the server if necessary and creates a zip file
    /// containing the task and the used elements.
    /// </summary>
    private IEnumerator LoadFiles(List<Task> tasks, string archiveParentPath, string usedMediaPath,
        string usedRecordingsPath, UnityAction onSuccess)
    {
        foreach (Task task in tasks)
        {
            bool errorDownloading = false;
            foreach (Media media in task.usedMedia)
            {
                string filePath = Path.Combine(usedMediaPath, Path.GetFileName(media.data));
                if (File.Exists(filePath))
                    continue;

                yield return RestConnector.DownloadFile("/media/" + media.id + "/file", filePath, () => { }, () =>
                {
                    Directory.Delete(archiveParentPath, true);
                    errorDownloading = true;
                    PopupScreenHandler.Instance.ShowMessage("popup-export-tasks-failed",
                        String.Format(TranslationController.Instance.Translate("file-connection-error"),
                            media.name));
                });

                if (errorDownloading)
                    yield break;
            }

            foreach (Recording recording in task.usedRecordings)
            {
                string filePath = Path.Combine(usedRecordingsPath, Path.GetFileName(recording.GetZipFile().FullName));
                if (File.Exists(filePath))
                    continue;

                yield return DataController.Instance.DownloadRecording(recording, filePath, () => { }, () =>
                {
                    Directory.Delete(archiveParentPath, true);
                    errorDownloading = true;
                    PopupScreenHandler.Instance.ShowMessage("popup-export-tasks-failed",
                        String.Format(TranslationController.Instance.Translate("file-connection-error"),
                            recording.name));
                });

                if (errorDownloading)
                    yield break;
            }
        }

        CreateZipFile(archiveParentPath, onSuccess);
    }

    private async void CreateZipFile(string archiveParentPath, UnityAction onSuccess)
    {
        await ZipUtil.CompressAsync(archiveParentPath, archiveParentPath + ".zip", onSuccess);
    }
}
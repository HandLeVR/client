using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// This controller downloads the used media files and recordings of a task.
/// </summary>
public class TaskPreparationController : Singleton<TaskPreparationController>
{
    public readonly Dictionary<long, AudioClip> loadedAudioClips = new();
    public readonly Dictionary<long, Sprite> loadedImages = new();
    public readonly Dictionary<long, Recording> loadedRecordings = new();

    // steps are used to visualize the progress of the download in form of a progress bar
    private int neededSteps;
    private int currentSteps;
    private bool errorDownloading;

    public void PrepareMedia(Task task, UnityAction onFinish)
    {
        currentSteps = 0;
        loadedAudioClips.Clear();
        loadedImages.Clear();
        loadedRecordings.Clear();
        DataController.Instance.coats.Clear();
        PopupScreenController.Instance.ShowLoadingScreen("loading-task", true);

        RestConnector.GetTaskData(task, () =>
        {
            neededSteps = task.usedMedia.Count + task.usedRecordings.Count;
            StartCoroutine(LoadFiles(task, () =>
            {
                PopupScreenController.Instance.ClosePopupScreen();
                onFinish.Invoke();
            }));
        }, () => PopupScreenController.Instance.ShowConfirmationScreen("connection-error"));
    }

    private IEnumerator LoadFiles(Task task, UnityAction onSuccess)
    {
        foreach (Media media in task.usedMedia)
        {
            yield return RestConnector.DownloadFile("/media/" + media.id + "/file", media.GetPath(),
                () => { }, () =>
                {
                    errorDownloading = true;
                    PopupScreenController.Instance.ShowConfirmationScreen("file-connection-error", media.name);
                });

            if (errorDownloading)
                yield break;

            UpdateLoadingBarScreen();
        }

        foreach (Recording recording in task.usedRecordings)
        {
            yield return DataController.Instance.DownloadRecording(recording, null, () =>
            {
                errorDownloading = true;
                PopupScreenController.Instance.ShowConfirmationScreen("recording-connection-error", recording.name);
            });

            if (errorDownloading)
                yield break;

            UpdateLoadingBarScreen();
        }

        foreach (Media media in task.usedMedia)
        {
            if (media.type == Media.MediaType.Audio)
                yield return LoadAudioFile(media);
            else if (media.type == Media.MediaType.Image)
                yield return LoadImage(media);
        }

        foreach (Recording recording in task.usedRecordings)
            loadedRecordings.Add(recording.id, recording);

        foreach (Coat coat in task.usedCoats)
            DataController.Instance.coats[coat.id] = coat;

        onSuccess.Invoke();
    }

    private void UpdateLoadingBarScreen()
    {
        currentSteps++;
        PopupScreenController.Instance.UpdateLoadingBar(currentSteps / (float)neededSteps);
    }

    private IEnumerator LoadImage(Media media)
    {
        using (UnityWebRequest uwr =
            UnityWebRequestTexture.GetTexture(Path.Combine(Application.streamingAssetsPath, media.data)))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError ||
                uwr.result == UnityWebRequest.Result.ProtocolError)
                Debug.Log(uwr.error);
            else
            {
                // SpriteMeshType.FullRect speeds up process
                // Source: https://forum.unity.com/threads/any-way-to-speed-up-sprite-create.529525
                Texture2D tex = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
                loadedImages[media.id] = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), 16f, 0, SpriteMeshType.FullRect);
            }
        }
    }

    private IEnumerator LoadAudioFile(Media media)
    {
        string url = "file:///" + Path.Combine(Application.streamingAssetsPath, media.data);
        yield return AudioUtil.LoadAudioFile(url, clip => loadedAudioClips[media.id] = clip);
    }
}
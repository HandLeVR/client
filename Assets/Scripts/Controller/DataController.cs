using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// disables warning for ZipUtil.ExtractAsync
#pragma warning disable 4014

/// <summary>
/// This class takes care of sending, receiving and holding the data.
/// </summary>
public class DataController : PersistentSingleton<DataController>
{
    // dictionaries temporary holding the data and allowing to access the elements by their ids
    public readonly Dictionary<long, User> users = new Dictionary<long, User>();
    public readonly Dictionary<long, UserGroup> userGroups = new Dictionary<long, UserGroup>();
    public readonly Dictionary<long, Media> media = new Dictionary<long, Media>();
    public readonly Dictionary<long, Workpiece> workpieces = new Dictionary<long, Workpiece>();
    public readonly Dictionary<long, Coat> coats = new Dictionary<long, Coat>();
    public readonly Dictionary<long, Task> tasks = new Dictionary<long, Task>();
    public readonly Dictionary<long, TaskCollection> taskCollections = new Dictionary<long, TaskCollection>();
    public readonly Dictionary<long, Recording> remoteRecordings = new Dictionary<long, Recording>();
    public readonly Dictionary<long, Media> availableAudios = new Dictionary<long, Media>();
    public readonly Dictionary<long, Media> availableImages = new Dictionary<long, Media>();
    public readonly Dictionary<long, Media> availableVideos = new Dictionary<long, Media>();

    // maintains the current local recordings
    public readonly List<Recording> localRecordings = new List<Recording>();

    // default coat which is defined in the fallback data
    // used when creating a new coat
    [HideInInspector] public Coat defaultCoat;

    // indicates whether there was a connection beforehand
    [HideInInspector] public ConnectionState connectionState = ConnectionState.Unknown;

    // paths referring to the files in the streaming assets folder of the authoring tool when running the build or
    // to the files in the own streaming assets folder if the application runs in the editor
    [HideInInspector] public string recordingsDirectoryPath;
    [HideInInspector] public string taskResultsDirectoryPath;
    [HideInInspector] public string fallbackDataPath;
    [HideInInspector] public string privacyFilePath;
    [HideInInspector] public string translationFilePath;
    [HideInInspector] public string configFilePath;
    [HideInInspector] public string streamingAssetsPath;
    [HideInInspector] public string exportPath;

    // indicates whether the reflection tool was selected in the application selection menu
    [HideInInspector] public bool reflectionToolChosen;

    // current user properties
    private User _user;
    public User CurrentUser { get; set; }
    public string CurrentAccessToken { get; set; }
    public string CurrentRefreshToken { get; set; }

    public new void Awake()
    {
        base.Awake();
        streamingAssetsPath = Application.streamingAssetsPath;
        recordingsDirectoryPath = Path.Combine(streamingAssetsPath, "Recordings");
        taskResultsDirectoryPath = Path.Combine(streamingAssetsPath, "TaskResults");
        fallbackDataPath = Path.Combine(streamingAssetsPath, "fallbackData.json");
        privacyFilePath = Path.Combine(streamingAssetsPath, "privacy.txt");
        translationFilePath = Path.Combine(streamingAssetsPath, "Translation.txt");
        configFilePath = Path.Combine(streamingAssetsPath, "Config.txt");
        exportPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Export");

        // loads the fall back data to allow the usage of the test mode of the training application without a server
        LoadFallbackData();

        // the first coat of the fallback data is the default coat and should not be usable
        defaultCoat = coats[0];
        coats.Remove(0);
    }

    /// <summary>
    /// Puts the object in the corresponding dictionary in dependence of the type of the object.
    /// </summary>
    public void Put(object obj)
    {
        if (obj.GetType() == typeof(User))
            users[((User)obj).id] = (User)obj;
        else if (obj.GetType() == typeof(UserGroup))
            userGroups[((UserGroup)obj).id] = (UserGroup)obj;
        else if (obj.GetType() == typeof(Media))
            media[((Media)obj).id] = (Media)obj;
        else if (obj.GetType() == typeof(Coat))
            coats[((Coat)obj).id] = (Coat)obj;
        else if (obj.GetType() == typeof(Task))
            tasks[((Task)obj).id] = (Task)obj;
    }

    /// <summary>
    /// Deletes the object from the corresponding dictionary in dependence of the type of the object.
    /// </summary>
    public void Delete(object obj)
    {
        if (obj.GetType() == typeof(User))
            users.Remove(((User)obj).id);
        else if (obj.GetType() == typeof(UserGroup))
            userGroups.Remove(((UserGroup)obj).id);
        else if (obj.GetType() == typeof(Media))
            media.Remove(((Media)obj).id);
        else if (obj.GetType() == typeof(Coat))
            coats.Remove(((Coat)obj).id);
        else if (obj.GetType() == typeof(Task))
            tasks.Remove(((Task)obj).id);
    }

    /// <summary>
    /// Removes media files after when quitting the application to avoid committing them.
    /// </summary>
    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        string videosDirectory = Path.Combine(Application.streamingAssetsPath,
            Media.MediaTypeToDirectory(Media.MediaType.Video));
        if (Directory.Exists(videosDirectory))
            new DirectoryInfo(videosDirectory).GetFiles().ToList().ForEach(file => file.Delete());
        string audioDirectory = Path.Combine(Application.streamingAssetsPath,
            Media.MediaTypeToDirectory(Media.MediaType.Audio));
        if (Directory.Exists(audioDirectory))
            new DirectoryInfo(audioDirectory).GetFiles().ToList().ForEach(file => file.Delete());
        string imagesDirectory = Path.Combine(Application.streamingAssetsPath,
            Media.MediaTypeToDirectory(Media.MediaType.Image));
        if (Directory.Exists(imagesDirectory))
            new DirectoryInfo(imagesDirectory).GetFiles().ToList().ForEach(file => file.Delete());
#endif
    }

    /// <summary>
    /// Loads the fall back data to allow the usage of the test mode of the training application without a server.
    /// </summary>
    private void LoadFallbackData()
    {
        JObject jObject = JObject.Parse(File.ReadAllText(fallbackDataPath));
        if (jObject == null)
            return;
        if (jObject.TryGetValue("coats", out JToken jToken))
            SetValues(jToken as JArray, coats);
        if (jObject.TryGetValue("workpieces", out jToken))
            SetValues(jToken as JArray, workpieces);
    }

    /// <summary>
    /// Loads the data from the server in dependence of the request type.
    /// </summary>
    /// <param name="requestType">Determines the data loaded from the server.</param>
    /// <param name="dataLoaded">Called after the data is loaded successfully.</param>
    /// <param name="error">Called if there was an error.</param>
    public void UpdateData(RequestType requestType, UnityAction dataLoaded, UnityAction error)
    {
        if (connectionState == ConnectionState.NoConnection)
        {
            // update the local recordings so they can be used in the test mode even without a server connection
            UpdateLocalRecordings();
        }
        else
            LoadFromServer(requestType, () =>
            {
                // request type is Basic on starting the VR application after login
                // UpdateLocalRecordings needs to be called after potentially remote recordings a loaded
                if (requestType == RequestType.Recordings || requestType == RequestType.Basic)
                    UpdateLocalRecordings();
                dataLoaded.Invoke();
            }, error);
    }

    /// <summary>
    /// Collects the local recordings and checks whether there were changes on the server which need to be adopted.
    /// For example if a recording was deleted from someone else on the server the id of the local recording needs to be reset.
    /// </summary>
    public void UpdateLocalRecordings()
    {
        localRecordings.Clear();
        if (!Directory.Exists(recordingsDirectoryPath))
            Directory.CreateDirectory(recordingsDirectoryPath);
        UpdateLocalRecordings(Directory.GetFiles(recordingsDirectoryPath, "*_recording.json",
            SearchOption.TopDirectoryOnly));
        if (Directory.Exists(Path.Combine(recordingsDirectoryPath, "Upload")))
            UpdateLocalRecordings(Directory.GetFiles(Path.Combine(recordingsDirectoryPath, "Upload"),
                "*_recording.json", SearchOption.TopDirectoryOnly));
    }

    /// <summary>
    /// Updates the local recordings in dependence of the remote recordings.
    /// </summary>
    private void UpdateLocalRecordings(string[] recordings)
    {
        foreach (string filePath in recordings)
        {
            Recording localRecording = JsonConvert.DeserializeObject<Recording>(File.ReadAllText(filePath));
            localRecordings.Add(localRecording);

            // if there is no connection we don't have any information about the remote recordings
            if (connectionState == ConnectionState.NoConnection)
                continue;

            // needed for new recordings
            if (string.IsNullOrEmpty(localRecording.hash))
            {
                localRecording.GenerateHash();
                localRecording.SaveRecordingFile();
            }

            // not uploaded recordings will not be checked for a match with an uploaded recording
            if (localRecording.id < 0)
                continue;

            // check whether there is a remote recording with the same id
            // if they have the same hash they are the same recording
            if (remoteRecordings.TryGetValue(localRecording.id, out Recording remoteRecording) &&
                localRecording.hash == remoteRecording.hash)
            {
                // we update the name, description or file name of the local recording if necessary
                localRecording.Update(remoteRecording);
            }
            // if there is no remote recording with the id or they have not the same hash, we set the
            // id to -1 to remove the connection to the remote recording
            // already downloaded recordings which are removed from the server may be deleted manually
            else
            {
                localRecording.RemoteRecordingRemoved();
            }
        }
    }

    /// <summary>
    /// Updates the dictionaries containing the available media elements.
    /// </summary>
    public void UpdateMedia()
    {
        availableAudios.Clear();
        availableImages.Clear();
        availableVideos.Clear();

        foreach (KeyValuePair<long, Media> m in media)
        {
            switch (m.Value.type)
            {
                case Media.MediaType.Audio:
                    availableAudios.Add(m.Key, m.Value);
                    break;
                case Media.MediaType.Image:
                    availableImages.Add(m.Key, m.Value);
                    break;
                case Media.MediaType.Video:
                    availableVideos.Add(m.Key, m.Value);
                    break;
            }
        }
    }

    /// <summary>
    /// Loads the data from the server and sets the data dictionaries in dependence oft the given request type.
    /// </summary>
    /// <param name="requestType">Request type determining the data loaded from the server.</param>
    /// <param name="dataLoaded">Called after the data is loaded successfully.</param>
    /// <param name="error">Called on error.</param>
    private void LoadFromServer(RequestType requestType, UnityAction dataLoaded, UnityAction error)
    {
        switch (requestType)
        {
            case RequestType.Tasks:
                RestConnector.SendGetRequest("/tasks")
                    .Then(helper => SetData(helper.Text, tasks))
                    .Then(() => RestConnector.SendGetRequest("/media"))
                    .Then(helper => SetData(helper.Text, media))
                    .Then(() => RestConnector.SendGetRequest("/workpieces"))
                    .Then(helper => SetData(helper.Text, workpieces))
                    .Then(() => RestConnector.SendGetRequest("/coats"))
                    .Then(helper => SetData(helper.Text, coats))
                    .Then(() => RestConnector.SendGetRequest("/recordings/noTaskResult"))
                    .Then(helper => SetData(helper.Text, remoteRecordings))
                    .Then(() => OnSuccess(dataLoaded))
                    .Catch(exception => { OnFailure(error, exception); });
                break;
            case RequestType.TaskCollections:
                RestConnector.SendGetRequest("/taskCollections")
                    .Then(helper => SetData(helper.Text, taskCollections))
                    .Then(() => RestConnector.SendGetRequest("/tasks"))
                    .Then(helper => SetData(helper.Text, tasks))
                    .Then(() => OnSuccess(dataLoaded))
                    .Catch(exception => OnFailure(error, exception));
                break;
            case RequestType.Users:
                RestConnector.SendGetRequest("/users")
                    .Then(helper => SetData(helper.Text, users))
                    .Then(() => RestConnector.SendGetRequest("/tasks"))
                    .Then(helper => SetData(helper.Text, tasks))
                    .Then(() => RestConnector.SendGetRequest("/taskCollections"))
                    .Then(helper => SetData(helper.Text, taskCollections))
                    .Then(() => OnSuccess(dataLoaded))
                    .Catch(exception => OnFailure(error, exception));
                break;
            case RequestType.UserGroups:
                RestConnector.SendGetRequest("/userGroups")
                    .Then(helper => SetData(helper.Text, userGroups))
                    .Then(() => RestConnector.SendGetRequest("/users"))
                    .Then(helper => SetData(helper.Text, users))
                    .Then(() => RestConnector.SendGetRequest("/tasks"))
                    .Then(helper => SetData(helper.Text, tasks))
                    .Then(() => RestConnector.SendGetRequest("/taskCollections"))
                    .Then(helper => SetData(helper.Text, taskCollections))
                    .Then(() => OnSuccess(dataLoaded))
                    .Catch(exception => OnFailure(error, exception));
                break;
            case RequestType.Coats:
                RestConnector.SendGetRequest("/coats")
                    .Then(helper => SetData(helper.Text, coats))
                    .Then(() => OnSuccess(dataLoaded))
                    .Catch(exception => OnFailure(error, exception));
                break;
            case RequestType.Media:
                RestConnector.SendGetRequest("/media")
                    .Then(helper => SetData(helper.Text, media))
                    .Then(() => OnSuccess(dataLoaded))
                    .Catch(exception => OnFailure(error, exception));
                break;
            case RequestType.Recordings:
                RestConnector.SendGetRequest("/recordings/noTaskResult")
                    .Then(helper => SetData(helper.Text, remoteRecordings))
                    .Then(() => OnSuccess(dataLoaded))
                    .Catch(exception => OnFailure(error, exception));
                break;
            case RequestType.Basic:
                RestConnector.SendGetRequest("/coats")
                    .Then(helper => SetData(helper.Text, coats))
                    .Then(() => RestConnector.SendGetRequest("/workpieces"))
                    .Then(helper => SetData(helper.Text, workpieces))
                    .Then(() => RestConnector.SendGetRequest("/recordings/noTaskResult"))
                    .Then(helper => SetData(helper.Text, remoteRecordings))
                    .Then(() => OnSuccess(dataLoaded))
                    .Catch(exception => OnFailure(error, exception));
                break;
        }
    }

    /// <summary>
    /// Sets the connection state to connected and calls the given action afterwards.
    /// </summary>
    private void OnSuccess(UnityAction dataLoaded)
    {
        if (connectionState == ConnectionState.Unknown)
            connectionState = ConnectionState.Connection;
        dataLoaded.Invoke();
    }

    /// <summary>
    /// Sets the connection state to not connected and calls the given action afterwards.
    /// </summary>
    private void OnFailure(UnityAction error, Exception exception)
    {
        Debug.Log(exception);
        if (connectionState == ConnectionState.Unknown)
            connectionState = ConnectionState.NoConnection;
        error.Invoke();
    }

    /// <summary>
    /// Sets the data contained by a response in dependence of the given dictionary.
    /// </summary>
    private void SetData<T>(string response, Dictionary<long, T> dictionary)
    {
        SetValues(JArray.Parse(response), dictionary);
        if (dictionary.Equals(media))
            UpdateMedia();
    }

    /// <summary>
    /// Sets the data contained by a json array in dependence of the given dictionary.
    /// </summary>
    private void SetValues<T>(JArray jArray, Dictionary<long, T> dictionary)
    {
        dictionary.Clear();
        foreach (JToken supportInfoJson in jArray)
        {
            T supportInfo = supportInfoJson.ToObject<T>();
            dictionary.Add(supportInfoJson["id"].Value<long>(), supportInfo);
        }
    }

    /// <summary>
    /// Deletes a recording on the server or local.
    /// </summary>
    /// <param name="recording">The recording we want to save.</param>
    /// <param name="location">On the server or locally?</param>
    /// <param name="onSuccess">Is called after successfully removing the recording.</param>
    /// <param name="onConflict">Is called if there is a conflict.</param>
    /// <param name="OnError">Is called if there is an error.</param>
    public void DeleteRecording(Recording recording, RecordingLocation location, UnityAction onSuccess,
        UnityAction<string> onConflict, UnityAction OnError)
    {
        // delete recording locally
        if (location == RecordingLocation.Local || location == RecordingLocation.LocalAndServer)
        {
            localRecordings.Remove(recording);
            recording.DeleteRecording();
        }

        // delete recording on the server
        if (location == RecordingLocation.Server || location == RecordingLocation.LocalAndServer)
        {
            RestConnector.Delete(recording, "/recordings/" + recording.id, () =>
            {
                if (location == RecordingLocation.Server)
                    recording.RemoteRecordingRemoved();
                onSuccess.Invoke();
            }, onConflict, OnError);
        }
        else
        {
            onSuccess.Invoke();
        }
    }

    /// <summary>
    /// Only loads the preview screenshot of the recording.
    /// </summary>
    /// <param name="recording">The corresponding recording.</param>
    /// <param name="texture">The texture in which the screenshot is loaded.</param>
    /// <returns>True if the screenshot exists, else False.</returns>
    public bool LoadRecordingScreenshot(Recording recording, out Texture2D texture)
    {
        texture = new Texture2D(1, 1);
        string path = recording.GetPreviewFilePath();
        if (!File.Exists(path))
            return false;
        byte[] content = File.ReadAllBytes(path);
        return texture.LoadImage(content);
    }

    /// <summary>
    /// Uploads recording
    /// </summary>
    /// <param name="zipFile">The path of the zip file which is created after all files belonging to the recording a packed.</param>
    /// <param name="zipDirectory">The directory of the zip file.</param>
    /// <param name="recording">The recording we want to upload.</param>
    /// <param name="onSuccess">Called on success.</param>
    /// <param name="onError">Called on error.</param>
    /// <param name="onConflict">Called on conflict.</param>
    /// <param name="onTooBig">Called if the recording is too big.</param>
    public async void UploadRecording(FileInfo zipFile, string zipDirectory, Recording recording,
        UnityAction<Recording> onSuccess, UnityAction onError, UnityAction<string> onConflict = null,
        UnityAction onTooBig = null)
    {
        // create a zip file and upload it afterwards
        await ZipUtil.CompressAsync(zipDirectory, zipFile.FullName,
            () =>
            {
                RestConnector.Upload(recording, "/recordings",
                    zipFile, onSuccess, onConflict, onTooBig, onError, () => ZipUtil.RemoveZipFile(zipFile.FullName));
            });
    }

    /// <summary>
    /// Uploads a task result.
    /// </summary>
    /// <param name="taskAssignment">The corresponding task assignment.</param>
    /// <param name="onSuccess">Called on success.</param>
    /// <param name="onError">Called on error.</param>
    public void UploadTaskResults(TaskAssignment taskAssignment, UnityAction onSuccess, UnityAction onError)
    {
        string[] recordings = GetRecordingsToUpload();

        if (recordings.Length == 0)
        {
            onSuccess.Invoke();
            return;
        }

        foreach (string filePath in recordings)
        {
            Recording recording = JsonConvert.DeserializeObject<Recording>(File.ReadAllText(filePath));
            if (taskAssignment == null || recording.taskResult.taskAssignment.id == taskAssignment.id)
            {
                UploadRecording(recording.GetZipFile(), recording.GetRecordingDirectory(), recording, remoteRecording =>
                    {
                        recording.Update(remoteRecording);
                        onSuccess.Invoke();
                    },
                    onError);
            }
        }
    }

    /// <summary>
    /// Collects all recordings in the upload directory.
    /// </summary>
    public string[] GetRecordingsToUpload()
    {
        string path = Path.Combine(taskResultsDirectoryPath, "Upload");
        return !Directory.Exists(path)
            ? new string[] { }
            : Directory.GetFiles(path, "*_recording.json", SearchOption.TopDirectoryOnly);
    }

    /// <summary>
    /// Downloads a recording.
    /// </summary>
    /// <param name="recording">The recording we want to download.</param>
    /// <param name="onSuccess">Called on success.</param>
    /// <param name="onError">Called on error.</param>
    public IEnumerator DownloadRecording(Recording recording, UnityAction onSuccess, UnityAction onError)
    {
        return DownloadRecording(recording, recording.GetZipFile().FullName, onSuccess, onError);
        ;
    }

    public IEnumerator DownloadRecording(Recording recording, string filePath, UnityAction onSuccess,
        UnityAction onError)
    {
        return RestConnector.DownloadFile("/recordings/" + recording.id + "/file", filePath, () =>
        {
            ZipUtil.ExtractAsync(recording.GetZipFile(), () =>
            {
                recording.SaveRecordingFile();
                onSuccess?.Invoke();
            });
        }, onError);
    }

    /// <summary>
    /// Determines the data we want to load when sending a request.
    /// </summary>
    public enum RequestType
    {
        Tasks,
        TaskCollections,
        Users,
        UserGroups,
        Coats,
        Media,
        Recordings,
        Basic
    }

    /// <summary>
    /// Describes the connection state to the server and determines whether the local db is used.
    /// </summary>
    public enum ConnectionState
    {
        // no information because no request was send yet
        Unknown,

        // there was a successful connection to the server since the start of the application
        Connection,

        // the first connection attempt failed
        NoConnection
    }
}
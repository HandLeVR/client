using System;
using System.IO;
using Newtonsoft.Json;

public class Recording
{
    public long id;
    public Permission permission;
    public string hash;
    public string name;
    public string description;
    public DateTime date;
    public float neededTime;
    public Workpiece workpiece;
    public Coat coat;
    public Coat baseCoat;
    public string data;
    public TaskResult taskResult;

    /// <summary>
    /// Returns th path to the zip file in form of a FileInfo.
    /// </summary>
    public FileInfo GetZipFile()
    {
        return new FileInfo(Path.Combine(GetRootDirectory(), data + ".zip"));
    }

    /// <summary>
    /// Gets the root directory of the recording which depends on whether the recording belongs to a task result.
    /// </summary>
    private string GetRootDirectory()
    {
        return taskResult == null
            ? DataController.Instance.recordingsDirectoryPath
            : DataController.Instance.taskResultsDirectoryPath;
    }

    /// <summary>
    /// Gets the directory. If the id is -1 the recording is in the "Upload" directory.
    /// </summary>
    public string GetRecordingDirectory()
    {
        return Path.Combine(GetRootDirectory(), id < 0 ? "Upload" : "", data);
    }

    /// <summary>
    /// Updates a local recording in dependence of a corresponding remote recording.
    /// </summary>
    public void Update(Recording remoteRecording)
    {
        bool updateRecordingFile = false;
        // recording was uploaded so we need to move it from the upload directory to the root recording directory
        if (remoteRecording.id != id || remoteRecording.data != data)
        {
            Directory.Move(GetRecordingDirectory(), Path.Combine(GetRootDirectory(), remoteRecording.data));
            updateRecordingFile = true;
            RemoveRecordingFile();
        }
        id = remoteRecording.id;
        data = remoteRecording.data;
        name = remoteRecording.name;
        description = remoteRecording.description;
        if (updateRecordingFile)
            SaveRecordingFile();
    }

    /// <summary>
    /// Removes the connection to a remote recording if it was removed remotely and moves the recording to the upload directory.
    /// </summary>
    public void RemoteRecordingRemoved()
    {
        RemoveRecordingFile();
        Directory.Move(GetRecordingDirectory(), Path.Combine(GetRootDirectory(), "Upload", data));
        id = -1;
        SaveRecordingFile();
    }

    /// <summary>
    /// Removes the recording file.
    /// </summary>
    private void RemoveRecordingFile()
    {
        File.Delete(GetRecordingFilePath());
    }

    /// <summary>
    /// Serializes the recording into a json file.
    /// </summary>
    public void SaveRecordingFile()
    {
        File.WriteAllText(GetRecordingFilePath(), JsonConvert.SerializeObject(this));
    }

    /// <summary>
    /// Returns the path of the json recording file.
    /// </summary>
    private string GetRecordingFilePath()
    {
        return Path.Combine(Path.Combine(GetRootDirectory(), id < 0 ? "Upload" : ""), data + "_recording.json");
    }

    /// <summary>
    /// Removes the recording with all of its files.
    /// </summary>
    public void DeleteRecording()
    {
        Directory.Delete(GetRecordingDirectory(), true);
        File.Delete(GetRecordingFilePath());
    }

    /// <summary>
    /// Gets the path of the preview file.
    /// </summary>
    /// <returns></returns>
    public string GetPreviewFilePath()
    {
        return Path.Combine(GetRecordingDirectory(), "preview.png");
    }

    /// <summary>
    /// Generates a hash code to compare to recording by considering the first 100 chars of the frames file.
    /// </summary>
    public void GenerateHash()
    {
        char[] buffer = new char[100];
        using (StreamReader reader = new StreamReader(Path.Combine(GetRecordingDirectory(), "frames.json")))
        {
            reader.Read(buffer, 0, buffer.Length);
        }

        hash = HashGenerationUtil.ComputeSha256Hash(new string(buffer));
    }

    public Recording ShallowCopy()
    {
        return (Recording) MemberwiseClone();
    }
}


public enum RecordingLocation
{
    Local,
    Server,
    LocalAndServer
}
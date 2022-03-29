using System;
using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;
using Newtonsoft.Json;
using translator;
using UnityEngine.Events;

/// <summary>
/// Records a paint application to a file.
/// </summary>
public class CreateRecordingController : Singleton<CreateRecordingController>
{
    public RecorderPanel recorderPanel;
    public ScreenshotCamera screenshotCamera;
    public bool recording;
    public bool resetDrawablesOnRecord;

    [HideInInspector] public RecordingData currentRecording;
    [HideInInspector] public bool isSavingFile;

    private Transform _rightHand;
    private Transform _head;
    private string _assetName;
    private float _timer;
    private AudioClip _recordedClip;

    private void FixedUpdate()
    {
        if (!recording)
            return;

        float wideStreamRegulationValue = ApplicationController.Instance.sprayGun.GetWideStreamRegulationValue();
        Frame frame = new Frame
        {
            sprayingValue = ApplicationController.Instance.sprayGun.GetSprayingValue(),
            triggerValue = ApplicationController.Instance.sprayGun.GetTriggerValue(),
            wideStreamRegulationValue = wideStreamRegulationValue < 1 ? wideStreamRegulationValue : null,
            followHead = new ObjectData(_head),
            rightHand = new ObjectData(_rightHand),
            pinSpotOrigin = new ObjectData(ApplicationController.Instance.sprayGun.pinSpotOrigin),
            mode = ApplicationController.Instance.sprayGun.currentMode,
            recordedCollisionList = PaintStandHitController.Instance.GenerateRecordedCollisionsList()
        };
        ApplicationController.Instance.paintStand.PersistPaintStand(frame, currentRecording.frames.Count == 0);
        currentRecording.frames.Add(frame);
        _timer += Time.fixedDeltaTime;
    }

    public void StartRecordButtonClicked()
    {
        HandleRecordingAudioAndUI();
        HandleRecordingCreation(true, onFinish: () => recorderPanel.StartRecording(false));
    }

    private void HandleRecordingAudioAndUI()
    {
        if (recording)
        {
            recorderPanel.loadingScreen.SetActive(true);
            return;
        }

        recorderPanel.StartRecording(true);

        // start recording
        if (recorderPanel.deviceName != TranslationController.Instance.Translate("paint-shop-no-microphone"))
        {
            Debug.Log("Started Audio Recording with Mic: " + recorderPanel.deviceName);
            _recordedClip = Microphone.Start(recorderPanel.deviceName, true, 999, 44100);

            if (Microphone.IsRecording(recorderPanel.deviceName))
            {
                // check that the mic is recording, otherwise you'll get stuck in an infinite loop waiting for it to start
                while (!(Microphone.GetPosition(recorderPanel.deviceName) > 0))
                {
                    // wait until the recording has started. 
                }

                Debug.Log("recording started with " + recorderPanel.deviceName);
            }
            else
            {
                // microphone doesn't work for some reason
                Debug.Log(recorderPanel.deviceName + " doesn't work!");
            }
        }
    }


    public void HandleRecordingCreation(bool withAudio, TaskAssignment taskAssignment = null,
        UnityAction onFinish = null)
    {
        // stop recording
        if (recording) 
        {
            recording = false;
            SaveToFile(_assetName, currentRecording, withAudio, taskAssignment, onFinish);
        }
        // start recording
        else 
        {
            currentRecording = new RecordingData
            {
                recording = new Recording
                {
                    id = -1,
                    coat = PaintController.Instance.chosenCoat,
                    baseCoat = PaintController.Instance.chosenBaseCoat,
                    workpiece = ApplicationController.Instance.currentSelectedWorkpiece,
                    date = DateTime.Now
                }
            };
            _head = ApplicationController.Instance.player.head.transform;
            _rightHand = ApplicationController.Instance.sprayGun.transform;

            recording = true;
            string dateStamp = DateTime.Now.ToString("dd.MM.yyyy");
            string timeStamp = DateTime.Now.ToString("HH-mm-ss");
            _assetName = TranslationController.Instance.Translate("paint-shop-recording") + " " + dateStamp + " " + timeStamp;
            if (resetDrawablesOnRecord)
                PaintController.Instance.ResetDrawables();
            _timer = 0;
        }
    }

    private void SaveToFile(string recordingName, RecordingData recordingData, bool withAudio,
        TaskAssignment taskAssignment, UnityAction onFinish = null)
    {
        isSavingFile = true;
        StartCoroutine(SaveToFileCoroutine(recordingName, recordingData, withAudio, taskAssignment, onFinish));
    }

    private IEnumerator SaveToFileCoroutine(string recordingName, RecordingData recordingData, bool withAudio,
        TaskAssignment taskAssignment, UnityAction onFinish)
    {
        Debug.Log("Saving File...");

        // all recordings are placed in the Upload folder at first indicating that they are not uploaded yet
        string uploadDirectoryPath = Path.Combine(taskAssignment != null
            ? DataController.Instance.taskResultsDirectoryPath
            : DataController.Instance.recordingsDirectoryPath, "Upload");
        string path = Path.Combine(uploadDirectoryPath, recordingName);

        recordingData.recording.name = recordingName;
        recordingData.recording.data = recordingName;
        if (taskAssignment != null)
            recordingData.recording.taskResult = new TaskResult(-1, DateTime.Now, taskAssignment, recordingData.recording);
        recordingData.recording.neededTime = _timer;
        recordingData.evaluationData = EvaluationController.Instance.GetAsEvaluationData();
        CustomDrawable customDrawable =
            ApplicationController.Instance.currentWorkpieceGameObject.GetComponentInChildren<CustomDrawable>();
        byte[] textureBytes = TextureSaver.ToTexture2D(customDrawable.heightmapOutput).EncodeToPNG();

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        bool done = false;
        int removeCount = Convert.ToInt32(1 / Time.fixedDeltaTime);
        new Thread(() =>
        {
            // remove last second (contains pressing finish button and modifies evaluation values) 
            if (recordingData.frames.Count > removeCount)
                recordingData.frames.RemoveRange(recordingData.frames.Count - removeCount, removeCount);

            File.WriteAllText(Path.Combine(path, "frames.json"),
                JsonConvert.SerializeObject(recordingData.frames, Formatting.None,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            recordingData.recording.GenerateHash();

            File.WriteAllText(Path.Combine(uploadDirectoryPath, $"{recordingName}_recording.json"),
                JsonConvert.SerializeObject(recordingData.recording, Formatting.None,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            File.WriteAllText(Path.Combine(path, "evaluationData.json"),
                JsonConvert.SerializeObject(recordingData.evaluationData, Formatting.None,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            TextureSaver.SaveTextureAsPNG(textureBytes, Path.Combine(path, "heightmap.png"));
            done = true;
        }).Start();

        while (!done)
            yield return null;

        string audioPath = Path.Combine(path, "audio.wav");

        if (recorderPanel.deviceName != TranslationController.Instance.Translate("paint-shop-no-microphone") && withAudio)
            SaveAudioRecording(audioPath);

        screenshotCamera.TakeScreenShot(Path.Combine(path, "preview.png"));

        while (screenshotCamera.IsTakingScreenShot())
            yield return null;

        isSavingFile = false;
        onFinish?.Invoke();
    }

    private void SaveAudioRecording(string path)
    {

        // capture the current clip data
        var position = Microphone.GetPosition(recorderPanel.deviceName);
        var soundData = new float[_recordedClip.samples * _recordedClip.channels];
        _recordedClip.GetData(soundData, 0);

        // create shortened array for the data that was used for recording
        var newData = new float[position * _recordedClip.channels];

        // copy the used samples to a new array
        for (int i = 0; i < newData.Length; i++)
        {
            newData[i] = soundData[i];
        }

        // one does not simply shorten an AudioClip, so we make a new one with the appropriate length
        var newClip = AudioClip.Create(_recordedClip.name, position, _recordedClip.channels, _recordedClip.frequency,
            false);

        // give it the data from the old clip
        newClip.SetData(newData, 0);

        // save file here at path
        if (!SavWav.Save(path, newClip))
            Debug.Log("Failed to save audio file");
    }
}
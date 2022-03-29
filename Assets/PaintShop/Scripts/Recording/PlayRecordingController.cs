using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;

/// <summary>
/// Plays a recorded paint application.
/// </summary>
public class PlayRecordingController : Singleton<PlayRecordingController>
{
    public bool playing;
    public Transform rightHand;
    public Transform followHead;
    public SprayGunRecorder sprayGunRecorder;
    public AudioSource audioSource;
    public Transform decalsParent;

    [HideInInspector] public UnityAction onFinish;
    [HideInInspector] public bool resetOnFinish = true;
    [HideInInspector] public RecordingData recordingData;
    [HideInInspector] public int currentIndex;

    private string _currentAssetName;
    private bool _framesLoaded;
    private int _hitCount;

    private void FixedUpdate()
    {
        if (!playing)
            return;

        SetChildrenActive(true);

        // checks if playing is enabled and there are frames left
        if (currentIndex < recordingData.frames.Count)
        {
            sprayGunRecorder.sprayingValue = recordingData.frames[currentIndex].sprayingValue;
            sprayGunRecorder.triggerValue = recordingData.frames[currentIndex].triggerValue;
            sprayGunRecorder.wideStreamRegulationValue =
                (float)(recordingData.frames[currentIndex].wideStreamRegulationValue == null
                    ? 1
                    : recordingData.frames[currentIndex].wideStreamRegulationValue);
            sprayGunRecorder.startPos = recordingData.frames[currentIndex].pinSpotOrigin.getPosition();
            sprayGunRecorder.direction = recordingData.frames[currentIndex].pinSpotOrigin.getRotation();
            sprayGunRecorder.isAdjustingPaintStand =
                recordingData.frames[currentIndex].mode == SprayGun.SprayGunMode.Hand;

            SetTransform(rightHand.transform, recordingData.frames[currentIndex].rightHand);
            SetTransform(followHead.transform, recordingData.frames[currentIndex].followHead);

            ApplicationController.Instance.paintStand.SetPaintStand(recordingData.frames[currentIndex],
                currentIndex > 0 ? recordingData.frames[currentIndex - 1] : null);

            // corrects angle to account for different model
            Vector3 rot = followHead.rotation.eulerAngles;
            rot = new Vector3(-rot.x, rot.y + 180, rot.z);
            followHead.rotation = Quaternion.Euler(rot);

            if (recordingData.frames[currentIndex].recordedCollisionList != null)
            {
                // destroy all paint stand hits and create them again (ensures consistency even if we jump to a timestamp)
                ClearHits();
                Vector3? lastPosition = null;
                
                foreach (var recordedCollision in recordingData.frames[currentIndex].recordedCollisionList)
                {
                    lastPosition = new Vector3(recordedCollision.hitPosX, recordedCollision.hitPosY,
                        recordedCollision.hitPosZ);
                    PaintStandHitController.Instance.SpawnHit((Vector3)lastPosition,
                        new Quaternion(recordedCollision.rotX, recordedCollision.rotY, recordedCollision.rotZ,
                            recordedCollision.rotW), decalsParent);
                }

                // play sound if there is a new hit this frame
                if (_hitCount != recordingData.frames[currentIndex].recordedCollisionList.Count && lastPosition != null)
                    PaintStandHitController.Instance.PlayAudio((Vector3)lastPosition);

                _hitCount = recordingData.frames[currentIndex].recordedCollisionList.Count;
            }

            currentIndex++;
        }
        else
        {
            // if index is 0 (all the animation has been played)
            StopAnimation();
        }
    }

    /// <summary>
    /// Stops the animation and resets everything.
    /// </summary>
    public void StopAnimation()
    {
        if (!playing)
            return;

        playing = false;
        if (resetOnFinish)
        {
            currentIndex = 0;
            sprayGunRecorder.sprayingValue = 0.0f;
            SetChildrenActive(false);
            ApplicationController.Instance.primarySprayGun = ApplicationController.Instance.sprayGun;
        }

        audioSource.Stop();
        onFinish?.Invoke();
    }


    /// <summary>
    /// Loads the animation if not loaded beforehand, plays the animation and executes the given action afterwards.
    /// </summary>
    public void PlayAnimation(Recording recording, UnityAction executeAfterLoading = null,
        bool loadCoatFromRecording = true, bool loadBaseCoatFromRecording = true)
    {
        if (playing)
        {
            // stop playing, is only called when you press the button again WHILE an animation is playing
            StopAnimation();
            PlayAnimation(recording, executeAfterLoading);
        }
        else
        {
            // load and play new recording file
            if (_currentAssetName != recording.data || !_framesLoaded)
                LoadRecording(recording, () => InitWorkpiece(executeAfterLoading, loadCoatFromRecording,
                    loadBaseCoatFromRecording, true));
            // play already loaded file
            else
                InitWorkpiece(executeAfterLoading, loadCoatFromRecording, loadBaseCoatFromRecording, true);
        }
    }

    /// <summary>
    /// Loads the frames and/or the properties of the given recording.
    /// </summary>
    public void LoadRecording(Recording recording, UnityAction executeAfterLoading = null, bool loadFrames = true,
        bool loadProperties = true, bool initWorkpiece = false)
    {
        gameObject.SetActive(true);
        if (_currentAssetName != recording.data || loadFrames != _framesLoaded)
            StartCoroutine(LoadRecordingCoroutine(recording, executeAfterLoading, loadFrames, loadProperties,
                initWorkpiece));
        else
            executeAfterLoading?.Invoke();
    }

    /// <summary>
    /// Coroutine that starts a new thread for reading and deserializing the recording json which then waits for the
    /// thread to complete and calls the callback method.
    /// </summary>
    private IEnumerator LoadRecordingCoroutine(Recording recording, UnityAction executeAfterLoading, bool loadFrames,
        bool loadProperties, bool initWorkpiece)
    {
        if (_currentAssetName != recording.data)
            _framesLoaded = false;
        _currentAssetName = recording.data;
        string pathEvaluationData = Path.Combine(recording.GetRecordingDirectory(), "evaluationData.json");
        string pathFrames = Path.Combine(recording.GetRecordingDirectory(), "frames.json");
        string pathAudio = Path.Combine(recording.GetRecordingDirectory(), "audio.wav");
        if (File.Exists(pathAudio))
            LoadWav(pathAudio);

        bool done = false;
        recordingData = new RecordingData { recording = recording };
        new Thread(() =>
        {
            if (loadProperties)
                recordingData.evaluationData =
                    JsonConvert.DeserializeObject<EvaluationData>(File.ReadAllText(pathEvaluationData));
            if (loadFrames)
                recordingData.frames = JsonConvert.DeserializeObject<List<Frame>>(File.ReadAllText(pathFrames));
            done = true;
            _framesLoaded = loadFrames;
        }).Start();

        while (!done)
        {
            yield return null;
        }

        if (initWorkpiece)
            InitWorkpiece(executeAfterLoading, true, true, false);
        executeAfterLoading?.Invoke();
    }

    /// <summary>
    /// Creates the workpiece and loads the coats.
    /// </summary>
    private void InitWorkpiece(UnityAction executeAfter, bool loadCoatFromRecording, bool loadBaseCoatFromRecording,
        bool playImmediately)
    {
        _hitCount = 0;
        ApplicationController.Instance.SpawnWorkpiece(recordingData.recording.workpiece);
        ApplicationController.Instance.primarySprayGun = sprayGunRecorder;
        if (loadCoatFromRecording)
            PaintController.Instance.LoadCoat(recordingData.recording.coat, false);
        if (loadBaseCoatFromRecording)
            PaintController.Instance.LoadBaseCoat(recordingData.recording.baseCoat);
        if (playImmediately)
        {
            SetChildrenActive(true);
            playing = true;
            audioSource.Play();
        }
        else
        {
            SetChildrenActive(false);
        }

        executeAfter?.Invoke();
    }

    /// <summary>
    /// Creates a workpiece and adapts its surface in dependence of the current recording.
    /// </summary>
    public void LoadResultFromRecording()
    {
        ApplicationController.Instance.SpawnWorkpiece(recordingData.recording.workpiece);
        PaintController.Instance.LoadCoat(recordingData.recording.coat, false);
        PaintController.Instance.LoadBaseCoat(recordingData.recording.baseCoat);
        EvaluationController.Instance.LoadEvaluationData(recordingData.evaluationData);
        ApplicationController.Instance.currentWorkpieceGameObject.GetComponentInChildren<CustomDrawable>()
            .LoadHeightmaps(LoadTexture());
    }

    /// <summary>
    /// Loads a recording with the given ending from the recordings folder.
    /// </summary>
    private Texture2D LoadTexture()
    {
        byte[] pngBytes =
            File.ReadAllBytes(Path.Combine(recordingData.recording.GetRecordingDirectory(), "heightmap.png"));
        Texture2D tex = new Texture2D(2048, 2048, TextureFormat.RGBAHalf, false);
        tex.LoadImage(pngBytes);
        return tex;
    }

    /// <summary>
    /// Activates or deactivates the animated objects.
    /// </summary>
    private void SetChildrenActive(bool active)
    {
        if (transform.GetChild(0).gameObject.activeSelf == active)
            return;

        foreach (Transform child in transform)
            if (child != decalsParent)
                child.gameObject.SetActive(active);
    }

    /// <summary>
    /// Sets the position and the rotation of an object in dependence of the given ObjectData.
    /// </summary>
    private void SetTransform(Transform t, ObjectData o)
    {
        t.gameObject.SetActive(true);
        t.position = new Vector3(o.posX, o.posY, o.posZ);
        t.rotation = new Quaternion(o.rotX, o.rotY, o.rotZ, o.rotW);
    }

    private void LoadWav(string path)
    {
        // Unload audio clip
        if (audioSource.clip != null)
        {
            audioSource.Stop();
            AudioClip clip = audioSource.clip;
            audioSource.clip = null;
            clip.UnloadAudioData();
            DestroyImmediate(clip, false);
        }

        StartCoroutine(AudioUtil.LoadAudioFile("file:///" + path, clip => audioSource.clip = clip));
    }

    public void ClearHits()
    {
        foreach (Transform decal in decalsParent)
            Destroy(decal.gameObject);
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the recordings in the reflection tool.
/// </summary>
public class ReflectionToolRecordingController : Singleton<ReflectionToolRecordingController>
{
    public GameObject timelineButtonPrefab;
    public Camera screenshotCamera;
    public ImageButton playButton;
    public Image fillBar;
    public GameObject loadingScreen;
    public Sprite playSprite;
    public Sprite pauseSprite;

    public int minTimeLineButtons = 5;
    public int maxTimeLineButtons = 10;
    public int maxTimeLineButtonsFrames = 10000;

    [HideInInspector] public bool playSprayGunAudio = true;
    [HideInInspector] public TimelineButton jumpToTimelineButton;
    [HideInInspector] public CustomDrawable customDrawable;

    private bool _stopNextFrame;
    private bool _playing;
    private int _currentRecordingSnapshotIndex;
    private bool _generatingTextures;
    private bool _loadedRecording;
    private bool _waitForUpdate;
    private List<TimelineButton> _timelineButtons;

    void Start()
    {
        // vr is not needed in the reflection tool
        DestroyImmediate(GameObject.Find("VR"));
        GameObject.Find("Monitor").SetActive(false);
        GameObject.Find("Spray Test Paper").SetActive(false);
        ApplicationController.Instance.ActivatePaintBoothAudio(false);
        ApplicationController.Instance.buttonsActive = false;
        Reset();
    }

    private void Update()
    {
        _waitForUpdate = false;
    }

    private void FixedUpdate()
    {
        // updates recording name in UI and fill amount every frame
        SetFillAmount();

        // generate the textures for the timeline buttons
        if (_generatingTextures)
        {
            // generate Texture 0
            if (_currentRecordingSnapshotIndex == 0)
                GenerateTextures(0);

            // generate a texture if the currentIndex aligns with the exact frame of the snapshot we want to generate
            if (_currentRecordingSnapshotIndex < _timelineButtons.Count &&
                PlayRecordingController.Instance.currentIndex ==
                _timelineButtons[_currentRecordingSnapshotIndex].frameIndex)
            {
                if (!_waitForUpdate)
                {
                    // pause initialization to wait for the next update cycle (needed because the camera needs to render the changes for the screenshot)
                    if (PlayRecordingController.Instance.playing)
                    {
                        Play(false);
                        _waitForUpdate = true;
                    }
                    else
                    {
                        GenerateTextures(_currentRecordingSnapshotIndex);
                        Play(true);
                    }
                }
            }
        }
        // after the recording as loaded the current values need to be updated every frame
        else if (_loadedRecording)
        {
            ReflectionToolUIController.Instance.UpdateCurrentAverageValues(
                EvaluationController.Instance.GetCorrectDistancePercentage(),
                EvaluationController.Instance.GetCorrectAnglePercentage(),
                EvaluationController.Instance.GetFullyPressedPercentage(),
                EvaluationController.Instance.GetAverageSpeed(),
                EvaluationController.Instance.GetCurrentCoatThickness());
            ReflectionToolUIController.Instance.UpdateCurrentValues(
                EvaluationController.Instance.GetCurrentDistance(),
                Mathf.Max(0, 90 - EvaluationController.Instance.GetCurrentAngle()),
                EvaluationController.Instance.GetColorConsumption(), EvaluationController.Instance.GetColorWastage(),
                EvaluationController.Instance.GetColorUsage(),
                ApplicationController.Instance.sprayGun.GetActualSprayingValue() * 100,
                EvaluationController.Instance.GetCurrentSpeed());
            HandleJumpTo();
        }
    }

    public bool RecordingInitialized()
    {
        return !_generatingTextures && _loadedRecording;
    }

    /// <summary>
    /// Generates or loads Textures at the current snapshot index.
    /// </summary>
    private void GenerateTextures(int snapshotIndex)
    {
        TimelineButton timelineButton = _timelineButtons[snapshotIndex];
        RenderTexture texture = RenderTexture.GetTemporary(customDrawable.textureSize, customDrawable.textureSize, 0, RenderTextureFormat.ARGBHalf);
        Graphics.Blit(customDrawable.heightmapOutput, texture);
        timelineButton.heightmap = texture;
        timelineButton.screenshot = TextureSaver.ToTexture2D(screenshotCamera.targetTexture);
        timelineButton.evaluationData = EvaluationController.Instance.GetAsEvaluationData();
        timelineButton.currentMode = PlayRecordingController.Instance.sprayGunRecorder.currentMode;
        timelineButton.sprayGunPathData = SprayGunPathController.Instance.GetCurrentSprayGunPathData();
        // save current PaintStandMovement to show hand after jump
        if (PlayRecordingController.Instance.sprayGunRecorder.IsDoingHandInteraction())
            timelineButton.currentPaintStandMovement =
                PlayRecordingController.Instance.sprayGunRecorder.lastPaintStandMovement;
        timelineButton.UpdatePreview();
        _currentRecordingSnapshotIndex++;
    }

    /// <summary>
    /// Sets the fill amount of the timeline bar according to the current index of the PlayRecordingController.Instance.
    /// </summary>
    private void SetFillAmount()
    {
        if (_loadedRecording)
            fillBar.fillAmount = Mathf.Lerp(0, 1,
                PlayRecordingController.Instance.currentIndex /
                (float)PlayRecordingController.Instance.recordingData.frames.Count);
    }

    /// <summary>
    /// Is called in order to start initializing a recording.
    /// </summary>
    public void InitializeRecording(Recording recording)
    {
        Reset();
        ApplicationController.Instance.SetPauseUpdate(false);
        loadingScreen.SetActive(true);
        _loadedRecording = false;
        PlayRecordingController.Instance.onFinish = FinishPlaying;
        PlayRecordingController.Instance.PlayAnimation(recording, AfterStart);
    }

    /// <summary>
    /// Is also called after initializing.
    /// </summary>
    private void FinishPlaying()
    {
        if (_generatingTextures)
        {
            _generatingTextures = false;
            Time.timeScale = 1;
            playButton.interactable = true;
            loadingScreen.SetActive(false);
            PlayRecordingController.Instance.resetOnFinish = false;
            jumpToTimelineButton = _timelineButtons[0];
            ApplicationController.Instance.ActivateSprayGunAudio(playSprayGunAudio);
            PaintStandHitController.Instance.playAudio = true;
        }

        Play(false);
    }


    /// <summary>
    /// Is called after the recording has been loaded in the PlayRecordingController.Instance.
    /// </summary>
    void AfterStart()
    {
        PlayRecordingController.Instance.resetOnFinish = true;
        ApplicationController.Instance.ActivateSprayGunAudio(false);
        PaintStandHitController.Instance.playAudio = false;
        Time.timeScale = 5;
        _loadedRecording = true;
        ApplicationController.Instance.sprayGun = PlayRecordingController.Instance.sprayGunRecorder;
        // sometimes the evaluation controller gets a reference on the old spray gun on a scene change
        // therefore we need to reset the spray gun here
        EvaluationController.Instance.SetSprayGun();

        DestroyTimelineButtons();

        _currentRecordingSnapshotIndex = 0;
        customDrawable =
            ApplicationController.Instance.currentWorkpieceGameObject.GetComponentInChildren<CustomDrawable>();

        _generatingTextures = true;

        GenerateTimelineButtons();
    }

    /// <summary>
    /// Generates timeline buttons by first finding the number of necessary buttons (5 second - 250 frames steps) and 
    /// then using that number as an index to iteratively assign the proper frame index corresponding to the recording
    /// </summary>
    private void GenerateTimelineButtons()
    {
        _timelineButtons = new List<TimelineButton>();
        int framesCount = PlayRecordingController.Instance.recordingData.frames.Count;
        int buttonCount = (int)Mathf.Lerp(minTimeLineButtons, maxTimeLineButtons,
            (float)framesCount / maxTimeLineButtonsFrames);

        float widthParent = fillBar.GetComponent<RectTransform>().rect.width;

        for (int i = 0; i <= buttonCount; i++)
        {
            TimelineButton timelineButton =
                Instantiate(timelineButtonPrefab, fillBar.transform).GetComponent<TimelineButton>();
            timelineButton.frameIndex = i < buttonCount ? i * (framesCount / buttonCount) : framesCount - 1;
            RectTransform rectTransform = timelineButton.GetComponent<RectTransform>();
            rectTransform.anchoredPosition =
                new Vector2(Mathf.Lerp(0, widthParent, (float)timelineButton.frameIndex / framesCount),
                    rectTransform.anchoredPosition.y);

            _timelineButtons.Add(timelineButton);
        }

        // Resets the current playback
        _currentRecordingSnapshotIndex = 0;
        customDrawable.Reset();
        PlayRecordingController.Instance.currentIndex = 0;
        Play(true);
    }

    /// <summary>
    /// Checks the current play state and if a recording is initialized and loaded to determine what the context
    /// sensitive play button does.
    /// </summary>
    public void OnPlayButtonClick()
    {
        if (_playing)
            Play(false);
        else if (_loadedRecording)
        {
            if (PlayRecordingController.Instance.currentIndex >=
                PlayRecordingController.Instance.recordingData.frames.Count)
            {
                PlayRecordingController.Instance.currentIndex = 0;
                jumpToTimelineButton = _timelineButtons[0];
            }

            Play(true);
        }
    }

    /// <summary>
    /// The stop button functions as a full reset
    /// </summary>
    public void Reset()
    {
        PlayRecordingController.Instance.sprayGunRecorder.currentMode = SprayGun.SprayGunMode.None;
        PlayRecordingController.Instance.currentIndex = 0;
        SetFillAmount();
        PlayRecordingController.Instance.StopAnimation();
        PaintController.Instance.ResetDrawables();
        SprayGunPathController.Instance.Reset();
        _playing = false;
        _loadedRecording = false;
        playButton.interactable = false;
        DestroyTimelineButtons();
    }

    private void DestroyTimelineButtons()
    {
        if (fillBar.transform.childCount > 0)
            foreach (var button in _timelineButtons)
                Destroy(button.gameObject);
    }

    /// <summary>
    /// Controls the play state.
    /// </summary>
    private void Play(bool play)
    {
        ApplicationController.Instance.SetPauseUpdate(!play);
        PlayRecordingController.Instance.playing = play;
        _playing = play;
        if (!_generatingTextures)
            playButton.buttonImage.sprite = !play ? playSprite : pauseSprite;
    }

    /// <summary>
    /// Handles jumping to a frame in the next FixedUpdate. After jumping we need to use an additional FixedUpdate
    /// so that the new position of the spray gun and the paint on the workpiece is visible after jumping.
    /// This class is set last in the Script Execution Order to ensure that the FixedUpdate methods in the other
    /// classes are called first.
    /// </summary>
    private void HandleJumpTo()
    {
        if (_stopNextFrame)
        {
            Play(false);
            _stopNextFrame = false;
        }

        if (jumpToTimelineButton)
        {
            customDrawable.Reset();
            customDrawable.LoadHeightmaps(jumpToTimelineButton.heightmap);
            EvaluationController.Instance.LoadEvaluationData(jumpToTimelineButton.evaluationData, false);
            PlayRecordingController.Instance.currentIndex = jumpToTimelineButton.frameIndex;
            PlayRecordingController.Instance.sprayGunRecorder.StopVisuals();
            PlayRecordingController.Instance.sprayGunRecorder.StopAudio();
            PlayRecordingController.Instance.sprayGunRecorder.lastPaintStandMovement =
                jumpToTimelineButton.currentPaintStandMovement;
            PlayRecordingController.Instance.sprayGunRecorder.currentMode = jumpToTimelineButton.currentMode;
            SprayGunPathController.Instance.LoadSprayGunPathData(jumpToTimelineButton.sprayGunPathData);
            jumpToTimelineButton = null;
            if (!_playing)
            {
                Play(true);
                _stopNextFrame = true;
            }
        }
    }
}
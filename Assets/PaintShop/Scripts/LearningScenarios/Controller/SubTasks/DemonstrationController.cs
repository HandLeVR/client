using Newtonsoft.Json.Linq;

/// <summary>
/// Manages the demonstration sub task. In this task a recording is played to demonstrate painting a workpiece.
/// </summary>
public class DemonstrationController : VRSubTaskController
{
    public TextPanel textPanel;

    private Recording recording;
    private bool playbackFinished;
    private bool recordingLoaded;
    private bool loadBaseCoatFromRecording;
    private bool loadCoatFromRecording;

    private void OnEnable()
    {
        JObject jsonObject = JObject.Parse(subTask.properties);
        string textMonitor = GetStringFromJSON("textMonitor", jsonObject);
        long recordingId = GetLongFromJSON("recordingId", jsonObject);
        recording = TaskPreparationController.Instance.loadedRecordings[recordingId];
        bool distanceRay = GetBoolFromJSON("distanceRay", jsonObject);
        bool distanceMarker = GetBoolFromJSON("distanceMarker", jsonObject);
        bool angleRay = GetBoolFromJSON("angleRay", jsonObject);
        int coatId = (int)jsonObject.GetValue("coatId");
        int baseCoatId = (int)jsonObject.GetValue("baseCoatId");

        loadCoatFromRecording = coatId == -2;
        LoadCoat(coatId);
        loadBaseCoatFromRecording = baseCoatId == -2;
        LoadBaseCoat(baseCoatId);
        ApplicationController.Instance.SpawnWorkpiece(ApplicationController.Instance.currentSelectedWorkpiece);

        ApplicationController.Instance.ActivateDistanceRaySprayingAssistance(distanceRay);
        ApplicationController.Instance.ActivateDistanceMarkerSprayingAssistance(distanceMarker);
        ApplicationController.Instance.ActivateAngleRaySprayingAssistance(angleRay);

        LearningScenariosMonitorController.Instance.ChangePanel(textPanel.gameObject);
        textPanel.InitPanel(textMonitor);
        SetEducationMasterAndCoins(true, dontShowGoldenCoin: true);

        PlayRecordingController.Instance.LoadRecording(TaskPreparationController.Instance.loadedRecordings[recordingId],
            () =>
            {
                if (containsAudio)
                    SpawnCoins(canSkipSpeech, true,  true);
                else
                    PlayRecording();
            });
        playbackFinished = false;
    }

    public override void ContinueCoinFadedOut()
    {
        if (!playbackFinished)
            PlayRecording();
        else
            base.ContinueCoinFadedOut();
    }

    private void PlayRecording()
    {
        PlayRecordingController.Instance.onFinish = OnFinishDemonstration;
        PlayRecordingController.Instance.PlayAnimation(recording, null, loadCoatFromRecording,
            loadBaseCoatFromRecording);
        SpawnCoins(canSkipAll, true);
    }

    public override void ContinueCoinSelected()
    {
        if (PlayRecordingController.Instance.playing)
            PlayRecordingController.Instance.StopAnimation();
        base.ContinueCoinSelected();
    }

    public override void ReturnCoinSelected()
    {
        if (PlayRecordingController.Instance.playing)
            PlayRecordingController.Instance.StopAnimation();
        base.ReturnCoinSelected();
    }

    protected override void AfterEducationMasterSpeech()
    {
        base.AfterEducationMasterSpeech();
        SpawnCoins(true, true, true);
    }

    private void OnFinishDemonstration()
    {
        playbackFinished = true;
        ApplicationController.Instance.ActivateDistanceRaySprayingAssistance(false);
        ApplicationController.Instance.ActivateDistanceMarkerSprayingAssistance(false);
        ApplicationController.Instance.ActivateAngleRaySprayingAssistance(false);
        SpawnCoins(true, true);
    }
}
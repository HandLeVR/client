using UnityEngine;
using Newtonsoft.Json.Linq;

/// <summary>
/// Manages the paint workpiece sub task. In this task the user can paint a workpiece.
/// </summary>
public class PaintWorkpieceController : VRSubTaskController
{
    public PaintWorkpiecePanel paintWorkpiecePanel;
    public MeshRenderer workpieceArea;

    private int minSprayTime;
    private int helpDuration;
    private bool minTimeReached;

    private void Update()
    {
        if (!minTimeReached && EvaluationController.Instance.GetSecondsSprayed() >= minSprayTime)
        {
            minTimeReached = true;
            SpawnCoins(canSkipSpeech, false);
        }

        // deactivate all spraying assistance after the set time
        if (helpDuration > 0 && EvaluationController.Instance.GetSecondsSprayed() >= helpDuration)
        {
            ApplicationController.Instance.DeactivateAllSprayingAssistance();
            helpDuration = 0;
            paintWorkpiecePanel.distanceRay.interactable = false;
            paintWorkpiecePanel.distanceMarker.interactable = false;
            paintWorkpiecePanel.angleRay.interactable = false;
        }

        ApplicationController.Instance.sprayGun.isDisabled =
            !workpieceArea.bounds.Contains(ApplicationController.Instance.sprayGun.transform.position);
    }

    private void OnEnable()
    {
        JObject jsonObject = JObject.Parse(subTask.properties);
        long workpieceId = (long)jsonObject.GetValue("workpieceId");
        long coatId = (long)jsonObject.GetValue("coatId");
        long baseCoatId = (long)jsonObject.GetValue("baseCoatId");
        bool optionDistanceRay = GetBoolFromJSON("optionDistanceRay", jsonObject);
        bool optionDistanceMarker = GetBoolFromJSON("optionDistanceMarker", jsonObject);
        bool optionAngleRay = GetBoolFromJSON("optionAngleRay", jsonObject);
        bool distanceRay = GetBoolFromJSON("distanceRay", jsonObject);
        bool distanceMarker = GetBoolFromJSON("distanceMarker", jsonObject);
        bool angleRay = GetBoolFromJSON("angleRay", jsonObject);
        minSprayTime = (int)jsonObject.GetValue("minSprayTime");
        if (jsonObject["helpDuration"] != null && (string)jsonObject["helpDuration"] != "")
            helpDuration = (int)jsonObject.GetValue("helpDuration");
        string textMonitor = GetStringFromJSON("textMonitor", jsonObject);

        ApplicationController.Instance.ActivateDistanceRaySprayingAssistance(distanceRay);
        ApplicationController.Instance.ActivateDistanceMarkerSprayingAssistance(distanceMarker);
        ApplicationController.Instance.ActivateAngleRaySprayingAssistance(angleRay);

        paintWorkpiecePanel.supportOptions.SetActive(optionDistanceRay || optionDistanceMarker || optionAngleRay);
        paintWorkpiecePanel.distanceRay.gameObject.SetActive(optionDistanceRay);
        paintWorkpiecePanel.distanceRay.interactable = true;
        paintWorkpiecePanel.distanceRay.isOn = distanceRay;
        paintWorkpiecePanel.distanceMarker.gameObject.SetActive(optionDistanceMarker);
        paintWorkpiecePanel.distanceMarker.interactable = true;
        paintWorkpiecePanel.distanceMarker.isOn = distanceMarker;
        paintWorkpiecePanel.angleRay.gameObject.SetActive(optionAngleRay);
        paintWorkpiecePanel.angleRay.interactable = true;
        paintWorkpiecePanel.angleRay.isOn = angleRay;

        ApplicationController.Instance.SpawnWorkpiece(DataController.Instance.workpieces[workpieceId]);
        LoadCoat(coatId);
        LoadBaseCoat(baseCoatId);

        paintWorkpiecePanel.InitPanel(textMonitor);
        LearningScenariosMonitorController.Instance.ChangePanel(paintWorkpiecePanel.gameObject);

        minTimeReached = minSprayTime <= 0;
        SetEducationMasterAndCoins(!minTimeReached, dontShowReturnCoin: true,
            position: VirtualInstructorController.InstructorPosition.Paint);

        // don't create a recording in the tutorial
        if (LearningScenariosTaskController.Instance.currentTaskAssignment != null)
            CreateRecordingController.Instance.HandleRecordingCreation(false);
    }

    /// <summary>
    /// Saves the recording before FinishedSubTask is called in parent class. This is important if this sub task is
    /// the last sub task to ensure that we know there is a recording before we return to the start panel. 
    /// </summary>
    protected override void FinishSubTask()
    {
        // don't create a recording in the tutorial or for own tasks
        if (LearningScenariosTaskController.Instance.currentTaskAssignment != null &&
            LearningScenariosTaskController.Instance.currentTaskAssignment.id != -1)
            CreateRecordingController.Instance.HandleRecordingCreation(false,
                LearningScenariosTaskController.Instance.currentTaskAssignment);
        base.FinishSubTask();
    }

    protected override void AfterEducationMasterSpeech()
    {
        base.AfterEducationMasterSpeech();
        SpawnCoins(minTimeReached, false);
    }
}
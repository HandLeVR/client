using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

/// <summary>
/// Manages the spray test sub task.
/// </summary>
public class SprayTestController : VRSubTaskController
{
    public TextPanel textPanel;
    public SprayTest sprayTest;
    public MeshRenderer sprayTestArea;

    private bool _sprayTestDone;
    private List<int> _possibleErrors;

    private void OnEnable()
    {
        JObject jsonObject = JObject.Parse(subTask.properties);
        int errorRate = GetIntFromJSON("errorRate", jsonObject);
        bool splittedSpray = GetBoolFromJSON("splittedSpray", jsonObject);
        bool excessiveMaterial = GetBoolFromJSON("excessiveMaterial", jsonObject);
        bool oneSidedCurved = GetBoolFromJSON("oneSidedCurved", jsonObject);
        bool oneSidedDisplaced = GetBoolFromJSON("oneSidedDisplaced", jsonObject);
        bool sShaped = GetBoolFromJSON("sShaped", jsonObject);
        bool flutteringSpray = GetBoolFromJSON("flutteringSpray", jsonObject);
        bool distanceRay = GetBoolFromJSON("distanceRay", jsonObject);
        bool distanceMarker = GetBoolFromJSON("distanceMarker", jsonObject);
        bool angleRay = GetBoolFromJSON("angleRay", jsonObject);
        int coatId = (int) jsonObject.GetValue("coatId");
        string textMonitor = GetStringFromJSON("textMonitor", jsonObject);
        bool skippable = GetBoolFromJSON("skippable", jsonObject);

        _possibleErrors = new List<int>();
        if (splittedSpray) _possibleErrors.Add(0);
        if (excessiveMaterial) _possibleErrors.Add(1);
        if (oneSidedCurved) _possibleErrors.Add(2);
        if (oneSidedDisplaced) _possibleErrors.Add(3);
        if (sShaped) _possibleErrors.Add(4);
        if (flutteringSpray) _possibleErrors.Add(5);

        sprayTest.errorRate = errorRate / 100f;

        ApplicationController.Instance.ActivateDistanceRaySprayingAssistance(distanceRay);
        ApplicationController.Instance.ActivateDistanceMarkerSprayingAssistance(distanceMarker);
        ApplicationController.Instance.ActivateAngleRaySprayingAssistance(angleRay);
        LoadCoat(coatId);

        SetEducationMasterAndCoins(true);
        SpawnCoins(canSkipAll, true);

        // show virtual instructor anyway to avoid sudden appearance after spraying
        if (!containsAudio)
        {
            VirtualInstructorController.Instance.Init(null);
            VirtualInstructorController.Instance.active = false;
        }
        
        // init spray test immediately not audio is set or it does not start immediately
        if (!containsAudio || !automaticAudio)
            sprayTest.InitSprayTest(_possibleErrors);

        textPanel.InitPanel(textMonitor);
        LearningScenariosMonitorController.Instance.ChangePanel(textPanel.gameObject);

        _sprayTestDone = false;
    }

    private void Update()
    {
        if (!_sprayTestDone && sprayTest.SprayTestDone())
        {
            _sprayTestDone = true;
            VirtualInstructorController.Instance.active = false;
            StartCoroutine(WaitFor.Seconds(1, PlaySprayAudioOnError));
        }

        ApplicationController.Instance.sprayGun.isDisabled =
            !sprayTestArea.bounds.Contains(ApplicationController.Instance.sprayGun.transform.position);
    }

    private void PlaySprayAudioOnError()
    {
       AudioClip audioClip = sprayTest.GetErrorAudioClip();
       if (audioClip == null)
           PlayFinalAudioIfSet();
       else
       {
           VirtualInstructorController.Instance.Init(audioClip, afterSpeech: PlayFinalAudioIfSet);
           VirtualInstructorController.Instance.active = true;
           VirtualInstructorController.Instance.Speak();
       }
    }

    protected override void Reset()
    {
        sprayTest.ResetSprayTest();
        EvaluationController.Instance.Reset();
        base.Reset();
    }

    protected override void AfterEducationMasterSpeech()
    {
        base.AfterEducationMasterSpeech();
        SpawnCoins(_sprayTestDone, true);
        if (_sprayTestDone)
            sprayTest.ResetSprayTest();
        else 
            sprayTest.InitSprayTest(_possibleErrors);
    }
}

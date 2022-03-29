using UnityEngine;
using UnityEngine.Serialization;
using Newtonsoft.Json.Linq;

/// <summary>
/// Manages the evaluation sub task. In this task evaluation parameter are visible on the monitor and the heatmap is
/// possibly activated.
/// </summary>
public class PaintEvaluationController : VRSubTaskController
{
    public LearningScenarioEvaluationPanel learningScenarioEvaluationPanel;
    
    // determines whether this sub task is called as a single task from the main menu
    // (user only wants to see the evaluation of the last run of the corresponding task)
    [HideInInspector] public bool isSingle;

    private void OnEnable()
    {
        JObject jsonObject = JObject.Parse(subTask.properties);
        bool heatMap = GetBoolFromJSON("heatMap", jsonObject);
        bool correctDistance = GetBoolFromJSON("correctDistance", jsonObject);
        bool correctAngle = GetBoolFromJSON("correctAngle", jsonObject);
        bool colorConsumption = GetBoolFromJSON("colorConsumption", jsonObject);
        bool colorWastage = GetBoolFromJSON("colorWastage", jsonObject);
        bool colorUsage = GetBoolFromJSON("colorUsage", jsonObject);
        bool fullyPressed = GetBoolFromJSON("fullyPressed", jsonObject);
        bool averageSpeed = GetBoolFromJSON("averageSpeed", jsonObject);
        bool coatThickness = GetBoolFromJSON("coatThickness", jsonObject);

        StartCoroutine(WaitFor.NextFixedUpdate(() => ApplicationController.Instance.SetEvaluationModeActive(heatMap)));

        learningScenarioEvaluationPanel.InitPanel(correctDistance, correctAngle, colorConsumption, colorWastage,
            colorUsage, fullyPressed, averageSpeed, coatThickness);
        learningScenarioEvaluationPanel.toggles.SetActive(heatMap);
        LearningScenariosMonitorController.Instance.ChangePanel(learningScenarioEvaluationPanel.gameObject);

        SetEducationMasterAndCoins(isSingle, position: VirtualInstructorController.InstructorPosition.Paint,
            showEducationMaster: !isSingle);
    }

    protected override void Reset()
    {
        ApplicationController.Instance.SetEvaluationModeActive(false);
        isSingle = false;
        base.Reset();
    }

    public override void ReturnCoinFadedOut()
    {
        if (isSingle)
        {
            Reset();
            LearningScenariosTaskController.Instance.ShowTaskPanel(true);
        }
        else
            base.ReturnCoinFadedOut();
    }

    protected override void AfterEducationMasterSpeech()
    {
        base.AfterEducationMasterSpeech();
        SpawnCoins(!isSingle,true);
    }
}

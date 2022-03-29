using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.Serialization;

/// <summary>
/// Manages the self assessment sub task. In this task the user can put golden spray guns in a basket to assess his
/// performance.
/// </summary>
public class SelfAssessmentController : VRSubTaskController
{
    public TextPanel textPanel;
    public SelfAssessmentTable selfAssessmentTablePrefab;

    private SelfAssessmentTable _selfAssessmentTable;

    private static readonly string SelfAssessmentFile =
        Path.Combine(Application.streamingAssetsPath, "selfAssessment.csv");

    private void OnEnable()
    {
        JObject jsonObject = JObject.Parse(subTask.properties);
        string textMonitor = GetStringFromJSON("textMonitor", jsonObject);

        textPanel.InitPanel(textMonitor);
        LearningScenariosMonitorController.Instance.ChangePanel(textPanel.gameObject);

        _selfAssessmentTable = Instantiate(selfAssessmentTablePrefab);

        SetEducationMasterAndCoins();
    }

    public override void ContinueCoinSelected()
    {
        SaveResults(LearningScenariosTaskController.Instance.currentTask.name,
            _selfAssessmentTable.GetNumberOfSprayGuns());
        base.ContinueCoinSelected();
        _selfAssessmentTable.FadeOut();
    }

    public override void ReturnCoinSelected()
    {
        base.ReturnCoinSelected();
        _selfAssessmentTable.FadeOut();
    }

    protected override void AfterEducationMasterSpeech()
    {
        SpawnCoins(true, true);
    }

    /// <summary>
    /// Saves the results in a file.
    /// </summary>
    private void SaveResults(string taskName, int numberOfSprayGuns)
    {
        if (!File.Exists(SelfAssessmentFile))
            File.WriteAllText(SelfAssessmentFile,
                "UserID;Name;Aufgabe;Zeitpunkt;Einschätzung");
        File.AppendAllText(SelfAssessmentFile,
            "\n" + String.Join(";", DataController.Instance.CurrentUser.id,
                DataController.Instance.CurrentUser.fullName, taskName,
                DateTime.Now.ToString("HH:mm:ss dd-MM-yyyy"), numberOfSprayGuns));
    }
}
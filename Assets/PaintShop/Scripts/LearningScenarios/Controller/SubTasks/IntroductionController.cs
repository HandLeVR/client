using Newtonsoft.Json.Linq;

/// <summary>
/// Manages the introduction sub task. In this task a text is displayed on the monitor and the virtual instructor speaks.
/// </summary>
public class IntroductionController : VRSubTaskController
{
    public TextPanel textPanel;

    private void OnEnable()
    {
        JObject jsonObject = JObject.Parse(subTask.properties);
        string textMonitor = GetStringFromJSON("textMonitor", jsonObject);

        textPanel.InitPanel(textMonitor);
        LearningScenariosMonitorController.Instance.ChangePanel(textPanel.gameObject);

        SetEducationMasterAndCoins();
    }

    protected override void AfterEducationMasterSpeech()
    { 
        SpawnCoins(true, true);
    }
}

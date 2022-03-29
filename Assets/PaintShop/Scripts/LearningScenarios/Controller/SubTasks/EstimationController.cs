using Newtonsoft.Json.Linq;

/// <summary>
/// Controller for the estimation task. In this task the user needs to give an estimation with the help of en
/// estimation object (thermometer, beaker or clock).
/// </summary>
public class EstimationController : VRSubTaskController
{
    public TextPanel textPanel;
    public Beaker beaker;
    public Clock clock;
    public Thermometer thermometer;

    private int _subTaskPhase;
    private bool _oldCanSkipSpeech;
    private EstimationObject _currentEstimationObject;

    private void OnEnable()
    {
        _subTaskPhase = 0;
        JObject jsonObject = JObject.Parse(subTask.properties);
        string textMonitor = GetStringFromJSON("textMonitor", jsonObject);
        int minimum = GetIntFromJSON("minimum", jsonObject);
        int maximum = GetIntFromJSON("maximum", jsonObject);
        string interactiveObject = GetStringFromJSON("interactiveObject", jsonObject);

        if (interactiveObject == "beaker")
        {
            _currentEstimationObject = beaker;
            beaker.minPercentage = (float) minimum / 10;
            beaker.maxPercentage = (float) maximum / 10;
        }
        else if (interactiveObject == "clock")
        {
            _currentEstimationObject = clock;
            clock.minimumValue = minimum;
            clock.maximumValue = maximum;
        }
        else
        {
            _currentEstimationObject = thermometer;
            thermometer.correctPercentageMin = minimum;
            thermometer.correctPercentageMax = maximum;
        }

        _currentEstimationObject.gameObject.SetActive(true);
        _currentEstimationObject.FadeIn();
        _currentEstimationObject.Reset();

        LearningScenariosMonitorController.Instance.ChangePanel(textPanel.gameObject);
        textPanel.InitPanel(textMonitor);

        SetEducationMasterAndCoins(dontShowGoldenCoin: true);
        _oldCanSkipSpeech = canSkipSpeech;
    }

    public override void ContinueCoinSelected()
    {
        if (_subTaskPhase != 1)
            return;

        _currentEstimationObject.FadeOut();
    }

    public override void ReturnCoinSelected()
    {
        _currentEstimationObject.FadeOut();
    }

    public override void ContinueCoinFadedOut()
    {
        if (_subTaskPhase == 0)
        {
            _currentEstimationObject.ShowSolution();
            SpawnCoins(_oldCanSkipSpeech || finalAudioClip == null, true);
            _subTaskPhase = 1;
            PlayFinalAudioIfSet();
        }
        else
        {
            base.FinishSubTask();
        }
    }

    protected override void AfterEducationMasterSpeech()
    {
        base.AfterEducationMasterSpeech();
        SpawnCoins(true, true, _subTaskPhase == 0);
    }
}

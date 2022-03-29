using System.Collections.Generic;
using Newtonsoft.Json.Linq;

/// <summary>
/// Controller for the coat selection task. In this task the user can select one of the coats which is then used
/// later in painting tasks.
/// </summary>
public class CoatSelectionController : VRSubTaskController
{
    public SelectionPanel selectionPanel;

    private Coat _selectedCoat;

    private void OnEnable()
    {
        JObject jsonObject = JObject.Parse(subTask.properties);
        string textMonitor = GetStringFromJSON("textMonitor", jsonObject);
        List<JObject> objects = GetItemsFromJSON("items", jsonObject);
        List<Coat> coats = new List<Coat>();
        objects.ForEach(o => coats.Add(DataController.Instance.coats[(long) o["coatId"]]));

        LearningScenariosMonitorController.Instance.ChangePanel("");
        selectionPanel.FadeIn(textMonitor, 1);
        coats.ForEach(coat => selectionPanel.AddItem(coat.name, onSelection: () => OnCoatSelection(coat)));

        _selectedCoat = null;
        SetEducationMasterAndCoins(true);
    }

    /// <summary>
    /// Is called on coat selection and sets the coat if the speech of the virtual instructor can be skipped
    /// or the speech is finished.
    /// </summary>
    private void OnCoatSelection(Coat coat)
    {
        _selectedCoat = coat;
        if (canSkipSpeech)
            MoveOn();
    }

    public override void ReturnCoinSelected()
    {
        selectionPanel.FadeOut();
    }

    /// <summary>
    /// Sets the coat after the speech of the virtual instructor if the coat is already selected.
    /// </summary>
    protected override void AfterEducationMasterSpeech()
    {
        base.AfterEducationMasterSpeech();
        if (_selectedCoat != null)
            MoveOn();
    }

    /// <summary>
    /// Sets the coat and finishes the sub task.
    /// </summary>
    private void MoveOn()
    {
        PaintController.Instance.LoadCoat(_selectedCoat, false);
        LearningScenariosTaskController.Instance.selectedCoat = _selectedCoat;
        selectionPanel.FadeOut();
        CoinController.Instance.FadeOutCoins(afterFadeOut: FinishSubTask);
    }
}

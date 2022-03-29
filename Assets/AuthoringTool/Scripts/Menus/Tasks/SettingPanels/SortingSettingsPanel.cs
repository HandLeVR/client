/// <summary>
/// Represents the settings panel for the sorting task.
/// </summary>
public class SortingSettingsPanel : BaseElementListSettingsPanel
{
    protected new void Awake()
    {
        base.Awake();
        InitSkippableDropdown(skippableDropdown);
        labelTemplate = "Gegenstand {0}:";
        maxNumObjects = 6;
        warningMessage.text = "Maximale Anzahl von " + maxNumObjects + " Gegenständen erreicht.";
    }

    protected override void AdditionalInitElement(BaseListElement newContainer)
    {
        SortableObjectListElement sortableObjectListElement = (SortableObjectListElement) newContainer;
        sortableObjectListElement.labelingInputField.onEndEdit.AddListener(delegate { SaveSettings(); });
        sortableObjectListElement.insideToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
    }
}

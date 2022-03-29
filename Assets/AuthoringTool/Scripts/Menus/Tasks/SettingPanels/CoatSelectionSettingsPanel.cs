using Newtonsoft.Json.Linq;

/// <summary>
/// Represents the settings panel for the coat selection task.
/// </summary>
public class CoatSelectionSettingsPanel : BaseElementListSettingsPanel
{
    protected new void Awake()
    {
        base.Awake();
        InitSkippableDropdown(skippableDropdown, addAllSkippableEntry:false);
        labelTemplate = "Lack {0}:";
        maxNumObjects = 6;
        warningMessage.text = "Maximale Anzahl von " + maxNumObjects + " Lacken erreicht.";
    }

    protected override void InitElement(bool isMax, bool isLast, bool isSingle, JObject element = null )
    {
        CoatListElement newContainer = Instantiate(ElementPrefab, ElementListContainer.transform).GetComponent<CoatListElement>();
        InitCoatInput(newContainer.coatInputField);
        newContainer.btnAdd.onClick.AddListener(AddSelectionElement);
        newContainer.btnRemove.onClick.AddListener(() => RemoveSelectionElement(newContainer.gameObject));
        newContainer.btnRemove.onClick.AddListener(SaveSettings);
        newContainer.btnAdd.gameObject.SetActive(!isMax && isLast);
        newContainer.btnRemove.gameObject.SetActive(!isSingle);
        newContainer.label.text = string.Format(labelTemplate, newContainer.transform.GetSiblingIndex() + 1);
        
        if (element == null)
            newContainer.SetUpForSettings();
        else
            newContainer.SetUpForSettings(element);
        
        AdditionalInitElement(newContainer);
    }
}

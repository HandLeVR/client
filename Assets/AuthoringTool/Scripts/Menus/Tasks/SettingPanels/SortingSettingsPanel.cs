using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using translator;

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

    protected override void InitElement(bool isMax, bool isLast, bool isSingle, JObject element = null)
    {
        BaseListElement newContainer =
            Instantiate(ElementPrefab, ElementListContainer.transform).GetComponent<BaseListElement>();
        newContainer.btnAdd.onClick.AddListener(AddSelectionElement);
        newContainer.btnRemove.onClick.AddListener(() => RemoveSelectionElement(newContainer.gameObject));
        newContainer.btnRemove.onClick.AddListener(SaveSettings);
        newContainer.btnAdd.gameObject.SetActive(!isMax && isLast);
        newContainer.btnRemove.gameObject.SetActive(!isSingle);
        newContainer.label.text = string.Format(labelTemplate, newContainer.transform.GetSiblingIndex() + 1);
        
        newContainer.dropdown.options = new List<TMP_Dropdown.OptionData>();
        DataController.Instance.sortableObjects.ForEach(s =>
            newContainer.dropdown.options.Add(
                new TMP_Dropdown.OptionData(TranslationController.Instance.Translate(s))));
        newContainer.dropdown.onValueChanged.AddListener(delegate { SaveSettings(); });
        newContainer.dropdown.RefreshShownValue();

        if (element == null)
            newContainer.SetUpForSettings();
        else
            newContainer.SetUpForSettings(element);
        
        AdditionalInitElement(newContainer);
    }

    protected override void AdditionalInitElement(BaseListElement newContainer)
    {
        SortableObjectListElement sortableObjectListElement = (SortableObjectListElement) newContainer;
        sortableObjectListElement.labelingInputField.onEndEdit.AddListener(delegate { SaveSettings(); });
        sortableObjectListElement.insideToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
    }
}

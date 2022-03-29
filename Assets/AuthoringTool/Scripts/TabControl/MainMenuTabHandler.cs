using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extends the normal table handler for the main menu. A popup message is now shown if there are unsaved changes.
/// </summary>
public class MainMenuTabHandler : TabHandler
{
    private readonly List<BaseMenuController> managementPanels = new List<BaseMenuController>();
    private int currentPanelIndex = -1;

    new void Awake()
    {
        base.Awake();
        foreach (GameObject panel in panels)
        {
            BaseMenuController managementPanel = panel.GetComponent<BaseMenuController>();
            if (managementPanel == null)
            {
                Debug.LogError(panel.name + " does not contain a " + nameof(BaseMenuController));
                return;
            }

            managementPanels.Add(managementPanel);
        }
    }

    public override void SetPanelActive(int index)
    {
        if (UnsavedChangesInCurrentPanel())
        {
            PopupScreenHandler.Instance.ShowUnsavedChanges(() => SetPanelActiveConfirmed(index));
            return;
        }

        SetPanelActiveConfirmed(index);
    }

    private void SetPanelActiveConfirmed(int index)
    {
        if (currentPanelIndex > 0 && currentPanelIndex < managementPanels.Count)
            managementPanels[currentPanelIndex].SetUnsavedChanges(false);
        currentPanelIndex = index;
        base.SetPanelActive(index);
    }

    public bool UnsavedChangesInCurrentPanel()
    {
        return currentPanelIndex >= 0 && currentPanelIndex < managementPanels.Count &&
               managementPanels[currentPanelIndex].HasUnsavedChanges();
    }
}
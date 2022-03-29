using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the color of tabs in dependence of their selection state and activates the corresponding panels on selection.
/// </summary>
public class TabHandler : MonoBehaviour
{
    public List<Button> tabs;
    public List<GameObject> panels;
    public bool changePanels = true;

    protected void Awake()
    {
        if (panels.Count != tabs.Count && changePanels)
        {
            Debug.LogError("Number of tabs does not equal number of panels.");
            return;
        }

        for (int i = 0; i < tabs.Count; i++)
        {
            var index = i;
            tabs[i].onClick.AddListener(() => SetPanelActive(index));
        }
        
        SetPanelActive(0);
    }

    private void OnEnable()
    {
        SetPanelActive(0);
    }

    public virtual void SetPanelActive(int index)
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            tabs[i].transform.GetComponent<Image>().color = index == i ? tabs[i].colors.normalColor : Color.white;
            if (changePanels)
                panels[i].SetActive(index == i);
        }
    }
}

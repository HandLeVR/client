using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the UI of the monitor in the learning scenario scene.
/// </summary>
public class LearningScenariosMonitorController : Singleton<LearningScenariosMonitorController>
{
    public GameObject defaultScreen;

    private List<GameObject> panels;

    private void Start()
    {
        panels = new List<GameObject>();
        foreach (Transform child in defaultScreen.transform)
            panels.Add(child.gameObject);
    }

    public void ChangePanel(GameObject panel)
    {
        ChangePanel(panel.name);
    }

    public void ChangePanel(string panel)
    {
        panels.ForEach(o => o.SetActive(false));
        foreach (GameObject p in panels)
            p.SetActive(p.name.Equals(panel));
    }
}

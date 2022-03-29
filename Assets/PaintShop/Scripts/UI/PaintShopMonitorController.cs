using System.Collections.Generic;
using System.Linq;
using TMPro;
using translator;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Manages the UI of the monitor in the main scene.
/// </summary>
public class PaintShopMonitorController : MonoBehaviour
{
    public List<GameObject> panels;
    public List<Image> tabs;
    public GameObject defaultScreen;
    public GameObject sprayGunInitializationScreen;
    public GameObject positionInitializationScreen;
    public TMP_Dropdown coatDropdown;
    public TMP_Dropdown baseCoatDropdown;
    public Text connectedSprayGunLabel;

    public Color activeTabColor;
    public Color inactiveTabColor;

    void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
            StartSprayGunInitialization();

        if (Keyboard.current.oKey.wasPressedThisFrame)
            StartPositionInitialization();
    }

    private void Start()
    {
        baseCoatDropdown.options = new List<TMP_Dropdown.OptionData>
            { new(TranslationController.Instance.Translate("paint-shop-no-coat")) };
        foreach (Coat coat in DataController.Instance.coats.Values.Where(coat => coat.type != CoatType.Topcoat))
            baseCoatDropdown.options.Add(new TMP_Dropdown.OptionData(coat.name));
        baseCoatDropdown.RefreshShownValue();
        
        coatDropdown.options = new List<TMP_Dropdown.OptionData>();
        foreach (Coat coat in DataController.Instance.coats.Values)
            coatDropdown.options.Add(new TMP_Dropdown.OptionData(coat.name));
        coatDropdown.RefreshShownValue();

        connectedSprayGunLabel.text = TranslationController.Instance.Translate("paint-shop-used-spray-gun",
            ApplicationController.Instance.RealSprayGunConnected()
                ? "paint-shop-printed-spray-gun"
                : "paint-shop-vr-controller");
    }

    public void SetActiveTab(int index)
    {
        int newTab = index >= tabs.Count || index < 0 ? 0 : index;

        tabs.ForEach(t => t.color = inactiveTabColor);
        panels.ForEach(p => p.SetActive(false));
        tabs[newTab].color = activeTabColor;
        panels[newTab].SetActive(true);
    }

    public void StartSprayGunInitialization(){
        defaultScreen.SetActive(false);
        sprayGunInitializationScreen.SetActive(true);
    }
    
    public void StartPositionInitialization(){
        defaultScreen.SetActive(false);
        positionInitializationScreen.SetActive(true);
    }

    public void ShowDefaultScreen(){
        defaultScreen.SetActive(true);
        sprayGunInitializationScreen.SetActive(false);
        positionInitializationScreen.SetActive(false);
    }
}

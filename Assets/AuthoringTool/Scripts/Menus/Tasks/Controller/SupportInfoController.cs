using System.Collections.Generic;
using translator;
using UnityEngine;

/// <summary>
/// Provides methods for the creation of support infos after drag & drop.
/// </summary>
public class SupportInfoController : Singleton<SupportInfoController>
{
    public GameObject basicPrefab;
    public List<BaseSupportInfoContainer> prefabs;
    public Transform supportInfoContainer;
    
    [HideInInspector] public BaseSupportInfoContainer currentSelection;

    private readonly Dictionary<string, BaseSupportInfoContainer> _prefabsMap = new ();

    void Awake()
    {
        // map the prefabs to there type to simplify later usage
        foreach (BaseSupportInfoContainer prefab in prefabs)
            _prefabsMap[prefab.type] = prefab;

        // initialize all available support infos which can be used in a support info task
        foreach (BaseSupportInfoContainer prefab in prefabs)
        {
            string name = TranslationController.Instance.TranslateSupportInfoType(prefab.type);
            string description = TranslationController.Instance.TranslateSupportInfoType(prefab.type, true);
            GameObject supportInfo = Instantiate(basicPrefab, supportInfoContainer);
            supportInfo.GetComponent<TooltipObject>().tooltipText = description;
            supportInfo.gameObject.GetComponent<BaseSupportInfoContainer>().supportInfoData =
                new SupportInfo(name, description, prefab.type, "");
            supportInfo.gameObject.GetComponent<BaseSupportInfoContainer>().supportInfoName.text = name;
        }
    }

    /// <summary>
    /// When dragging, the original prefab is moved, so we need to create a clone at the originals positions to take the originals place.
    /// </summary>
    public void CloneSupportInfo(int siblingIndex)
    {
        BaseSupportInfoContainer newOriginal = Instantiate(currentSelection, supportInfoContainer);
        newOriginal.supportInfoData = currentSelection.supportInfoData.Copy();
        newOriginal.transform.SetSiblingIndex(siblingIndex);
    }

    /// <summary>
    /// Creates a support info element in the settings panel (right side).
    /// </summary>
    public GameObject CreateSupportInfoSettings(SupportInfo supportInfo, Transform parent, bool saveSettings)
    {
        BaseSupportInfoContainer newSupportInfo = Instantiate(_prefabsMap[supportInfo.type], parent);
        newSupportInfo.SetupForSettings(supportInfo, saveSettings);
        return newSupportInfo.gameObject;
    }

    /// <summary>
    /// Creates a support info element in the list of sub tasks (middle segment) which can only displayed there
    /// but not modified.
    /// </summary>
    public BaseSupportInfoContainer CreateSupportInfoDisplaying(SupportInfo supportInfo, Transform parent)
    {
        BaseSupportInfoContainer newSupportInfo = Instantiate(_prefabsMap[supportInfo.type], parent);
        newSupportInfo.SetupForDisplaying(supportInfo);
        newSupportInfo.GetComponent<BaseDragHandler>().enabled = false;
        return newSupportInfo;
    }
}
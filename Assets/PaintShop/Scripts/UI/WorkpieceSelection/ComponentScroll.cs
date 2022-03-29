using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Container for the ScrollItems (with the workpiece preview pictures).
/// </summary>
public class ComponentScroll : MonoBehaviour
{
    public ScrollItem scrollItemPrefab;
    public Workpiece currentWorkpiece;

    void Start()
    {
        List<Workpiece> workpieces = DataController.Instance.workpieces.Values.ToList();
        for (int i = 0; i < workpieces.Count; i++)
        {
            Sprite preview = Resources.Load<Sprite>(workpieces[i].data + "_preview");
            ScrollItem newScrollItem = Instantiate(scrollItemPrefab, transform);
            newScrollItem.relatedWorkpiece = workpieces[i];
            newScrollItem.transform.GetChild(0).GetComponent<Image>().sprite = preview;
            newScrollItem.sprite = preview;
            newScrollItem.componentScroll = this;
            if (i == 0)
                newScrollItem.ShowBigPreviewPic();
        }
    }

    public void SpawnWorkpieceInScene()
    {
        if (currentWorkpiece != null)
            ApplicationController.Instance.SpawnWorkpiece(currentWorkpiece);
    }
}
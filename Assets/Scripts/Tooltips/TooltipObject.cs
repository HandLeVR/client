using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string tooltipText;

    private TooltipHandler tooltipHandler;
    private GameObject mainCanvas;
    private bool active;
    private bool isShowing;
    // time user needs to stay on the object to activate the tooltip
    private float currentOffsetShow;
    // time till tooltip will be hidden
    private float currentOffsetHide;

    private void Awake()
    {
        mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        tooltipHandler = mainCanvas.transform.Find("Tooltip Screen").GetComponent<TooltipHandler>();
        currentOffsetHide = tooltipHandler.offsetHide;
        currentOffsetShow = tooltipHandler.offsetShow;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        active = true;
        currentOffsetHide = tooltipHandler.offsetHide;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        active = false;
        currentOffsetShow = tooltipHandler.offsetShow;
    }

    private void OnDestroy()
    {
        if (isShowing)
            tooltipHandler.HideTooltip();
    }

    void Update()
    {
        // show the tooltip after the offset time
        if (active && !isShowing)
        {
            if (currentOffsetShow >= 0.0f)
            {
                currentOffsetShow -= Time.deltaTime;
            }
            else if (!tooltipHandler.gameObject.activeSelf)
            {
                ShowTooltip(tooltipText);
                isShowing = true;
            }
        }
        // hide the tooltip after the offset time
        else if (!active && isShowing)
        {
            if (currentOffsetHide >= 0.0f)
            {
                currentOffsetHide -= Time.deltaTime;
            }
            else if (tooltipHandler.gameObject.activeSelf)
            {
                tooltipHandler.HideTooltip();
                isShowing = false;
            }
        }
    }

    protected virtual void ShowTooltip(string text)
    {
        tooltipHandler.ShowTooltip(text);
    }
}
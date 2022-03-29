using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Creates a tooltip whereat the position of the tooltip depends on the position of the mouse pointer.
/// </summary>
public class TooltipHandler : MonoBehaviour
{
    public GameObject tooltipContainer;
    public TextMeshProUGUI tooltipText;
    // time user needs to stay on the object to activate the tooltip
    public float offsetShow = 1.0f; 
    // time till tooltip will be hidden
    public float offsetHide = 0.0f; 

    /// <summary>
    /// Displays the tooltip with the given content.
    /// </summary>
    public void ShowTooltip(string content)
    {
        tooltipText.text = content;
        tooltipContainer.GetComponent<RectTransform>().position = CalculateValidPosition(Mouse.current.position.ReadValue());
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the tooltip.
    /// </summary>
    public void HideTooltip()
    {
        tooltipText.text = "";
        tooltipContainer.transform.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
        gameObject.SetActive(false);
    }


    /// <summary>
    /// sketch:
    /// _______________________
    /// |          :          |
    /// |    TL    :    TR    |
    /// |          :          |
    /// |..........:..........|
    /// |          :          |
    /// |    BL    :    BR    |
    /// |__________:__________|
    /// </summary>
    /// <param name="mousePosition">Mouse position on screen as Vector2.</param>
    /// <returns></returns>
    private Vector2 CalculateValidPosition(Vector2 mousePosition)
    {
        // this RectTransform represents the screen area where a tooltip can be shown
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, mousePosition, null,
            out Vector2 positionInRectangle);
        
        // detect the quadrant and set pivot and position:
        // quadrant = TL
        if (positionInRectangle.x < 0f && positionInRectangle.y >= 0f) 
        {
            tooltipContainer.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);
            return mousePosition + new Vector2(12, 0);
        }
        // quadrant = BL
        if (positionInRectangle.x < 0f && positionInRectangle.y < 0f) 
        {
            tooltipContainer.GetComponent<RectTransform>().pivot = new Vector2(0f, 0f);
            return mousePosition + new Vector2(0, 0);
        }
        // quadrant = TR
        if (positionInRectangle.x >= 0f && positionInRectangle.y >= 0f) 
        {
            tooltipContainer.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f);
            return mousePosition + new Vector2(0, 0);
        }
        // quadrant = BR
        if (positionInRectangle.x >= 0f && positionInRectangle.y < 0f) 
        {
            tooltipContainer.GetComponent<RectTransform>().pivot = new Vector2(1f, 0f);
            return mousePosition + new Vector2(0, 0);
        }

        return Vector2.zero;
    }
}
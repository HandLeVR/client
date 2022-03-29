using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a day in the calendar visualization.
/// </summary>
public class DayContainer : MonoBehaviour
{
    public TextMeshProUGUI dayLabel;
    public Image backgroundImage;
    public Color disabledColor; // not part of current month
    public Color weekendColor;
    public Color holidayColor;
    public Image todayImage;

    public void Init(int d, bool today, bool isHoliday, string tooltipText)
    {
        dayLabel.text = d.ToString();
        todayImage.gameObject.SetActive(today);
        int sibIndex = transform.GetSiblingIndex();
        Button button = GetComponent<Button>();
        ColorBlock colorBlock = button.colors;
        if ((sibIndex - 5) % 7 == 0 || (sibIndex - 6) % 7 == 0)
            backgroundImage.color = weekendColor;
        if (isHoliday)
            backgroundImage.color = holidayColor;
        button.colors = colorBlock;
        TooltipObject toolTip = GetComponent<TooltipObject>();
        toolTip.tooltipText = tooltipText;
        toolTip.enabled = !String.IsNullOrEmpty(tooltipText);
    }

    public void DisableDay()
    {
        dayLabel.gameObject.SetActive(false);
        backgroundImage.color = disabledColor;
        todayImage.gameObject.SetActive(false);
    }
}
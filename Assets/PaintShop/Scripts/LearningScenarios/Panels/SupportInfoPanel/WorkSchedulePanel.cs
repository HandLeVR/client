using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a work schedule.
/// </summary>
public class WorkSchedulePanel : MonoBehaviour
{
    public Button prevPageButton;
    public Button nextPageButton;
    public GameObject[] pages;
    public TextMeshProUGUI currentPageText;

    private int currentPage;
    private const string currentPageTemplate = "{0} / {1}";

    private void OnEnable()
    {
        currentPage = 0;
        UpdatePage();
        UpdateButtons();
    }

    public void ShowPrevPage()
    {
        currentPage--;
        UpdatePage();
        UpdateButtons();
    }

    public void ShowNextPage()
    {
        currentPage++;
        UpdatePage();
        UpdateButtons();
    }

    private void UpdatePage()
    {
        for (int i = 0; i < pages.Length; i++)
            pages[i].SetActive(i == currentPage);

        currentPageText.text = string.Format(currentPageTemplate, currentPage + 1, pages.Length);
    }

    private void UpdateButtons()
    {
        nextPageButton.interactable = currentPage != pages.Length - 1;
        prevPageButton.interactable = currentPage != 0;
    }
}

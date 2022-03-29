using TMPro;
using translator;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Allows to display a popup on the learning tasks monitor.
/// </summary>
public class PopupScreenController : Singleton<PopupScreenController>
{
    public GameObject messageScreen;
    public TextMeshProUGUI messageText;
    
    public GameObject loadingScreen;
    public TextMeshProUGUI loadingHeader;
    public Slider loadingBar;

    public Button confirmButton;

    private void Awake()
    {
        confirmButton.onClick.AddListener(ClosePopupScreen);
    }

    public void ShowLoadingScreen(string headerTextTranslationKey, bool showLoadingBar = false)
    {
        gameObject.SetActive(true);
        messageScreen.SetActive(false);
        loadingScreen.SetActive(true);
        loadingBar.gameObject.SetActive(showLoadingBar);
        loadingBar.value = 0;
        loadingHeader.text = TranslationController.Instance.Translate(headerTextTranslationKey);
    }
    
    public void UpdateLoadingBar(float loadingStatus)
    {
        loadingBar.value = loadingStatus;
    }

    public void ShowConfirmationScreen(string textTranslationKey, params object[] formatStrings)
    {
        gameObject.SetActive(true);
        messageScreen.SetActive(true);
        loadingScreen.SetActive(false);
        confirmButton.gameObject.SetActive(true);
        messageText.text = TranslationController.Instance.Translate(textTranslationKey, formatStrings);;
    }

    public void ClosePopupScreen()
    {
        gameObject.SetActive(false);
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// The Controller of the application selection scene which shows a menu allowing to select the scenes to load. Is
/// loaded after logging in and as an additive scene when vr scenes are loaded. In vr scenes the menu can be
/// minimized and maximized.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    public Camera sceneCamera;
    public GameObject menu;
    public Button authoringToolButton;
    public Button reflectionToolButton;
    public Button learningTasksButton;
    public Button playgroundButton;
    public Button logoutButton;
    public Button exitButton;
    public Button minimizeButton;
    public Button maximizeButton;
    public EventSystem eventSystem;

    public void Awake()
    {
        authoringToolButton.onClick.AddListener(LoadAuthoringTool);
        playgroundButton.onClick.AddListener(LoadPlaygroundScene);
        learningTasksButton.onClick.AddListener(LoadLearningTasksScene);
        reflectionToolButton.onClick.AddListener(LoadReflectionTool);
        exitButton.onClick.AddListener(ExitApplication);
        logoutButton.onClick.AddListener(Logout);
        minimizeButton.onClick.AddListener(() => Minimize(true));
        maximizeButton.onClick.AddListener(() => Minimize(false));
        if (SceneManager.GetSceneByName("PaintShop").IsValid())
        {
            Minimize(true);
            sceneCamera.gameObject.SetActive(false);
            // we need to deactivate the event system to prevent two active event systems
            eventSystem.gameObject.SetActive(false);
        }

        // only teachers can use the authoring tool
        if (DataController.Instance.CurrentUser.role != User.Role.Teacher &&
            DataController.Instance.CurrentUser.role != User.Role.RestrictedTeacher)
            authoringToolButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Minimizes or maximizes the menu.
    /// </summary>
    private void Minimize(bool minimize)
    {
        menu.SetActive(!minimize);
        minimizeButton.gameObject.SetActive(!minimize);
        maximizeButton.gameObject.SetActive(minimize);
    }

    private void LoadAuthoringTool()
    {
        PopupScreenHandler.Instance.ShowLoadingScreen("popup-start-application", "popup-please-wait");
        SceneManager.LoadScene("AuthoringTool");
    }

    private void LoadReflectionTool()
    {
        PopupScreenHandler.Instance.ShowLoadingScreen("popup-start-application", "popup-please-wait");
        DataController.Instance.reflectionToolChosen = true;
        SceneManager.LoadScene("PaintShop");
    }

    private void LoadPlaygroundScene()
    {
        PopupScreenHandler.Instance.ShowLoadingData();
        DataController.Instance.UpdateData(DataController.RequestType.Basic,
            () =>
            {
                PopupScreenHandler.Instance.ShowLoadingScreen("popup-start-application", "popup-please-wait");
                LoadVRScene("PaintShop");
            }, PopupScreenHandler.Instance.ShowConnectionError);
    }

    private void LoadLearningTasksScene()
    {
        PopupScreenHandler.Instance.ShowLoadingData();
        DataController.Instance.UpdateData(DataController.RequestType.Basic,
            () =>
            {
                PopupScreenHandler.Instance.ShowLoadingScreen("popup-start-application", "popup-please-wait");
                LoadVRScene("PaintShopDynamicScenario");
            }, PopupScreenHandler.Instance.ShowConnectionError);
    }

    private void LoadVRScene(string sceneName)
    {
        DataController.Instance.reflectionToolChosen = false;
        SceneManager.LoadScene("PaintShop");
        if (sceneName != "PaintShop")
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    private void ExitApplication()
    {
        PopupScreenHandler.Instance.ShowExitApplication();
    }

    private void Logout()
    {
        PopupScreenHandler.Instance.ShowYesNo("popup-logout", "popup-logout-confirm", () =>
        {
            DataController.Instance.CurrentUser = null;
            DataController.Instance.CurrentAccessToken = null;
            DataController.Instance.CurrentRefreshToken = null;
            SceneManager.LoadScene("Login");
        });
    }
}
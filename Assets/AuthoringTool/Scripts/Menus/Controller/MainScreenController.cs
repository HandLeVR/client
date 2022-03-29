using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// The main controller that handles the loading of data.
/// </summary>
public class MainScreenController : Singleton<MainScreenController>
{
    public MainMenuTabHandler panelTabHandler;

    private void Awake()
    {
        // debug (in the build version we always have a connection to the server at this point)
        if (DataController.Instance.connectionState == DataController.ConnectionState.Unknown)
        {
            // set debug data
            DataController.Instance.CurrentUser = new User { id = 1, role = User.Role.Teacher};
        }
    }

    public void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            PopupScreenHandler.Instance.ShowExitApplication();
    }

    /// <summary>
    /// Loads data in dependence of the given request type. Shows the loading screen while data is loaded.
    /// </summary>
    public void LoadData(DataController.RequestType requestType, UnityAction doAfterwards)
    {
        PopupScreenHandler.Instance.ShowLoadingData();
        DataController.ConnectionState priorConnectionState = DataController.Instance.connectionState;
        DataController.Instance.UpdateData(requestType, () =>
        {
            PopupScreenHandler.Instance.gameObject.SetActive(false);
            doAfterwards.Invoke();
        }, () =>
        {
            if (DataController.Instance.connectionState == DataController.ConnectionState.Connection)
                PopupScreenHandler.Instance.ShowConnectionError();
            else
            {
                if (priorConnectionState == DataController.ConnectionState.Unknown)
                    PopupScreenHandler.Instance.ShowMessage("popup-load-data", "popup-initial-connection-error");
                doAfterwards.Invoke();
            }
        });
    }

    public void LoadMainMenuScreen()
    {
        if (panelTabHandler.UnsavedChangesInCurrentPanel())
        {
            PopupScreenHandler.Instance.ShowUnsavedChanges(() => SceneManager.LoadScene("MainMenu"));
            return;
        }

        SceneManager.LoadScene("MainMenu");
    }
}
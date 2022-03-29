using System.Net;
using Proyecto26;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;

/// <summary>
/// Handles the login in the login scene.
/// </summary>
public class LoginScreenController : MonoBehaviour
{
    public GameObject loginMenu;
    public PasswordMenuController passwordMenuController;
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;

    public void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            PopupScreenHandler.Instance.ShowExitApplication();
    }

    /// <summary>
    /// Tries to login the user.
    /// </summary>
    public void Login()
    {
        if (!CheckFields())
            return;

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-login", "popup-please-wait");

        RequestHelper requestHelper = new RequestHelper
        {
            Uri = ConfigController.Instance.GetURLPrefix() +
                  ConfigController.Instance.GetStringValue(ConfigController.SERVER_CLIENT_USER_NAME) + ":" +
                  ConfigController.Instance.GetStringValue(ConfigController.SERVER_CLIENT_SECRET) + "@" +
                  ConfigController.Instance.GetStringValue(ConfigController.SERVER_OAUTH_LOGIN_URL) +
                  "&username=" + usernameField.text + "&password=" + passwordField.text,
            CertificateHandler = new CustomCertificateHandler()
        };

        RestClient.Get(requestHelper, LoginCallback);
    }

    /// <summary>
    /// Is called after we get the response from the server.
    /// </summary>
    private void LoginCallback(RequestException exception, ResponseHelper response)
    {
        if (exception != null)
        {
            if (exception.IsHttpError && exception.StatusCode == (long)HttpStatusCode.BadRequest)
                PopupScreenHandler.Instance.ShowMessage("popup-wrong-input", "popup-wrong-input-text");
            else
                PopupScreenHandler.Instance.ShowConnectionError();
            Debug.Log("HTTP Request Error: " + response.Error);
            return;
        }

        PopupScreenHandler.Instance.Close();

        // save current user data 
        var jsonObject = JObject.Parse(response.Text);
        DataController.Instance.CurrentUser = jsonObject.GetValue("user").ToObject<User>();
        DataController.Instance.CurrentAccessToken = jsonObject.GetValue("access_token").ToString();
        DataController.Instance.CurrentRefreshToken = jsonObject.GetValue("refresh_token").ToString();
        DataController.Instance.connectionState = DataController.ConnectionState.Connection;

        // shows the terms of service text and requests a password change afterwards if this is the first login
        if (!DataController.Instance.CurrentUser.passwordChanged)
            PopupScreenHandler.Instance.ShowTermsOfService(
                new PopupScreenHandler.ButtonConfig(SetFirstPassword, "popup-button-accept"),
                new PopupScreenHandler.ButtonConfig(PopupScreenHandler.Instance.Close, "popup-button-decline"));
        else
            SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Starts the process to change the password by answering a security question.
    /// </summary>
    public void ChangePassword()
    {
        loginMenu.SetActive(false);
        passwordMenuController.ChangePassword(() => loginMenu.SetActive(true));
    }

    /// <summary>
    /// Requests the user to change the password.
    /// </summary>
    private void SetFirstPassword()
    {
        PopupScreenHandler.Instance.Close();
        loginMenu.SetActive(false);
        passwordMenuController.SetFirstPassword(() => loginMenu.SetActive(true));
    }

    /// <summary>
    /// Continues without the login which opens the test scene.
    /// </summary>
    public void NoLogin()
    {
        PopupScreenHandler.Instance.ShowLoadingData();
        DataController.Instance.connectionState = DataController.ConnectionState.NoConnection;
        SceneManager.LoadScene("PaintShop");
    }

    public void StartTutorial()
    {
        PopupScreenHandler.Instance.ShowLoadingData();
        SceneManager.LoadScene("PaintShop");
        SceneManager.LoadSceneAsync("PaintShopDynamicScenario", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Opens the exit application menu.
    /// </summary>
    public void ExitApplication()
    {
        PopupScreenHandler.Instance.ShowExitApplication();
    }

    /// <summary>
    /// Checks whether the login fields contain proper values.
    /// </summary>
    /// <returns></returns>
    private bool CheckFields()
    {
        if (usernameField.text.Equals(""))
        {
            PopupScreenHandler.Instance.ShowMessage("popup-missing-values", "popup-missing-user-name");
            return false;
        }

        if (passwordField.text.Equals(""))
        {
            PopupScreenHandler.Instance.ShowMessage("popup-missing-values", "popup-missing-password");
            return false;
        }

        return true;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Proyecto26;
using RSG;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Contains functions to communicate withe REST services of the server.
/// </summary>
public static class RestConnector
{
    /// <summary>
    /// Sends a POST or PUT request.
    /// </summary>
    /// <param name="obj">The object which we serialize and send to the server.</param>
    /// <param name="method">POST or PUT</param>
    /// <param name="path">The path of the REST service. The base URL is added automatically.</param>
    /// <param name="onSuccess">Called on success. Contains the updated/new object.</param>
    /// <param name="onError">Called on error.</param>
    /// <param name="onConflict">Called on conflict.</param>
    /// <param name="onNotFound">Called on not found.</param>
    /// <param name="updateLocal">Determines whether the local data is updated too.</param>
    /// <typeparam name="T">Determined by the given object</typeparam>
    public static void Update<T>(T obj, string method, string path, UnityAction<T> onSuccess, UnityAction onError,
        UnityAction<string> onConflict = null, UnityAction<string> onNotFound = null, bool updateLocal = true)
    {
        SendRequest(method, path, JsonConvert.SerializeObject(obj), (exception, helper) =>
        {
            if (exception != null)
            {
                if (helper.StatusCode == 409 && onConflict != null)
                    onConflict.Invoke(JObject.Parse(helper.Text)["message"].ToString());
                else if (helper.StatusCode == 404 && onNotFound != null)
                    onNotFound.Invoke(JObject.Parse(helper.Text)["message"].ToString());
                else
                    OnFailure(onError, exception);
            }
            else
            {
                T newObj = JsonConvert.DeserializeObject<T>(helper.Text);
                if (updateLocal)
                    DataController.Instance.Put(newObj);
                onSuccess.Invoke(newObj);
            }
        });
    }

    /// <summary>
    /// Calls the update function with the PUT method.
    /// </summary>
    public static void Put<T>(T obj, string path, UnityAction<T> onSuccess, UnityAction onError,
        UnityAction<string> onConflict = null, UnityAction<string> onNotFound = null, bool updateLocal = true)
    {
        Update(obj, UnityWebRequest.kHttpVerbPUT, path, onSuccess, onError, onConflict, onNotFound, updateLocal);
    }

    /// <summary>
    /// Calls the update function with the POST method.
    /// </summary>
    public static void Post<T>(T obj, string path, UnityAction<T> onSuccess, UnityAction onError,
        UnityAction<string> onConflict = null, UnityAction<string> onNotFound = null, bool updateLocal = true)
    {
        Update(obj, UnityWebRequest.kHttpVerbPOST, path, onSuccess, onError, onConflict, onNotFound, updateLocal);
    }

    /// <summary>
    /// Sends a delete request
    /// </summary>
    /// <param name="obj">The object we want to delete which is only used here to delete is locally.</param>
    /// <param name="path">Path of the rest service which contains the id of the element we want to delete.</param>
    /// <param name="onSuccess">Called on success. Contains the updated/new object.</param>
    /// <param name="onError">Called on error.</param>
    /// <param name="onConflict">Called on conflict.</param>
    /// <param name="deleteLocal">Determines whether we want to delete the object locally.</param>
    public static void Delete<T>(T obj, string path, UnityAction onSuccess, UnityAction<string> onConflict,
        UnityAction onError, bool deleteLocal = true)
    {
        SendRequest(UnityWebRequest.kHttpVerbDELETE, path, "", (exception, helper) =>
        {
            if (exception != null)
            {
                if (helper.StatusCode == 409 && onConflict != null)
                    onConflict.Invoke(JObject.Parse(helper.Text)["message"].ToString());
                else
                    OnFailure(onError, exception);
            }
            else
            {
                if (deleteLocal)
                    DataController.Instance.Delete(obj);
                onSuccess.Invoke();
            }
        });
    }

    /// <summary>
    /// Sends a get request and if successful returns the deserialized object in the on success event.
    /// </summary>
    /// <param name="path">Path of the rest service.</param>
    /// <param name="onSuccess">Called on success.</param>
    /// <param name="onError">Called on error.</param>
    /// <typeparam name="T">Type of the object.</typeparam>
    public static void GetObject<T>(string path, UnityAction<T> onSuccess, UnityAction onError)
    {
        SendGetRequest(path)
            .Then(helper => onSuccess.Invoke(JsonConvert.DeserializeObject<T>(helper.Text)))
            .Catch(exception => OnFailure(onError, exception));
    }

    /// <summary>
    /// Sets the connection state to no connection and calls the given action.
    /// </summary>
    private static void OnFailure(UnityAction onError, Exception exception)
    {
        Debug.Log(exception);
        if (DataController.Instance.connectionState == DataController.ConnectionState.Unknown)
            DataController.Instance.connectionState = DataController.ConnectionState.NoConnection;
        onError.Invoke();
    }

    /// <summary>
    /// Sends a request to the server.
    /// </summary>
    /// <param name="requestType">Type of the request (POST,PUT or DELETE).</param>
    /// <param name="path">Path of REST service.</param>
    /// <param name="body">Body of the message.</param>
    /// <param name="callback">Called after we got the result.</param>
    private static void SendRequest(string requestType, string path, string body,
        Action<RequestException, ResponseHelper> callback)
    {
        RequestHelper requestHelper = new RequestHelper
        {
            Uri = ConfigController.Instance.GetFullServerURL() + path,
            BodyString = body,
            Method = requestType,
            CertificateHandler = new CustomCertificateHandler()
        };
        if (!string.IsNullOrEmpty(DataController.Instance.CurrentAccessToken))
            requestHelper.Headers["Authorization"] = "Bearer " + DataController.Instance.CurrentAccessToken;
        RestClient.Request(requestHelper,
            (exception, helper) => RefreshTokenIntercept(exception, requestHelper, helper, callback));
    }

    /// <summary>
    /// Sends a request to the server.
    /// </summary>
    /// <param name="requestType">Type of the request (POST,PUT or DELETE).</param>
    /// <param name="path">Path of REST service.</param>
    /// <param name="form">The WWWForm which can be used to send files.</param>
    /// <param name="callback">Called after we got the result.</param>
    public static void SendRequest(string requestType, string path, WWWForm form,
        Action<RequestException, ResponseHelper> callback)
    {
        RequestHelper requestHelper = new RequestHelper
        {
            Uri = ConfigController.Instance.GetFullServerURL() + path,
            FormData = form,
            Method = requestType,
            CertificateHandler = new CustomCertificateHandler()
        };
        if (!string.IsNullOrEmpty(DataController.Instance.CurrentAccessToken))
            requestHelper.Headers["Authorization"] = "Bearer " + DataController.Instance.CurrentAccessToken;
        RestClient.Request(requestHelper,
            (exception, helper) => RefreshTokenIntercept(exception, requestHelper, helper, callback));
    }

    /// <summary>
    /// Sends a get request.
    /// </summary>
    public static IPromise<ResponseHelper> SendGetRequest(string path)
    {
        RequestHelper requestHelper = new RequestHelper
        {
            Uri = ConfigController.Instance.GetFullServerURL() + path,
            CertificateHandler = new CustomCertificateHandler()
        };
        if (!string.IsNullOrEmpty(DataController.Instance.CurrentAccessToken))
            requestHelper.Headers["Authorization"] = "Bearer " + DataController.Instance.CurrentAccessToken;
        var promise = new Promise<ResponseHelper>();
        RestClient.Get(requestHelper,
            (exception, helper) => RefreshTokenIntercept(exception, requestHelper, helper, promise.Promisify));
        return promise;
    }

    /// <summary>
    /// Updates the user groups and task assignments of the given user object.
    /// </summary>
    public static void GetUserData(User user, UnityAction onSuccess, UnityAction onError)
    {
        SendGetRequest("/users/" + user.id + "/userGroups")
            .Then(helper => user.userGroups = JsonConvert.DeserializeObject<List<UserGroup>>(helper.Text))
            .Then(_ => SendGetRequest("/users/" + user.id + "/taskAssignments"))
            .Then(helper => user.taskAssignments = JsonConvert.DeserializeObject<List<TaskAssignment>>(helper.Text))
            .Then(_ => onSuccess.Invoke())
            .Catch(exception => OnFailure(onError, exception));
    }

    /// <summary>
    /// Updates the task assignments of a user
    /// </summary>
    /// <param name="user">The user we want to update.</param>
    /// <param name="onlyWithTaskResults">Determines if we only want to get task assignments with task results.</param>
    /// <param name="onSuccess">Called on success.</param>
    /// <param name="onError">Called on error.</param>
    public static void GetUserTaskAssignments(User user, bool onlyWithTaskResults, UnityAction onSuccess,
        UnityAction onError)
    {
        SendGetRequest("/users/" + user.id + "/taskAssignments" + (onlyWithTaskResults ? "WithTaskResults" : ""))
            .Then(helper => user.taskAssignments = JsonConvert.DeserializeObject<List<TaskAssignment>>(helper.Text))
            .Then(_ => onSuccess.Invoke())
            .Catch(exception => OnFailure(onError, exception));
    }

    /// <summary>
    /// Gets media elements, recordings, coats and workpieces used in a task
    /// </summary>
    public static void GetTaskData(Task task, UnityAction onSuccess, UnityAction onError)
    {
        SendGetRequest("/tasks/" + task.id + "/media")
            .Then(helper => task.usedMedia = JsonConvert.DeserializeObject<HashSet<Media>>(helper.Text))
            .Then(mediaSet => mediaSet.ToList().ForEach(media => DataController.Instance.media[media.id] = media))
            .Then(() => SendGetRequest("/tasks/" + task.id + "/recordings"))
            .Then(helper => task.usedRecordings = JsonConvert.DeserializeObject<HashSet<Recording>>(helper.Text))
            .Then(_ => SendGetRequest("/tasks/" + task.id + "/coats"))
            .Then(helper => task.usedCoats = JsonConvert.DeserializeObject<HashSet<Coat>>(helper.Text))
            .Then(_ => SendGetRequest("/tasks/" + task.id + "/workpieces"))
            .Then(helper => task.usedWorkpieces = JsonConvert.DeserializeObject<HashSet<Workpiece>>(helper.Text))
            .Then(_ => onSuccess.Invoke())
            .Catch(exception => OnFailure(onError, exception));
    }

    /// <summary>
    /// Requests the security question set up by the user.
    /// </summary>
    public static void GetSecurityQuestion(string userName, UnityAction<long, string> onSuccess,
        UnityAction onWrongNickname, UnityAction onError)
    {
        SendRequest(UnityWebRequest.kHttpVerbGET, "/users/" + userName + "/getSecurityQuestion", "",
            (exception, helper) =>
            {
                if (exception != null)
                {
                    if (helper.StatusCode == 404)
                        onWrongNickname.Invoke();
                    else
                        OnFailure(onError, exception);
                }
                else
                {
                    JObject jObject = JObject.Parse(helper.Text);
                    onSuccess.Invoke(jObject["userId"].ToObject<long>(),
                        jObject["securityQuestion"].ToObject<string>());
                }
            });
    }

    /// <summary>
    /// Validates whether the security question was answered correctly.
    /// </summary>
    public static void ValidateSecurityAnswer(long userId, string securityAnswer, UnityAction<bool> onSuccess,
        UnityAction onError)
    {
        SendRequest(UnityWebRequest.kHttpVerbGET, "/users/" + userId + "/validateSecurityAnswer", securityAnswer,
            (exception, helper) =>
            {
                if (exception != null)
                    OnFailure(onError, exception);
                else
                    onSuccess.Invoke(bool.Parse(helper.Text));
            });
    }

    /// <summary>
    /// Validates whether the password is correct.
    /// </summary>
    public static void ValidatePassword(long userId, string password, UnityAction<bool> onSuccess, UnityAction onError)
    {
        SendRequest(UnityWebRequest.kHttpVerbGET, "/users/" + userId + "/validatePassword", password,
            (exception, helper) =>
            {
                if (exception != null)
                    OnFailure(onError, exception);
                else
                    onSuccess.Invoke(bool.Parse(helper.Text));
            });
    }

    /// <summary>
    /// Updates the user password.
    /// The parameter oldSecurityAnswer is empty if the update the password on the first login of the user. If the user
    /// forgot his password oldSecurityAnswer contains the answer of the the security question and is used for authorization.
    /// </summary>
    public static void UpdateUserPassword(string oldSecurityAnswer, string password, string securityQuestion,
        string securityAnswer, long userId, UnityAction onSuccess, UnityAction onError)
    {
        SendRequest(UnityWebRequest.kHttpVerbPUT,
            "/users/" + userId + "/updatePassword" +
            (string.IsNullOrEmpty(oldSecurityAnswer) ? "" : "WithSecurityQuestion"), new JObject(
                new JProperty("oldSecurityAnswer", oldSecurityAnswer), new JProperty("password", password),
                new JProperty("securityQuestion", securityQuestion),
                new JProperty("securityAnswer", securityAnswer)).ToString(),
            (exception, _) =>
            {
                if (exception != null)
                    OnFailure(onError, exception);
                else
                    onSuccess();
            });
    }

    /// <summary>
    /// Disables the user and makes the name anonymous.
    /// </summary>
    public static void DisableUser(long userId, UnityAction onSuccess, UnityAction onError)
    {
        SendRequest(UnityWebRequest.kHttpVerbPUT, "/users/" + userId + "/disable", "",
            (exception, _) =>
            {
                if (exception != null)
                    OnFailure(onError, exception);
                else
                    onSuccess();
            });
    }

    /// <summary>
    /// Uploads a file to the server.
    /// </summary>
    public static void Upload<T>(T obj, string path, FileInfo fileInfo, UnityAction<T> onSuccess,
        UnityAction<string> onConflict, UnityAction onTooBig, UnityAction onError, UnityAction onFinal = null)
    {
        if (fileInfo.Length > ConfigController.Instance.GetMaxFileSize() * 1024 * 1024)
        {
            onTooBig.Invoke();
            onFinal?.Invoke();
            return;
        }
            
        string json = JsonConvert.SerializeObject(obj);
        var form = new WWWForm();
        form.AddField("obj", json);
        byte[] fileBytes = File.ReadAllBytes(fileInfo.FullName);
        form.AddBinaryData("file", fileBytes, fileInfo.Name);

        RequestHelper requestHelper = new RequestHelper
        {
            Uri = ConfigController.Instance.GetFullServerURL() + path,
            CertificateHandler = new CustomCertificateHandler()
        };
        if (!string.IsNullOrEmpty(DataController.Instance.CurrentAccessToken))
            requestHelper.Headers["Authorization"] = "Bearer " + DataController.Instance.CurrentAccessToken;
        SendRequest(UnityWebRequest.kHttpVerbPOST, path, form, (exception, helper) =>
        {
            try
            {
                if (exception != null)
                {
                    if (helper.StatusCode == 409 && onConflict != null)
                        onConflict.Invoke(JObject.Parse(helper.Text)["message"].ToString());
                    else if (helper.StatusCode == 417 && onTooBig != null)
                        onTooBig.Invoke();
                    else
                    {
                        Debug.LogError(helper.Error);
                        onError.Invoke();
                    }
                }
                else
                {
                    T newResult = JsonConvert.DeserializeObject<T>(helper.Text);
                    onSuccess(newResult);
                }
            }
            finally
            {
                onFinal?.Invoke();
            }
        });
    }
    
    /// <summary>
    /// Uploads an import file to the server.
    /// </summary>
    public static void UploadImportFile(string path, FileInfo fileInfo, UnityAction<String> onSuccess,
        UnityAction onBadRequest, UnityAction onTooBig, UnityAction onError, UnityAction onFinal = null)
    {
        if (fileInfo.Length > ConfigController.Instance.GetMaxFileSize() * 1024 * 1024)
        {
            onTooBig.Invoke();
            onFinal?.Invoke();
            return;
        }
        
        byte[] fileBytes = File.ReadAllBytes(fileInfo.FullName);
        var form = new WWWForm();
        form.AddField("fileName", fileInfo.Name);
        form.AddBinaryData("file", fileBytes, fileInfo.Name);
        RequestHelper requestHelper = new RequestHelper
        {
            Uri = ConfigController.Instance.GetFullServerURL() + path,
            CertificateHandler = new CustomCertificateHandler()
        };
        if (!string.IsNullOrEmpty(DataController.Instance.CurrentAccessToken))
            requestHelper.Headers["Authorization"] = "Bearer " + DataController.Instance.CurrentAccessToken;
        SendRequest(UnityWebRequest.kHttpVerbPOST, path, form, (exception, helper) =>
        {
            try
            {
                if (exception != null)
                {
                    if (helper.StatusCode == 400 && onBadRequest != null)
                        onBadRequest.Invoke();
                    else if (helper.StatusCode == 417 && onTooBig != null)
                        onTooBig.Invoke();
                    else
                    {
                        Debug.LogError(helper.Error);
                        onError.Invoke();
                    }
                }
                else
                {
                    String newResult = "Uploaded the zip file complete.";
                    onSuccess(newResult);
                }
            }
            finally
            {
                onFinal?.Invoke();
            }
        });
    }

    /// <summary>
    /// Copies the file in an async thread.
    /// </summary>
    public static async void CopyFileAsync(string sourcePath, string destinationPath, UnityAction onFinish)
    {
        using (Stream source = File.Open(sourcePath, FileMode.Open))
        {
            string directoryPath = Path.GetDirectoryName(destinationPath);
            if (directoryPath != null && !Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            using (Stream destination = File.Create(destinationPath))
            {
                await source.CopyToAsync(destination);
            }
        }

        onFinish.Invoke();
    }

    /// <summary>
    /// Downloads the file
    /// </summary>
    public static IEnumerator DownloadFile(string path, string filePath, UnityAction onSuccess, UnityAction onError)
    {
        UnityWebRequest request = UnityWebRequest.Get(ConfigController.Instance.GetFullServerURL() + path);
        DownloadHandlerFile dh = new DownloadHandlerFile(filePath);
        dh.removeFileOnAbort = true;
        request.downloadHandler = dh;
        request.certificateHandler = new CustomCertificateHandler();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
            onError.Invoke();
        else
        {
            while (!request.downloadHandler.isDone)
                yield return null;
            onSuccess.Invoke();
        }
    }

    /// <summary>
    /// Refreshes the access token if it has timed out.
    /// </summary>
    private static void RefreshTokenIntercept(RequestException exception, RequestHelper requestHelper,
        ResponseHelper responseHelper, Action<RequestException, ResponseHelper> callback)
    {
        if (exception != null && responseHelper.StatusCode == 401)
        {
            RestClient.Get(
                ConfigController.Instance.GetStringValue(
                    ConfigController.Instance.GetURLPrefix() +
                    ConfigController.Instance.GetStringValue(ConfigController.SERVER_CLIENT_USER_NAME) + ":" +
                    ConfigController.Instance.GetStringValue(ConfigController.SERVER_CLIENT_SECRET) + "@" +
                    ConfigController.SERVER_OAUTH_REFRESH_TOKEN_URL) + "&refresh_token=" +
                DataController.Instance.CurrentRefreshToken,
                (newException, newHelper) =>
                {
                    if (newException == null)
                    {
                        var jsonObject = JObject.Parse(newHelper.Text);
                        DataController.Instance.CurrentAccessToken = jsonObject.GetValue("access_token").ToString();
                        requestHelper.Headers["Authorization"] = "Bearer " + DataController.Instance.CurrentAccessToken;
                        RestClient.Request(requestHelper, callback);
                    }
                    else
                    {
                        callback(exception, responseHelper);
                    }
                });
        }
        else
            callback(exception, responseHelper);
    }
}
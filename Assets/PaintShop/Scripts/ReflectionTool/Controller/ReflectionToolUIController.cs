using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using translator;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;

// disable warning "Because this call is not awaited, execution of the current method continues before the call is completed"
#pragma warning disable 4014

/// <summary>
/// Controls the ui part of the reflection tool.
/// </summary>
public class ReflectionToolUIController : Singleton<ReflectionToolUIController>
{
    [Header("Recording Selection")] public TMP_Dropdown usersDropdown;
    public TMP_Dropdown learningScenarioDropdown;
    public ScrollRect tableScrollRect;
    public TaskResultRow taskResultRowPrefab;
    public Button loadRecordingButton;

    [Header("Parameter Visibility Toggles")]
    public Toggle currentValueToggle;

    public Toggle currentAverageValueToggle;
    public Toggle finalValueToggle;

    [Header("Reflection Parameter")] public Transform row1Container;
    public Transform row2Container;
    public Transform row3Container;
    public Transform row4Container;
    public EvaluationParameterContainer evaluationParameterPrefab;
    public ColorEvaluationParameterContainer colorEvaluationParameterPrefab;
    public Transform emptyPrefab;

    [Header("Misc")] public Toggle pathSpeedToggle;
    public Material grayscaleMaterial;

    [Header("Coat Thickness")] public Canvas canvas;
    public Camera controlledCamera;
    public RectTransform cameraImage;
    public MeshRenderer marker;

    private EvaluationParameterContainer _correctDistanceEPC;
    private EvaluationParameterContainer _correctAngleEPC;
    private ColorEvaluationParameterContainer _colorMultiEPC;
    private EvaluationParameterContainer _triggerDepressedEPC;
    private EvaluationParameterContainer _averageSpeedEPC;
    private EvaluationParameterContainer _layerThicknessEPC;

    private TaskResult _currentTaskResult;
    private Dictionary<Task, List<TaskResult>> _possibleTaskResults;
    private List<TaskResult> _currentTaskResults;
    private List<TaskResultRow> _taskResultRows;
    private List<User> _users;
    private User _currentUser;

    private CustomDrawable _markerCustomDrawable;
    private Vector2 _markerUV;
    private float _layerThicknessAtPosition;

    private void Start()
    {
        PopupScreenHandler.Instance.ShowLoadingData();
        InitContainerDefault();
        InitToggles();
        usersDropdown.options = new List<TMP_Dropdown.OptionData>();
        if (DataController.Instance.CurrentUser.role != User.Role.Teacher &&
            DataController.Instance.CurrentUser.role != User.Role.RestrictedTeacher)
        {
            usersDropdown.interactable = false;
            usersDropdown.options.Add(new TMP_Dropdown.OptionData(DataController.Instance.CurrentUser.fullName));
            usersDropdown.value = 0;
            LoadUserTasks(DataController.Instance.CurrentUser);
        }
        else
        {
            DataController.Instance.UpdateData(DataController.RequestType.Users, () =>
                {
                    PopupScreenHandler.Instance.Close();
                    InitUsersDropdown();
                }, PopupScreenHandler.Instance.ShowConnectionError
            );
            usersDropdown.onValueChanged.AddListener(SelectUser);
            learningScenarioDropdown.options.Clear();
            learningScenarioDropdown.options.Add(
                new TMP_Dropdown.OptionData(TranslationController.Instance.Translate("reflection-tool-choose-task")));
            learningScenarioDropdown.RefreshShownValue();
            learningScenarioDropdown.interactable = false;
        }
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame &&
            ReflectionToolRecordingController.Instance.RecordingInitialized() &&
            RectTransformUtility.RectangleContainsScreenPoint(cameraImage.parent.GetComponent<RectTransform>(),
                Mouse.current.position.ReadValue()))
        {
            // determine the ray hitting the workpiece at the mouse position
            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
            // center if the bounds of the camera image relative to the canvas
            Vector2 boundsCenter =
                RectTransformUtility.CalculateRelativeRectTransformBounds(canvas.transform, cameraImage.transform)
                    .center;
            // mouse position in the canvas (pixels of the rect differ from the pixelRect)
            Vector2 mousePos = Mouse.current.position.ReadValue() / canvas.pixelRect.size *
                               canvasRectTransform.rect.size;
            // offset of the cameraImage relative to the canvas
            Vector2 posOffset = (canvasRectTransform.rect.size / 2 + boundsCenter) - cameraImage.rect.size / 2;
            // position in the camera image (value between (0,0) and (1,1))
            Vector2 posInCameraImage = (mousePos - posOffset) / cameraImage.rect.size;
            Ray ray = controlledCamera.ViewportPointToRay(posInCameraImage);

            if (Physics.Raycast(ray, out RaycastHit raycastHit, 10, LayerMask.GetMask("Drawable")))
            {
                _markerCustomDrawable = raycastHit.collider.GetComponent<CustomDrawable>();
                if (_markerCustomDrawable)
                    ActivateMarker(raycastHit);
            }
            else
                DeactivateMarker();
        }

        if (marker.gameObject.activeSelf && _markerCustomDrawable)
        {
            // read the one pixel marked
            RenderTexture.active = _markerCustomDrawable.heightmapOutput;
            Texture2D rgbTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            rgbTex.ReadPixels(
                new Rect(_markerUV.x * _markerCustomDrawable.textureSize,
                    (1 - _markerUV.y) * _markerCustomDrawable.textureSize,
                    1, 1), 0, 0);
            rgbTex.Apply();
            RenderTexture.active = null;
            _layerThicknessAtPosition = HeightmapToFloat(rgbTex.GetPixel(0, 0));
            _layerThicknessEPC.currentValueContainer.SetDoNotUnfreezeMe(false);
            _layerThicknessEPC.currentValueContainer.UnfreezeContent();
        }
    }

    private void SelectUser(int index)
    {
        if (index > 0)
        {
            LoadUserTasks(_users[index - 1]);
            return;
        }
        
        learningScenarioDropdown.value = 0;
        learningScenarioDropdown.interactable = false;
        CreateTaskResultTable();
    }

    private void ActivateMarker(RaycastHit raycastHit)
    {
        marker.transform.position = raycastHit.point;
        marker.gameObject.SetActive(true);
        marker.transform.SetParent(_markerCustomDrawable.transform);
        _markerUV = raycastHit.textureCoord;
    }

    private void DeactivateMarker()
    {
        marker.gameObject.SetActive(false);
        marker.transform.SetParent(null);
        _layerThicknessEPC.currentValueContainer.SetDoNotUnfreezeMe(true);
        _layerThicknessEPC.currentValueContainer.FreezeContent();
    }

    private float HeightmapToFloat(Color pixel)
    {
        return Mathf.Max(pixel.b, pixel.a) / PaintController.Instance.targetMinThicknessWetAlpha *
               PaintController.Instance.chosenCoat.targetMinThicknessWet;
    }

    private void LoadUserTasks(User user)
    {
        PopupScreenHandler.Instance.ShowLoadingData();
        RestConnector.GetUserTaskAssignments(user, true,
            () =>
            {
                _currentUser = user;
                InitLearningTaskDropdown(user);
                PopupScreenHandler.Instance.Close();
            }, PopupScreenHandler.Instance.ShowConnectionError);
    }

    /// <summary>
    /// Show all learning tasks with valid task results. 
    /// </summary>
    private void InitLearningTaskDropdown(User user)
    {
        learningScenarioDropdown.interactable = true;
        _possibleTaskResults = new Dictionary<Task, List<TaskResult>>();
        foreach (TaskAssignment assignment in user.taskAssignments)
        {
            if (!_possibleTaskResults.ContainsKey(assignment.task))
                _possibleTaskResults[assignment.task] = new List<TaskResult>();
            _possibleTaskResults[assignment.task].AddRange(assignment.taskResults);
        }
        
        learningScenarioDropdown.options.Clear();
        learningScenarioDropdown.options.Add(
            new TMP_Dropdown.OptionData(TranslationController.Instance.Translate("reflection-tool-choose-task")));
        foreach (KeyValuePair<Task, List<TaskResult>> pair in _possibleTaskResults)
        {
            learningScenarioDropdown.options.Add(new TMP_Dropdown.OptionData(pair.Key.name));
            // set the task result of the recording to simplify loading later
            pair.Value.ForEach(result => result.recording.taskResult = result);
        }

        learningScenarioDropdown.value = 0;
        learningScenarioDropdown.RefreshShownValue();
    }

    private void InitUsersDropdown()
    {
        usersDropdown.options.Add(
            new TMP_Dropdown.OptionData(TranslationController.Instance.Translate("reflection-tool-choose-user")));
        _possibleTaskResults = new Dictionary<Task, List<TaskResult>>();
        _users = DataController.Instance.users.Values.OrderBy(user => user.fullName).ToList();
        _users.ForEach(user => usersDropdown.options.Add(new TMP_Dropdown.OptionData(user.fullName)));
        usersDropdown.RefreshShownValue();
    }

    /// <summary>
    /// Load result values from a recording after selecting it in the table.
    /// </summary>
    private void SelectRow(int index)
    {
        DeactivateMarker();
        loadRecordingButton.interactable = false;
        _taskResultRows.ForEach(r => r.Deselect());
        _taskResultRows[index].Select();
        _currentTaskResult = _currentTaskResults[index];
        // TODO: only download if not already present
        PopupScreenHandler.Instance.ShowLoadingData();
        StartCoroutine(RestConnector.DownloadFile("/recordings/" + _currentTaskResult.recording.id + "/file",
            _currentTaskResult.recording.GetZipFile().FullName,
            () => ZipUtil.ExtractAsync(_currentTaskResult.recording.GetZipFile(),
                () =>
                {
                    _currentTaskResult.recording.SaveRecordingFile();
                    PlayRecordingController.Instance.LoadRecording(_currentTaskResult.recording, UpdateFinalValues,
                        false, initWorkpiece: true);
                    EvaluationController.Instance.Reset();
                    _taskResultRows[index].task.usedCoats.ToList()
                        .ForEach(coat => DataController.Instance.coats[coat.id] = coat);
                    _taskResultRows[index].task.usedWorkpieces.ToList().ForEach(workpiece =>
                        DataController.Instance.workpieces[workpiece.id] = workpiece
                    );
                    PopupScreenHandler.Instance.Close();
                }),
            PopupScreenHandler.Instance.ShowConnectionError));
    }

    /// <summary>
    /// Updates the final values.
    /// </summary>
    private void UpdateFinalValues()
    {
        EvaluationData data = PlayRecordingController.Instance.recordingData.evaluationData;
        EvaluationController.Instance.LoadEvaluationData(data);
        UpdateFinalValues(
            EvaluationController.Instance.GetCorrectDistancePercentage(),
            EvaluationController.Instance.GetCorrectAnglePercentage(),
            EvaluationController.Instance.GetColorConsumption(), EvaluationController.Instance.GetColorWastage(),
            EvaluationController.Instance.GetColorUsage(),
            EvaluationController.Instance.GetFullyPressedPercentage(),
            EvaluationController.Instance.GetAverageSpeed(), EvaluationController.Instance.GetCurrentCoatThickness());
        loadRecordingButton.interactable = true;
        // needed to modify line width
        SprayGunPathController.Instance.finalAverageSpeed = EvaluationController.Instance.GetAverageSpeed();
        ReflectionToolRecordingController.Instance.Reset();
    }

    /// <summary>
    /// Creates the task result table after selecting a learning task.
    /// </summary>
    public void CreateTaskResultTable()
    {
        tableScrollRect.content.transform.DestroyAllChildren();
        loadRecordingButton.interactable = false;
        if (learningScenarioDropdown.value == 0)
            return;
        KeyValuePair<Task, List<TaskResult>> pair = _possibleTaskResults.ToList()[learningScenarioDropdown.value - 1];
        _currentTaskResults = pair.Value;
        _taskResultRows = new List<TaskResultRow>();
        for (int i = 0; i < _currentTaskResults.Count; i++)
        {
            TaskResultRow row = Instantiate(taskResultRowPrefab, tableScrollRect.content.transform);
            row.Init(pair.Key, _currentTaskResults[i].recording.date, _currentTaskResults[i].recording.neededTime, i,
                SelectRow, DeleteResult);
            _taskResultRows.Add(row);
        }
    }

    /// <summary>
    /// Deletes the task result and the corresponding recording.
    /// </summary>
    private void DeleteResult(int index, bool confirmed)
    {
        if (!confirmed)
        {
            PopupScreenHandler.Instance.ShowConfirmation("popup-remove-task-result",
                "popup-remove-task-result-confirmation", () => DeleteResult(index, true));
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-remove-task-result", "popup-removing-task-result");
        RestConnector.Delete(_currentTaskResults[index], "/taskResults/" + _currentTaskResults[index].id, () =>
            {
                TaskAssignment taskAssignment = _currentUser.taskAssignments.Find(assignment =>
                    assignment.taskResults.Contains(_currentTaskResults[index]));
                taskAssignment.taskResults.Remove(_currentTaskResults[index]);
                
                // if there a now task results for a task anymore we need to remove the entry in the dropdown
                if (taskAssignment.taskResults.Count == 0)
                {
                    _currentUser.taskAssignments.Remove(taskAssignment);
                    InitLearningTaskDropdown(_currentUser);
                    ReflectionToolRecordingController.Instance.Reset();
                }
                // reset the recorder if the removed task result was currently selected
                else if (_currentTaskResults[index].Equals(_currentTaskResult))
                    ReflectionToolRecordingController.Instance.Reset();
                
                // delete recording files
                _currentTaskResults[index].recording.DeleteRecording();

                // remove the row representing the task result
                Destroy(_taskResultRows[index].gameObject);

                PopupScreenHandler.Instance.ShowMessage("popup-remove-task-result", "popup-removed-task-result");
            }, _ => PopupScreenHandler.Instance.ShowConnectionError(), PopupScreenHandler.Instance.ShowConnectionError,
            false);
    }

    /// <summary>
    /// Leads the corresponding recording of the selected task result.
    /// </summary>
    public void LoadRecoding()
    {
        ReflectionToolRecordingController.Instance.InitializeRecording(_currentTaskResult.recording);
    }

    /// <summary>
    /// Create the necessary containers to display the attributes.
    /// </summary>
    private void InitContainerDefault()
    {
        _colorMultiEPC = Instantiate(colorEvaluationParameterPrefab, row1Container);
        _colorMultiEPC.gameObject.name = "Paint Multi EPC";
        _colorMultiEPC.InitColorContainer();
        _correctDistanceEPC = Instantiate(evaluationParameterPrefab, row2Container);
        _correctDistanceEPC.gameObject.name = "Correct Distance EPC";
        _correctDistanceEPC.InitContainerCorrectDistance();
        _correctAngleEPC = Instantiate(evaluationParameterPrefab, row2Container);
        _correctAngleEPC.gameObject.name = "Correct Angle EPC";
        _correctAngleEPC.InitContainerCorrectAngle();
        _triggerDepressedEPC = Instantiate(evaluationParameterPrefab, row3Container);
        _triggerDepressedEPC.gameObject.name = "Trigger Pressed EPC";
        _triggerDepressedEPC.InitContainerTriggerPressed();
        _averageSpeedEPC = Instantiate(evaluationParameterPrefab, row3Container);
        _averageSpeedEPC.gameObject.name = "Average Speed EPC";
        _averageSpeedEPC.InitContainerSpeed();
        _layerThicknessEPC = Instantiate(evaluationParameterPrefab, row4Container);
        _layerThicknessEPC.gameObject.name = "Layer Thickness EPC";
        _layerThicknessEPC.InitContainerThickness();
        Transform emptyDummy = Instantiate(emptyPrefab, row4Container);
        emptyDummy.gameObject.name = "Empty Dummy";
    }

    /// <summary>
    /// Initialize the reflection parameter toggles.
    /// </summary>
    private void InitToggles()
    {
        currentValueToggle.onValueChanged.AddListener(delegate
        {
            _correctDistanceEPC.ToggleFrozenCurrentValue();
            _correctAngleEPC.ToggleFrozenCurrentValue();
            _colorMultiEPC.ToggleFrozenCurrentValue();
            _triggerDepressedEPC.ToggleFrozenCurrentValue();
            _averageSpeedEPC.ToggleFrozenCurrentValue();
            _layerThicknessEPC.ToggleFrozenCurrentValue();
        });
        currentAverageValueToggle.onValueChanged.AddListener(delegate
        {
            _correctDistanceEPC.ToggleFrozenCurrentAverageValue();
            _correctAngleEPC.ToggleFrozenCurrentAverageValue();
            _triggerDepressedEPC.ToggleFrozenCurrentAverageValue();
            _averageSpeedEPC.ToggleFrozenCurrentAverageValue();
            _layerThicknessEPC.ToggleFrozenCurrentAverageValue();
        });
        finalValueToggle.onValueChanged.AddListener(delegate
        {
            _correctDistanceEPC.ToggleFrozenFinalValue();
            _correctAngleEPC.ToggleFrozenFinalValue();
            _colorMultiEPC.ToggleFrozenFinalValue();
            _triggerDepressedEPC.ToggleFrozenFinalValue();
            _averageSpeedEPC.ToggleFrozenFinalValue();
            _layerThicknessEPC.ToggleFrozenFinalValue();
        });
    }

    /// <summary>
    /// Update all 'current value' containers by given new values.
    /// </summary>
    public void UpdateCurrentValues(float correctDistance, float correctAngle, float colorConsumption,
        float colorWasted, float colorOnWorkpiece, float triggerDepressed, float averageSpeed)
    {
        _correctDistanceEPC.UpdateCurrentContainerContent(correctDistance, "cm");
        _correctAngleEPC.UpdateCurrentContainerContent(correctAngle, "°");
        _colorMultiEPC.UpdateCurrentContainerContent(colorConsumption, colorWasted, colorOnWorkpiece);
        _triggerDepressedEPC.UpdateCurrentContainerContent(triggerDepressed);
        _averageSpeedEPC.UpdateCurrentContainerContent(averageSpeed, "m/s");
        _layerThicknessEPC.UpdateCurrentContainerContent(_layerThicknessAtPosition, "μm");
    }

    /// <summary>
    /// Update all 'current average value' containers by given new values.
    /// </summary>
    public void UpdateCurrentAverageValues(float correctDistance, float correctAngle, float triggerDepressed,
        float averageSpeed, float currentLayerThickness)
    {
        _correctDistanceEPC.UpdateCurrentAverageContainerContent(correctDistance);
        _correctAngleEPC.UpdateCurrentAverageContainerContent(correctAngle);
        _triggerDepressedEPC.UpdateCurrentAverageContainerContent(triggerDepressed);
        _averageSpeedEPC.UpdateCurrentAverageContainerContent(averageSpeed, "m/s");
        _layerThicknessEPC.UpdateCurrentAverageContainerContent(currentLayerThickness, "μm");
    }

    public void PlayPause()
    {
        ReflectionToolRecordingController.Instance.OnPlayButtonClick();
    }

    public void ActivateDistanceRaySprayingAssistance(bool active)
    {
        ApplicationController.Instance.ActivateDistanceRaySprayingAssistance(active);
    }

    public void ActivateDistanceMarkerSprayingAssistance(bool active)
    {
        ApplicationController.Instance.ActivateDistanceMarkerSprayingAssistance(active);
    }

    public void ActivateAngleRaySprayingAssistance(bool active)
    {
        ApplicationController.Instance.ActivateAngleRaySprayingAssistance(active);
    }

    public void ActivateHeatMap(bool active)
    {
        ApplicationController.Instance.showHeatMap = active;
        ApplicationController.Instance.currentWorkpieceGameObject.GetComponentInChildren<CustomDrawable>()
            .UpdateWorkpiece();
    }

    public void ActivateSprayGunAudio(bool active)
    {
        ReflectionToolRecordingController.Instance.playSprayGunAudio = active;
        ApplicationController.Instance.ActivateSprayGunAudio(active);
    }

    public void ActivateSprayCone(bool active)
    {
        ApplicationController.Instance.ActivateSprayCone(active);
    }

    public void ActivateSprayGunPath(bool active)
    {
        SprayGunPathController.Instance.ShowPath(active);
        pathSpeedToggle.interactable = active;
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Update all 'final average value' containers by given new values.
    /// </summary>
    private void UpdateFinalValues(float correctDistance, float correctAngle, float colorConsumption, float colorWasted,
        float colorOnWorkpiece, float triggerDepressed, float averageSpeed, float coatThickness)
    {
        _correctDistanceEPC.UpdateFinalAverageContainerContent(correctDistance);
        _correctAngleEPC.UpdateFinalAverageContainerContent(correctAngle);
        _colorMultiEPC.UpdateFinalAverageContainerContent(colorConsumption, colorWasted, colorOnWorkpiece);
        _triggerDepressedEPC.UpdateFinalAverageContainerContent(triggerDepressed);
        _averageSpeedEPC.UpdateFinalAverageContainerContent(averageSpeed, "m/s");
        _layerThicknessEPC.UpdateFinalAverageContainerContent(coatThickness, "μm");
    }
}
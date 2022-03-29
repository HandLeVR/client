using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Main controller managing the key parts of the application. Is used in the default paint shop scene which is the
/// base scene for every 3d level including the reflection tool.
/// </summary>
public class ApplicationController : Singleton<ApplicationController>
{
    public FramesPerSecond framesPerSecond;
    public XRPlayer player;
    public AudioSource paintBoothAudio;
    public Material invisibleMaterial;
    public bool debug;

    // the current selected workpiece (not the instance)
    [HideInInspector] public Workpiece currentSelectedWorkpiece;

    // the current spawned workpiece
    [HideInInspector] public GameObject currentWorkpieceGameObject;

    // the name of the port the spray gun is connected (if connected)
    [HideInInspector] public string realSprayGunCOMPortName;

    // determines whether the spraying sound is played
    [HideInInspector] public bool playSprayGunAudio = true;

    // determines whether the spray cone is visible
    [HideInInspector] public bool showSprayCone = true;

    // determines whether the heatmap mode is active
    [HideInInspector] public bool showHeatMap;

    // determines whether the heatmap is shown in the workpiece or the applied paint (then the heatmap is visible through the magnifying glass)
    [HideInInspector] public bool showHeatmapOnWorkpiece;

    // determines whether the spray gun stops shortly on collisions with the paint stand
    [HideInInspector] public bool stopSprayGunOnCollision = true;

    // is true if the spray gun is initialized
    [HideInInspector] public bool sprayGunIsInitializing;

    // determines the currently active spray assistance
    [HideInInspector] public SprayingAssistance.SprayingAssistanceType sprayingAssistanceType = 0;

    // the currently touched PaintStandMovement (bars of the paint stand to adjust the height or rotation)
    [HideInInspector] public PaintStandMovement currentPaintStandMovement;

    // the currently touched draggables
    [HideInInspector] public HashSet<Draggable> currentDraggables;

    // the currently active draggable (closest to the hand)
    [HideInInspector] public Draggable currentDraggable;

    // the primary spray gun (is changed if a recording is played)
    [HideInInspector] public SprayGun primarySprayGun;

    // the coin which is currently touched
    [HideInInspector] public Coin currentTouchedCoin;

    // the coin which is currently pointed on
    [HideInInspector] public Coin currentPointedCoin;

    // the paint stand
    [HideInInspector] public PaintStand paintStand;

    // the clone of the workpiece used to realize the magnifying glass
    [HideInInspector] public GameObject workpieceClone;

    // needed to disable short cut buttons in the reflection tool which only make sense in a vr scene
    [HideInInspector] public bool buttonsActive = true;

    // current coin (prefers the touched coin)
    public Coin CurrentCoin => currentTouchedCoin ? currentTouchedCoin : currentPointedCoin;

    // the used spray gun (controller or real spray gun)
    public SprayGun sprayGun
    {
        get => _sprayGun;
        set
        {
            _sprayGun = value;
            primarySprayGun = value;
            _onSprayGunSpawned.Invoke();
        }
    }

    // the current tool in the hand of the user
    public SprayGun.Tool currentTool
    {
        get => _currentTool;
        set
        {
            if (_currentTool == SprayGun.Tool.MagnifyingGlass)
            {
                if (!showHeatmapOnWorkpiece)
                    InvertHeatmapVisibility();
                CloneWorkpiece(false);
            }

            if (value == SprayGun.Tool.MagnifyingGlass)
                CloneWorkpiece(true);
            _currentTool = value;
            sprayGun.SetTool(currentTool);
        }
    }

    private SprayGun.Tool _currentTool;
    private SprayGun _sprayGun;
    private readonly UnityEvent _onSprayGunSpawned = new();

    private void Awake()
    {
        // check if a real spray gun is connected
        realSprayGunCOMPortName = GetRealSprayGunPort();

        // create the small hood workpiece or the first workpiece found
        SpawnWorkpiece(
            DataController.Instance.workpieces.Values.FirstOrDefault(workpiece =>
                workpiece.data.Equals("CarComponents/hood_2")) ?? DataController.Instance.workpieces[0]);

        // debug (in the build version we always have a connection to the server at this point)
        if (DataController.Instance.connectionState == DataController.ConnectionState.Unknown && debug)
        {
            // set debug data
            DataController.Instance.CurrentUser = new User { id = 1 };
            DataController.Instance.connectionState = DataController.ConnectionState.NoConnection;
            DataController.Instance.UpdateData(DataController.RequestType.Basic, () => { }, () => { });
        }

        // deactivates the caution tape and the chaperone cube if an Oculus Quest is connected because the boundaries
        // of the play area is not handed over to SteamVR
        /*if (SteamVR.instance != null && SteamVR.instance.hmd_ModelNumber.Contains("Quest"))
        {
            player.GetComponentInChildren<MeshRenderer>().enabled = false;
            player.transform.parent.GetComponentInChildren<ChaperoneCube>().gameObject.SetActive(false);
        }*/

        // the reflection tool was chosen in the select application menu
        if (DataController.Instance.reflectionToolChosen)
            SceneManager.LoadScene("ReflectionTool", LoadSceneMode.Additive);
        // application selection scene is only loaded if a user is logged in
        else if (DataController.Instance.connectionState != DataController.ConnectionState.NoConnection &&
                 DataController.Instance.CurrentUser != null)
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Additive);

        paintStand = currentWorkpieceGameObject.GetComponentInChildren<PaintStand>();
        currentDraggables = new HashSet<Draggable>();
    }

    private void Update()
    {
        HighlightClosestDraggable();

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Application.Quit();

        if (!buttonsActive)
            return;

        // (de)activate the display of the frames per second
        if (Keyboard.current.dKey.wasPressedThisFrame && framesPerSecond != null)
            framesPerSecond.enabled = !framesPerSecond.enabled;

        if (Keyboard.current.rKey.wasPressedThisFrame)
            player.RecenterPosition();

        if (Keyboard.current.hKey.wasPressedThisFrame)
            showHeatMap = !showHeatMap;
    }

    /// <summary>
    /// The port to the spray gun needs to be closed if the application is closed or a new scene is loaded.
    /// </summary>
    private void OnDisable()
    {
        if (sprayGun)
            sprayGun.GetComponent<RealSprayGun>()?.Disconnect();
    }

    /// <summary>
    /// Executes an action immediately if the spray gun is already spawned or as soon as it spawns.
    /// </summary>
    public void ExecuteAfterSprayGunSpawn(UnityAction action)
    {
        if (sprayGun)
            action.Invoke();
        else
            _onSprayGunSpawned.AddListener(action);
    }

    /// <summary>
    /// Activates or deactivates the evaluation mode with the corresponding tool.
    /// </summary>
    public void SetEvaluationModeActive(bool active, SprayGun.Tool tool = SprayGun.Tool.MagnifyingGlass)
    {
        showHeatMap = active;
        currentTool = active ? tool : SprayGun.Tool.SprayGun;
        sprayGun.isDisabled = active;
    }

    /// <summary>
    /// Checks whether the real spray gun is connected
    /// </summary>
    public bool RealSprayGunConnected()
    {
        return realSprayGunCOMPortName != null;
    }

    /// <summary>
    /// Finds the closed draggable in all touched draggables, sets it to the current draggable and highlights it.
    /// </summary>
    private void HighlightClosestDraggable()
    {
        if (sprayGun == null || sprayGun.currentDraggable != null)
            return;

        float oldDistance = Mathf.Infinity;
        currentDraggable = null;
        foreach (Draggable draggable in currentDraggables)
        {
            float distance = Vector3.Distance(sprayGun.pinSpotOrigin.transform.position, draggable.transform.position);
            if (distance < oldDistance)
            {
                oldDistance = distance;
                currentDraggable = draggable;
            }
        }
    }

    /// <summary>
    /// Spawns a workpiece, destroys the old workpiece and replaces the
    /// old workpiece in the list of drawables maintained by the PaintController.
    /// </summary>
    public void SpawnWorkpiece(Workpiece workpiece)
    {
        // only destroy if active, otherwise we get an error message if the workpiece was
        // deactivated at the start of the scene (release of uninitialized render textures)
        if (currentWorkpieceGameObject != null && currentWorkpieceGameObject.activeSelf)
            DestroyImmediate(currentWorkpieceGameObject);
        GameObject prefab = Resources.Load<GameObject>(workpiece.data);
        currentWorkpieceGameObject = Instantiate(prefab);
        currentSelectedWorkpiece = workpiece;
        PaintController.Instance.SetDrawable(currentWorkpieceGameObject.GetComponentInChildren<CustomDrawable>());
        EvaluationController.Instance.Reset();
        paintStand = currentWorkpieceGameObject.GetComponentInChildren<PaintStand>();
        PaintStandHitController.Instance.ClearHits();
        PlayRecordingController.Instance.ClearHits();
    }

    /// <summary>
    /// Pauses the update routine in SprayGun and CustomDrawables
    /// </summary>
    public void SetPauseUpdate(bool value)
    {
        if (sprayGun != null)
        {
            if (value)
            {
                sprayGun.StopVisuals();
                sprayGun.StopAudio();
            }

            sprayGun.enabled = !value;
            sprayGun.isDisabled = value;
        }

        foreach (var drawable in PaintController.Instance.drawables)
            drawable.enabled = !value;
    }

    /// <summary>
    /// Shows the heatmap or the painted coat on the workpiece in evaluation mode.
    /// </summary>
    public void InvertHeatmapVisibility()
    {
        if (currentTool == SprayGun.Tool.Flashlight)
        {
            // sets the bool for CustomDrawable to read
            showHeatmapOnWorkpiece = !showHeatmapOnWorkpiece;
        }
        else if (currentTool == SprayGun.Tool.MagnifyingGlass)
        {
            Renderer workpieceRenderer = currentWorkpieceGameObject.GetComponentInChildren<Renderer>();

            if (showHeatmapOnWorkpiece)
            {
                // if invisibility IS NOT currently inverted then do invert it
                workpieceClone.GetComponent<Renderer>().material = new Material(workpieceRenderer.material);
                workpieceRenderer.material = new Material(invisibleMaterial);
                showHeatmapOnWorkpiece = false;
            }
            else
            {
                // if invisibility IS currently inverted then do invert it
                workpieceRenderer.material = new Material(workpieceClone.GetComponent<Renderer>().material);
                workpieceClone.GetComponent<Renderer>().material = new Material(invisibleMaterial);
                showHeatmapOnWorkpiece = true;
            }
        }
    }

    public void ActivatePaintBoothAudio(bool activate)
    {
        if (activate)
            paintBoothAudio.Play();
        else
            paintBoothAudio.Stop();
    }

    public void ActivateSprayGunAudio(bool activate)
    {
        playSprayGunAudio = activate;
        if (!activate && sprayGun)
            sprayGun.StopAudio();
    }

    public void ActivateSprayCone(bool activate)
    {
        showSprayCone = activate;
        if (!activate && sprayGun)
            sprayGun.StopVisuals();
    }

    public void DeactivateAllSprayingAssistance()
    {
        sprayingAssistanceType = 0x0;
    }

    public void ActivateDistanceMarkerSprayingAssistance(bool active)
    {
        ActivateSprayingAssistance(active, SprayingAssistance.SprayingAssistanceType.DistanceMarker);
    }

    public void ActivateDistanceRaySprayingAssistance(bool active)
    {
        ActivateSprayingAssistance(active, SprayingAssistance.SprayingAssistanceType.DistanceRay);
    }

    public void ActivateVibrationSprayingAssistance(bool active)
    {
        ActivateSprayingAssistance(active, SprayingAssistance.SprayingAssistanceType.Vibration);
    }

    public void ActivateAngleRaySprayingAssistance(bool active)
    {
        ActivateSprayingAssistance(active, SprayingAssistance.SprayingAssistanceType.AngleRay);
    }

    private void ActivateSprayingAssistance(bool active, SprayingAssistance.SprayingAssistanceType type)
    {
        if (active)
            sprayingAssistanceType |= type;
        else
            sprayingAssistanceType &= ~type;
    }

    public void SetStopSprayGunOnCollision(bool stop)
    {
        stopSprayGunOnCollision = stop;
    }

    /// <summary>
    /// Clones the workpiece and sets the shader of the material to make the magnifying glass work.
    /// </summary>
    private void CloneWorkpiece(bool active)
    {
        if (active)
        {
            showHeatmapOnWorkpiece = true;
            CustomDrawable currentDrawable = currentWorkpieceGameObject.GetComponentInChildren<CustomDrawable>();
            GameObject workpiece = currentDrawable.gameObject;
            workpieceClone = Instantiate(workpiece, workpiece.transform.position, workpiece.transform.rotation)
                .gameObject;
            workpieceClone.GetComponent<Renderer>().material = new Material(invisibleMaterial);
            currentDrawable.currentCopy = workpieceClone.GetComponent<CustomDrawable>();
        }
        else
        {
            Destroy(workpieceClone);
            currentWorkpieceGameObject.GetComponentInChildren<CustomDrawable>().currentCopy = null;
        }
    }


    /// <summary>
    /// Iterates over all available serial ports to find the port the spray gun is connected to if there
    /// is a spray gun connected.
    /// </summary>
    private string GetRealSprayGunPort()
    {
        foreach (string portName in SerialPort.GetPortNames())
        {
            SerialPort sp = new SerialPort(portName, 19200) { ReadTimeout = 100, WriteTimeout = 100 };
            try
            {
                sp.Open();
                for (int tries = 10; tries > 0; tries--)
                {
                    try
                    {
                        // in the arduino script we specified that the current values should be returned if a "v" is send
                        sp.WriteLine("v");
                        string stringValue = sp.ReadLine();
                        if (stringValue.Split(' ').Length >= 1)
                        {
                            return portName;
                        }
                    }
                    catch (Exception)
                    {
                        // ignore timeouts
                    }
                }
            }
            catch (Exception)
            {
                // ignore "broken" ports
            }
            finally
            {
                sp.Close();
            }
        }

        return null;
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Used when the spray gun is controlled by a Controller.
/// </summary>
public class SprayGun : CustomDrawingController
{
    [Tooltip("The origin for the SpotDrawer. Should be placed at the muzzle of the gun.")]
    public Transform pinSpotOrigin;

    [Tooltip("The pressing point where the trigger starts.")]
    public float startTriggerValue = 0f;

    [Tooltip("The pressing point up to which only air is produced by the spray gun. Afterwards the coat is produced.")]
    public float airTriggerValue = 0.2f;

    [Tooltip("The pressing point where the maximum pressure is used.")]
    public float fullTriggerValue = 1f;

    [Tooltip("The maximum rotation of the trigger. Used to animate the trigger.")]
    public float maxTriggerRotation = 25f;

    [Tooltip("The point the trigger is rotated around.")]
    public Transform TriggerRotationPoint;

    [Tooltip("The parent of the visible spray gun object to be able to make the spray gun transparent.")]
    public GameObject sprayGunObjects;

    [Tooltip("The normal spray gun tank texture visible while painting the workpiece.")]
    public Texture2D tankTextureSpray;

    [Tooltip("The spray gun tank texture with a teleport indicator visible when teleporting is possible.")]
    public Texture2D tankTextureTeleport;

    // spray visuals
    [HideInInspector] public ParticleSystem spray;
    [HideInInspector] public SprayCone sprayCone;

    // audio of the spray gun
    [HideInInspector] public AudioSource audioSource;
    [HideInInspector] public Vector3 velocity;

    // indicates the current mode
    [HideInInspector] public SprayGunMode currentMode;

    // the object which is currently grabbed
    [HideInInspector] public Draggable currentDraggable;

    // control spraying
    private bool _triggerPressed;
    private ActionToFloat _sprayAction;
    private bool _isDisabled;

    // needed to determine whether paint can flow into the spray gun
    private Transform _beakerOrientation;
    private bool _upsideDown;

    // for handling visuals of the spray gun
    protected List<MeshRenderer> _meshList;
    private MeshRenderer _tank;
    private LineDrawer _lineDrawer;

    // used to correctly animate the trigger
    protected Quaternion _initialTriggerRotation;

    // ui interaction
    private Selectable _currentSelectable;
    private ScrollRect _currentScrollRect;
    private RaycastHit _currentUiHit;
    private bool _menuButtonPressed;
    private Vector3 _lastHitPosition;

    // needed to scale the spray cookie in dependence of the movements between to frames
    private Transform _tmpPinSpotOrigin;
    protected Vector3 _lastStartPos;
    protected Quaternion _lastDirection;
    private Texture _originalCookie;
    private RenderTexture _modifiedCookie;

    // tools which can replace the spray gun in evaluation mode
    private GameObject _flashlight;
    private GameObject _magnifyingGlass;

    // control teleporting
    private Teleport _teleport;

    // the custom values for the controller spray gun (can be used to adapt trigger position where painting starts)
    private string _controllerSprayGunValuesFile;

    private const float MinSprayStartLifetime = 0.5f;
    private const float MaxSprayStartLifetime = 1f;
    private const float MinSprayEmissionRate = 10;
    private const float MaxSprayEmissionRate = 50;

    private static readonly int TargetHeightId = Shader.PropertyToID("_TargetHeight");

    // disables paint application but interaction with the spray gun is still possible
    public bool isDisabled
    {
        get => _isDisabled || !isInPaintRange;
        set => _isDisabled = value;
    }

    // determines whether the spray gun is close enough to a drawable to allow painting
    public bool isInPaintRange { get; private set; }

    /// <summary>
    /// The SprayGunMode which actions are currently possible with the spray gun.
    ///
    /// None: Idle mode from which it is possible to switch to every other mode. From other modes it is only
    /// possible to switch to None.
    /// Spray: Indicates that the user is currently spraying. Is is only possible to switch to this mode if the spray
    /// gun is close enough to a drawable.
    /// UI: Indicates that the user currently points on the monitor. A ray is drawn and interactables can be selected
    /// in the UI.
    /// Hand: Indicates that the user currently interacts with an object like a Draggable or the paint stand. The spray
    /// gun is invisible in this mode and a Hand is shown at the object.
    /// Coin: Indicates that the user currently points on coin or the spray can touches a coin.
    /// Teleport: Indicates that the user currently selects a target for teleporting.
    /// </summary>
    public enum SprayGunMode
    {
        None,
        Spray,
        UI,
        Hand,
        Coin,
        Teleport
    }

    /// <summary>
    /// Indicates the tool the user currently holds in his hands (virtually displayed). The flashlight and the
    /// magnifying glass are used in the evaluation mode.
    /// </summary>
    public enum Tool
    {
        SprayGun,
        MagnifyingGlass,
        Flashlight,
        None
    }

    public virtual void Awake()
    {
        _controllerSprayGunValuesFile = Application.dataPath + "/StreamingAssets/ControllerSprayGunCalibration.json";
        _sprayAction = GetComponent<ActionToFloat>();

        isDisabled = false;
        paintSpotDrawer = FindObjectOfType<CustomSpotDrawer>();
        spray = GetComponentInChildren<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
        _lineDrawer = GetComponent<LineDrawer>();
        sprayCone = GetComponentInChildren<SprayCone>();
        _beakerOrientation = transform.Find("Beaker Orientation");
        _flashlight = transform.FindDeepChild("Flashlight")?.gameObject;
        _magnifyingGlass = transform.FindDeepChild("Magnifying Glass")?.gameObject;
        _meshList = sprayGunObjects
            ? sprayGunObjects.GetComponentsInChildren<MeshRenderer>().ToList()
            : new List<MeshRenderer>();
        _tmpPinSpotOrigin = new GameObject("Temp Pin Spot Origin").GetComponent<Transform>();
        _teleport = GetComponentInChildren<Teleport>();
        _tank = sprayGunObjects != null
            ? sprayGunObjects.transform.FindDeepChild("Tank").GetComponent<MeshRenderer>()
            : null;
        TrySetCurrentMode(SprayGunMode.None);
        if (TriggerRotationPoint)
            _initialTriggerRotation = TriggerRotationPoint.localRotation;
        _originalCookie = PaintController.Instance.cookie;
        _modifiedCookie =
            new RenderTexture(_originalCookie.width, _originalCookie.height, 0, RenderTextureFormat.ARGBHalf);
    }

    public virtual void Start()
    {
        // read the calibration file and set variables 
        ReadValuesFromFile();
    }

    /// <summary>
    /// Sets the spray gun mode but only if the current mode is None or the next mode is None.
    /// </summary>
    private void TrySetCurrentMode(SprayGunMode mode)
    {
        if (currentMode != SprayGunMode.None && mode != SprayGunMode.None)
            return;

        currentMode = mode;
    }

    public virtual void FixedUpdate()
    {
        // don't do anything if the spray gun position is currently initialized by the user
        if (ApplicationController.Instance.sprayGunIsInitializing)
        {
            TrySetCurrentMode(SprayGunMode.None);
            _teleport.teleportAllowed = false;
            return;
        }

        isInPaintRange = GetClosestDrawable().sprayBox.bounds.Contains(pinSpotOrigin.position);

        // check if we need to switch to another mode in this frame
        if (currentMode == SprayGunMode.None || currentMode == SprayGunMode.Coin)
            CheckForCoinPointing();
        if (currentMode == SprayGunMode.None || currentMode == SprayGunMode.UI)
            CheckForUI();
        if (currentMode == SprayGunMode.None || currentMode == SprayGunMode.Spray)
            CheckForSpray();
        if (currentMode == SprayGunMode.None || currentMode == SprayGunMode.Hand)
            CheckForHand();
        _teleport.teleportAllowed =
            !CanSpray() && (currentMode == SprayGunMode.Teleport || currentMode == SprayGunMode.None);
        if (currentMode == SprayGunMode.None || currentMode == SprayGunMode.Teleport)
            CheckForTeleport();

        if (Time.deltaTime > 0)
            velocity = velocity * 0.4f + ((pinSpotOrigin.transform.position - _lastStartPos) / Time.deltaTime) * 0.6f;

        _lastStartPos = pinSpotOrigin.transform.position;
        _lastDirection = pinSpotOrigin.transform.rotation;

        AnimateSprayGun();
        DoFlashlightShine();
        UpdateTeleportIndicator();
    }

    protected virtual void Update()
    {
        // if this would be done in FixedUpdate it would cause flickering of the ray
        if ((currentMode == SprayGunMode.UI ||
             currentMode == SprayGunMode.Coin && !ApplicationController.Instance.currentTouchedCoin) &&
            _currentUiHit.collider != null)
            // draw ray of the spray gun points on a ui or a coin
            _lineDrawer.DrawLineInGameView(pinSpotOrigin.position, _currentUiHit.point, Color.green);

        if (!_triggerPressed && GetSprayingValue() > 0.05f)
            _triggerPressed = true;
        if (GetSprayingValue() <= 0.05f)
            _triggerPressed = false;
    }

    /// <summary>
    /// Checks if the spray gun is pointing on a UI and sets the current mode accordingly.
    /// </summary>
    private void CheckForUI()
    {
        // create ray that checks collision with all layer but SprayGun and PointBooth (they are always in the way)
        Physics.Raycast(pinSpotOrigin.position, pinSpotOrigin.forward, out _currentUiHit, Mathf.Infinity,
            LayerMask.GetMask("UI"));

        if (_currentUiHit.collider != null && !ApplicationController.Instance.currentDraggable &&
            !ApplicationController.Instance.currentTouchedCoin)
            DoUiInteraction();
        else if (currentMode == SprayGunMode.UI && GetSprayingValue() <= 0.05f)
        {
            EventSystem.current.SetSelectedGameObject(null);
            _currentSelectable = null;
            _currentScrollRect = null;
            TrySetCurrentMode(SprayGunMode.None);
        }
    }

    /// <summary>
    /// Handles selection and scrolling when the spray gun is pointing on a UI (including the virtual instructor).
    /// </summary>
    protected virtual void DoUiInteraction()
    {
        TrySetCurrentMode(SprayGunMode.UI);

        _currentSelectable = _currentUiHit.collider.gameObject.GetComponent<Selectable>();
        if (_currentSelectable != null)
        {
            _currentSelectable.Select();
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            _currentSelectable = null;
            if (_currentUiHit.collider.gameObject.name.Equals("Handle"))
            {
                _currentScrollRect = _currentUiHit.collider.gameObject.GetComponentInParent<ScrollRect>();
                _currentSelectable = _currentScrollRect.vertical
                    ? _currentScrollRect.verticalScrollbar
                    : _currentScrollRect.horizontalScrollbar;
                _currentSelectable.Select();
            }
        }

        if (_currentUiHit.collider.gameObject.name.Equals("SeekSlider"))
        {
            if (GetSprayingValue() > 0.05f)
                LearningScenariosTaskController.Instance.videoPlayerPanel.SetSlider(_currentUiHit.point);
        }

        VirtualInstructorController virtualInstructorController =
            _currentUiHit.collider.transform.parent.GetComponent<VirtualInstructorController>();
        if (virtualInstructorController && virtualInstructorController.active)
            virtualInstructorController.Highlight();

        if (GetSprayingValue() > 0.05f)
        {
            if (!_menuButtonPressed)
            {
                if (virtualInstructorController && virtualInstructorController.active)
                    virtualInstructorController.SpeakOrStop();
                else
                    PressSelectable();
                _menuButtonPressed = true;
            }
            else if (_currentScrollRect != null)
            {
                _currentScrollRect.content.anchoredPosition = GetNewScrollViewContentAnchoredPosition();
            }

            _lastHitPosition = _currentUiHit.point;
        }
        else
        {
            _currentScrollRect = null;
            _menuButtonPressed = false;
        }
    }

    /// <summary>
    /// Handles the selection of the different types of selectables.
    /// </summary>
    private void PressSelectable()
    {
        if (_currentSelectable == null || !_currentSelectable.IsInteractable())
            return;

        if (_currentSelectable is Button button)
            button.onClick.Invoke();
        else if (_currentSelectable is Toggle toggle)
            toggle.isOn = !toggle.isOn;
        else if (_currentSelectable is TMP_Dropdown dropdown)
        {
            if (dropdown.IsExpanded)
                dropdown.Hide();
            else
                dropdown.Show();
        }
    }

    /// <summary>
    /// Calculates the new anchored position of the current ScrollView content. Setting the anchored Position of
    /// ScrollView content scrolls the content to this position and automatically moves the scroll bar. We need to
    /// calculate the scroll bar length and set it into relation to the height of the content. 
    /// </summary>
    private Vector2 GetNewScrollViewContentAnchoredPosition()
    {
        if (_currentScrollRect.vertical)
        {
            Scrollbar bar = _currentScrollRect.verticalScrollbar;
            float scrollBarLength = bar.GetComponent<RectTransform>().rect.height - bar.handleRect.rect.height;
            float factor = _currentScrollRect.content.GetComponent<RectTransform>().sizeDelta.y / scrollBarLength;
            var anchoredPosition = _currentScrollRect.content.anchoredPosition;
            Vector3 relLastHitPosition = bar.transform.InverseTransformPoint(_lastHitPosition);
            Vector3 relCurrentHitPosition = bar.transform.InverseTransformPoint(_currentUiHit.point);
            return new Vector2(anchoredPosition.x,
                anchoredPosition.y + (relLastHitPosition.y - relCurrentHitPosition.y) * factor);
        }
        else
        {
            Scrollbar bar = _currentScrollRect.horizontalScrollbar;
            float scrollBarLength = bar.GetComponent<RectTransform>().rect.width - bar.handleRect.rect.width;
            float factor = _currentScrollRect.content.GetComponent<RectTransform>().sizeDelta.x / scrollBarLength;
            var anchoredPosition = _currentScrollRect.content.anchoredPosition;
            Vector3 relLastHitPosition = bar.transform.InverseTransformPoint(_lastHitPosition);
            Vector3 relCurrentHitPosition = bar.transform.InverseTransformPoint(_currentUiHit.point);
            return new Vector2(anchoredPosition.x + (relLastHitPosition.x - relCurrentHitPosition.x) * factor,
                anchoredPosition.y);
        }
    }

    /// <summary>
    /// Checks whether a draggable is touched by the spray gun and sets the current mode accordingly.
    /// </summary>
    private void CheckForHand()
    {
        if (IsDoingHandInteraction() && IsPrimarySprayGun())
            DoHandInteraction();
        else if (currentMode == SprayGunMode.Hand)
        {
            TrySetCurrentMode(SprayGunMode.None);
            _meshList.ForEach(m => m.SetMaterialsAlpha(1));
        }
    }

    /// <summary>
    /// Checks whether the paint stand or a draggable is touched by the spray gun.
    /// </summary>
    public virtual bool IsDoingHandInteraction()
    {
        return ApplicationController.Instance.currentPaintStandMovement ||
               ApplicationController.Instance.currentDraggable;
    }

    /// <summary>
    /// Handles the interaction with objects which can be modified by hand.
    /// </summary>
    private void DoHandInteraction()
    {
        TrySetCurrentMode(SprayGunMode.Hand);

        if (IsDoingHandInteraction())
        {
            HandleDraggableMovement();
            HandlePaintStandMovement();
        }
    }

    /// <summary>
    /// Sets the current mode to spraying and handles spraying if the spray gun is close to the workpiece and no other
    /// mode is active.
    /// </summary>
    private void CheckForSpray()
    {
        if ((!isDisabled || currentMode == SprayGunMode.Spray) && GetSprayingValue() > 0.05f)
            DoSpray(pinSpotOrigin.position, pinSpotOrigin.rotation);
        else if (GetSprayingValue() <= 0.05f && currentMode == SprayGunMode.Spray)
        {
            TrySetCurrentMode(SprayGunMode.None);
            StopVisuals();
            StopAudio();
        }
    }

    /// <summary>
    /// Hides the spray cone and stops the spray particle system.
    /// </summary>
    public void StopVisuals()
    {
        sprayCone.visibility = 0;
        if (spray.isPlaying)
            spray.Stop();
    }

    public void StopAudio()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
    }

    /// <summary>
    /// Handles spraying of the spray gun.
    /// </summary>
    protected virtual void DoSpray(Vector3 startPos, Quaternion direction)
    {
        // only allow the recording spray gun to spray if a recording is played
        if (PlayRecordingController.Instance.playing && GetType() != typeof(SprayGunRecorder))
            return;

        TrySetCurrentMode(SprayGunMode.Spray);

        // calculate the mod point from the position and rotation of the spray gun muzzle in the last to frames
        Vector3 moveDir = startPos - _lastStartPos;
        Vector3 centerPos = _lastStartPos + moveDir / 2;
        Quaternion centerRot = Quaternion.Lerp(_lastDirection, direction, 0.5f);

        if (scaleCookie)
        {
            // scale the cookie to avoid gabs between cookies if the spray gun moved too fast
            PaintController.Instance.wideStreamCookieShader.SetFloat(TargetHeightId,
                (int)(128 + (_originalCookie.height - 128) * GetWideStreamRegulationValue()));
            Graphics.Blit(_originalCookie, _modifiedCookie, PaintController.Instance.wideStreamCookieShader);
            PaintController.Instance.cookie = _modifiedCookie;
        }

        paintSpotDrawer.cookie = PaintController.Instance.cookie;

        // determine whether paint can flow into the spray gun
        float angle = Vector3.Angle(Vector3.down, _beakerOrientation.up);
        _upsideDown = angle < 90;
        if (_upsideDown)
            StopVisuals();

        // set the strength of the spray particles
        ParticleSystem.MainModule secMain = spray.main;
        secMain.startLifetime = Mathf.Lerp(MinSprayStartLifetime, MaxSprayStartLifetime, GetSprayingValue());
        ParticleSystem.EmissionModule secEmission = spray.emission;
        secEmission.rateOverTime = Mathf.Lerp(MinSprayEmissionRate, MaxSprayEmissionRate, GetSprayingValue());

        // set the intensity of the cookie in dependence of the amount the trigger was pressend
        float spotDrawerIntensityDiff = PaintController.Instance.maxSpotDrawerIntensity / 2;
        paintSpotDrawer.intensity =
            !_upsideDown ? spotDrawerIntensityDiff + spotDrawerIntensityDiff * GetSprayingValue() : 0;

        // adopt the size of the spray cone
        sprayCone.SetWideStream(GetWideStreamRegulationValue());
        sprayCone.visibility = !ApplicationController.Instance.showSprayCone || _upsideDown ? 0 : GetSprayingValue();

        // control the color of the spray cone
        Color sprayColor = PaintController.Instance.sprayColor;
        sprayCone.SetColor(PaintController.Instance.sprayColor);
        var secSprayMain = spray.main;
        secSprayMain.startColor =
            new Color(sprayColor.r, sprayColor.g, sprayColor.b, secSprayMain.startColor.color.a);
        var colorOverLifetime = spray.colorOverLifetime;
        var gradient = colorOverLifetime.color.gradient;
        gradient.colorKeys = new[]
        {
            new GradientColorKey(PaintController.Instance.sprayColor, 0.0f),
            new GradientColorKey(PaintController.Instance.sprayColor, 1.0f)
        };
        colorOverLifetime.color = gradient;

        // control audios
        if (spray.isStopped && ApplicationController.Instance.showSprayCone && !_upsideDown)
            spray.Play();
        if (!audioSource.isPlaying && ApplicationController.Instance.playSprayGunAudio)
            audioSource.Play();
        audioSource.volume = Mathf.Clamp(GetSprayingValue(), 0.25f, 1f);

        // set the position of the cookie drawer to the calculated center position
        paintSpotDrawer.transform.position = centerPos;
        paintSpotDrawer.transform.rotation = centerRot;

        // draw the cookie 
        _tmpPinSpotOrigin.position = centerPos;
        _tmpPinSpotOrigin.rotation = centerRot;
        Vector3 localDirection = _tmpPinSpotOrigin.InverseTransformDirection(startPos - _lastStartPos);
        Draw(localDirection);
    }


    /// <summary>
    /// Checks whether the paint gun is pointing at a coin and sets the current mode accordingly (touching a coin is
    /// handled in the coin itself).
    /// </summary>
    private void CheckForCoinPointing()
    {
        // create ray that checks collision with all layer but SprayGun and PointBooth (they are always in the way)
        Physics.Raycast(pinSpotOrigin.position, pinSpotOrigin.forward, out _currentUiHit, Mathf.Infinity,
            ~ LayerMask.GetMask("SprayGun", "PaintBooth", "Hitbox"));

        if (!ApplicationController.Instance.currentTouchedCoin && _currentUiHit.collider != null &&
            _currentUiHit.collider.gameObject.layer == LayerMask.NameToLayer("Coin"))
        {
            Coin coin = _currentUiHit.collider.GetComponent<Coin>();
            if (!coin)
                coin = _currentUiHit.collider.transform.parent.GetComponent<Coin>();
            if (ApplicationController.Instance.currentPointedCoin != null &&
                ApplicationController.Instance.currentPointedCoin != coin)
                ApplicationController.Instance.currentPointedCoin.StopInteract();
            ApplicationController.Instance.currentPointedCoin = coin;
        }
        else
        {
            if (ApplicationController.Instance.currentPointedCoin)
                ApplicationController.Instance.currentPointedCoin.StopInteract();
            ApplicationController.Instance.currentPointedCoin = null;
        }

        if (ApplicationController.Instance.CurrentCoin)
            DoCoinInteraction();

        // only return to None mode if coin is not currently pressed
        else if (currentMode == SprayGunMode.Coin && GetSprayingValue() <= 0.05f)
            TrySetCurrentMode(SprayGunMode.None);
    }

    /// <summary>
    /// Handles interaction with a coin.
    /// </summary>
    private void DoCoinInteraction()
    {
        TrySetCurrentMode(SprayGunMode.Coin);
        ApplicationController.Instance.CurrentCoin.DoInteract();
    }

    /// <summary>
    /// Changes to the teleport mode and handles teleporting if the spray gun is not close to the workpiece and no
    /// other mode is active.
    /// </summary>
    private void CheckForTeleport()
    {
        if (!CanSpray() && GetSprayingValue() > 0.05f)
            TrySetCurrentMode(SprayGunMode.Teleport);
        else if (currentMode == SprayGunMode.Teleport)
            TrySetCurrentMode(SprayGunMode.None);
    }

    private void UpdateTeleportIndicator()
    {
        if (_tank)
            _tank.material.mainTexture = _teleport.teleportAllowed ? tankTextureTeleport : tankTextureSpray;
    }

    /// <summary>
    /// Tells us whether the user would be able to spray if he presses the trigger.
    /// </summary>
    public bool CanSpray()
    {
        return currentMode == SprayGunMode.Spray || !isDisabled && currentMode == SprayGunMode.None;
    }

    public virtual float GetSprayingValue()
    {
        return (GetTriggerValue() - airTriggerValue) / (fullTriggerValue - airTriggerValue);
    }

    public float GetActualSprayingValue()
    {
        if (currentMode == SprayGunMode.Spray)
            return GetSprayingValue();
        return 0;
    }

    public void SetStartVal()
    {
        startTriggerValue = GetTriggerValue();
    }

    public void SetAirVal()
    {
        airTriggerValue = GetTriggerValue();
    }

    public void SetFullVal()
    {
        fullTriggerValue = GetTriggerValue();
    }

    protected virtual void AnimateSprayGun()
    {
        TriggerRotationPoint.localRotation =
            _initialTriggerRotation * Quaternion.Euler(0, maxTriggerRotation * GetTriggerValue(), 0);
    }


    protected virtual float GetMaterialRegulationValue()
    {
        return 1;
    }

    public virtual float GetWideStreamRegulationValue()
    {
        return 1;
    }

    protected virtual float GetAirMicrometerValue()
    {
        return 1;
    }

    public virtual float GetTriggerValue()
    {
        return _sprayAction.currentValue;
    }


    /// <summary>
    /// Handles the flashlight in the evaluation mode. Analogous to DoSpray() but without the unnecessary sound + spray
    /// animation.
    /// </summary>
    private void DoFlashlightShine()
    {
        if (ApplicationController.Instance.currentTool != Tool.Flashlight)
            return;

        paintSpotDrawer.transform.position = pinSpotOrigin.transform.position;
        paintSpotDrawer.transform.rotation = pinSpotOrigin.transform.rotation;

        FlashlightDraw();
    }

    private void HandleDraggableMovement()
    {
        if (GetSprayingValue() > 0.05f)
        {
            if (ApplicationController.Instance.currentDraggable)
            {
                ApplicationController.Instance.currentDraggable.Drag();
                currentDraggable = ApplicationController.Instance.currentDraggable;
                _meshList.ForEach(m => m.SetMaterialsAlpha(0.5f));
            }
        }
        else if (currentDraggable)
        {
            currentDraggable.Release();
            currentDraggable = null;
            _meshList.ForEach(m => m.SetMaterialsAlpha(1));
        }
    }

    protected virtual void HandlePaintStandMovement()
    {
        if (ApplicationController.Instance.currentPaintStandMovement)
        {
            if (GetSprayingValue() > 0.05f)
            {
                ApplicationController.Instance.currentPaintStandMovement.ExecuteMovement(pinSpotOrigin);
                _meshList.ForEach(m => m.SetMaterialsAlpha(0.5f));
            }
            else
            {
                ApplicationController.Instance.currentPaintStandMovement.NoMovement();
                _meshList.ForEach(m => m.SetMaterialsAlpha(1));
            }
        }
    }

    private void OnApplicationQuit()
    {
        _modifiedCookie.Release();
    }

    class SprayGunValues
    {
        public float startTriggerValue;
        public float airTriggerValue;
        public float fullTriggerValue;
        public Vector3 localPosition;
        public Quaternion localRotation;
    }

    public virtual void WriteValuesToFile()
    {
        SprayGunValues sprayGunValues = new SprayGunValues
        {
            startTriggerValue = startTriggerValue,
            airTriggerValue = airTriggerValue,
            fullTriggerValue = fullTriggerValue,
            localRotation = transform.localRotation,
            localPosition = transform.localPosition
        };
        File.WriteAllText(_controllerSprayGunValuesFile, JsonUtility.ToJson(sprayGunValues));
    }

    public virtual void ReadValuesFromFile()
    {
        if (File.Exists(_controllerSprayGunValuesFile))
        {
            SprayGunValues sprayGunValues = JsonUtility.FromJson<SprayGunValues>(
                File.ReadAllText(_controllerSprayGunValuesFile));
            startTriggerValue = sprayGunValues.startTriggerValue;
            airTriggerValue = sprayGunValues.airTriggerValue;
            fullTriggerValue = sprayGunValues.fullTriggerValue;
            transform.localRotation = sprayGunValues.localRotation;
            transform.localPosition = sprayGunValues.localPosition;
        }
    }

    /// <summary>
    /// Needed to avoid that the user spray gun interferes the interactions of the recorded spray gun (e.g. interacting
    /// with the paint stand).
    /// </summary>
    public bool IsPrimarySprayGun()
    {
        return ApplicationController.Instance.primarySprayGun.Equals(this);
    }

    public virtual void SetTool(Tool tool)
    {
        sprayGunObjects.SetActive(tool == Tool.SprayGun);
        _magnifyingGlass.SetActive(tool == Tool.MagnifyingGlass);
        _flashlight.SetActive(tool == Tool.Flashlight);
    }
}
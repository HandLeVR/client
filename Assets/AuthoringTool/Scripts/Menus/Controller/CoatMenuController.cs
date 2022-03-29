using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using translator;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Controller for the coat menu.
/// </summary>
public class CoatMenuController : BaseMenuController
{
    [Header("General")] public Transform coatList;
    public CoatTableElement CoatTableElementPrefab;
    public Button newButton;
    public Button modeButton;
    public Button resetAdvancedValuesButton;
    public Button openColorPickerButton;
    public Button closeColorPickerButton;
    public ColorPicker colorPicker;

    [Header("Coat Properties")] public TMP_InputField nameInputField;
    public TMP_InputField descriptionInputField;
    public TMP_Dropdown typeDropdown;
    public TMP_InputField colorRedInputField;
    public TMP_InputField colorGreenInputField;
    public TMP_InputField colorBlueInputField;
    public TMP_InputField costInputField;
    public TMP_InputField viscosityInputField;
    public TMP_InputField minThicknessDryInputField;
    public TMP_InputField maxThicknessDryInputField;
    public TMP_InputField roughnessInputField;
    public TMP_InputField solidVolumeInputField;
    public TMP_InputField hardenerDividendInputField;
    public TMP_InputField hardenerDivisorInputField;
    public TMP_InputField thinnerPercentageInputField;
    public TMP_InputField dryingTypeInputField;
    public TMP_InputField dryingTemperatureInputField;
    public TMP_InputField dryingTimeInputField;
    public TMP_InputField minSprayDistanceInputField;
    public TMP_InputField maxSprayDistanceInputField;
    public TMP_InputField glossWetInputField;
    public TMP_InputField glossDryInputField;
    public TMP_InputField minThicknessWetInputField;
    public TMP_InputField maxThicknessWetInputField;
    public TMP_InputField fullOpacityMinThicknessWetInputField;
    public TMP_InputField fullOpacityMinThicknessDryInputField;
    public TMP_InputField fullGlossMinThicknessWetInputField;
    public TMP_InputField fullGlossMinThicknessDryInputField;
    public TMP_InputField runsStartThicknessWetInputField;

    [Header("Coat Preview")] public GameObject coatPreviewElements;
    public GameObject coatPreviewBlocker;
    public TextMeshProUGUI currentCoatThicknessLabel;
    public TextMeshProUGUI maxCoatThicknessLabel;
    public MeshRenderer coatPreviewWorkpiece;
    public float maxNormalMapStrength = 0.3f;
    public float minViscosity = 10;
    public float maxViscosity = 40;

    private CoatTableElement _currentCoatTableElement;
    private Material _originalCoatPreviewMaterial;
    private Material _coatPreviewMaterial;
    private List<Selectable> _selectables;
    private List<Selectable> _standardSelectables;
    private List<Selectable> _advancedSelectables;
    private List<TMP_InputField> _overwrittenFields;
    private List<TMP_InputField> _inputFields;

    private bool _inNormalMode = true;
    private bool _inWetState = true;
    private Color _currentColor;

    private float _currentSliderValue = 1;

    // used to avoid updating the overwritten field more than once during a frame
    private bool _updatedThisFrame;

    // needed to avoid setting color to often and if only one color is set
    private bool _dontUpdateCoatPreview;

    private static readonly int BumpScaleID = Shader.PropertyToID("_BumpScale");
    private static readonly int SmoothnessID = Shader.PropertyToID("_Smoothness");

    private void Update()
    {
        _updatedThisFrame = false;
    }

    private void Awake()
    {
        _standardSelectables = new List<Selectable>
        {
            nameInputField, typeDropdown, descriptionInputField, costInputField, viscosityInputField,
            minThicknessDryInputField, maxThicknessDryInputField, colorRedInputField, colorGreenInputField,
            colorBlueInputField, openColorPickerButton
        };
        _advancedSelectables = new List<Selectable>
        {
            solidVolumeInputField, hardenerDividendInputField, hardenerDivisorInputField, thinnerPercentageInputField,
            dryingTypeInputField,
            dryingTemperatureInputField, dryingTimeInputField, minSprayDistanceInputField, maxSprayDistanceInputField,
            glossWetInputField, glossDryInputField, roughnessInputField, minThicknessWetInputField,
            maxThicknessWetInputField, fullOpacityMinThicknessWetInputField, fullOpacityMinThicknessDryInputField,
            fullGlossMinThicknessWetInputField, fullGlossMinThicknessDryInputField, runsStartThicknessWetInputField
        };
        _selectables = new List<Selectable> { saveButton, modeButton, resetAdvancedValuesButton };
        _selectables.AddRange(_standardSelectables);
        _selectables.AddRange(_advancedSelectables);
        _inputFields = new List<TMP_InputField>();
        foreach (var selectable in _selectables)
        {
            TMP_InputField inputField = selectable.GetComponent<TMP_InputField>();
            if (inputField)
                _inputFields.Add(inputField);
        }

        _overwrittenFields = new List<TMP_InputField>();

        foreach (TMP_InputField inputField in _inputFields)
            if (inputField != nameInputField && inputField != descriptionInputField &&
                inputField != dryingTypeInputField && inputField != dryingTimeInputField)
                inputField.onValidateInput += (text, _, addedChar) => ValidateDecimal(text, addedChar);

        newButton.onClick.AddListener(() => SetUpByCoat(null, false));
        saveButton.onClick.AddListener(SaveCoat);
        modeButton.onClick.AddListener(ToggleMode);
        resetAdvancedValuesButton.onClick.AddListener(() => OverwriteAdvancedValues(true));
        openColorPickerButton.onClick.AddListener(() =>
        {
            colorPicker.transform.parent.parent.gameObject.SetActive(true);
            colorPicker.color = GetCoatColor();
        });
        closeColorPickerButton.onClick.AddListener(
            () => colorPicker.transform.parent.parent.gameObject.SetActive(false));
        colorPicker.onColorChanged += SetColorInfo;

        _originalCoatPreviewMaterial = coatPreviewWorkpiece.material;
    }

    private void OnEnable()
    {
        _coatPreviewMaterial = new Material(_originalCoatPreviewMaterial);
        coatPreviewWorkpiece.material = _coatPreviewMaterial;
        coatList.DestroyImmediateAllChildren();
        MainScreenController.Instance.LoadData(DataController.RequestType.Coats, InstantiateContainer);
        coatPreviewElements.SetActive(true);
    }

    private void OnDisable()
    {
        if (coatPreviewElements)
            coatPreviewElements.SetActive(false);
    }

    private void Start()
    {
        // initialize all input fields with listeners which validate the input on changes
        nameInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(nameInputField, Le(3, nameInputField.text.Length) && Le(nameInputField.text.Length, 40));
        });
        SetCoatTypeDropdown();
        descriptionInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(descriptionInputField, Le(descriptionInputField.text.Length, 200));
        });
        descriptionInputField.textComponent.enableWordWrapping = true;
        typeDropdown.onValueChanged.AddListener(_ => { SetUnsavedChanges(true); });
        colorRedInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(colorRedInputField, Le(0, colorRedInputField) && Le(colorRedInputField, 255));
            SetColor();
        });
        colorGreenInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(colorGreenInputField, Le(0, colorGreenInputField) && Le(colorGreenInputField, 255));
            SetColor();
        });
        colorBlueInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(colorBlueInputField, Le(0, colorBlueInputField) && Le(colorBlueInputField, 255));
            SetColor();
        });
        costInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(costInputField, Le(0, costInputField) && Le(costInputField, 1000));
        });
        solidVolumeInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(solidVolumeInputField, Le(0, solidVolumeInputField) && Le(solidVolumeInputField, 100));
            _overwrittenFields.Add(solidVolumeInputField);
            OverwriteAdvancedValues(false);
            CollectOverwrittenInputFields();
        });
        roughnessInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(roughnessInputField, Le(0, roughnessInputField) && Le(roughnessInputField, 100));
            UpdateCoatPreview();
            CollectOverwrittenInputFields();
        });
        viscosityInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(viscosityInputField, Le(1, viscosityInputField) && Le(viscosityInputField, 100));
            UpdateCoatPreview();
        });
        hardenerDividendInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(hardenerDividendInputField,
                Le(1, hardenerDividendInputField) && Le(hardenerDividendInputField, 10));
            CollectOverwrittenInputFields();
        });
        hardenerDivisorInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(hardenerDivisorInputField,
                Le(1, hardenerDivisorInputField) && Le(hardenerDivisorInputField, 10));
            CollectOverwrittenInputFields();
        });
        thinnerPercentageInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(thinnerPercentageInputField,
                Le(1, thinnerPercentageInputField) && Le(thinnerPercentageInputField, 100));
            CollectOverwrittenInputFields();
        });
        dryingTypeInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(dryingTypeInputField,
                Le(3, dryingTypeInputField.text.Length) && Le(dryingTypeInputField.text.Length, 40));
            CollectOverwrittenInputFields();
        });
        dryingTemperatureInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(dryingTemperatureInputField,
                Le(0, dryingTemperatureInputField) && Le(dryingTemperatureInputField, 500));
            CollectOverwrittenInputFields();
        });
        dryingTimeInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(dryingTimeInputField, Le(0, dryingTimeInputField) && Le(dryingTimeInputField, 1000));
            CollectOverwrittenInputFields();
        });
        minSprayDistanceInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(minSprayDistanceInputField,
                Le(10, minSprayDistanceInputField) && Le(minSprayDistanceInputField, 30));
            maxSprayDistanceInputField.onValueChanged.Invoke(maxSprayDistanceInputField.text);
            CollectOverwrittenInputFields();
        });
        maxSprayDistanceInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(maxSprayDistanceInputField,
                Le(10, maxSprayDistanceInputField) && Le(maxSprayDistanceInputField, 30) &&
                Le(minSprayDistanceInputField, maxSprayDistanceInputField));
            CollectOverwrittenInputFields();
        });
        glossWetInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(glossWetInputField, Le(1, glossWetInputField) && Le(glossWetInputField, 100));
            glossDryInputField.onValueChanged.Invoke(glossDryInputField.text);
            _overwrittenFields.Add(glossWetInputField);
            OverwriteAdvancedValues(false);
            UpdateCoatPreview();
            CollectOverwrittenInputFields();
        });
        glossDryInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(glossDryInputField,
                Le(0, glossDryInputField) && Le(glossDryInputField, 100) && Le(glossDryInputField, glossWetInputField));
            UpdateCoatPreview();
            CollectOverwrittenInputFields();
        });
        minThicknessWetInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(minThicknessWetInputField,
                Le(1, minThicknessWetInputField) && Le(minThicknessWetInputField, 300) &&
                Le(minThicknessWetInputField, maxThicknessWetInputField));
            _overwrittenFields.Add(minThicknessWetInputField);
            minThicknessDryInputField.onValueChanged.Invoke(minThicknessDryInputField.text);
            fullOpacityMinThicknessWetInputField.onValueChanged.Invoke(fullOpacityMinThicknessWetInputField.text);
            OverwriteAdvancedValues(false);
            CollectOverwrittenInputFields();
        });
        minThicknessDryInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(minThicknessDryInputField,
                Le(1, minThicknessDryInputField) && Le(minThicknessDryInputField, 300) &&
                Le(minThicknessDryInputField, minThicknessWetInputField) &&
                Le(minThicknessDryInputField, maxThicknessDryInputField));
            fullOpacityMinThicknessDryInputField.onValueChanged.Invoke(fullOpacityMinThicknessDryInputField.text);
            fullGlossMinThicknessDryInputField.onValueChanged.Invoke(fullGlossMinThicknessDryInputField.text);
            OverwriteAdvancedValues(false);
            CollectOverwrittenInputFields();
        });
        maxThicknessWetInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(maxThicknessWetInputField,
                Le(1, maxThicknessWetInputField) && Le(maxThicknessWetInputField, 300));
            _overwrittenFields.Add(maxThicknessWetInputField);
            maxThicknessDryInputField.onValueChanged.Invoke(maxThicknessDryInputField.text);
            minThicknessWetInputField.onValueChanged.Invoke(minThicknessWetInputField.text);
            runsStartThicknessWetInputField.onValueChanged.Invoke(runsStartThicknessWetInputField.text);
            OverwriteAdvancedValues(false);
            UpdateCoatPreview();
            CollectOverwrittenInputFields();
        });
        maxThicknessDryInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(maxThicknessDryInputField,
                Le(1, maxThicknessDryInputField) && Le(maxThicknessDryInputField, 300) &&
                Le(maxThicknessDryInputField, maxThicknessWetInputField));
            minThicknessDryInputField.onValueChanged.Invoke(minThicknessDryInputField.text);
            OverwriteAdvancedValues(false);
            UpdateCoatPreview();
            CollectOverwrittenInputFields();
        });
        fullOpacityMinThicknessWetInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(fullOpacityMinThicknessWetInputField,
                Le(1, fullOpacityMinThicknessWetInputField) && Le(fullOpacityMinThicknessWetInputField, 300) &&
                Le(fullOpacityMinThicknessWetInputField, minThicknessWetInputField));
            fullOpacityMinThicknessDryInputField.onValueChanged.Invoke(fullOpacityMinThicknessDryInputField.text);
            UpdateCoatPreview();
            CollectOverwrittenInputFields();
        });
        fullOpacityMinThicknessDryInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(fullOpacityMinThicknessDryInputField,
                Le(1, fullOpacityMinThicknessDryInputField) && Le(fullOpacityMinThicknessDryInputField, 300) &&
                Le(fullOpacityMinThicknessDryInputField, minThicknessDryInputField) &&
                Le(fullOpacityMinThicknessDryInputField, fullOpacityMinThicknessWetInputField));
            UpdateCoatPreview();
            CollectOverwrittenInputFields();
        });
        fullGlossMinThicknessWetInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(fullGlossMinThicknessWetInputField,
                Le(1, fullGlossMinThicknessWetInputField) && Le(fullGlossMinThicknessWetInputField, 300) &&
                Le(fullGlossMinThicknessWetInputField, minThicknessWetInputField));
            fullGlossMinThicknessDryInputField.onValueChanged.Invoke(fullGlossMinThicknessDryInputField.text);
            UpdateCoatPreview();
            CollectOverwrittenInputFields();
        });
        fullGlossMinThicknessDryInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(fullGlossMinThicknessDryInputField,
                Le(1, fullGlossMinThicknessDryInputField) && Le(fullGlossMinThicknessDryInputField, 300) &&
                Le(fullGlossMinThicknessDryInputField, minThicknessDryInputField) &&
                Le(fullGlossMinThicknessDryInputField, fullGlossMinThicknessWetInputField));
            UpdateCoatPreview();
            CollectOverwrittenInputFields();
        });
        runsStartThicknessWetInputField.onValueChanged.AddListener(_ =>
        {
            ValidateInput(runsStartThicknessWetInputField,
                Le(1, runsStartThicknessWetInputField) && Le(runsStartThicknessWetInputField, 300) &&
                Le(maxThicknessWetInputField, runsStartThicknessWetInputField));
            CollectOverwrittenInputFields();
        });
    }

    /// <summary>
    /// Creates the list of available coats.
    /// </summary>
    private void InstantiateContainer()
    {
        coatList.DestroyImmediateAllChildren();
        foreach (var coat in DataController.Instance.coats.Values)
            AddContainer(coat);
        StartCoroutine(ResetFieldsCoroutine());
        SortCoats();
    }

    /// <summary>
    /// Adds an entry to the list on base of the given coat.
    /// </summary>
    private CoatTableElement AddContainer(Coat coat)
    {
        CoatTableElement container = Instantiate(CoatTableElementPrefab, coatList);
        container.Init(coat, SetUpByCoat, DeleteCoat);
        return container;
    }

    /// <summary>
    /// Deletes a coat on the server.
    /// </summary>
    private void DeleteCoat(CoatTableElement container, bool confirmed)
    {
        if (!confirmed)
        {
            PopupScreenHandler.Instance.ShowConfirmation("popup-remove-coat", "popup-remove-coat-confirm",
                () => DeleteCoat(container, true));
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-remove-coat", "popup-removing-coat");
        RestConnector.Delete(container.coat, "/coats/" + container.coat.id, () =>
            {
                if (_currentCoatTableElement != null &&
                    _currentCoatTableElement.coat.id == container.coat.id)
                    ResetFields();
                Destroy(container.gameObject);
                PopupScreenHandler.Instance.ShowMessage("popup-remove-coat", "popup-removed-coat");
            },
            conflict =>
            {
                PopupScreenHandler.Instance.ShowMessage("popup-saving-error",
                    conflict == "RECORDING" ? "popup-coat-usage-recording" : "popup-coat-usage-task");
            }, PopupScreenHandler.Instance.ShowConnectionError);
    }

    /// <summary>
    /// Checks whether the number in input field a is less than the number in input field b. False is returned
    /// if there is no number in one of the input fields.
    /// </summary>
    private bool Le(TMP_InputField a, TMP_InputField b)
    {
        try
        {
            float aFloat = float.Parse(a.text);
            float bFloat = float.Parse(b.text);
            return aFloat <= bFloat;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks whether the number a is less than the number in input field b. False is returned if there is no
    /// number the input field.
    /// </summary>
    private bool Le(float a, TMP_InputField b)
    {
        try
        {
            float bFloat = float.Parse(b.text);
            return a <= bFloat;
        }
        catch (FormatException)
        {
            return false;
        }
    }


    /// <summary>
    /// Checks whether the number in input field a is less than the number b. False is returned if there is no
    /// number the input field.
    /// </summary>
    private bool Le(TMP_InputField a, float b)
    {
        try
        {
            float aFloat = float.Parse(a.text);
            return aFloat <= b;
        }
        catch (FormatException)
        {
            return false;
        }
    }
    
    /// <summary>
    /// Checks whether a is less than b. 
    /// </summary>
    private bool Le(float a, float b)
    {
        return a <= b;
    }

    /// <summary>
    /// Changes the visual appearance of the input field in dependence of the given bool.
    /// </summary>
    private void ValidateInput(TMP_InputField inputField, bool validInput)
    {
        var inputFieldColors = inputField.colors;
        inputFieldColors.normalColor = validInput ? normalColor : warningColor;
        inputField.colors = inputFieldColors;
        SetUnsavedChanges(true);
    }

    /// <summary>
    /// Activates the normal or advanced mode. In the advanced mode more input fields are available.
    /// </summary>
    private void ToggleMode(bool toNormalMode)
    {
        _inNormalMode = toNormalMode;
        _advancedSelectables.ForEach(selectable => selectable.interactable = !toNormalMode);
        _standardSelectables.ForEach(selectable => selectable.interactable = true);
        modeButton.interactable = true;
        resetAdvancedValuesButton.interactable = true;
        modeButton.GetComponentInChildren<TMP_Text>().text =
            TranslationController.Instance.Translate(toNormalMode ? "coat-advanced-mode" : "coat-normal-mode");
    }

    /// <summary>
    /// Toggles the normal or advanced mode.
    /// </summary>
    private void ToggleMode()
    {
        ToggleMode(!_inNormalMode);
    }

    /// <summary>
    /// Initializes all input fields on the right side of the menu by the given coat element.
    /// </summary>
    private void SetUpByCoat(CoatTableElement coatTableElement, bool confirmed)
    {
        if (HasUnsavedChanges() && !confirmed)
        {
            PopupScreenHandler.Instance.ShowUnsavedChanges(() => SetUpByCoat(coatTableElement, true));
            return;
        }

        _currentCoatTableElement = coatTableElement;
        Coat currentCoat = coatTableElement != null
            ? coatTableElement.coat
            : DataController.Instance.defaultCoat;
        _dontUpdateCoatPreview = true;
        nameInputField.text = currentCoat.name;
        typeDropdown.value = typeDropdown.options.FindIndex(option =>
            option.text == TranslationController.Instance.Translate(currentCoat.type.ToString()));
        descriptionInputField.text = currentCoat.description;
        SetColorInfo(currentCoat.color);
        costInputField.text = currentCoat.costs.ToString("0.##");
        solidVolumeInputField.text = currentCoat.solidVolume.ToString("0.##");
        roughnessInputField.text = currentCoat.roughness.ToString("0.##");
        viscosityInputField.text = currentCoat.viscosity.ToString("0.##");
        GetHardenerRatioByString(currentCoat.hardenerMixRatio);
        thinnerPercentageInputField.text = currentCoat.thinnerPercentage.ToString("0.##");
        dryingTypeInputField.text = currentCoat.dryingType;
        dryingTemperatureInputField.text = currentCoat.dryingTemperature.ToString("0.##");
        dryingTimeInputField.text = currentCoat.dryingTime.ToString();
        minSprayDistanceInputField.text = currentCoat.minSprayDistance.ToString("0.##");
        maxSprayDistanceInputField.text = currentCoat.maxSprayDistance.ToString("0.##");
        glossWetInputField.text = currentCoat.glossWet.ToString("0.##");
        glossDryInputField.text = currentCoat.glossDry.ToString("0.##");
        minThicknessWetInputField.text = currentCoat.targetMinThicknessWet.ToString("0.##");
        minThicknessDryInputField.text = currentCoat.targetMinThicknessDry.ToString("0.##");
        maxThicknessWetInputField.text = currentCoat.targetMaxThicknessWet.ToString("0.##");
        maxThicknessDryInputField.text = currentCoat.targetMaxThicknessDry.ToString("0.##");
        fullOpacityMinThicknessWetInputField.text = currentCoat.fullOpacityMinThicknessWet.ToString("0.##");
        fullOpacityMinThicknessDryInputField.text = currentCoat.fullOpacityMinThicknessDry.ToString("0.##");
        fullGlossMinThicknessWetInputField.text = currentCoat.fullGlossMinThicknessWet.ToString("0.##");
        fullGlossMinThicknessDryInputField.text = currentCoat.fullGlossMinThicknessDry.ToString("0.##");
        runsStartThicknessWetInputField.text = currentCoat.runsStartThicknessWet.ToString("0.##");
        _dontUpdateCoatPreview = false;
        SetColor();

        // solve TMP bug where the text is not aligned correctly sometimes
        foreach (Selectable selectable in _selectables)
        {
            if (selectable.TryGetComponent(out TMP_InputField inputField))
                inputField.textComponent.alignment = TextAlignmentOptions.Left;
            else if (selectable.TryGetComponent(out TMP_Dropdown dropdown))
                dropdown.captionText.alignment = TextAlignmentOptions.Left;
        }

        ToggleMode(true);
        SetUnsavedChanges(false);

        _overwrittenFields = new List<TMP_InputField>();
        if (_currentCoatTableElement == null)
            OverwriteAdvancedValues(true);
        else
            CollectOverwrittenInputFields();
    }

    /// <summary>
    /// Coroutine is needed to solve formatting bug in TMP text elements.
    /// </summary>
    private IEnumerator ResetFieldsCoroutine()
    {
        yield return new WaitForEndOfFrame();
        ResetFields();
    }

    /// <summary>
    /// Resets the values of the input fields to the default coat values.
    /// </summary>
    private void ResetFields()
    {
        SetUpByCoat(null, true);
        _selectables.ForEach(selectable => selectable.interactable = false);
        SetUnsavedChanges(false);
    }

    /// <summary>
    /// Resets the values of the input fields which are available in the advanced mode. The boolean determines whether
    /// input fields which are overwritten by the user should be reset too.
    /// </summary>
    private void OverwriteAdvancedValues(bool force)
    {
        if (_dontUpdateCoatPreview)
            return;

        try
        {
            CoatType type = Enum.GetValues(typeof(CoatType)).Cast<CoatType>().ToList()[typeDropdown.value];
            Coat coat = DataController.Instance.defaultCoat;
            SetValue(roughnessInputField, coat.roughness, force);
            SetValue(solidVolumeInputField, coat.solidVolume, force);
            GetHardenerRatioByString(coat.hardenerMixRatio);
            SetValue(thinnerPercentageInputField, coat.thinnerPercentage, force);
            if (force || !_overwrittenFields.Contains(dryingTypeInputField))
                dryingTypeInputField.text = coat.dryingType;
            SetValue(dryingTemperatureInputField, coat.dryingTemperature, force);
            SetValue(dryingTimeInputField, coat.dryingTime, force);
            SetValue(minSprayDistanceInputField, coat.minSprayDistance, force);
            SetValue(maxSprayDistanceInputField, coat.maxSprayDistance, force);
            SetValue(glossWetInputField, coat.glossWet, force);
            if (type == CoatType.Clearcoat || type == CoatType.Topcoat)
                SetValue(glossDryInputField, coat.glossWet / 100 * 97.5f, force);
            else
                SetValue(glossDryInputField, coat.glossWet / 100 * 50, force);
            float minThicknessDry = float.Parse(minThicknessDryInputField.text);
            float solidVolume = float.Parse(solidVolumeInputField.text);
            SetValue(minThicknessWetInputField, minThicknessDry * 100 / solidVolume, force);
            float minThicknessWet = float.Parse(minThicknessWetInputField.text);
            float maxThicknessDry = float.Parse(maxThicknessDryInputField.text);
            SetValue(maxThicknessWetInputField, minThicknessWet + (maxThicknessDry - minThicknessDry), force);
            SetValue(fullOpacityMinThicknessWetInputField, minThicknessWet, force);
            SetValue(fullOpacityMinThicknessDryInputField, minThicknessDry, force);
            SetValue(fullGlossMinThicknessWetInputField, minThicknessWet, force);
            SetValue(fullGlossMinThicknessDryInputField, minThicknessDry, force);
            float maxThicknessWet = float.Parse(maxThicknessWetInputField.text);
            SetValue(runsStartThicknessWetInputField, maxThicknessWet / 100 * 110, force);
        }
        catch (FormatException)
        {
        }
    }

    /// <summary>
    /// Collects all input fields which are overwritten by the user. They are needed to avoid that the are
    /// automatically derived from values they depend on.
    /// </summary>
    private void CollectOverwrittenInputFields()
    {
        if (_dontUpdateCoatPreview || _updatedThisFrame)
            return;
        _updatedThisFrame = true;
        // do it at the end of the current frame to ensure that all changes to input field values are done
        StartCoroutine(GetOverwrittenInputFieldsCoroutine());
    }

    private IEnumerator GetOverwrittenInputFieldsCoroutine()
    {
        yield return new WaitForEndOfFrame();

        _overwrittenFields = new List<TMP_InputField>();
        CoatType type = Enum.GetValues(typeof(CoatType)).Cast<CoatType>().ToList()[typeDropdown.value];
        Coat coat = DataController.Instance.defaultCoat;
        float roughness = ParseValue(roughnessInputField.text);
        if (Math.Abs(coat.roughness - roughness) > 0.01f)
            _overwrittenFields.Add(roughnessInputField);
        float solidVolume = ParseValue(solidVolumeInputField.text);
        if (Math.Abs(coat.solidVolume - solidVolume) > 0.01f)
            _overwrittenFields.Add(solidVolumeInputField);
        if ((hardenerDividendInputField.text + "/" + hardenerDivisorInputField.text) != coat.hardenerMixRatio)
        {
            _overwrittenFields.Add(hardenerDividendInputField);
            _overwrittenFields.Add(hardenerDivisorInputField);
        }

        float thinnerPercentage = ParseValue(thinnerPercentageInputField.text);
        if (Math.Abs(coat.thinnerPercentage - thinnerPercentage) > 0.01f)
            _overwrittenFields.Add(thinnerPercentageInputField);
        if (coat.dryingType != dryingTypeInputField.text)
            _overwrittenFields.Add(dryingTypeInputField);
        float dryingTemperature = ParseValue(dryingTemperatureInputField.text);
        if (Math.Abs(coat.dryingTemperature - dryingTemperature) > 0.01f)
            _overwrittenFields.Add(dryingTemperatureInputField);
        float dryingTime = ParseValue(dryingTimeInputField.text);
        if (Math.Abs(coat.dryingTime - dryingTime) > 0.01f)
            _overwrittenFields.Add(dryingTimeInputField);
        float minSprayDistance = ParseValue(minSprayDistanceInputField.text);
        if (Math.Abs(coat.minSprayDistance - minSprayDistance) > 0.01f)
            _overwrittenFields.Add(minSprayDistanceInputField);
        float maxSprayDistance = ParseValue(maxSprayDistanceInputField.text);
        if (Math.Abs(coat.maxSprayDistance - maxSprayDistance) > 0.01f)
            _overwrittenFields.Add(maxSprayDistanceInputField);
        float glossWet = ParseValue(glossWetInputField.text);
        if (Math.Abs(coat.glossWet - glossWet) > 0.01f)
            _overwrittenFields.Add(glossWetInputField);
        float glossDry = ParseValue(glossDryInputField.text);
        if ((type == CoatType.Clearcoat || type == CoatType.Topcoat) &&
            Math.Abs(glossWet / 100 * 97.5f - glossDry) > 0.01f)
            _overwrittenFields.Add(glossDryInputField);
        else if ((type == CoatType.Basecoat || type == CoatType.Primer) &&
                 Math.Abs(glossWet / 100 * 50 - glossDry) > 0.01f)
            _overwrittenFields.Add(glossDryInputField);
        float minThicknessDry = ParseValue(minThicknessDryInputField.text);
        float minThicknessWet = ParseValue(minThicknessWetInputField.text);
        if (Math.Abs(minThicknessDry * 100 / solidVolume - minThicknessWet) > 0.01f)
            _overwrittenFields.Add(minThicknessWetInputField);
        float maxThicknessDry = ParseValue(maxThicknessDryInputField.text);
        float maxThicknessWet = ParseValue(maxThicknessWetInputField.text);
        if (Math.Abs((minThicknessWet + (maxThicknessDry - minThicknessDry)) - maxThicknessWet) > 0.01f)
            _overwrittenFields.Add(maxThicknessWetInputField);
        float fullOpacityMinThicknessWet = ParseValue(fullOpacityMinThicknessWetInputField.text);
        if (Math.Abs(minThicknessWet - fullOpacityMinThicknessWet) > 0.01f)
            _overwrittenFields.Add(fullOpacityMinThicknessWetInputField);
        float fullOpacityMinThicknessDry = ParseValue(fullOpacityMinThicknessDryInputField.text);
        if (Math.Abs(minThicknessDry - fullOpacityMinThicknessDry) > 0.01f)
            _overwrittenFields.Add(fullOpacityMinThicknessDryInputField);
        float fullGlossMinThicknessWet = ParseValue(fullGlossMinThicknessWetInputField.text);
        if (Math.Abs(minThicknessWet - fullGlossMinThicknessWet) > 0.01f)
            _overwrittenFields.Add(fullGlossMinThicknessWetInputField);
        float fullGlossMinThicknessDry = ParseValue(fullGlossMinThicknessDryInputField.text);
        if (Math.Abs(minThicknessDry - fullGlossMinThicknessDry) > 0.01f)
            _overwrittenFields.Add(fullGlossMinThicknessDryInputField);
    }

    private float ParseValue(string text)
    {
        return float.TryParse(text, out float result) ? result : 0;
    }

    /// <summary>
    /// Sets the value of an input field but only if it is not overwritten or if it is forced.
    /// </summary>
    private void SetValue(TMP_InputField inputField, float value, bool force = false)
    {
        if (force || !_overwrittenFields.Contains(inputField))
            inputField.text = value.ToString("0.##");
    }

    private void SetColorInfo(Color color)
    {
        colorRedInputField.text = ((int)(color.r * 255.0f)).ToString(CultureInfo.InvariantCulture);
        colorGreenInputField.text = ((int)(color.g * 255.0f)).ToString(CultureInfo.InvariantCulture);
        colorBlueInputField.text = ((int)(color.b * 255.0f)).ToString(CultureInfo.InvariantCulture);
        SetColor();
        UpdateCoatPreview();
    }

    private void SetColor()
    {
        if (_dontUpdateCoatPreview || FieldInvalid())
            return;
        _currentColor = GetCoatColor();
        UpdateCoatPreview();
    }

    private bool FieldInvalid()
    {
        foreach (TMP_InputField inputField in _inputFields)
        {
            if (inputField.colors.normalColor != normalColor)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Updates the coat preview in dependence of the coat settings.
    /// </summary>
    private void UpdateCoatPreview()
    {
        if (_dontUpdateCoatPreview)
            return;

        if (FieldInvalid())
        {
            coatPreviewBlocker.SetActive(true);
            return;
        }

        coatPreviewBlocker.SetActive(false);

        float targetMaxThickness = _inWetState
            ? float.Parse(maxThicknessWetInputField.text)
            : float.Parse(maxThicknessDryInputField.text);

        currentCoatThicknessLabel.text = (targetMaxThickness * _currentSliderValue).ToString("0");
        maxCoatThicknessLabel.text = targetMaxThickness.ToString("0");

        float viscosity = float.Parse(viscosityInputField.text);
        // taken from PaintController of the vr application
        viscosity = Mathf.Sqrt(Mathf.Clamp(viscosity, minViscosity, maxViscosity) - minViscosity) /
                    Mathf.Sqrt(maxViscosity - minViscosity);
        float roughness = float.Parse(roughnessInputField.text);
        float fullOpacityMinThickness = _inWetState
            ? float.Parse(fullOpacityMinThicknessWetInputField.text)
            : float.Parse(fullOpacityMinThicknessDryInputField.text);
        float fullOpacityValue = Mathf.Clamp01(targetMaxThickness / fullOpacityMinThickness * _currentSliderValue);
        _coatPreviewMaterial.SetFloat(BumpScaleID,
            maxNormalMapStrength * (roughness / 100) * viscosity * fullOpacityValue);
        _coatPreviewMaterial.color = Color.Lerp(Color.white, _currentColor, fullOpacityValue);

        float gloss = _inWetState ? float.Parse(glossWetInputField.text) : float.Parse(glossDryInputField.text);
        float fullGlossMinThickness = _inWetState
            ? float.Parse(fullGlossMinThicknessWetInputField.text)
            : float.Parse(fullGlossMinThicknessDryInputField.text);
        float fullGlossValue = Mathf.Clamp01(targetMaxThickness / fullGlossMinThickness * _currentSliderValue);
        _coatPreviewMaterial.SetFloat(SmoothnessID, gloss / 100 * fullGlossValue);
    }

    private void SetCoatTypeDropdown()
    {
        typeDropdown.options.Clear();
        foreach (CoatType type in Enum.GetValues(typeof(CoatType)))
            typeDropdown.options.Add(
                new TMP_Dropdown.OptionData(TranslationController.Instance.Translate(type.ToString())));
    }

    private void GetHardenerRatioByString(string ratio)
    {
        hardenerDividendInputField.text = ratio.Split('/')[0];
        hardenerDivisorInputField.text = ratio.Split('/')[1];
    }

    /// <summary>
    /// Saves the coat on the server
    /// </summary>
    private void SaveCoat()
    {
        if (_selectables.Exists(selectable => selectable.colors.normalColor != normalColor))
        {
            PopupScreenHandler.Instance.ShowMessage("popup-save-coat", "popup-wrong-values");
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen("popup-save-coat", "popup-saving-coat");
        Coat coat = _currentCoatTableElement != null
            ? _currentCoatTableElement.coat
            : new Coat { id = -1 };

        coat.name = nameInputField.text;
        coat.type = Enum.GetValues(typeof(CoatType)).Cast<CoatType>().ToList()[typeDropdown.value];
        coat.description = descriptionInputField.text;
        coat.costs = float.Parse(costInputField.text);
        coat.solidVolume = float.Parse(solidVolumeInputField.text);
        coat.roughness = float.Parse(roughnessInputField.text);
        coat.viscosity = float.Parse(viscosityInputField.text);
        coat.hardenerMixRatio = hardenerDividendInputField.text + "/" + hardenerDivisorInputField.text;
        coat.thinnerPercentage = float.Parse(thinnerPercentageInputField.text);
        coat.dryingType = dryingTypeInputField.text;
        coat.dryingTemperature = float.Parse(dryingTemperatureInputField.text);
        coat.dryingTime = int.Parse(dryingTimeInputField.text);
        coat.minSprayDistance = float.Parse(minSprayDistanceInputField.text);
        coat.maxSprayDistance = float.Parse(maxSprayDistanceInputField.text);
        coat.glossWet = float.Parse(glossWetInputField.text);
        coat.glossDry = float.Parse(glossDryInputField.text);
        coat.fullGlossMinThicknessWet = float.Parse(fullGlossMinThicknessWetInputField.text);
        coat.fullGlossMinThicknessDry = float.Parse(fullGlossMinThicknessDryInputField.text);
        coat.targetMinThicknessWet = float.Parse(minThicknessWetInputField.text);
        coat.targetMinThicknessDry = float.Parse(minThicknessDryInputField.text);
        coat.targetMaxThicknessWet = float.Parse(maxThicknessWetInputField.text);
        coat.targetMaxThicknessDry = float.Parse(maxThicknessDryInputField.text);
        coat.fullOpacityMinThicknessWet = float.Parse(fullOpacityMinThicknessWetInputField.text);
        coat.fullOpacityMinThicknessDry = float.Parse(fullOpacityMinThicknessDryInputField.text);
        coat.runsStartThicknessWet = float.Parse(runsStartThicknessWetInputField.text);
        coat.color = GetCoatColor();


        RestConnector.Update(coat, coat.id < 0 ? UnityWebRequest.kHttpVerbPOST : UnityWebRequest.kHttpVerbPUT,
            "/coats",
            newCoat =>
            {
                if (_currentCoatTableElement == null)
                    _currentCoatTableElement = AddContainer(newCoat);
                else
                    _currentCoatTableElement.Init(newCoat, SetUpByCoat, DeleteCoat);

                _currentCoatTableElement.containerButton.Select();
                SortCoats();
                SetUnsavedChanges(false);
                PopupScreenHandler.Instance.ShowMessage("popup-save-coat", "popup-saved-coat");
            }, PopupScreenHandler.Instance.ShowConnectionError,
            conflict => PopupScreenHandler.Instance.ShowMessage("popup-saving-error",
                conflict == "NAME" ? "popup-coat-duplicate-name" : "popup-coat-usage-recording")
        );
    }

    private Color GetCoatColor()
    {
        return new Color(int.Parse(colorRedInputField.text) / 255f, int.Parse(colorGreenInputField.text) / 255f,
            int.Parse(colorBlueInputField.text) / 255f, 1);
    }

    public void SetCoatThicknessSlider(float value)
    {
        _currentSliderValue = value;
        UpdateCoatPreview();
    }

    public void SetCurrentState(bool wetState)
    {
        _inWetState = wetState;
        UpdateCoatPreview();
    }

    private void SortCoats()
    {
        SortListElements<CoatTableElement>(coatList,
            (e1, e2) => String.Compare(e1.coat.name, e2.coat.name, StringComparison.CurrentCultureIgnoreCase));
    }

    private char ValidateDecimal(string text, char charToValidate)
    {
        if (Char.IsDigit(charToValidate))
            return charToValidate;
        if (charToValidate == ',' && !text.Contains(','))
            return charToValidate;
        return '\0';
    }
}
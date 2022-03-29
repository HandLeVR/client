using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using translator;
using UnityEngine;
using UnityEngine.Events;
using Button = UnityEngine.UI.Button;

/// <summary>
/// Realizes a popup which allows to select an element. The type of the element is determined by the input field
/// which is clicked to open the selection popup.
/// </summary>
public class SelectionPopup : Singleton<SelectionPopup>
{
    public SelectionElement selectionElementPrefab;
    public Transform ContentContainer;
    public TextMeshProUGUI headingTMP;
    public TMP_InputField searchInputField;

    public AudioSelectionPreview audioPreviewPanel;
    public ImageSelectionPreview imagePreviewPanel;
    public VideoSelectionPreview videoPreviewPanel;
    public TaskSelectionPreview taskPreviewPanel;
    public RecordingSelectionPreview recordingPreviewPanel;
    public CoatSelectionPreview coatPreviewPanel;
    public TaskCollectionSelectionPreview taskCollectionPreviewPanel;

    public Button selectButton;
    public Button closeButton;

    private TextMeshProUGUI _selectButtonLabel;
    private List<SelectionElement> _elementObjects;
    private List<long> _selectedIds;
    private UnityAction _onAdd;
    private bool _isMultiSelect;

    private void Awake()
    {
        _selectButtonLabel = selectButton.GetComponentInChildren<TextMeshProUGUI>();
        searchInputField.onValueChanged.AddListener(value =>
            _elementObjects.ForEach(element => element.Filter(value.ToLower())));
        selectButton.onClick.AddListener(ChooseElement);
    }

    /// <summary>
    /// Opens the selection panel and generates the list of selectable elements in dependence of the given type.
    /// </summary>
    public void Init<T>(T t, UnityAction onAdd, List<Tuple<string, long, UnityAction>> initialElementTuples = null,
        string selectButtonLabel = "selection-popup-button-select")
    {
        gameObject.SetActive(true);
        _selectButtonLabel.text = TranslationController.Instance.Translate(selectButtonLabel);
        _onAdd = onAdd;
        _isMultiSelect = t.Equals(typeof(TaskCollection)) || t.Equals(typeof(Task));
        List<Tuple<string, long, UnityAction>> elementTuples =
            initialElementTuples ?? new List<Tuple<string, long, UnityAction>>();
        if (initialElementTuples == null)
        {
            _elementObjects = new List<SelectionElement>();
            ContentContainer.DestroyAllChildren();
        }

        if (t.Equals(Media.MediaType.Audio))
        {
            headingTMP.text = TranslationController.Instance.Translate("selection-popup-audio");
            CreateRemoveSelectionElement();
            foreach (var pair in DataController.Instance.availableAudios)
                elementTuples.Add(new Tuple<string, long, UnityAction>(pair.Value.name, pair.Key,
                    () => ShowMediaPreview(pair.Value)));
        }
        else if (t.Equals(Media.MediaType.Image))
        {
            headingTMP.text = TranslationController.Instance.Translate("selection-popup-image");
            CreateRemoveSelectionElement();
            foreach (var pair in DataController.Instance.availableImages)
                elementTuples.Add(new Tuple<string, long, UnityAction>(pair.Value.name, pair.Key,
                    () => ShowMediaPreview(pair.Value)));
        }
        else if (t.Equals(Media.MediaType.Video))
        {
            headingTMP.text = TranslationController.Instance.Translate("selection-popup-video");
            CreateRemoveSelectionElement();
            foreach (var pair in DataController.Instance.availableVideos)
                elementTuples.Add(new Tuple<string, long, UnityAction>(pair.Value.name, pair.Key,
                    () => ShowMediaPreview(pair.Value)));
        }
        else if (t.Equals(typeof(Task)))
        {
            taskPreviewPanel.SetDeadlineSelectionActive(true);
            DeadlinePicker.Instance.selectedDate = null;
            headingTMP.text = TranslationController.Instance.Translate("selection-popup-tasks");
            foreach (var pair in DataController.Instance.tasks)
                elementTuples.Add(new Tuple<string, long, UnityAction>(pair.Value.name, pair.Key,
                    () => taskPreviewPanel.SetUpPreviewPanel(pair.Value)));
        }
        else if (t.Equals(typeof(TaskCollection)))
        {
            DeadlinePicker.Instance.selectedDate = null;
            headingTMP.text = TranslationController.Instance.Translate("selection-popup-task-collections");
            foreach (var pair in DataController.Instance.taskCollections)
                elementTuples.Add(new Tuple<string, long, UnityAction>(pair.Value.name, pair.Key,
                    () => taskCollectionPreviewPanel.SetUpPreviewPanel(pair.Value)));
        }
        else if (t.Equals(typeof(Recording)))
        {
            headingTMP.text = TranslationController.Instance.Translate("selection-popup-recording");
            CreateRemoveSelectionElement();
            foreach (var pair in DataController.Instance.remoteRecordings)
                elementTuples.Add(new Tuple<string, long, UnityAction>(pair.Value.name, pair.Key,
                    () => recordingPreviewPanel.SetUpPreviewPanel(pair.Value)));
        }

        elementTuples.Sort((tuple1, tuple2) =>
            String.Compare(tuple1.Item1, tuple2.Item1, StringComparison.Ordinal));

        foreach (Tuple<string, long, UnityAction> element in elementTuples)
            if (_isMultiSelect)
                CreateElement(element.Item1, element.Item2, null, element.Item3);
            else
                CreateElement(element.Item1, element.Item2, () => SetSelectedId(element.Item2), element.Item3);

        InitPreviewSection(t);
        closeButton.onClick.AddListener(Close);
    }

    public void SelectElement(long id)
    {
        foreach (Transform element in ContentContainer)
        {
            SelectionElement selectionElement = element.GetComponent<SelectionElement>();
            if (selectionElement != null && selectionElement.id == id)
                selectionElement.checkbox.isOn = true;
        }
    }

    public void InitForCoat(UnityAction onAdd, bool addFromSelectionEntry, bool addFromRecordingEntry,
        bool noCoatEntry, bool noClearCoat)
    {
        headingTMP.text = TranslationController.Instance.Translate("selection-popup-coat");
        ContentContainer.DestroyAllChildren();
        _elementObjects = new List<SelectionElement>();
        List<Tuple<string, long, UnityAction>> elementTuples = new List<Tuple<string, long, UnityAction>>();

        CreateRemoveSelectionElement();

        if (addFromSelectionEntry)
            CreateSpecialCoatElement(Statics.COAT_FROM_COAT_SELECTION, -1);
        if (addFromRecordingEntry)
            CreateSpecialCoatElement(Statics.COAT_FROM_RECORDING, -2);
        if (noCoatEntry)
            CreateSpecialCoatElement(Statics.NO_COAT, -3);

        elementTuples.Sort((tuple1, tuple2) => String.Compare(tuple1.Item1, tuple2.Item1, StringComparison.Ordinal));

        foreach (var pair in DataController.Instance.coats)
            if (!noClearCoat || pair.Value.type != CoatType.Clearcoat)
                elementTuples.Add(new Tuple<string, long, UnityAction>(pair.Value.name, pair.Key,
                    () => coatPreviewPanel.SetUpPreviewPanel(pair.Value)));

        Init(typeof(Coat), onAdd, elementTuples);
    }

    private void InitPreviewSection<T>(T t)
    {
        audioPreviewPanel.gameObject.SetActive(t.Equals(Media.MediaType.Audio));
        imagePreviewPanel.gameObject.SetActive(t.Equals(Media.MediaType.Image));
        videoPreviewPanel.gameObject.SetActive(t.Equals(Media.MediaType.Video));
        taskPreviewPanel.gameObject.SetActive(t.Equals(typeof(Task)));
        taskCollectionPreviewPanel.gameObject.SetActive(t.Equals(typeof(TaskCollection)));
        recordingPreviewPanel.gameObject.SetActive(t.Equals(typeof(Recording)));
        coatPreviewPanel.gameObject.SetActive(t.Equals(typeof(Coat)));
        selectButton.interactable = false;
    }

    private void ShowMediaPreview(Media media)
    {
        String path = media.GetPath();
        if (!File.Exists(path))
        {
            PopupScreenHandler.Instance.ShowLoadingScreen("selection-popup-load-preview",
                "selection-popup-loading-preview");
            StartCoroutine(RestConnector.DownloadFile("/media/" + media.id + "/file", path, () =>
            {
                SetUpMediaPreviewPanel(media);
                PopupScreenHandler.Instance.Close();
            }, () =>
            {
                SetUpMediaPreviewPanel(media);
                PopupScreenHandler.Instance.ShowMessage("selection-popup-load-preview",
                    "popup-connection-error-try-later");
            }));
        }
        else
            SetUpMediaPreviewPanel(media);
    }

    private void SetUpMediaPreviewPanel(Media media)
    {
        if (media.type.Equals(Media.MediaType.Audio))
            audioPreviewPanel.SetUpPreviewPanel(media);
        else if (media.type.Equals(Media.MediaType.Image))
            imagePreviewPanel.SetUpPreviewPanel(media);
        else if (media.type.Equals(Media.MediaType.Video))
            videoPreviewPanel.SetUpPreviewPanel(media);
    }

    /// <summary>
    /// Creates a selection element.
    /// </summary>
    private SelectionElement CreateElement(String title, long id, UnityAction onClick = null,
        UnityAction previewAction = null, bool alwaysVisible = false)
    {
        SelectionElement element = Instantiate(selectionElementPrefab, ContentContainer);
        _elementObjects.Add(element);
        element.Init(title, id, alwaysVisible);
        if (onClick != null)
        {
            element.checkbox.gameObject.SetActive(false);
            element.button.onClick.AddListener(onClick);
            element.button.onClick.AddListener(() => selectButton.interactable = true);
        }
        else
        {
            element.checkbox.gameObject.SetActive(true);
            element.checkbox.onValueChanged.AddListener(_ => selectButton.interactable = CheckIfSomethingSelected());
        }

        if (previewAction != null)
            element.button.onClick.AddListener(previewAction);
        return element;
    }

    /// <summary>
    /// Renew _checkedTasks/Collections, if there is one ore mor selected, the add button can be enabled, else disabled.
    /// </summary>
    /// <returns>true: one or more tasks/collections selected, false: none selected</returns>
    private bool CheckIfSomethingSelected()
    {
        GetCheckedElements();
        return _selectedIds.Count > 0;
    }

    /// <summary>
    /// Function called by the "add file button".
    /// </summary>
    private void ChooseElement()
    {
        if (_isMultiSelect)
            GetCheckedElements();
        _onAdd.Invoke();
        Close();
    }

    /// <summary>
    /// Find all tasks/taskCollections with checked toggles that have not been assigned already.
    /// </summary>
    private void GetCheckedElements()
    {
        _selectedIds = new List<long>();
        foreach (Transform child in ContentContainer)
        {
            SelectionElement currentChild = child.GetComponent<SelectionElement>();
            if (currentChild && currentChild.checkbox.isOn && currentChild.checkbox.interactable)
                _selectedIds.Add(currentChild.id);
        }
    }

    private void CreateRemoveSelectionElement()
    {
        SelectionElement element =
            CreateSpecialSelectionElement(TranslationController.Instance.Translate("selection-popup-remove"), -10);
        element.label.color = Color.red;
    }

    /// <summary>
    /// For coat selection the are some special elements which do not represent coats.
    /// They are generated with this method.
    /// </summary>
    private void CreateSpecialCoatElement(string title, long id)
    {
        SelectionElement element = CreateSpecialSelectionElement(title, id);
        element.label.color = new Color(0, 0, 0.5f, 1);
    }

    private SelectionElement CreateSpecialSelectionElement(string title, long id)
    {
        return CreateElement(title, id, () =>
        {
            SetSelectedId(id);
            _onAdd.Invoke();
            Close();
        }, alwaysVisible: true);
    }

    private void Close()
    {
        searchInputField.text = String.Empty;
        ContentContainer.DestroyAllChildren();
        gameObject.SetActive(false);
    }

    private void SetSelectedId(long id)
    {
        _selectedIds = new List<long> { id };
    }

    public List<long> GetSelectedIds()
    {
        return _selectedIds;
    }

    public long GetSelectedId()
    {
        return _selectedIds[0];
    }

    public bool IsValidSelection()
    {
        return _selectedIds[0] != -10;
    }
}
using System;
using System.IO;
using System.Linq;
using SFB;
using TMPro;
using translator;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller for the media menu.
/// </summary>
public class MediaMenuController : BaseMenuController
{
    public Transform mediaList;
    public TextMeshProUGUI header;
    public Button newButton;
    public TMP_InputField nameInputField;
    public TextMeshProUGUI pathTMP;
    public TMP_InputField descriptionInputField;
    public MediaTableElement mediaTableElementPrefab;
    public TabHandler tabHandler;

    [Header("Preview")] public Image previewImage;
    public VideoAndAudioPlayerController previewVideoAndAudio;

    private FileInfo _fileInfo;
    private string _missingValuesString;
    private MediaTableElement _currentContainer;
    private Media.MediaType _currentMediaType;
    private string _currentElement;

    private void Awake()
    {
        _currentMediaType = Media.MediaType.Image;
        newButton.onClick.AddListener(() => SetUpByMedia(null, false));
        saveButton.onClick.AddListener(SaveMedia);
        nameInputField.onValueChanged.AddListener(_ => SetUnsavedChanges(true));
        descriptionInputField.onValueChanged.AddListener(_ => SetUnsavedChanges(true));
        ResetFields();
    }

    private void OnEnable()
    {
        MainScreenController.Instance.LoadData(DataController.RequestType.Media, () => SetCurrentMediaType(1));
    }

    /// <summary>
    /// Initializes the view in dependence of a media type.
    /// </summary>
    public void SetCurrentMediaType(int mediaTypeIndex)
    {
        _currentMediaType = Enum.GetValues(typeof(Media.MediaType)).Cast<Media.MediaType>().ToList()[mediaTypeIndex];
        _currentElement = TranslationController.Instance.Translate(_currentMediaType.ToString());
        newButton.GetComponentInChildren<TextMeshProUGUI>().text =
            _currentElement + " " + TranslationController.Instance.Translate("media-add");
        saveButton.GetComponentInChildren<TextMeshProUGUI>().text =
            _currentElement + " " + TranslationController.Instance.Translate("media-save");
        ResetFields();
        InstantiateContainer();
        // needed to set active tab on change to media view
        tabHandler.SetPanelActive(mediaTypeIndex - 1);
    }

    private void ResetFields()
    {
        nameInputField.text = String.Empty;
        descriptionInputField.text = String.Empty;
        pathTMP.text = String.Empty;
        previewImage.gameObject.SetActive(false);
        previewVideoAndAudio.gameObject.SetActive(false);
        previewVideoAndAudio.SetVideoActive(false);
        SetUnsavedChanges(false);
    }

    /// <summary>
    /// Creates the list of available media elements.
    /// </summary>
    private void InstantiateContainer()
    {
        mediaList.DestroyImmediateAllChildren();
        header.text = TranslationController.Instance.Translate("media-list-of") + " ";
        switch (_currentMediaType)
        {
            case Media.MediaType.Image:
                header.text += TranslationController.Instance.Translate("media-images");
                foreach (Media media in DataController.Instance.availableImages.Values)
                    AddContainer(media);
                break;
            case Media.MediaType.Video:
                header.text += TranslationController.Instance.Translate("media-videos");
                foreach (Media media in DataController.Instance.availableVideos.Values)
                    AddContainer(media);
                previewVideoAndAudio.gameObject.SetActive(true);
                previewVideoAndAudio.ChangeMediaType(Media.MediaType.Video);
                break;
            case Media.MediaType.Audio:
                header.text += TranslationController.Instance.Translate("media-audios");
                foreach (Media media in DataController.Instance.availableAudios.Values)
                    AddContainer(media);
                previewVideoAndAudio.gameObject.SetActive(true);
                previewVideoAndAudio.ChangeMediaType(Media.MediaType.Audio);
                break;
        }

        SortMediaElements();
    }

    /// <summary>
    /// Adds a media list entry.
    /// </summary>
    private MediaTableElement AddContainer(Media media)
    {
        MediaTableElement newContainer = Instantiate(mediaTableElementPrefab, mediaList);
        newContainer.Init(media, SetUpByMedia, DeleteMedia);
        return newContainer;
    }

    /// <summary>
    /// Deletes a media file on the server and locally if necessary.
    /// </summary>
    private void DeleteMedia(MediaTableElement container, bool confirmed)
    {
        if (!confirmed)
        {
            PopupScreenHandler.Instance.ShowConfirmation(
                _currentElement + " " + TranslationController.Instance.Translate("popup-remove-media"),
                "popup-remove-media-confirm", () => DeleteMedia(container, true));
            return;
        }

        PopupScreenHandler.Instance.ShowLoadingScreen(
            _currentElement + " " + TranslationController.Instance.Translate("popup-remove-media"),
            _currentElement + " " + TranslationController.Instance.Translate("popup-removing-media"));
        RestConnector.Delete(container.media, "/media/" + container.media.id, () =>
            {
                if (container != null && _currentContainer != null && _currentContainer.media.id == container.media.id)
                    ResetFields();
                string path = Path.Combine(Application.streamingAssetsPath, container.media.data);
                if (File.Exists(path))
                    File.Delete(path);
                Destroy(container.gameObject);
                PopupScreenHandler.Instance.ShowMessage(
                    _currentElement + " " + TranslationController.Instance.Translate("popup-remove-media"),
                    _currentElement + " " + TranslationController.Instance.Translate("popup-removed-media"));
            }, _ => PopupScreenHandler.Instance.ShowMessage(
                _currentElement + " " + TranslationController.Instance.Translate("popup-remove-media"),
                "popup-remove-media-error"),
            PopupScreenHandler.Instance.ShowConnectionError);
    }

    /// <summary>
    /// Opens an file explorer to navigate through local files to upload one. Uses the StandaloneFileBrowser 
    /// and a file filter depending on the kind of media wanting to upload.
    /// If the user does not choose a file, the panel will be closed.
    /// </summary>
    private bool OpenFileExplorerLimitedByType()
    {
        var extensions = new[] { new ExtensionFilter() };
        if (_currentMediaType == Media.MediaType.Image)
            extensions[0] = new ExtensionFilter("Image", "png", "jpg", "jpeg", "bmp");
        else if (_currentMediaType == Media.MediaType.Video)
            extensions[0] =
                new ExtensionFilter("Video", "3g2", "3gp", "3gp2", "3gpp", "avi", "m4a", "m4v", "mov", "mp4");
        else if (_currentMediaType == Media.MediaType.Audio)
            extensions[0] = new ExtensionFilter("Audio", "mp3", "wav", "ogg");
        var path = StandaloneFileBrowser.OpenFilePanel(TranslationController.Instance.Translate("media-select"), "",
            extensions, false);
        if (path.Length > 0)
        {
            // file chosen
            ShowFileProperties(new FileInfo(path[0]));
            return true;
        }

        return false;
    }

    /// <summary>
    /// Sets the corresponding input fields if a file is chosen for upload.
    /// </summary>
    private void ShowFileProperties(FileInfo fileInfo)
    {
        _fileInfo = fileInfo;
        pathTMP.text = fileInfo.FullName;
        LoadPreview(fileInfo.FullName);
        nameInputField.text = Path.GetFileNameWithoutExtension(fileInfo.Name);
        descriptionInputField.text = "";
    }

    /// <summary>
    /// Loads the preview for a media file.
    /// </summary>
    private void LoadPreview(Media media)
    {
        String path = media.GetPath();
        if (!File.Exists(path))
        {
            PopupScreenHandler.Instance.ShowLoadingScreen("popup-media-load-preview", "popup-media-loading-file");
            StartCoroutine(RestConnector.DownloadFile("/media/" + media.id + "/file", path, () =>
            {
                LoadPreview(path);
                PopupScreenHandler.Instance.Close();
            }, () =>
            {
                LoadPreview(path);
                PopupScreenHandler.Instance.ShowMessage("popup-media-load-preview", "popup-connection-error-try-later");
            }));
        }
        else
            LoadPreview(path);
    }

    private void LoadPreview(string path)
    {
        previewImage.gameObject.SetActive(false);
        if (!File.Exists(path))
        {
            previewVideoAndAudio.SetVideoActive(false);
            return;
        }

        switch (_currentMediaType)
        {
            case Media.MediaType.Image:
                byte[] content = File.ReadAllBytes(path);
                Texture2D texture = new Texture2D(1, 1);
                if (texture.LoadImage(content))
                {
                    previewImage.gameObject.SetActive(true);
                    previewImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f));
                    previewImage.preserveAspect = true;
                }

                break;
            case Media.MediaType.Video:
                previewVideoAndAudio.LoadVideo(path);
                break;
            case Media.MediaType.Audio:
                previewVideoAndAudio.LoadAudio(path);
                break;
        }
    }

    /// <summary>
    /// Sets up the fields on the right side by the properties of the selected media file.
    /// </summary>
    private void SetUpByMedia(MediaTableElement mediaTableElement, bool confirmed)
    {
        if (HasUnsavedChanges() && !confirmed)
        {
            PopupScreenHandler.Instance.ShowConfirmation(
                _currentElement + " " + TranslationController.Instance.Translate("popup-media-save"),
                "popup-unsaved-changes-confirm", () => SetUpByMedia(mediaTableElement, true));
            return;
        }

        _currentContainer = mediaTableElement;

        if (mediaTableElement == null)
        {
            if (OpenFileExplorerLimitedByType())
            {
                nameInputField.interactable = true;
                descriptionInputField.interactable = true;
                SetUnsavedChanges(true);
            }
        }
        else
        {
            nameInputField.text = _currentContainer.media.name;
            nameInputField.interactable = true;
            descriptionInputField.text = _currentContainer.media.description;
            descriptionInputField.interactable = true;
            pathTMP.text = _currentContainer.media.data;
            LoadPreview(_currentContainer.media);
            SetUnsavedChanges(false);
        }
    }

    /// <summary>
    /// Checks, if some fields aren't filled in correctly.
    /// </summary>
    /// <returns>returns true if something is missing, otherwise false</returns>
    private bool ValuesMissing()
    {
        return nameInputField.text == "";
    }

    /// <summary>
    /// Saves the media file on the server.
    /// </summary>
    private void SaveMedia()
    {
        if (ValuesMissing())
        {
            PopupScreenHandler.Instance.ShowMessage(
                _currentElement + " " + TranslationController.Instance.Translate("popup-media-save"),
                "popup-media-missing-values");
            return;
        }

        if (!ValidationUtil.ValidateName(nameInputField.text))
            return;

        // we need to stop preview to avoid IO sharing exception
        previewVideoAndAudio.Stop();

        PopupScreenHandler.Instance.ShowLoadingScreen(
            _currentElement + " " + TranslationController.Instance.Translate("popup-media-save"),
            _currentElement + " " + TranslationController.Instance.Translate("popup-media-saving"));
        Media media = _currentContainer != null ? _currentContainer.media : new Media { id = -1 };
        media.name = nameInputField.text;
        media.description = descriptionInputField.text;
        media.type = _currentMediaType;
        // old path is needed to rename file after successful server request
        if (!_currentContainer)
        {
            if (!File.Exists(_fileInfo.FullName))
            {
                PopupScreenHandler.Instance.ShowMessage(
                    _currentElement + " " + TranslationController.Instance.Translate("popup-media-save"),
                    "popup-media-missing-file");
                return;
            }

            media.data = Path.Combine(Media.MediaTypeToDirectory(_currentMediaType), media.name + _fileInfo.Extension);
        }

        if (media.id >= 0)
        {
            RestConnector.Put(media, "/media", newMedia =>
            {
                File.Move(media.GetPath(), newMedia.GetPath());
                SaveMediaSuccess(newMedia);
            }, PopupScreenHandler.Instance.ShowConnectionError, _ => PopupScreenHandler.Instance.ShowMessage(
                "popup-media-existing", "popup-media-existing-text"));
        }
        else
        {
            RestConnector.Upload(media, "/media", _fileInfo, newMedia =>
                {
                    RestConnector.CopyFileAsync(_fileInfo.FullName, newMedia.GetPath(),
                        () => SaveMediaSuccess(newMedia));
                }, _ => PopupScreenHandler.Instance.ShowMessage("popup-media-existing", "popup-media-existing-text"),
                () => PopupScreenHandler.Instance.ShowMessage("popup-file-too-big",
                    TranslationController.Instance.Translate("popup-file-too-big-text",
                        ConfigController.Instance.GetMaxFileSize())),
                PopupScreenHandler.Instance.ShowConnectionError);
        }
    }

    private void SaveMediaSuccess(Media media)
    {
        if (_currentContainer == null)
        {
            _currentContainer = AddContainer(media);
            SetUpByMedia(_currentContainer, true);
        }
        else
            _currentContainer.Init(media, SetUpByMedia, DeleteMedia);

        DataController.Instance.media[media.id] = media;
        DataController.Instance.UpdateMedia();
        _currentContainer.containerButton.Select();
        SetUnsavedChanges(false);
        PopupScreenHandler.Instance.ShowMessage(
            _currentElement + " " + TranslationController.Instance.Translate("popup-media-save"),
            _currentElement + " " + TranslationController.Instance.Translate("popup-media-saved"));
        SortMediaElements();
    }

    private void SortMediaElements()
    {
        SortListElements<MediaTableElement>(mediaList,
            (e1, e2) => String.Compare(e1.media.name, e2.media.name, StringComparison.CurrentCultureIgnoreCase));
    }
}
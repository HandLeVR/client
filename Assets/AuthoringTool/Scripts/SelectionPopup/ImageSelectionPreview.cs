using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Provides an image preview
/// </summary>
public class ImageSelectionPreview : MonoBehaviour
{
    public TMP_InputField imageNameInputField;
    public TMP_InputField descriptionInputField;
    public TMP_InputField authorInputField;
    public Image image;
    public Transform fullscreenContainer;
    public MediaPreviewFullscreenImage fullscreenImagePreview;
    public Button btn_zoom;

    private void OnEnable()
    {
        ClearPreview();
    }

    public void SetUpPreviewPanel(Media media)
    {
        imageNameInputField.text = media.name;
        descriptionInputField.gameObject.SetActive(media.description.Length > 0);
        descriptionInputField.text = media.description;
        descriptionInputField.textComponent.enableWordWrapping = true;
        authorInputField.text = media.permission.createdByFullName;
        btn_zoom.onClick.AddListener(ShowFullscreenPreview);
        btn_zoom.interactable = false;
        if (File.Exists(media.GetPath()))
            StartCoroutine(LoadImageFileCoroutine(Path.Combine(Application.streamingAssetsPath, media.data)));
    }

    /// <summary>
    /// Attention: Only JPG and PNG formats are supported. (https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequestTexture.GetTexture.html)
    /// </summary>
    private IEnumerator LoadImageFileCoroutine(string filepath)
    {
        string url = "file:///" + filepath;
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            uwr.useHttpContinue = false;
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError ||
                uwr.result == UnityWebRequest.Result.ProtocolError)
                Debug.Log(uwr.error + "" + filepath);
            else
            {
                var texture = DownloadHandlerTexture.GetContent(uwr);
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 100f);
                image.preserveAspect = true;
                btn_zoom.interactable = true;
            }
        }
    }

    private void ClearPreview()
    {
        imageNameInputField.text = "";
        descriptionInputField.text = "";
        image.sprite = null;
        btn_zoom.interactable = false;
    }
    
    private void ShowFullscreenPreview()
    {
        if (image.sprite != null)
        {
            fullscreenContainer.gameObject.SetActive(true);
            foreach (Transform child in fullscreenContainer.GetChild(0))
                child.gameObject.SetActive(false);
            fullscreenImagePreview.gameObject.SetActive(true);
            fullscreenImagePreview.Init(image.sprite.name, image.sprite.texture);
        }
    }
}

using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// This class realizes a preview for audio clips. It displays all relevant information and allows to play the clip.
/// </summary>
public class AudioSelectionPreview : MonoBehaviour
{
    public TMP_InputField audioNameInputField;
    public AudioSource audioSource;
    public TMP_InputField descriptionInputField;
    public TMP_InputField authorInputField;
    public Scrollbar slider;
    public TextMeshProUGUI currentTimeTextField;
    public TextMeshProUGUI maxTimeTextField;

    private float max;

    void Update()
    {
        if (audioSource.isPlaying)
        {
            TimeSpan current_timespan = TimeSpan.FromSeconds(audioSource.time);
            currentTimeTextField.text =
                $"{current_timespan.Hours:D2}:{current_timespan.Minutes:D2}:{current_timespan.Seconds:D2}";
            slider.value = audioSource.time / max;
        }
    }

    private void OnEnable()
    {
        ClearPreview();
    }

    public void SetUpPreviewPanel(Media media)
    {
        audioNameInputField.text = media.name;
        descriptionInputField.gameObject.SetActive(media.description.Length > 0);
        descriptionInputField.text = media.description;
        descriptionInputField.textComponent.enableWordWrapping = true;
        authorInputField.text = media.permission.createdByFullName;
        if (File.Exists(media.GetPath()))
            StartCoroutine(LoadAudioFileCoroutine(Path.Combine(Application.streamingAssetsPath, media.data)));
    }

    private void ClearPreview()
    {
        audioNameInputField.text = "";
        descriptionInputField.text = "";
        currentTimeTextField.text = "";
        maxTimeTextField.text = "";
        audioSource.clip = null;
    }

    private IEnumerator LoadAudioFileCoroutine(string filepath)
    {
        string url = "file:///" + filepath;
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN))
        {
            uwr.useHttpContinue = false;
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError ||
                uwr.result == UnityWebRequest.Result.ProtocolError)
                Debug.Log(uwr.error + "" + filepath);
            else
            {
                audioSource.clip = DownloadHandlerAudioClip.GetContent(uwr);
                max = audioSource.clip.length;
                TimeSpan timespan = TimeSpan.FromSeconds(max);
                maxTimeTextField.text = $"{timespan.Hours:D2}:{timespan.Minutes:D2}:{timespan.Seconds:D2}";
                slider.value = 0.0f;
            }
        }
    }
}
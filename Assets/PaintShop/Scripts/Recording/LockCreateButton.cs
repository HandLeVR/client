using UnityEngine;

/// <summary>
/// Locks the button for creating a recording while recording.
/// </summary>
public class LockCreateButton : MonoBehaviour
{
    public GameObject createButton;

    void Update()
    {
        if (PlayRecordingController.Instance.playing | CreateRecordingController.Instance.recording)
            createButton.SetActive(false);
        else
            createButton.SetActive(true);
    }
}

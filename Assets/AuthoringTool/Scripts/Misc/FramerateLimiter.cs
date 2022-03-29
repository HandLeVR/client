using UnityEngine;

/// <summary>
/// Limits the frame rate of the application.
/// </summary>
public class FramerateLimiter : MonoBehaviour
{
    public int targetFrameRate = 30;

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate;
    }
}
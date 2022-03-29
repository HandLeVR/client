using UnityEngine;

/// <summary>
/// Displays the current frames per second in the upper left corner of the window.
/// </summary>
public class FramesPerSecond : MonoBehaviour
{
    float deltaTime = 0.0f;
    public float avgFrameRate;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = Color.black;
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        avgFrameRate = Time.frameCount / Time.time;
        string text = string.Format("{0:0.0} ms ({1:0.} fps) - Avg FPS {2:0.} fps", msec, fps,avgFrameRate);
        GUI.Label(rect, text, style);
    }
}

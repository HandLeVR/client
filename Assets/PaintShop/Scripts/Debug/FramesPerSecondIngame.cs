using UnityEngine;
using UnityEngine.UI;

public class FramesPerSecondIngame : MonoBehaviour
{
    private Text text;
    private float deltaTime = 0.0f;

    private void Start()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        text.text = $"{msec:0.0} ms ({fps:0.} fps)";
    }
}

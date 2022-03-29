using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sets the output of the evaluation camera to a render texture that is part of the UI.
/// </summary>
[ExecuteInEditMode]
public class CameraViewController : MonoBehaviour
{
    public int renderResolutionWidth;
    public int renderResolutionHeight;
    public Camera evaluationCamera;
    public RawImage renderTextureOutput;

    void Start()
    {
        evaluationCamera.targetTexture = new RenderTexture(renderResolutionWidth, renderResolutionHeight, 24);
        renderTextureOutput.texture = evaluationCamera.targetTexture;
    }
}

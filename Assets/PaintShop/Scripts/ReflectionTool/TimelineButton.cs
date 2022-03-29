using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Button representing time marks in the evaluation tool where the user can jump to.
/// </summary>
public class TimelineButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RawImage preview;
    public RenderTexture heightmap;
    public EvaluationData evaluationData;
    public Texture2D screenshot;
    public PaintStandMovement currentPaintStandMovement;
    public SprayGun.SprayGunMode currentMode;
    public SprayGunPathData sprayGunPathData;
    public int frameIndex;

    private void OnDestroy()
    {
        RenderTexture.ReleaseTemporary(heightmap);
    }

    // Displays the screenshot when the mouse pointer enters
    public void OnPointerEnter(PointerEventData eventData)
    {
        preview.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        preview.enabled = false;
    }

    public void OnButtonClick()
    {
        ReflectionToolRecordingController.Instance.jumpToTimelineButton = this;
    }

    public void UpdatePreview()
    {
        preview.texture = screenshot;
    }
}

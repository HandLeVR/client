using UnityEngine;

/// <summary>
/// Custom version of the DrawingController which allows to draw with the spray gun and introduces cookie scaling
/// for fast movements.
/// </summary>
public class CustomDrawingController : MonoBehaviour
{
    [Tooltip("Activates cookie scaling to avoid gabs between cookies on fast movements.")]
    public bool scaleCookie;

    [HideInInspector] public CustomSpotDrawer paintSpotDrawer;

    private static readonly int TargetWidthId = Shader.PropertyToID("_TargetWidth");
    private static readonly int TargetHeightId = Shader.PropertyToID("_TargetHeight");
    private static readonly int DirectionId = Shader.PropertyToID("_Direction");

    /// <summary>
    /// Draws the cookie and scales it if necessary. The local direction is the moving direction from the view point
    /// of the spray origin. This allows to determine how the cookie needs to be modified.
    /// </summary>
    protected void Draw(Vector2 localDirection)
    {
        Texture originalCookie = PaintController.Instance.cookie;
        Material cookieScaleShader = PaintController.Instance.cookieScaleShader;
        float distanceToTexScale = PaintController.Instance.distanceToCookieScale;
        float maxCookieSizeIncrease = PaintController.Instance.maxCookieSizeIncrease;

        int addWidth = 0;
        int addHeight = 0;
        if (scaleCookie)
        {
            addWidth = (int)Mathf.Min(Mathf.Abs(localDirection.x) * distanceToTexScale, maxCookieSizeIncrease);
            addHeight = (int)Mathf.Min(Mathf.Abs(localDirection.y) * distanceToTexScale, maxCookieSizeIncrease);
        }

        var rd = new RenderTextureDescriptor(originalCookie.width + addWidth, originalCookie.height + addHeight)
        {
            sRGB = false
        };
        
        RenderTexture newCookie = null;
        if (scaleCookie)
        {
            newCookie = RenderTexture.GetTemporary(rd);
            cookieScaleShader.SetFloat(TargetWidthId, newCookie.width);
            cookieScaleShader.SetFloat(TargetHeightId, newCookie.height);
            cookieScaleShader.SetFloat(DirectionId, localDirection.x * localDirection.y);
            Graphics.Blit(originalCookie, newCookie, cookieScaleShader);
            paintSpotDrawer.cookie = newCookie;

            // depth texture needs to be scaled too 
            paintSpotDrawer.depthOutputScaled = RenderTexture.GetTemporary(originalCookie.width + addWidth,
                originalCookie.height + addHeight, 16, RenderTextureFormat.RFloat);
        }
        else
        {
            paintSpotDrawer.cookie = originalCookie;
            paintSpotDrawer.depthOutputScaled = RenderTexture.GetTemporary(originalCookie.width, originalCookie.height,
                16, RenderTextureFormat.RFloat);
        }

        Rect rect = new Rect(0, 0, 1, 1);
        if (rd.width > rd.height)
            rect.height = rd.height / (float)rd.width;
        else
            rect.width = rd.width / (float)rd.height;

        paintSpotDrawer.cameraRect = rect;
        float diff = PaintController.Instance.spotDrawerMaxAngle - PaintController.Instance.minSpotDrawerAngle;
        float fieldOfView = PaintController.Instance.minSpotDrawerAngle +
                            diff * (addHeight / PaintController.Instance.maxCookieSizeIncrease);
        paintSpotDrawer.fieldOfView = fieldOfView;

        // Possibly the fixedDeltaTime is changed for other headsets by the OpenXR framework. For the oculus quest,
        // which was the development device, 0.01388889 is the target fixedDeltaTime and is now used as the reference.
        // For other fixed update rates the intensity has to be adapted accordingly to get a similar color application.
        float targetFixedDeltaTime = 0.01388889f;
        float diffToTarget = Time.fixedDeltaTime - targetFixedDeltaTime;
        float intensityModifier = (targetFixedDeltaTime + diffToTarget / 2) *
                                  (1 - Mathf.Clamp01(localDirection.magnitude /
                                                     PaintController.Instance.maxDistanceIntensity));
        paintSpotDrawer.intensityModifier = intensityModifier;

        CustomDrawable targetDrawable = GetClosestDrawable();
        if (targetDrawable != null)
            paintSpotDrawer.Draw(targetDrawable);
        
        if (scaleCookie)
            RenderTexture.ReleaseTemporary(newCookie);
        RenderTexture.ReleaseTemporary(paintSpotDrawer.depthOutputScaled);
    }

    /// <summary>
    /// Draws the flashlight cookie which is used in the evaluation mode.
    /// </summary>
    protected void FlashlightDraw()
    {
        paintSpotDrawer.intensityModifier = 1;
        paintSpotDrawer.cameraRect = new Rect(0, 0, 1, 1);
        paintSpotDrawer.cookie = PaintController.Instance.flashlightCookie;
        paintSpotDrawer.fieldOfView = PaintController.Instance.flashlightSpotDrawerAngle;
        paintSpotDrawer.Draw(GetClosestDrawable());
    }

    /// <summary>
    /// Get closest drawable to avoid calling expensive draw methods on all drawables.
    /// </summary>
    protected CustomDrawable GetClosestDrawable()
    {
        CustomDrawable targetDrawable = null;
        float minDistance = Mathf.Infinity;
        foreach (var drawable in PaintController.Instance.drawables)
        {
            float distance = Vector3.Distance(transform.position, drawable.transform.position);
            if (distance < minDistance)
            {
                targetDrawable = drawable;
                minDistance = distance;
            }
        }

        return targetDrawable;
    }
}
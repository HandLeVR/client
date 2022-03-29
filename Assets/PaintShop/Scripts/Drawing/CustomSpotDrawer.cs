using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Adapted version of the SpotDrawer from the ProjectionSpray asset.
/// </summary>
public class CustomSpotDrawer : MonoBehaviour
{
    public Material drawingMat;
    public Material flashlightDrawingMaterial;
    public Color color = Color.white;
    public int shadowMapResolution = 1024;

    [HideInInspector] public float intensity = 1;
    [HideInInspector] public Texture cookie;
    [HideInInspector] public Rect cameraRect;
    [HideInInspector] public float fieldOfView = 75;
    [HideInInspector] public float intensityModifier = 1;

    [HideInInspector] public RenderTexture depthOutput;
    [HideInInspector] public RenderTexture depthOutputScaled;
    
    private Camera _c;
    private Material _drawingMaterial;

    private static readonly int DrawerPosId = Shader.PropertyToID("_DrawerPos");
    private static readonly int DrawerDirId = Shader.PropertyToID("_DrawerDir");
    private static readonly int EmissionId = Shader.PropertyToID("_Emission");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int WorldToDrawerMatrixId = Shader.PropertyToID("_WorldToDrawerMatrix");
    private static readonly int ProjMatrixId = Shader.PropertyToID("_ProjMatrix");
    private static readonly int CookieId = Shader.PropertyToID("_Cookie");
    private static readonly int DrawerDepthId = Shader.PropertyToID("_DrawerDepth");
    private static readonly int MinDistanceId = Shader.PropertyToID("_MinDistance");
    private static readonly int MaxDistanceId = Shader.PropertyToID("_MaxDistance");

    private Camera Camera
    {
        get
        {
            if (_c == null)
            {
                _c = GetComponent<Camera>();
                if (_c == null)
                    _c = gameObject.AddComponent<Camera>();
                depthOutput = new RenderTexture(shadowMapResolution, shadowMapResolution, 16,
                    RenderTextureFormat.RFloat);
                depthOutput.wrapMode = TextureWrapMode.Clamp;
                depthOutput.Create();
                _c.targetTexture = depthOutput;
                _c.clearFlags = CameraClearFlags.SolidColor;
                _c.backgroundColor = Color.red;
                _c.nearClipPlane = 0.01f;
                _c.farClipPlane = 1f;
                _c.enabled = false;
                _c.fieldOfView = fieldOfView;
                // enable depth renderer
                _c.GetUniversalAdditionalCameraData().SetRenderer(1);
                // needed because otherwise unity treats the camera like a vr camera
                cameraRect = _c.rect;
                _c.stereoTargetEye = StereoTargetEyeMask.None;
                _c.cullingMask = LayerMask.GetMask("Drawable", "NoReflection");
            }

            return _c;
        }
    }

    /// <summary>
    /// Creates a depth texture and a material containing the cookie.
    /// 
    /// The depth texture is like an image taken from the position of the spot drawer. The distance of each pixel of
    /// the image to the position of the spot drawer is represented by the value of the alpha channel.
    ///
    /// The cookie represents the paint added to the workpiece in the current frame.
    /// </summary>
    private void UpdateDrawingMat()
    {
        var currentRt = RenderTexture.active;
        RenderTexture.active = depthOutput;
        GL.Clear(true, true, Color.white * Camera.farClipPlane);
        // needs to be set for the unscaled depth output
        Camera.rect = new Rect(0, 0, 1, 1);
        // needs to be set constantly because otherwise unity overrides it because of VR
        Camera.fieldOfView = 75;
        Camera.Render();
        RenderTexture.active = currentRt;
        // scale depth cookie (simple and doesn't match scaling of the cookie but should)
        Graphics.Blit(depthOutput, depthOutputScaled);

        Camera.rect = cameraRect;
        Camera.fieldOfView = fieldOfView;
        var projMatrix = Camera.projectionMatrix;
        var worldToDrawerMatrix = transform.worldToLocalMatrix;

        _drawingMaterial = ApplicationController.Instance.currentTool != SprayGun.Tool.Flashlight
            ? drawingMat
            : flashlightDrawingMaterial;

        _drawingMaterial.SetVector(DrawerPosId, transform.position);
        _drawingMaterial.SetVector(DrawerDirId, transform.forward);
        _drawingMaterial.SetFloat(EmissionId, intensity * intensityModifier);
        _drawingMaterial.SetColor(ColorId, color);
        _drawingMaterial.SetMatrix(WorldToDrawerMatrixId, worldToDrawerMatrix);
        _drawingMaterial.SetMatrix(ProjMatrixId, projMatrix);
        _drawingMaterial.SetTexture(CookieId, cookie);
        _drawingMaterial.SetTexture(DrawerDepthId, depthOutputScaled);
        _drawingMaterial.SetFloat(MinDistanceId, 0.15f);
        _drawingMaterial.SetFloat(MaxDistanceId, 0.20f);
    }
    
    public void Draw(CustomDrawable drawable)
    {
        UpdateDrawingMat();
        drawable.Draw(_drawingMaterial);
    }
}
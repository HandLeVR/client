using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotDrawer : MonoBehaviour {

    public Material drawingMat;
    
    public float intencity = 1f;
    public Color color = Color.white;
    [Range(0.01f, 90f)] public float angle = 30f;
    public float range = 10f;
    public Texture cookie;
    public int shadowMapResolution = 1024;

    Shader depthRenderShader { get { return Shader.Find("Unlit/depthRender"); } }

    Camera Camera
    {
        get
        {
            if (_c == null)
            {
                _c = GetComponent<Camera>();
                if (_c == null)
                    _c = gameObject.AddComponent<Camera>();
                depthOutput = new RenderTexture(shadowMapResolution, shadowMapResolution, 16, RenderTextureFormat.RFloat);
                depthOutput.wrapMode = TextureWrapMode.Clamp;
                depthOutput.Create();
                _c.targetTexture = depthOutput;
                _c.SetReplacementShader(depthRenderShader, "RenderType");
                _c.clearFlags = CameraClearFlags.Nothing;
                _c.nearClipPlane = 0.01f;
                _c.enabled = false;
            }
            return _c;
        }
    }
    Camera _c;
    RenderTexture depthOutput;

    public void UpdateDrawingMat()
    {
        var currentRt = RenderTexture.active;
        RenderTexture.active = depthOutput;
        GL.Clear(true, true, Color.white * Camera.farClipPlane);
        Camera.fieldOfView = angle;
        Camera.nearClipPlane = 0.01f;
        Camera.farClipPlane = range;
        Camera.Render();
        RenderTexture.active = currentRt;

        var projMatrix = Camera.projectionMatrix;
        var worldToDrawerMatrix = transform.worldToLocalMatrix;

        drawingMat.SetVector("_DrawerPos", transform.position);
        drawingMat.SetFloat("_Emission", intencity * Time.smoothDeltaTime);
        drawingMat.SetColor("_Color", color);
        drawingMat.SetMatrix("_WorldToDrawerMatrix", worldToDrawerMatrix);
        drawingMat.SetMatrix("_ProjMatrix", projMatrix);
        drawingMat.SetTexture("_Cookie", cookie);
        drawingMat.SetTexture("_DrawerDepth", depthOutput);
    }

    public void Draw(Drawable drawable)
    {
        drawable.Draw(drawingMat);
    }
}

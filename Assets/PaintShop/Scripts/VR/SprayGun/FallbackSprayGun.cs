using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Used in debug mode when no vr headset is connected.
/// </summary>
public class FallbackSprayGun : SprayGun
{
    public Camera noSteamVRFallbackCamera;
    public float distance = 0.175f;
    public LayerMask hitLayers;

    public override float GetSprayingValue()
    {
        return Mouse.current.leftButton.isPressed ? 1 : 0;
    }

    public override float GetTriggerValue()
    {
        return Mouse.current.leftButton.isPressed ? 1 : 0;
    }

    public override void Start()
    {
        ApplicationController.Instance.sprayGun = this;
    }

    public override void FixedUpdate()
    {
        Ray ray = noSteamVRFallbackCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var raycastHit, 10, hitLayers))
        {
            Vector3 direction = (noSteamVRFallbackCamera.transform.position - raycastHit.point).normalized;
            transform.position = raycastHit.point + direction * distance;
            transform.rotation = Quaternion.LookRotation(-direction, Vector3.up);

            // print coat thickness at the current hit
            CustomDrawable drawable = raycastHit.collider.GetComponent<CustomDrawable>();
            if (Keyboard.current.spaceKey.wasPressedThisFrame && drawable)
            {
                Vector2 pixelUV = raycastHit.textureCoord;
                RenderTexture.active = drawable.heightmapOutput;
                Texture2D rgbTex = new Texture2D(drawable.textureSize, drawable.textureSize, TextureFormat.RGBA32,
                    false);
                rgbTex.ReadPixels(new Rect(0, 0, drawable.textureSize, drawable.textureSize), 0, 0);
                rgbTex.Apply();
                RenderTexture.active = null;
                Debug.Log(HeightmapToFloat(rgbTex.GetPixel((int) (pixelUV.x * drawable.textureSize),
                    (int) (pixelUV.y * drawable.textureSize))));
            }
        }

        base.FixedUpdate();
    }

    private float HeightmapToFloat(Color height)
    {
        return Mathf.Max(0,
            Mathf.Round(height.r * 255) * Mathf.Pow(256, 0) + Mathf.Round(height.g * 255) * Mathf.Pow(256, 1) +
            Mathf.Round(height.b * 255) * Mathf.Pow(256, 2));
    }

    protected override void AnimateSprayGun() 
    {
        //do nothing
    }
    
    public override void SetTool(Tool tool)
    {
        //do nothing
    }

}

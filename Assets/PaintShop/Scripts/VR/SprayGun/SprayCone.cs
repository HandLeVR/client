using UnityEngine;

/// <summary>
/// Controls the spray cone.
///
/// Shader is based on: https://www.youtube.com/watch?v=rihJzWq7sE4
/// </summary>
public class SprayCone : MonoBehaviour
{
    public float buildUpTime = 0.2f;
    public float visibility = 0f;
    public float maxRange = 0.6f;
    public SprayGun sprayGun;
    public float currentVisibility;

    private Material _material;
    private float _visibilityVelocity;
    
    private static readonly int ColorId = Shader.PropertyToID("Color_686BCB55");
    private static readonly int DepthTextureId = Shader.PropertyToID("DepthTexture");
    private static readonly int RangeId = Shader.PropertyToID("RangeV");

    private void Start()
    {
        _material = GetComponent<MeshRenderer>().material;
        sprayGun = transform.parent.GetComponent<SprayGun>();
    }

    private void Update()
    {
        _material.SetTexture(DepthTextureId, sprayGun.paintSpotDrawer.depthOutput);
        // smoothly increase visibility but do it faster if the trigger is released
        currentVisibility = Mathf.SmoothDamp(currentVisibility, visibility * maxRange, ref _visibilityVelocity, buildUpTime);
        _material.SetFloat(RangeId, currentVisibility);
    }

    public void SetColor(Color color)
    {
        _material.SetColor(ColorId,color);
    }

    public void SetWideStream(float factor)
    {
        transform.localScale = new Vector3(transform.localScale.x, Mathf.Lerp(transform.localScale.x, 1, factor),
            transform.localScale.z);
    }
}

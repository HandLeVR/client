using UnityEngine;

[ExecuteInEditMode]
public class PointLightComponent : MonoBehaviour
{
    static MaterialPropertyBlock mpb;

    public Renderer targetRenderer;
    public float intencity = 1f;
    public Color color = Color.white;

    void Update()
    {
        if (targetRenderer == null)
            return;
        if (mpb == null)
            mpb = new MaterialPropertyBlock();

        targetRenderer.GetPropertyBlock(mpb);
        mpb.SetVector("_LitPos", transform.position);
        mpb.SetFloat("_Intencity", intencity);
        mpb.SetColor("_LitCol", color);
        targetRenderer.SetPropertyBlock(mpb);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, intencity);
    }
}

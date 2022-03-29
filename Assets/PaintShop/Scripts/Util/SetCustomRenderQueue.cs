using UnityEngine;

/// <summary>
/// Sets the render queue value of a material. Sometimes needed in combination of transparent materials.
/// </summary>
public class SetCustomRenderQueue : MonoBehaviour
{
    public int customRenderQueue = 30000;
    
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().material.renderQueue = customRenderQueue;
    }
}

using UnityEngine;

/// <summary>
/// A rotating circle used for indicating loading acitivities.
/// </summary>
public class Spinner : MonoBehaviour
{
    public float rotateSpeed = 300f;
    
    private RectTransform rectComponent;

    private void Start()
    {
        rectComponent = GetComponent<RectTransform>();
    }

    private void Update()
    {
        rectComponent.Rotate(0f, 0f, -rotateSpeed * Time.deltaTime);
    }
}

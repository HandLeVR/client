using UnityEngine;

/// <summary>
/// Ensures that the body hand always points at the round paint can with the palm inwards.
/// </summary>
[ExecuteInEditMode]
public class BodyHandRotation : MonoBehaviour
{
    public Transform centerOfObject;
    public float distance = 0.05f;

    void Update()
    {
        Vector3 towardsHand = (transform.position-centerOfObject.position).normalized;
        transform.position = (centerOfObject.position + towardsHand * distance);
        transform.LookAt(centerOfObject.position,centerOfObject.up);
    }
}

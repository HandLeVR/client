using UnityEngine;

/// <summary>
/// Represents the player rig.
/// </summary>
public class XRPlayer : MonoBehaviour
{
    public Transform head;

    private Vector3 _startPosition;
    
    void Awake()
    {
        if (!head)
            transform.Find("Head");

        _startPosition = transform.position;
    }

    public void RecenterPosition()
    {
        transform.position = _startPosition;
    }
}

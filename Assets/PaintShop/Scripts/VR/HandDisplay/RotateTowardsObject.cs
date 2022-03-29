using UnityEngine;

/// <summary>
/// Rotates the object towards a target with euler angles to manually adjust for an offset if necessary.
/// </summary>
public class RotateTowardsObject : MonoBehaviour
{
    public Transform target;
    public Draggable draggable;
    public float eulerX;
    public float eulerY;
    public float eulerZ;
    
    private bool _objectHeld;
    private Vector3 _direction;
    private Quaternion _lookRotation;

    void Update()
    {
        // lock rotation while dragging
        if (ApplicationController.Instance.currentDraggable == draggable && draggable.IsBeingDragged())
            return;

        _direction = (target.position - transform.position).normalized;
        _lookRotation = Quaternion.LookRotation(_direction);

        Vector3 eulerNewRotation = _lookRotation.eulerAngles;

        // x,y,z are set in editor to adjust orientation of the hand to align with subjective "direction" of where individually posed hands should be pointing
        eulerNewRotation.x = eulerNewRotation.x + eulerX;
        eulerNewRotation.y = eulerNewRotation.y + eulerY;
        eulerNewRotation.z = eulerNewRotation.z + eulerZ;
        
        transform.rotation = Quaternion.Euler(eulerNewRotation);
    }
}

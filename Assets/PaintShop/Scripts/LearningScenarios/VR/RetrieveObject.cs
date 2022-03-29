using UnityEngine;

/// <summary>
/// Respawns an object if it leaves the predefined interaction area.
/// </summary>
public class RetrieveObject : MonoBehaviour
{
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Rigidbody _rig;
    private Draggable _draggable;
    private bool _resetFlag;

    void Start()
    {
        _resetFlag = false;
        _draggable = GetComponent<Draggable>();
        _rig = GetComponent<Rigidbody>();
        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("BoundingBox"))
            return;
        _resetFlag = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("BoundingBox"))
            return;
        _resetFlag = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("BoundingBox"))
            return;
        _resetFlag = true;
    }

    private void Update()
    {
        if (_resetFlag && !_draggable.IsBeingDragged())
        {
            _resetFlag = false;
            ResetPosition();
        }
    }

    void ResetPosition()
    {
        _rig.velocity = Vector3.zero;
        _rig.angularVelocity = Vector3.zero;
        transform.position = _startPosition;
        transform.rotation = _startRotation;
    }
}
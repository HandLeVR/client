using UnityEngine;

/// <summary>
/// Manages the interaction between spray gun and clock
/// </summary>
public class ClockHand : Draggable
{
    [HideInInspector] public bool isActive;

    private float _angleOffset;

    private void OnEnable()
    {
        isActive = true;
        transform.transform.localEulerAngles = Vector3.zero;
    }

    public override void Drag()
    {
        if (!isActive)
            return;

        // calculate angle the minute hand moved in the plane of the dial of the clock
        Vector3 localTargetDirection = transform.parent.InverseTransformDirection(
            ApplicationController.Instance.sprayGun.pinSpotOrigin.transform.position - transform.position);
        float angle = Vector2.SignedAngle(Vector2.down, new Vector2(localTargetDirection.x, -localTargetDirection.z));
        Vector3 eulerAngles = transform.localRotation.eulerAngles;

        if (!isDragged)
            _angleOffset = angle - eulerAngles.y;
        else
            eulerAngles.y = angle - _angleOffset;

        transform.localEulerAngles = eulerAngles;
        isDragged = true;
    }

    public override void Release()
    {
        if (!isDragged)
            return;
        isDragged = false;
    }
}
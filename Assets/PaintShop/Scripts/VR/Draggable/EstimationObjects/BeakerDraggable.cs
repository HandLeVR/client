using UnityEngine;

/// <summary>
/// Manages interaction between spray gun and beaker.
/// </summary>
public class BeakerDraggable : Draggable
{
    public Beaker controller;
    public float velocity;
    
    private Vector3 _previousControllerPosition;
    private Vector3 _currentControllerPosition;

    void Update()
    {
        _previousControllerPosition = _currentControllerPosition;
        _currentControllerPosition = ApplicationController.Instance.sprayGun.pinSpotOrigin.position;
    }

    public override void Drag()
    {
        if (controller.beakerSet)
            return;
        
        controller.ChangeFluidLevelValue(GetControllerChangeValue());
    }

    /// <summary>
    /// Compare previous and current controller position
    /// </summary>
    /// <returns>Y difference multiplied by velocity</returns>
    private float GetControllerChangeValue()
    {
        float previousY = _previousControllerPosition.y;
        float currentY = _currentControllerPosition.y;
        float change = currentY - previousY;
        return change * velocity;
    }

    public override void Release()
    {
        if (!isDragged)
            return;
        isDragged = false;
    }
}


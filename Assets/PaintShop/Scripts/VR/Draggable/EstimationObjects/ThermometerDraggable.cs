using UnityEngine;

/// <summary>
/// Manages the interaction between the spray gun and the thermometer.
/// </summary>
public class ThermometerDraggable : Draggable
{
    public Thermometer controller;
    public float velocity;

    private Vector3 _previousControllerPosition;
    private Vector3 _currentControllerPosition;
    
    void Update()
    {
        _previousControllerPosition = _currentControllerPosition;
        _currentControllerPosition = ApplicationController.Instance.sprayGun.pinSpotOrigin.position;
    }
    
    /// <summary>
    /// Compare previous and current controller position
    /// </summary>
    /// <returns>Y difference multiplied by velocity</returns>
    float GetControllerChangeValue()
    {
        float previousY = _previousControllerPosition.y;
        float currentY = _currentControllerPosition.y;
        float change = currentY - previousY;
        return change * velocity;
    }
    
    public override void Drag()
    {
        if (controller.thermometerSet)
            return;
        
        controller.ChangeLevelValue(GetControllerChangeValue());
    }

    public override void Release()
    {
        if(!isDragged)
            return;
        isDragged = false;
    }
}

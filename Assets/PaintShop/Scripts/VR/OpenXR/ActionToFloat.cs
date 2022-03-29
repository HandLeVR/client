using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.Samples.ControllerSample;

/// <summary>
/// Maps the input action to a float value.
/// </summary>
public class ActionToFloat : ActionToControl
{
    [HideInInspector] public float currentValue;
    
    protected override void OnActionPerformed(InputAction.CallbackContext ctx) => UpdateValue(ctx);
    protected override void OnActionStarted(InputAction.CallbackContext ctx) => UpdateValue(ctx);
    protected override void OnActionCanceled(InputAction.CallbackContext ctx) => UpdateValue(ctx);

    private void UpdateValue(InputAction.CallbackContext ctx) => currentValue = ctx.ReadValue<float>();
}

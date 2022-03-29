using UnityEngine;

/// <summary>
/// Represents the spray gun used in a recording..
/// </summary>
public class SprayGunRecorder : SprayGun
{
    [HideInInspector] public float sprayingValue;
    [HideInInspector] public float triggerValue;
    [HideInInspector] public float wideStreamRegulationValue;
    [HideInInspector] public Vector3 startPos;
    [HideInInspector] public Quaternion direction;
    [HideInInspector] public bool isAdjustingPaintStand;

    public PaintStandMovement lastPaintStandMovement
    {
        get => _lastPaintStandMovement;
        set
        {
            if (_lastPaintStandMovement)
                _lastPaintStandMovement.ShowHand(0);
            if (!value)
                _meshList.ForEach(m => m.SetMaterialsAlpha(1));
            else 
                value.ShowHand(1);
            _lastPaintStandMovement = value;
        }
    }

    private PaintStandMovement _lastPaintStandMovement;

    public override void Awake()
    {
        base.Awake();
        sprayingValue = 0.0f;
        triggerValue = 0.0f;
    }
    
    protected override void AnimateSprayGun()
    {
        TriggerRotationPoint.localRotation = _initialTriggerRotation * Quaternion.Euler(0, maxTriggerRotation * GetSprayingValue(), 0);
    }

    protected override void DoSpray(Vector3 startPos, Quaternion direction)
    {
        base.DoSpray(this.startPos, this.direction);
    }

    protected override void DoUiInteraction()
    {
        // do nothing to prevent interacting with the UI
    }

    public override bool IsDoingHandInteraction()
    {
        if (!isAdjustingPaintStand && lastPaintStandMovement)
            lastPaintStandMovement.ShowHand(0);
        return isAdjustingPaintStand;
    }

    protected override void HandlePaintStandMovement()
    {
        // paint stand rotation and translation is done through the recording

        if (ApplicationController.Instance.currentPaintStandMovement)
            lastPaintStandMovement = ApplicationController.Instance.currentPaintStandMovement;
        
        if (isAdjustingPaintStand)
        {
            if (GetSprayingValue() > 0.05f)
            {
                lastPaintStandMovement?.ShowHand(1);
                _meshList.ForEach(m => m.SetMaterialsAlpha(0.5f));
            }
            else
            {
                lastPaintStandMovement?.ShowHand(0.5f);
                _meshList.ForEach(m => m.SetMaterialsAlpha(1));
            }
        }
        else
        {
            lastPaintStandMovement?.ShowHand(0);
            _meshList.ForEach(m => m.SetMaterialsAlpha(1));
        }
    }
    
    public override float GetSprayingValue()
    {
        return sprayingValue;
    }
    
    public override float GetTriggerValue()
    {
        // override trigger value in old recordings where the trigger value was not recorded
        if (sprayingValue > 0 && triggerValue <= 0)
            return 1;
        return triggerValue;
    }

    public override float GetWideStreamRegulationValue()
    {
        return wideStreamRegulationValue;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        _lastStartPos = startPos;
        _lastDirection = direction;
    }
}

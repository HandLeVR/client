using System;
using UnityEngine;

/// <summary>
/// Parent class for paint stand movements. Provides functions to show and hide a virtual
/// hand when entering or leaving the corresponding collider.
/// </summary>
public abstract class PaintStandMovement : MonoBehaviour
{
    public GameObject visualHand;
    public GameObject centralRod;

    protected bool isMoving;
    protected Transform workpiece;
    
    private bool isTouched;

    private void Awake()
    {
        workpiece = GetComponentInParent<PaintStand>().transform.parent.GetComponentInChildren<CustomDrawable>()
            .transform;
    }

    /// <summary>
    /// If the muzzle of the paint gun enters the collider the CurrentPaintStandAdjustment of the
    /// ApplicationController is set. This indicates, that a area of the paint stand is touched which
    /// allows to adjust the paint stand.
    /// </summary>
    protected void OnCollisionEnter(Collision other)
    {
        SprayGun sprayGun = GetSprayGun(other);
        if (other.gameObject.layer != LayerMask.NameToLayer("SprayGun") || isMoving ||
            sprayGun.IsDoingHandInteraction() || !sprayGun.IsPrimarySprayGun())
            return;

        ApplicationController.Instance.currentPaintStandMovement = this;
        isTouched = true;
    }

    /// <summary>
    /// Resets everything if the muzzle of the paint gun leaves the collider.
    /// </summary>
    protected void OnCollisionExit(Collision other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("SprayGun") || !GetSprayGun(other).IsPrimarySprayGun() ||
            ApplicationController.Instance.currentPaintStandMovement != this)
            return;

        isTouched = false;
        if (!isMoving)
        {
            // only reset the hand and the PaintController variable if the paint stand is currently not moving
            // this ensures that the hand is still visible and we can still adjust the paint stand
            ShowHand(0);
            ApplicationController.Instance.currentPaintStandMovement = null;
        }
    }

    /// <summary>
    /// Displays the hand with the specified alpha.
    /// </summary>
    public void ShowHand(float alpha)
    {
        if (alpha > 0)
        {
            visualHand.SetActive(true);
            SkinnedMeshRenderer meshRenderer = visualHand.GetComponentInChildren<SkinnedMeshRenderer>();
            meshRenderer.SetMaterialsAlpha(alpha);
        }
        else
        {
            visualHand.SetActive(false);
        }
    }

    /// <summary>
    /// Is called if the trigger of the paint gun is not pressed.
    /// </summary>
    public void NoMovement()
    {
        ApplicationController.Instance.paintStand.changedTransform = false;
        
        isMoving = false;
        if (!isTouched)
        {
            // resets the hand and the PaintController variable if the muzzle of the
            // paint gun left the collider beforehand
            ShowHand(0);
            ApplicationController.Instance.currentPaintStandMovement = null;
        }
        else if (!visualHand.activeSelf)
        {
            ShowHand(0.5f);
        }
    }

    /// <summary>
    /// Allows to move the object. The movement depends on the child object.
    /// </summary>
    public abstract void ExecuteMovement(Transform pinSpotOrigin);

    private SprayGun GetSprayGun(Collision other)
    {
        SprayGun sprayGun = other.gameObject.GetComponentInParent<SprayGun>();
        if (!sprayGun)
            // for fallback spray gun
            sprayGun = other.gameObject.GetComponentInChildren<SprayGun>();
        return sprayGun;
    }
}

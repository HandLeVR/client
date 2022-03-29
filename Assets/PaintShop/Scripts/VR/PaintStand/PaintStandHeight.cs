using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Allows to adjust the height of the paint stand.
/// </summary>
public class PaintStandHeight : PaintStandMovement
{
    public Transform topStopper;
    public Transform bottomStopper;
    private List<Transform> adjustableObjects;
    private Vector3 lastPos;

    private void Start()
    {
        adjustableObjects = new List<Transform>();
        FindObjectsOfType<PaintStandHeight>().ToList().ForEach(x => adjustableObjects.Add(x.transform));
        adjustableObjects.Add(workpiece);
    }
    
    public override void ExecuteMovement(Transform pinSpotOrigin)
    {
        if (isMoving)
        {
            Vector3 currentPos = pinSpotOrigin.position;
            Vector3 relCurrentPos = centralRod.transform.InverseTransformPoint(currentPos);
            Vector3 relLastPos = centralRod.transform.InverseTransformPoint(lastPos);
            Vector3 offset = new Vector3(0, 0, (relCurrentPos - relLastPos).z);
            if (ValidateNewPositionAll(offset))
            {
                offset = centralRod.transform.TransformVector(offset);
                foreach (Transform obj in adjustableObjects)
                    obj.position += offset;
            }
            lastPos = currentPos;
            
            // also move the clone of the workpiece in evaluation mode
            if (ApplicationController.Instance.workpieceClone)
            {
                ApplicationController.Instance.workpieceClone.transform.position = workpiece.position;
                ApplicationController.Instance.workpieceClone.transform.rotation = workpiece.rotation;
            }
        }
        else
        {
            lastPos = pinSpotOrigin.position;
            isMoving = true;
            ShowHand(1);
        }
        ApplicationController.Instance.paintStand.changedTransform = true;
    }

    /// <summary>
    /// Checks whether the positions of all bars are valid (no stopper hits the central rod).
    /// </summary>
    private bool ValidateNewPositionAll(Vector3 offset)
    {
        foreach (Transform obj in adjustableObjects)
        {
            PaintStandHeight paintStandHeight = obj.GetComponent<PaintStandHeight>();
            if (paintStandHeight && !paintStandHeight.ValidateNewPosition(offset))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Checks whether the positions of this bar is valid (no stopper hits the central rod).
    /// </summary>
    private bool ValidateNewPosition(Vector3 offset)
    {
        Vector3 topStopperRelPos = centralRod.transform.InverseTransformPoint(topStopper.position) + offset;
        Vector3 bottomStopperRelPos = centralRod.transform.InverseTransformPoint(bottomStopper.position) + offset;
        return topStopperRelPos.z > 0.028 && bottomStopperRelPos.z < -0.025;
    }
    
    /// <summary>
    /// Moves the hand along the currently touched bar if the paint stand is not moved.
    /// </summary>
    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("SprayGun") && !isMoving && other.gameObject.GetComponentInParent<SprayGun>().IsPrimarySprayGun())
        {
            Vector3 relPos = transform.InverseTransformPoint(other.transform.position);
            Vector3 visHandPos = visualHand.transform.localPosition;
            visHandPos.z = relPos.z;
            visualHand.transform.localPosition = visHandPos;
        }
    }
}

using UnityEngine;

/// <summary>
/// Allows to adjust the rotation of the paint stand.
/// </summary>
public class PaintStandRotation : PaintStandMovement
{
    private Vector3 lastDirection;

    public override void ExecuteMovement(Transform pinSpotOrigin)
    {
        if (isMoving)
        {
            Vector3 currentDirection = pinSpotOrigin.position - centralRod.transform.position;
            Vector3 relCurrentDirection = centralRod.transform.InverseTransformDirection(currentDirection);
            relCurrentDirection.x = 0;
            Vector3 relLastDirection = centralRod.transform.InverseTransformDirection(lastDirection);
            relLastDirection.x = 0;
            float angle = Vector3.SignedAngle(relCurrentDirection, relLastDirection, centralRod.transform.right);
            centralRod.transform.RotateAround(centralRod.transform.position, centralRod.transform.right, angle);
            workpiece.transform.RotateAround(centralRod.transform.position, centralRod.transform.right, angle);
            lastDirection = currentDirection;
            
            // also move the clone of the workpiece in evaluation mode
            if (ApplicationController.Instance.workpieceClone)
            {
                ApplicationController.Instance.workpieceClone.transform.position = workpiece.position;
                ApplicationController.Instance.workpieceClone.transform.rotation = workpiece.rotation;
            }
        }
        else
        {
            lastDirection = pinSpotOrigin.position - centralRod.transform.position;
            isMoving = true;
            ShowHand(1);
        }
        ApplicationController.Instance.paintStand.changedTransform = true;
    }
}
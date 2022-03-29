
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Represents a draggable ball.
/// </summary>
public class Ball : Draggable
{
    private bool lockBall;

    private void Start()
    {
        lockBall = false;
    }

    void Update()
    {
        if (isDragged)
            SelectionPanel.Instance.CheckDistance(transform);
    }

    public override void Drag()
    {
        if (lockBall)
            return;
        
        if (!isDragged)
        {
            SpringJoint joint = gameObject.GetComponent<SpringJoint>();
            if (joint)
            {
                joint.connectedBody.GetComponentInParent<ChoiceItem>().HasBall = false;
                Destroy(joint);
            }

            body.useGravity = true;
        }

        base.Drag();
    }
    
    public override void Release()
    {
        if (!isDragged)
            return;
        
        transform.parent = null;
        body.isKinematic = false;
        if (SelectionPanel.Instance.selectedChoice && !SelectionPanel.Instance.selectedChoice.HasBall)
        {
            SpringJoint joint = gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = Vector3.zero;
            joint.spring = 100;
            joint.damper = 10;
            joint.tolerance = 0;
            joint.connectedBody = SelectionPanel.Instance.selectedChoice.circleRigidbody;
            body.useGravity = false;
            SelectionPanel.Instance.selectedChoice.HasBall = true;
            if (LearningScenariosTaskController.Instance.currentSubTaskController is CoatSelectionController)
            {
                lockBall = true;
            }
        }
        else
            body.AddForce(ApplicationController.Instance.sprayGun.velocity * 100);
        isDragged = false;
        // needed to allow destruction of the game object on scene change
        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
        SelectionPanel.Instance.UpdateSelected();
    }
}

using System.Linq;
using QuickOutline;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Base class for objects that can be grabbed and released.
/// </summary>
public class Draggable : MonoBehaviour
{
    public bool isDisabled;
    
    protected bool isDragged;
    protected Rigidbody body;
    
    private Outline[] _outlines;

    protected void Awake()
    {
        body = GetComponent<Rigidbody>();
        _outlines = GetComponentsInChildren<Outline>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDisabled || other.gameObject.layer != LayerMask.NameToLayer("SprayGun"))
            return; 
        ApplicationController.Instance.currentDraggables.Add(this);
        Highlight(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (isDisabled || other.gameObject.layer != LayerMask.NameToLayer("SprayGun"))
            return;
        ApplicationController.Instance.currentDraggables.Remove(this);
        Highlight(false);
    }

    /// <summary>
    /// Controls the highlighting of the object if there outlines attached to the object.
    /// </summary>
    private void Highlight(bool highlight)
    {
        _outlines.ToList().ForEach(outline => outline.enabled = highlight);
    }

    public virtual void Drag()
    {
        if (isDragged)
            return;

        transform.parent = ApplicationController.Instance.sprayGun.transform;
        body.isKinematic = true;
        isDragged = true;
    }

    public virtual void Release()
    {
        if (!isDragged)
            return;
        
        transform.parent = null;
        body.isKinematic = false;
        // simulate throwing of the object
        body.AddForce(ApplicationController.Instance.sprayGun.velocity * 100);
        isDragged = false;
        // needed to allow destruction of the game object on scene change
        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
    }

    public bool IsBeingDragged()
    {
        return isDragged;
    }
}

using UnityEngine;

/// <summary>
/// Allows to display an individual hand attached to a handle.
/// </summary>
public class HandDisplayHandle : MonoBehaviour
{
    public GameObject hand;

    private Draggable _parentDraggable;
    private SkinnedMeshRenderer _meshRenderer;

    private void Start()
    {
        _parentDraggable = GetComponentInParent<Draggable>();
        _meshRenderer = hand.GetComponentInChildren<SkinnedMeshRenderer>();
    }

    private void Update()
    {
        if (ApplicationController.Instance.currentDraggable != _parentDraggable)
        {
            hand.SetActive(false);
            return;
        }
        
        Color color = _meshRenderer.material.color;
        // solid or transparent hand
        color.a = _parentDraggable.IsBeingDragged() ? 1f : 0.4f;
        _meshRenderer.material.color = color;

        hand.SetActive(true);
    }
}
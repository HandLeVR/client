using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows to display a hand bidirectional if the object is touched by the spray gun.
/// This applies to handle of BigCanister and the golden spray gun.
/// This script manages display of 2 hands depending on direction.
/// </summary>
public class HandDisplayBidirectional : MonoBehaviour
{
    public GameObject frontHand;
    public GameObject backHand;
    public bool isBigCanister;

    private Draggable _parentDraggable;
    private readonly List<SkinnedMeshRenderer> _meshRenderers = new();

    private void Start()
    {
        _parentDraggable = GetComponentInParent<Draggable>();
        _meshRenderers.Add(frontHand.GetComponentInChildren<SkinnedMeshRenderer>());
        _meshRenderers.Add(backHand.GetComponentInChildren<SkinnedMeshRenderer>());
    }

    void Update()
    {
        // only display if object is currentDraggable
        if (ApplicationController.Instance.currentDraggable != _parentDraggable)
        {
            backHand.SetActive(false);
            frontHand.SetActive(false);
            return;
        }

        Color color = _meshRenderers[0].material.color;
        // solid or transparent hand
        color.a = _parentDraggable.IsBeingDragged() ? 1f : 0.4f;
        foreach (var meshRenderer in _meshRenderers)
            meshRenderer.material.color = color;

        Vector3 direction = (transform.position - ApplicationController.Instance.sprayGun.pinSpotOrigin.position)
            .normalized;

        float angle;

        // use direction between object and spray gun to decide from which side the spray gun is approaching
        if (isBigCanister)
        {
            angle = Vector3.Angle(transform.right, direction);

            if (angle >= 90)
            {
                backHand.SetActive(true);
                frontHand.SetActive(false);
            }
            else
            {
                backHand.SetActive(false);
                frontHand.SetActive(true);
            }
        }
        else
        {
            angle = Vector3.Angle(transform.up, Vector3.up);

            if (angle >= 90)
            {
                if (!_parentDraggable.IsBeingDragged())
                {
                    backHand.SetActive(true);
                    frontHand.SetActive(false);
                }
            }
            else
            {
                if (!_parentDraggable.IsBeingDragged())
                {
                    backHand.SetActive(false);
                    frontHand.SetActive(true);
                }
            }
        }
    }
}
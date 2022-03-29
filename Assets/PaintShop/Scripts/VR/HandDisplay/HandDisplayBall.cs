using UnityEngine;

/// <summary>
/// Displays a hand at the ball if touched by the spray gun.
/// </summary>
public class HandDisplayBall : MonoBehaviour
{
    public GameObject hand;
    public GameObject target;
    public float minDistanceFromCenter;

    private float _maxDistance;
    private bool _active;
    private Draggable _parentDraggable;
    private SkinnedMeshRenderer _meshRenderer;

    private void Start()
    {
        _maxDistance = GetComponent<SphereCollider>().radius;
        _parentDraggable = GetComponentInParent<Draggable>();
        _meshRenderer = hand.GetComponentInChildren<SkinnedMeshRenderer>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("SprayGun") &&
            other.gameObject.GetComponentInParent<SprayGun>().IsPrimarySprayGun())
        {
            hand.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("SprayGun") ||
            !other.gameObject.GetComponentInParent<SprayGun>().IsPrimarySprayGun())
            return;

        // only display the hand if this object is the currentDraggable
        hand.SetActive(ApplicationController.Instance.currentDraggable == _parentDraggable);

        // distance between spray hun and object center
        Vector3 towardsSprayGun = ApplicationController.Instance.sprayGun.pinSpotOrigin.position -
                                  target.transform.position;
        towardsSprayGun = Vector3.Normalize(towardsSprayGun);

        float currentDistanceToSprayGun = Vector3.Distance(target.transform.position,
            ApplicationController.Instance.sprayGun.pinSpotOrigin.position);
        float distanceFromCenter;

        // set distance within the parameters
        if (currentDistanceToSprayGun > _maxDistance)
            distanceFromCenter = _maxDistance;
        else
            distanceFromCenter = currentDistanceToSprayGun > minDistanceFromCenter
                ? currentDistanceToSprayGun
                : minDistanceFromCenter;

        Vector3 newPosition = target.transform.position;
        newPosition = newPosition + towardsSprayGun * distanceFromCenter;

        hand.transform.position = newPosition;
    }

    private void Update()
    {
        if (_parentDraggable.IsBeingDragged() && ApplicationController.Instance.currentDraggable == _parentDraggable)
        {
            // solid color
            Color color = _meshRenderer.material.color;
            color.a = 1f;
            _meshRenderer.material.color = color;
        }

        if (!_parentDraggable.IsBeingDragged() && ApplicationController.Instance.currentDraggable == _parentDraggable)
        {
            // transparent hand
            Color color = _meshRenderer.material.color;
            color.a = 0.4f;
            _meshRenderer.material.color = color;
        }
    }
}
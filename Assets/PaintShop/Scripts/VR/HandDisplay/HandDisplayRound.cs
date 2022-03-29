using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

/// <summary>
/// Allows displaying hands at the RoundCanister.
/// Manages 3 hands.
/// </summary>
public class HandDisplayRound : MonoBehaviour
{
    public GameObject topHand;
    public GameObject bottomHand;
    public GameObject bodyHand;
    public Transform rangeSensorTop;
    public Transform rangeSensorBottom;
    public Transform centerOfObject;
    public float distance = 0.05f;
    public float range;

    private Vector3 _topHandLocalPosition;
    private Vector3 _bottomHandLocalPosition;
    private Vector3 _bodyHandLocalPosition;
    private GameObject _activeHand;
    private float _rotationDegrees;
    private Draggable _parentDraggable;

    private readonly List<SkinnedMeshRenderer> _meshRenderers = new();

    private void Start()
    {
        _parentDraggable = GetComponentInParent<Draggable>();

        _topHandLocalPosition = topHand.transform.localPosition;
        _bottomHandLocalPosition = bottomHand.transform.localPosition;

        _meshRenderers.Add(topHand.GetComponentInChildren<SkinnedMeshRenderer>());
        _meshRenderers.Add(bottomHand.GetComponentInChildren<SkinnedMeshRenderer>());
        _meshRenderers.Add(bodyHand.GetComponentInChildren<SkinnedMeshRenderer>());
    }

    void Update()
    {
        // Only display if object is currentDraggable
        if (ApplicationController.Instance.currentDraggable != _parentDraggable)
        {
            topHand.SetActive(false);
            bottomHand.SetActive(false);
            bodyHand.SetActive(false);
            return;
        }
        
        Color color = _meshRenderers[0].material.color;
        // solid or transparent hand
        color.a = _parentDraggable.IsBeingDragged() ? 1f : 0.4f;
        foreach (var meshRenderer in _meshRenderers)
            meshRenderer.material.color = color;

        // use sensors to determine which hand should be active
        Vector3 target = ApplicationController.Instance.sprayGun.pinSpotOrigin.position;
        if (Vector3.Distance(target, rangeSensorTop.position) < range)
            _activeHand = topHand;
        else
            _activeHand = Vector3.Distance(target, rangeSensorBottom.position) < range ? bottomHand : bodyHand;

        topHand.SetActive(false);
        bottomHand.SetActive(false);
        bodyHand.SetActive(false);
        _activeHand.SetActive(true);

        // do not change local rotation if object is being dragged
        if (_parentDraggable.IsBeingDragged())
            return;

        // rotate according to which hand is active
        if (_activeHand == topHand)
        {
            topHand.transform.localPosition = _topHandLocalPosition;
            float distanceToPlane = Vector3.Dot(transform.up, target - _activeHand.transform.position);
            Vector3 plantPoint = target - _activeHand.transform.up * distanceToPlane;
            _activeHand.transform.LookAt(plantPoint, transform.up);
            _activeHand.transform.Rotate(0, 90, 0, Space.Self);
        }

        if (_activeHand == bottomHand)
        {
            bottomHand.transform.localPosition = _bottomHandLocalPosition;
            float distanceToPlane = Vector3.Dot(-transform.up, target - _activeHand.transform.position);
            Vector3 plantPoint = target - _activeHand.transform.up * distanceToPlane;
            _activeHand.transform.LookAt(plantPoint, -transform.up);
            _activeHand.transform.Rotate(0, 90, 0, Space.Self);
        }

        if (_activeHand == bodyHand)
        {
            Vector3 towardsSprayGun = (target - centerOfObject.position).normalized;
            _activeHand.transform.position = (centerOfObject.position + towardsSprayGun * distance);
            _activeHand.transform.LookAt(centerOfObject.position, centerOfObject.up);
        }
    }
}
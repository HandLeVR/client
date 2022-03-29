using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Draws a gravity based launch arc for teleportation aiming.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class LaunchArc : MonoBehaviour
{
    // velocity of projectile used to simulate the arc
    public float velocity = 12f;

    // a lower amount of points will result in a lower resolution arc.
    public float maxAmountOfPoints;
    public LayerMask collidableLayers;

    public GameObject targetObject;
    public Color validTeleportColor;
    public Color invalidTeleportColor;

    [HideInInspector] public bool displayArc = false;
    [HideInInspector] public Vector3 teleportTargetPoint;
    [HideInInspector] public bool validTeleport = false;

    private float _gravity;
    private float _angle;
    private float _height;
    private LineRenderer _lineRenderer;
    private Transform _pinSpotOrigin;
    private GameObject target;
    private float _maxDistance;
    private MeshRenderer _targetMeshRenderer;


    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 0;
        // Use built-in gravity for arc equation
        _gravity = Mathf.Abs(Physics.gravity.y);
        target = Instantiate(targetObject);
        Vector3 scale = target.transform.localScale;
        scale.x = 20f;
        scale.y = 20f;
        scale.z = 40f;
        target.transform.localScale = scale;
        _targetMeshRenderer = target.GetComponent<MeshRenderer>();
        _lineRenderer.enabled = false;
        _targetMeshRenderer.enabled = false;
        validTeleport = false;
    }

    private void FixedUpdate()
    {
        if (displayArc)
        {
            // Enables all visuals
            _pinSpotOrigin = ApplicationController.Instance.sprayGun.pinSpotOrigin;
            _targetMeshRenderer.enabled = true;
            DrawArc();
        }
        else
        {
            // Disables all visuals
            _lineRenderer.enabled = false;
            _targetMeshRenderer.enabled = false;
            validTeleport = false;
        }

        // Color the visualization according to the validity of the teleport
        if (validTeleport)
        {
            _lineRenderer.material.color = validTeleportColor;
            _targetMeshRenderer.sharedMaterial.color = validTeleportColor;
        }
        else
        {
            _lineRenderer.material.color = invalidTeleportColor;
            _targetMeshRenderer.sharedMaterial.color = invalidTeleportColor;
        }
    }

    private void DrawArc()
    {
        // assign variables for the arc equation: direction, angle, height

        Vector3 direction = _pinSpotOrigin.forward.normalized;
        _angle = Mathf.Asin(direction.y);
        _height = _pinSpotOrigin.position.y;

        // prepare loop and do first iteration by adding the origin point

        bool continueLoop = true;
        int i = 1;
        List<Vector3> points = new List<Vector3>();
        Vector3 point = _pinSpotOrigin.position;
        points.Add(_pinSpotOrigin.position);

        while (continueLoop)
        {
            // determines time and step size depending on the maxAmountOfPoints
            float time = i / maxAmountOfPoints;
            float step = velocity * time * Mathf.Cos(_angle);

            // do step along direction vector
            point = _pinSpotOrigin.position + direction * step;

            // calculate height at given time
            point.y = _height + velocity * time * Mathf.Sin(_angle) - 0.5f * _gravity * time * time;

            i++;

            RaycastHit hit;
            // if it collides with something collidable, place last point at the point of collision and end the loop
            if (Physics.Linecast(points[points.Count - 1], point, out hit, collidableLayers))
            {
                continueLoop = false;
                point = hit.point;

                validTeleport = hit.transform.gameObject.layer == LayerMask.NameToLayer("Teleport");
            }

            points.Add(point);

            if (i > maxAmountOfPoints)
            {
                continueLoop = false;
            }
        }

        _lineRenderer.positionCount = points.Count;
        _lineRenderer.SetPositions(points.ToArray());
        _lineRenderer.enabled = true;

        Vector3 targetPosition = points[points.Count - 1];
        targetPosition.y = targetPosition.y + 0.0176f;
        target.transform.position = targetPosition;
        teleportTargetPoint = points[points.Count - 1];
    }
}
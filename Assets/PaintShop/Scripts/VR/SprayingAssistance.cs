using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.Input;

/// <summary>
/// Provides assistance mechanisms to support the user while spraying.
/// </summary>
public class SprayingAssistance : MonoBehaviour
{
    public Transform pinSpotOrigin;
    public Transform upperRayCastOrigin;
    public Transform lowerRayCastOrigin;
    public GameObject distanceMarker1;
    public GameObject distanceMarker2;
    public InputActionReference hapticAction;
    public LayerMask collidableLayers;

    [Tooltip("The color of the angle helper ray. Transparency will be set in the script.")]
    public Color color;

    [Header("Angle Helper settings")] 
    public float optimalAngle = 90f;
    [Tooltip("The angle to the optimal angle where the fading of the angle ray starts.")]
    public float angleThreshold = 20f;
    public float vectorLerpSpeed = 25f;

    // optimal spraying distance controlled by the paint controller
    private float _optimalDistance = 0.175f;
    private float _optimalDistanceRayOffset = 0.025f;

    // new and prev direction for smoothness
    private Vector3 _newDir;
    private Vector3 _previousDir;

    // visuals
    private readonly List<LineDrawer> _distanceRays = new();
    private LineDrawer _angleLineDrawer;
    private Material _materialDistanceMarker1;
    private Material _materialDistanceMarker2;
    
    // the spray gun
    private SprayGun _sprayGun;
    
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    void Start()
    {
        foreach (Transform child in transform.Find("Distance Rays"))
            _distanceRays.Add(child.GetComponent<LineDrawer>());
        _angleLineDrawer = transform.Find("Angle Ray").GetComponent<LineDrawer>();
        _materialDistanceMarker1 = distanceMarker1.GetComponentInChildren<MeshRenderer>().material;
        _materialDistanceMarker2 = distanceMarker2.GetComponentInChildren<MeshRenderer>().material;
        _sprayGun = transform.parent.GetComponent<SprayGun>();
    }

    void Update()
    {
        if (!_sprayGun.CanSpray())
        {
            distanceMarker1.SetActive(false);
            distanceMarker2.SetActive(false);
            return;
        }

        // recalculate optimal distance
        _optimalDistance = (PaintController.Instance.minSprayDistance + PaintController.Instance.maxSprayDistance) / 2;
        _optimalDistanceRayOffset = _optimalDistance - PaintController.Instance.minSprayDistance;

        // check if the workpiece is being hit
        Physics.Raycast(pinSpotOrigin.position, pinSpotOrigin.forward, out var workpieceHit,
            Mathf.Infinity, collidableLayers);

        // set distance ray if needed
        if (ApplicationController.Instance.sprayingAssistanceType.HasFlag(SprayingAssistanceType.DistanceRay))
        {
            if (workpieceHit.collider)
            {
                float distance = Vector3.Distance(pinSpotOrigin.position, workpieceHit.point);
                Vector3 thresholdFar = LerpByDistance(pinSpotOrigin.position, workpieceHit.point,
                    _optimalDistance - _optimalDistanceRayOffset);
                Vector3 thresholdClose = LerpByDistance(pinSpotOrigin.position, workpieceHit.point,
                    _optimalDistance + _optimalDistanceRayOffset);

                if (distance > _optimalDistance + _optimalDistanceRayOffset)
                {
                    // too far
                    _distanceRays[0].DrawLineInGameView(pinSpotOrigin.position, thresholdFar, Color.red);
                    _distanceRays[1].DrawLineInGameView(thresholdFar, thresholdClose, Color.green);
                    _distanceRays[2].DrawLineInGameView(thresholdClose, workpieceHit.point, Color.blue);
                }
                else if (distance < _optimalDistance - _optimalDistanceRayOffset)
                {
                    // too close
                    _distanceRays[2].DrawLineInGameView(pinSpotOrigin.position, workpieceHit.point, Color.red);
                }
                else
                {
                    // perfect
                    _distanceRays[1].DrawLineInGameView(thresholdFar, workpieceHit.point, Color.green);
                    _distanceRays[2].DrawLineInGameView(pinSpotOrigin.position, thresholdFar, Color.red);
                }
            }
            // check if a hit box around the workpiece is being hit
            else if (Physics.Raycast(pinSpotOrigin.position, pinSpotOrigin.forward, out _, Mathf.Infinity,
                1 << LayerMask.NameToLayer("Hitbox")) || _sprayGun.isInPaintRange)
            {
                Vector3 tmpEndPoint = pinSpotOrigin.position + pinSpotOrigin.forward * 30;
                Vector3 thresholdFar = LerpByDistance(pinSpotOrigin.position, tmpEndPoint,
                    _optimalDistance - _optimalDistanceRayOffset);
                Vector3 thresholdClose = LerpByDistance(pinSpotOrigin.position, tmpEndPoint,
                    _optimalDistance + _optimalDistanceRayOffset);
                _distanceRays[0].DrawLineInGameView(pinSpotOrigin.position, thresholdFar, Color.red);
                _distanceRays[1].DrawLineInGameView(thresholdFar, thresholdClose, Color.green);
                _distanceRays[2].DrawLineInGameView(thresholdClose, tmpEndPoint, Color.blue);
            }
        }

        if (!workpieceHit.collider)
        {
            // if workpiece is not hit then we don't use any distance checkers
            distanceMarker1.SetActive(false);
            distanceMarker2.SetActive(false);
            return;
        }

        // set distance marker if needed
        if (ApplicationController.Instance.sprayingAssistanceType.HasFlag(SprayingAssistanceType.DistanceMarker))
        {
            Vector3 forward = pinSpotOrigin.forward * _optimalDistance;

            Quaternion positiveAngle = Quaternion.AngleAxis(45, -pinSpotOrigin.right);
            Quaternion negativeAngle = Quaternion.AngleAxis(-45, -pinSpotOrigin.right);

            // calculate upper and lower positions of ray cast origins
            upperRayCastOrigin.position = pinSpotOrigin.position +
                                          Mathf.Tan(45 * Mathf.Deg2Rad) * _optimalDistance * pinSpotOrigin.up;
            lowerRayCastOrigin.position = pinSpotOrigin.position +
                                          Mathf.Tan(45 * Mathf.Deg2Rad) * _optimalDistance * -pinSpotOrigin.up;

            // determine color of the marker in dependence of the distance
            Color markerColor;
            float distance = Vector3.Distance(pinSpotOrigin.position, workpieceHit.point);
            if (distance > _optimalDistance + _optimalDistanceRayOffset)
                markerColor = Color.blue;
            else if (distance < _optimalDistance - _optimalDistanceRayOffset)
                markerColor = Color.red;
            else
                markerColor = Color.green;

            Plane plane = new Plane(forward, workpieceHit.point);

            Debug.DrawRay(pinSpotOrigin.position, forward, Color.green);
            Debug.DrawRay(upperRayCastOrigin.position, negativeAngle * forward * 3, Color.red);
            Debug.DrawRay(lowerRayCastOrigin.position, positiveAngle * forward * 3, Color.red);

            if (plane.Raycast(new Ray(upperRayCastOrigin.position, negativeAngle * forward), out float enter))
            {
                _materialDistanceMarker1.SetColor(ColorId, markerColor);
                distanceMarker1.SetActive(true);
                distanceMarker1.transform.position =
                    upperRayCastOrigin.position + (negativeAngle * forward).normalized * enter;
            }
            else
            {
                distanceMarker1.SetActive(false);
            }

            if (plane.Raycast(new Ray(lowerRayCastOrigin.position, positiveAngle * forward), out enter))
            {
                _materialDistanceMarker2.SetColor(ColorId, markerColor);
                distanceMarker2.SetActive(true);
                distanceMarker2.transform.position =
                    lowerRayCastOrigin.position + (positiveAngle * forward).normalized * enter;
            }
            else
            {
                distanceMarker2.SetActive(false);
            }
        }
        else
        {
            distanceMarker1.SetActive(false);
            distanceMarker2.SetActive(false);
        }

        // set angle ray if needed
        if (ApplicationController.Instance.sprayingAssistanceType.HasFlag(SprayingAssistanceType.AngleRay))
        {
            Physics.Raycast(pinSpotOrigin.position, pinSpotOrigin.forward, out var raycastHit, Mathf.Infinity,
                collidableLayers);

            if (raycastHit.collider)
                _newDir = Quaternion.Euler(optimalAngle - 90, 0, 0) * raycastHit.normal;

            // lerp the ideal position for smoothness
            Vector3 lerpDir = Vector3.Lerp(_previousDir, _newDir, Time.deltaTime * vectorLerpSpeed);
            _previousDir = lerpDir;

            // line is only drawn when workpiece is hit
            // transparency is lerped exponentially when current distance within the distance threshold
            if (raycastHit.collider)
            {
                float angleToTarget = Vector3.Angle(lerpDir, pinSpotOrigin.position - raycastHit.point);

                if (angleToTarget > angleThreshold)
                    angleToTarget = 1;
                else if (angleToTarget < angleThreshold)
                    angleToTarget = angleToTarget / angleThreshold;

                angleToTarget = Mathf.Pow(angleToTarget, 4);
                color.a = Mathf.Lerp(0f, 1f, angleToTarget);
                _angleLineDrawer.DrawLineInGameView(raycastHit.point,
                    raycastHit.point + lerpDir.normalized * _optimalDistance, color);
            }
        }

        if (ApplicationController.Instance.sprayingAssistanceType.HasFlag(SprayingAssistanceType.Vibration))
        {
            float difference = Mathf.Abs(workpieceHit.distance - _optimalDistance);
            float amplitude = Mathf.Clamp01(difference / _optimalDistance);
            if (workpieceHit.distance > _optimalDistance + _optimalDistanceRayOffset)
                OpenXRInput.SendHapticImpulse(hapticAction, amplitude, 1000);
            else if (workpieceHit.distance < _optimalDistance - _optimalDistanceRayOffset)
                OpenXRInput.SendHapticImpulse(hapticAction, amplitude, 1000);
            else
                OpenXRInput.StopHaptics(hapticAction);
        }
    }

    /// <summary>
    /// Calculate point P that lies between point A and B with distance x from A towards B.
    /// </summary>
    private Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
    {
        return x * Vector3.Normalize(B - A) + A;
    }

    [System.Flags]
    public enum SprayingAssistanceType
    {
        DistanceMarker = 0x1,
        DistanceRay = 0x2,
        AngleRay = 0x4,
        Vibration = 0x8
    }
}
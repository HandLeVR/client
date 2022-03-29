using UnityEngine;
using UnityEngine.XR.OpenXR.Samples.ControllerSample;

/// <summary>
/// Allows the user to teleport.
/// </summary>
[RequireComponent(typeof(LaunchArc))]
public class Teleport : MonoBehaviour
{
    public Transform playerRig;

    [HideInInspector] public bool teleportAllowed;

    private Transform _head;
    private LineDrawer _lineDrawer;
    private bool _buttonPressed;
    private bool _hit;
    private RaycastHit _currentHit;
    private LaunchArc _launchArc;

    private void Awake()
    {
        _launchArc = GetComponent<LaunchArc>();
        if (!playerRig && GetComponentInParent<TrackingModeOrigin>())
        {
            playerRig = GetComponentInParent<TrackingModeOrigin>().transform;
            _head = playerRig.GetChild(0);
        }
    }

    private void Update()
    {
        if (!teleportAllowed)
            return;

        if (ApplicationController.Instance.sprayGun.GetSprayingValue() > 0.05f)
        {
            _buttonPressed = true;
            _launchArc.displayArc = true;
        }
        else if (_buttonPressed)
        {
            _buttonPressed = false;
            if (_launchArc.validTeleport)
            {
                // we want to place ourself at the target position
                // therefore wo need to add the way from our head position to the player rig to the target position
                Vector3 target = playerRig.position - _head.position + _launchArc.teleportTargetPoint;
                playerRig.position = new Vector3(target.x, playerRig.transform.position.y, target.z);
            }
            _launchArc.displayArc = false;
        }
    }
}
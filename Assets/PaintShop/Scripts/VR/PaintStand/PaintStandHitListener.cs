using UnityEngine;

/// <summary>
/// Recognizes when the spray gun hits the paint stand and hands over the needed data to the PaintStandHitController.
/// </summary>
public class PaintStandHitListener : MonoBehaviour
{
    private AudioSource _audioSource;

    private void Awake()
    {
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.convex = true;
    }

    /// <summary>
    /// Gets the contact point and contact normal to properly position and rotate decals.
    /// </summary>
    public void OnCollisionEnter(Collision other)
    {
        if (!ApplicationController.Instance.sprayGun ||
            ApplicationController.Instance.sprayGun.currentMode != SprayGun.SprayGunMode.Spray)
            return;

        ContactPoint contact = other.GetContact(0);
        Vector3 hitPosition = contact.thisCollider.ClosestPointOnBounds(contact.point);
        Quaternion rot = Quaternion.FromToRotation(Vector3.down, contact.normal);
        hitPosition = hitPosition - 0.000001f * contact.normal;
        
        PaintStandHitController.Instance.HandleHit(hitPosition, rot, transform);
    }
}

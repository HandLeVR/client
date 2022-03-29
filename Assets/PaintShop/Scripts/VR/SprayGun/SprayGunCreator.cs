using UnityEngine;

/// <summary>
/// Creates a controller spray gun (if no real spray gun is connected) or a real spray gun.
/// </summary>
public class SprayGunCreator : MonoBehaviour
{
    public SprayGun controllerSprayGunPrefab;
    public RealSprayGun realSprayGunPrefab;

    public void OnEnable()
    {
        if (ApplicationController.Instance.sprayGun != null)
            return;

        ActivateSprayGun(!ApplicationController.Instance.RealSprayGunConnected()
            ? controllerSprayGunPrefab
            : realSprayGunPrefab);
    }

    /// <summary>
    /// Activates the corresponding spray gun.
    /// </summary>
    private void ActivateSprayGun(SprayGun prefab)
    {
        SprayGun sprayGun = Instantiate(prefab, transform);
        ApplicationController.Instance.sprayGun = sprayGun;
        Debug.Log("Activated " + prefab.name);
    }
}
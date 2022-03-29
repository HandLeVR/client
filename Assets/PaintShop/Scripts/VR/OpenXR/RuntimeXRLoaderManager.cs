using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

public class RuntimeXRLoaderManager : MonoBehaviour
{
    [Tooltip("Determines whether vr is started or stopped.")]
    public bool loadVR;

    private void Start()
    {
        if (loadVR)
            StartCoroutine(StartXR());
        else if (XRGeneralSettings.Instance.Manager.isInitializationComplete)
            StartCoroutine(NoVR());
    }

    IEnumerator StartXR()
    {
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
        XRGeneralSettings.Instance.Manager.StartSubsystems();
    }

    IEnumerator NoVR()
    {
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
    }
}
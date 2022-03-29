using System;
using UnityEngine;

/// <summary>
/// Creates a marker in the center of the play area. The color and shape of the marker is determined by the
/// MeshRenderer and the used material.
/// </summary>
public class CenterMarker : MonoBehaviour
{
    public Transform vrCameraPosition;
    public GameObject centerMarker;
    public Boolean centerMarkerActive;

    private MeshRenderer _meshRenderer;

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        _meshRenderer.enabled = centerMarkerActive;
        centerMarker.transform.position = vrCameraPosition.position;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a draggable canister.
/// </summary>
public class Canister : Draggable
{
    public Text flatLabel;
    public String labelText;

    private List<MeshRenderer> _meshRenderers;
    private CanvasGroup _canvasGroup;

    protected new void Awake()
    {
        base.Awake();
        _meshRenderers = GetComponentsInChildren<MeshRenderer>().ToList();
        _canvasGroup = flatLabel.GetComponentInParent<CanvasGroup>();
    }

    private void Start()
    {
        flatLabel.text = labelText;
    }

    public void FadeIn(float fadeTime)
    {
        _meshRenderers.ForEach(meshRenderer => meshRenderer.FadeInMaterials(fadeTime));
        _canvasGroup.alpha = 0;
        StartCoroutine(Lerp.Alpha(_canvasGroup, 1, fadeTime));
    }

    public void FadeOut(float fadeTime)
    {
        _meshRenderers.ForEach(meshRenderer => meshRenderer.FadeOutMaterials(fadeTime));
        StartCoroutine(Lerp.Alpha(_canvasGroup, 0, fadeTime));
    }


}

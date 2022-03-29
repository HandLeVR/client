using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Provides a functions modify the materials of Renderer.
/// </summary>
public static class RendererExtension
{
    public static void SetMaterialsAlpha(this Renderer mesh, float alpha)
    {
        if (alpha < 1)
            MaterialController.Instance.SetMaterialsTransparent(mesh);
        else
            MaterialController.Instance.SetMaterialsOpaque(mesh);

        foreach (var m in mesh.materials)
            m.color = new Color(m.color.r, m.color.g, m.color.b, alpha);
    }

    private static void DoOnCounterZero(Counter counter, UnityAction onZero)
    {
        counter.count--;
        if (counter.count == 0)
            onZero();
    }

    public static void FadeInMaterials(this Renderer mesh, float duration, UnityAction executeAfter = null)
    {
        mesh.SetMaterialsAlpha(0);
        Counter counter = new Counter();
        foreach (var m in mesh.materials)
        {
            counter.count++;
            Color startColor = new Color(m.color.r, m.color.g, m.color.b, 0);
            Color endColor = new Color(m.color.r, m.color.g, m.color.b, 1);
            CoroutineHelper.Instance.Coroutine(Lerp.Color(mesh, startColor, endColor, duration, () =>
                DoOnCounterZero(counter, () =>
                {
                    MaterialController.Instance.SetMaterialsOpaque(mesh);
                    executeAfter?.Invoke();
                })));
        }
    }

    public static void FadeMaterialsToOriginalAlpha(this Renderer mesh, float duration, UnityAction executeAfter = null)
    {
        float alpha = mesh.material.color.a;
        MaterialController.Instance.SetMaterialsTransparent(mesh);
        Counter counter = new Counter();
        foreach (var m in mesh.materials)
        {
            if (m.shader.name != "Universal Render Pipeline/Lit")
                continue;
            counter.count++;
            Color startColor = new Color(m.color.r, m.color.g, m.color.b, 0);
            Color endColor = new Color(m.color.r, m.color.g, m.color.b, alpha);
            CoroutineHelper.Instance.Coroutine(Lerp.Color(mesh, startColor, endColor, duration, () =>
                DoOnCounterZero(counter, () =>
                {
                    MaterialController.Instance.SetMaterialsOpaque(mesh);
                    executeAfter?.Invoke();
                })));
        }
    }

    public static void FadeOutMaterials(this Renderer mesh, float duration, UnityAction executeAfter = null)
    {
        MaterialController.Instance.SetMaterialsTransparent(mesh);
        Counter counter = new Counter();
        foreach (var m in mesh.materials)
        {
            if (m.shader.name != "Universal Render Pipeline/Lit")
                continue;
            counter.count++;
            Color startColor = new Color(m.color.r, m.color.g, m.color.b, 1);
            Color endColor = new Color(m.color.r, m.color.g, m.color.b, 0);
            CoroutineHelper.Instance.Coroutine(Lerp.Color(mesh, startColor, endColor, duration,
                () => DoOnCounterZero(counter, () => executeAfter?.Invoke())));
        }
    }

    public static void FadeOutMaterialsAndSetOriginalAlpha(this List<Renderer> meshs, float duration,
        UnityAction executeAfter = null)
    {
        Counter counter = new Counter();
        List<float> oldAlphas = new List<float>();
        foreach (var mesh in meshs)
        {
            MaterialController.Instance.SetMaterialsTransparent(mesh);
            foreach (var m in mesh.materials)
            {
                if (m.shader.name != "Universal Render Pipeline/Lit")
                    continue;
                counter.count++;
                oldAlphas.Add(m.color.a);
                Color startColor = new Color(m.color.r, m.color.g, m.color.b, m.color.a);
                Color endColor = new Color(m.color.r, m.color.g, m.color.b, 0);
                CoroutineHelper.Instance.Coroutine(Lerp.Color(mesh, startColor, endColor, duration,
                    () => DoOnCounterZero(counter, () =>
                    {
                        foreach (var mesh1 in meshs)
                        {
                            MaterialController.Instance.SetMaterialsOpaque(mesh);
                            foreach (var m1 in mesh1.materials)
                            {
                                if (m1.shader.name != "Universal Render Pipeline/Lit")
                                    continue;
                                m1.color = new Color(m1.color.r, m1.color.g, m1.color.b, oldAlphas[0]);
                                oldAlphas.RemoveAt(0);
                            }
                        }

                        executeAfter?.Invoke();
                    })));
            }
        }
    }

    public static void FadeInMaterial(this Renderer mesh, float duration, UnityAction executeAfter = null)
    {
        MaterialController.Instance.SetMaterialTransparent(mesh);
        Color startColor = new Color(mesh.material.color.r, mesh.material.color.g, mesh.material.color.b, 0);
        Color endColor = new Color(mesh.material.color.r, mesh.material.color.g, mesh.material.color.b, 1);

        CoroutineHelper.Instance.Coroutine(Lerp.Color(mesh, startColor, endColor, duration,
            () =>
            {
                MaterialController.Instance.SetMaterialOpaque(mesh);
                executeAfter?.Invoke();
            }));
    }

    public static void FadeOutMaterial(this List<Renderer> meshs, float duration, UnityAction executeAfter = null)
    {
        Counter counter = new Counter();
        foreach (var mesh in meshs)
        {
            counter.count++;
            MaterialController.Instance.SetMaterialTransparent(mesh);
            Color startColor = new Color(mesh.material.color.r, mesh.material.color.g, mesh.material.color.b, 1);
            Color endColor = new Color(mesh.material.color.r, mesh.material.color.g, mesh.material.color.b, 0);
            CoroutineHelper.Instance.Coroutine(Lerp.Color(mesh, startColor, endColor, duration,
                () => DoOnCounterZero(counter, () => executeAfter?.Invoke())));
        }
    }

    class Counter
    {
        public int count;
    }
}
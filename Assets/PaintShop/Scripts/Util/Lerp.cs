using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Provides functions to lerp different things.
/// </summary>
public static class Lerp
{
    public static IEnumerator Alpha(CanvasGroup canvasGroup, float to, float time, UnityAction onFinish = null)
    {
        return DoLerp(t => canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, to, t), time, onFinish);
    }

    public static IEnumerator Color(Renderer renderer, Color from, Color to, float time, UnityAction onFinish = null)
    {
        return DoLerp(t => renderer.material.color = Color32.Lerp(from, to, t), time, onFinish);
    }

    public static IEnumerator ColorShared(Renderer renderer, Color from, Color to, float time, UnityAction onFinish = null)
    {
        return DoLerp(t => renderer.sharedMaterial.color = Color32.Lerp(from, to, t), time, onFinish);
    }

    public static IEnumerator Color(Image image, Color from, Color to, float time, UnityAction onFinish = null)
    {
        return DoLerp(t => image.color = Color32.Lerp(from, to, t), time, onFinish);
    }

    public static IEnumerator Volume(AudioSource audioSource, float to, float time, UnityAction onFinish = null)
    {
        return DoLerp(t => audioSource.volume = Mathf.Lerp(audioSource.volume, to, t), time, onFinish);
    }

    public static IEnumerator DoLerp(UnityAction<float> lerpAction, float time, UnityAction onFinish = null)
    {
        float currentTime = 0f;

        while (currentTime < time)
        {
            currentTime += Time.deltaTime;
            if (currentTime > time) currentTime = time;
            lerpAction(currentTime / time);
            yield return null;
        }
        
        onFinish?.Invoke();
    }

    /// <summary>
    /// Lerps a float value. Another function is needed to return the lerped value.
    /// </summary>
    public static IEnumerator Float(UnityAction<float> lerpAction, float from, float to, float time)
    {
        float currentTime = 0f;

        while (currentTime < time)
        {
            currentTime += Time.deltaTime;
            if (currentTime > time) currentTime = time;
            lerpAction(Mathf.Lerp(from, to, currentTime / time));
            yield return null;
        }
    }
}
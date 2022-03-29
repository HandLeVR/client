using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// Displays quality criteria (sequentially) on the monitor.
/// </summary>
public class QualityCriteriaPanel : MonoBehaviour
{
    public CanvasGroup[] topics;
    [HideInInspector] public bool sequential;

    private IEnumerator coroutine;

    private void OnEnable()
    {
        if (sequential)
        {
            topics.ToList().ForEach(topic => topic.alpha = 0);
            coroutine = ShowTopics();
            StartCoroutine(coroutine);
        }
        else
        {
            topics.ToList().ForEach(topic => topic.alpha = 1);
        }
    }

    private IEnumerator ShowTopics()
    {
        foreach (var topic in topics)
        {
            StartCoroutine(Lerp.Alpha(topic, 1, 0.5f));
            yield return new WaitForSeconds(2f);
        }
    }

    private void OnDisable()
    {
        StopCoroutine(coroutine);
    }
}
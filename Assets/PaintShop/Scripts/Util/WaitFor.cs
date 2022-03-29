using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Provides WaitFor functions which can be used in coroutines.
/// </summary>
public class WaitFor : MonoBehaviour
{
    /// <summary>
    /// Executes the action after waiting the specified number of frames.
    /// </summary>
    public static IEnumerator Frames(int frameCount, UnityAction action)
    {
        while (frameCount > 0)
        {
            frameCount--;
            yield return null;
        }
        action.Invoke();
    }
    
    /// <summary>
    /// Waits until the next FixedUpdate is finished.
    /// </summary>
    public static IEnumerator NextFixedUpdate(UnityAction action)
    {
        yield return new WaitForFixedUpdate();
        // needed to execute action after the next FixedUpdate and not before
        yield return null;
        action.Invoke();
    }
    
    public static IEnumerator DoEndOfFrame(UnityAction action)
    {
        yield return new WaitForEndOfFrame();
        action.Invoke();
    }

    public static IEnumerator Seconds(float seconds, UnityAction action)
    {
        yield return new WaitForSeconds(seconds);
        
        action.Invoke();
    }
}

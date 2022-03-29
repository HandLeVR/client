using System.Collections;

/// <summary>
/// This class is needed to allow the execution of Coroutines in static methods. 
/// </summary>
public class CoroutineHelper : Singleton<CoroutineHelper>
{
    public void Coroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
}

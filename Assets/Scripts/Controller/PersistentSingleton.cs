using UnityEngine;

/// <summary>
/// Inherit from this base class to create a persistant singleton. That means this singleton is not removed on
/// scene changes.
/// </summary>
public class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>
    /// Static instance of PersistentGameObjectSingleton which allows it to be accessed by any other script.
    /// </summary>
    public static T Instance { get;  private set; }
 
    public void Awake()
    {
        if (Instance == null)
        {
            //if not, set instance to this
            Instance = GetComponent<T>();
            
            // DontDestroyOnLoad only works on root objects
            transform.parent = null;
 
            //Sets this to not be destroyed when reloading scene
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GlobalManager.
            DestroyImmediate(gameObject);
        }
    }
}
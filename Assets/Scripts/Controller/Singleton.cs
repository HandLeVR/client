using UnityEngine;

/// <summary>
/// Source: https://wiki.unity3d.com/index.php/Singleton
/// 
/// Inherit from this base class to create a singleton.
/// e.g. public class MyClassName : Singleton<MyClassName> {}
///
/// Note: We use a simple singleton (which is not really a singleton) as the objects deriving from the singleton
/// classes always already exist in a scene on start. This singleton is removed on scene changes.
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T m_Instance;
 
    /// <summary>
    /// Access singleton instance through this propriety.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (m_Instance == null)
            {
                // Search for existing instance.
                m_Instance = GameObjectUtil.FindGameObjectInScene<T>();
            }

            return m_Instance;
        }

        protected set => m_Instance = value;
    }
}
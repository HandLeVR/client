using UnityEditor;
using UnityEngine;

public class GameObjectUtil : MonoBehaviour
{
    /// <summary>
    /// Finds the object of the given type in the scene.
    /// We need to check if it is not a prefab beforehand.
    ///
    /// Source: https://answers.unity.com/questions/890636/find-an-inactive-game-object.html
    /// </summary>
    public static T FindGameObjectInScene<T>() where T : MonoBehaviour
    {
        foreach (T go in (T[]) Resources.FindObjectsOfTypeAll(typeof(T)))
        {
            if (go.hideFlags != HideFlags.None)
                continue;
#if (UNITY_EDITOR)
            if (PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.NotAPrefab &&
                PrefabUtility.GetPrefabInstanceStatus(go) != PrefabInstanceStatus.NotAPrefab)
                continue;
#endif
            return go;
        }

        return null;
    }
}
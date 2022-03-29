using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Extension methods for the Transform component.
/// </summary>
public static class TransformExtension
{
    public static Transform DestroyImmediateAllChildren(this Transform transform, params string[] dontDestroy)
    {
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in transform)
            if (!dontDestroy.Contains(child.name))
                toDestroy.Add(child.gameObject);

        foreach (GameObject child in toDestroy)
            Object.DestroyImmediate(child.gameObject);
        
        return transform;
    }
    public static Transform DestroyAllChildren(this Transform transform, params string[] dontDestroy)
    {
        foreach (Transform child in transform)
            if (!dontDestroy.Contains(child.name))
                Object.Destroy(child.gameObject);

        return transform;
    }
    
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(aParent);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name == aName)
                return c;
            foreach(Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    } 
}

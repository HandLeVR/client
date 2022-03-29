using UnityEngine;

/// <summary>
/// Hides an object after the given time.
/// </summary>
public class GameObjectActiveTimer : MonoBehaviour
{
    public GameObject objectToActivate;
    public float hideAfter = 1.0f;
    
    private float currentOffsetHide;

    private void Update()
    {
        if (!objectToActivate.activeSelf) 
            return;
        
        if (currentOffsetHide >= 0f)
            currentOffsetHide -= Time.deltaTime;
        else
            objectToActivate.SetActive(false);
    }

    public void ShowObject()
    {
        currentOffsetHide = hideAfter;
        objectToActivate.SetActive(true);
    }
}

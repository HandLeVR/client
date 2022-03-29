using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A selected child should be highlighted as long as no other sibling object has been clicked.
/// Please attach to the desired "content" object of a ScrollView which contains child objects based
/// on a button prefab to highlight the last selected elements.
/// </summary>
public class ChildHighlighter : MonoBehaviour
{
    private EventSystem eventSystem;
    private GameObject lastChild;

    [Tooltip("A prefab containing a button the colors a used from.")]
    public GameObject templateButton;

    private ColorBlock normalColors;
    private ColorBlock highlightColors;

    void Start()
    {
        eventSystem = FindObjectOfType<EventSystem>();
        normalColors = templateButton.GetComponent<Button>().colors;
        highlightColors = normalColors;
        // copy the highlighting color
        highlightColors.normalColor = normalColors.selectedColor;
    }

    void Update()
    {
        if (eventSystem == null || eventSystem.currentSelectedGameObject == null)
            return;

        // object is not one of our children
        if (!eventSystem.currentSelectedGameObject.transform.parent.Equals(transform))
            return;

        // object is preview which we ignore
        if (eventSystem.currentSelectedGameObject.transform.name.Equals("Preview"))
            return;
        
        // something was selected before
        if (lastChild != null) 
        {
            // clicked other child than beforehand
            if (!eventSystem.currentSelectedGameObject.Equals(lastChild)) 
            {
                // set highlight color
                eventSystem.currentSelectedGameObject.GetComponent<Button>().colors = highlightColors; 
                // reset normal color
                lastChild.GetComponent<Button>().colors = normalColors;
                // update object
                lastChild = eventSystem.currentSelectedGameObject; 
            }
            //else: nothing to change
        }
        // nothing selected before
        else 
        {
            eventSystem.currentSelectedGameObject.GetComponent<Button>().colors = highlightColors; // set highlight color
            lastChild = eventSystem.currentSelectedGameObject; // update object
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;
using UnityEngine.Serialization;

/// <summary>
/// Base class for list elements displayed in a table (e.g. all available users in the user menu). 
/// </summary>
public class BasicTableElement : MonoBehaviour
{
    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;
    public TextMeshProUGUI text3;
    public TextMeshProUGUI text4;
    public TextMeshProUGUI text5;
    public TextMeshProUGUI text6;
    public Image image;
    public Button deleteButton;
    public Button infoButton;
    
    [HideInInspector] public Button containerButton;

    private void Awake()
    {
        containerButton = GetComponent<Button>();
    }
}

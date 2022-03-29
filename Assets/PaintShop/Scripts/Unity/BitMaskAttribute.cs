using UnityEngine;

/// <summary>
/// Allows to create custom dropdown in the inspector for enums.
/// Needs to be placed outside a Editor folder to be treated different.
/// 
/// Source: https://answers.unity.com/questions/393992/custom-inspector-multi-select-enum-dropdown.html
/// </summary>
public class BitMaskAttribute : PropertyAttribute
{
    public System.Type propType;
    public BitMaskAttribute(System.Type aType)
    {
        propType = aType;
    }
}

using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Provides method for setting input fields used by multiple classes.
/// </summary>
public static class InputFieldUtil
{
    public static void SetCoatInputFieldSettings(TMP_InputField inputCoat, JObject json, string propertyName)
    {
        if (json.TryGetValue(propertyName, out JToken coatJson))
        {
            long id = (long)coatJson;
            inputCoat.placeholder.GetComponent<TextMeshProUGUI>().text = id.ToString();
            if (id >= 0)
            {
                Coat coat = DataController.Instance.coats[(long)coatJson];
                inputCoat.text = coat.name;
            }
            else if (id == -3)
                inputCoat.text = Statics.NO_COAT;
            else if (id == -2)
                inputCoat.text = Statics.COAT_FROM_RECORDING;
            else if (id == -1)
                inputCoat.text = Statics.COAT_FROM_COAT_SELECTION;
        }
        else
        {
            inputCoat.placeholder.GetComponent<TextMeshProUGUI>().text = "Klicken zur Auswahl eines Lackes";
            inputCoat.text = "";
        }
    }
    
    public static void SetCoatInputFieldDisplaying(TMP_InputField inputField, JObject json, string propertyName)
    {
        if (json.TryGetValue(propertyName, out JToken coatIdJson))
        {
            long coatId = (long) coatIdJson;
            if (coatId == -1)
                inputField.text = Statics.COAT_FROM_COAT_SELECTION;
            else if (coatId == -2)
                inputField.text = Statics.COAT_FROM_RECORDING;
            else if (coatId == -3)
                inputField.text = Statics.NO_COAT;
            else if (coatId > 0)
                inputField.text = DataController.Instance.coats[coatId].name;
        }
        else
            inputField.text = "";

        inputField.transform.parent.gameObject.SetActive(inputField.text != "");
    }
}

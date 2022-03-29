using UnityEngine;
using UnityEditor;
using System.IO;

/*  Based on the scripts "https://gameart.eu.org/unity-extracting-meshes-fbx-files/" 
 *  and https://docs.unity3d.com/ScriptReference/PrefabUtility.html 
 *  
 *  Will create a new folder for each car model selected, move the original *.FBX 
 *  (, save the parts as prefabs) and create preview-PNGs from car as well as from every single part.
 *  
 *  CarA.FBX  -> FolderCarA
 *                  ----> CarA.FBX
 *                  ----> CarA_preview.PNG
 *                  ----> CarA_Part0.prefab 
 *                  ----> CarA_Part0_preview.PNG
 *                  .
 *                  .
 *                  .
 *                  ----> CarA_Partn.prefab  
 *                  ----> CarA_Partn_preview.PNG
 *  Usage: In Unity, right-click on car model(s) and select "PrepareFBXForComponentSelection"
 *  from the context menu.
 */
public static class PartAndPNGCreator
{
    private static string _Titel = "Preparing FBX For Component Selection";

    // Create contextmenu entry, check if selection is a validate *.FBX file
    [MenuItem("Assets/PrepareFBXForComponentSelection", validate = true)]
    private static bool SelectionValidate()
    {
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            if (!AssetDatabase.GetAssetPath(Selection.objects[i]).EndsWith(".FBX")) return false;
        }
        return true;
    }

    [MenuItem("Assets/PrepareFBXForComponentSelection")]
    private static void PrepareFBXForComponentSelectionMenuEntry()
    {
        EditorUtility.DisplayProgressBar(_Titel, "", 0);
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            EditorUtility.DisplayProgressBar(_Titel, Selection.objects[i].name, (float)i / (Selection.objects.Length - 1));
            PrepareFBXForComponentSelection(Selection.objects[i]);
        }
        EditorUtility.ClearProgressBar();
    }

    private static void PrepareFBXForComponentSelection(Object @object)
    {
        // create necessary folder:
        string selectionPath = AssetDatabase.GetAssetPath(@object);
        string parentfolderPath = selectionPath.Substring(0, selectionPath.Length - (@object.name.Length + 5));
        string objectFolderPath = parentfolderPath + "/" + @object.name;
        Debug.Log(selectionPath);
        Debug.Log(parentfolderPath);
        Debug.Log(objectFolderPath);

        if (!AssetDatabase.IsValidFolder(objectFolderPath)) AssetDatabase.CreateFolder(parentfolderPath, @object.name);

        // load all sub assets of the car model (all sub models)
        Object[] CarModel = AssetDatabase.LoadAllAssetRepresentationsAtPath(selectionPath);

        for (int i = 0; i < CarModel.Length; i++)
        {
            // ignore .meta, .mesh ... files
            if (CarModel[i] is GameObject)
            {
                EditorUtility.DisplayProgressBar("Iterate through model parts", @object.name + " : " + CarModel[i].name, (float)i / (CarModel.Length - 1));
                GameObject gameObject = Object.Instantiate(CarModel[i]) as GameObject; // instantiate gameObject in Scene
                gameObject.name = gameObject.name.Substring(0, gameObject.name.Length - 7); //remove "(Clone) from name"
                if (gameObject.transform.childCount == 0) // only the separate parts, not the whole model
                {
                    string partName = objectFolderPath + "\\" + gameObject.name + ".prefab";
                    PrefabUtility.SaveAsPrefabAsset(gameObject, partName);
                }
                string pngName = objectFolderPath + "\\" + gameObject.name + "_preview.PNG";
                CreatePNGFromGameObject(gameObject, pngName);                
                Object.DestroyImmediate(gameObject); //remove gameObject from Scene
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreatePNGFromGameObject(GameObject gameObject, string path)
    {
        Texture2D pic = null;
        // GetAssetPreview() is asynchronous, otherwise the picture may be null if we try to save it with File.WriteAllBytes()
        while (pic == null) 
        {
            pic = AssetPreview.GetAssetPreview(gameObject);
        }
        byte[] _bytes = pic.EncodeToPNG();
        try
        {
            File.WriteAllBytes(path, _bytes);
        }
        
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }
}
 using UnityEngine;
 using UnityEditor;

 /// <summary>
 /// Allows the generation of previews of a 3d model.
 ///
 /// Attention:
 /// - The target objects needs static lightning to get the same lightning condition of the scene in the preview image
 /// - Virtual Reality support needs to be disabled in the XR settings
 /// </summary>
 public class PreviewCreationManager : MonoBehaviour
 {
     public GameObject targetObject;
     public ScreenshotCamera screenshotCamera;
     public string assetPath = "Assets/";

     public void OnEnable()
     {
         screenshotCamera.transform.LookAt(targetObject.transform);
         string previewPath = assetPath + targetObject.name + "_preview" + ".png";
         screenshotCamera.TakeScreenShot(previewPath);
     }
 }
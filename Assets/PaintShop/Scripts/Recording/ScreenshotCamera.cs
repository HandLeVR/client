using UnityEngine;

/// <summary>
/// Allows the creation of screenshots.
/// 
/// Source: https://answers.unity.com/questions/22954/how-to-save-a-picture-take-screenshot-from-a-camer.html
/// </summary>
public class ScreenshotCamera : MonoBehaviour {
    public int resWidth = 1024; 
    public int resHeight = 1024;
    public bool lookAtWorkpiece = true;

    private Camera cam;
    private bool takeHiResShot = false;
    private string filePath;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }
 
    public void TakeScreenShot(string path)
    {
        gameObject.SetActive(true);
        filePath = path;
        takeHiResShot = true;
    }

    public bool IsTakingScreenShot()
    {
        return takeHiResShot;
    }
 
    void LateUpdate() {
        if (takeHiResShot) {
            if (lookAtWorkpiece)
                transform.LookAt(ApplicationController.Instance.currentWorkpieceGameObject.transform.GetChild(0));
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGBA32, false);
            cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            cam.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = filePath;
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log($"Took screenshot to: {filename}");
            takeHiResShot = false;
            gameObject.SetActive(false);
        }
    }
}

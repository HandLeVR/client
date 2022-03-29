using UnityEngine;

/// <summary>
/// Can be used to saves RenderTextures as a file.
/// 
/// example usage: TextureSaver.SaveTextureAsPNG(mainOutput, "D:\\Nutzer\\vr_experiments\\Downloads\\metallicOutput.png");
/// </summary>
public class TextureSaver : MonoBehaviour
{
    public static void SaveTextureAsPNG(RenderTexture _texture, string _fullPath)
    {
        byte[] _bytes = ToTexture2D(_texture).EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullPath, _bytes);
        Debug.Log(_bytes.Length/2048  + "Kb was saved as: " + _fullPath);
    }
    
    public static void SaveTextureAsPNG(byte[] _texture, string _fullPath)
    {
        byte[] _bytes = _texture;
        System.IO.File.WriteAllBytes(_fullPath, _bytes);
        Debug.Log(_bytes.Length/2048  + "Kb was saved as: " + _fullPath);
    }
    
    public static Texture2D ToTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBAHalf, false);
        var currentActive = RenderTexture.active;
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        RenderTexture.active = currentActive;
        return tex;
    }
}

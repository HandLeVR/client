using System.IO;
using System.Linq;
using translator;

/// <summary>
/// Provides validation methods.
/// </summary>
public static class ValidationUtil
{

    public static bool ValidateName(string name)
    {
        char invalidChar = GetInvalidFilenameChar(name);
        if (!char.IsWhiteSpace(invalidChar))
        {
            PopupScreenHandler.Instance.ShowMessage("popup-invalid-name",
                string.Format(TranslationController.Instance.Translate("popup-invalid-name."), invalidChar));
            return false;
        }

        if (name.Length > 50)
        {
            PopupScreenHandler.Instance.ShowMessage("popup-name-long", "popup-name-long-text");
            return false;
        }

        return true;
    }
    
    private static char GetInvalidFilenameChar(string fileName)
    {
        foreach (var t in fileName)
            if (Path.GetInvalidFileNameChars().Contains(t))
                return t;

        return ' ';
    }
}
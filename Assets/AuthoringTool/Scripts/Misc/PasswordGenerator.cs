using System.Text;
using UnityEngine;

/// <summary>
/// Generates a random password.
/// </summary>
public static class PasswordGenerator
{
    /// <summary>
    /// 0-9 A-Z a-z (Length of roomName === 6; up from 4). 
    /// https://answers.unity.com/questions/241219/random-code-generation.html
    /// </summary>
    /// <returns></returns>
    public static string GenerateRandomAlphaNumericStr(int desiredLength)
    {
        StringBuilder codeSB = new StringBuilder(""); 
        char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        while (codeSB.Length < desiredLength)
            codeSB.Append(chars[Random.Range(0, chars.Length)]);

        return codeSB.ToString();
    }
}

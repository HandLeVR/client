using System.IO;
using System.IO.Compression;
using UnityEngine.Events;

/// <summary>
/// Util functions to compress files into an archive or to unpack an archive.
/// </summary>
public static class ZipUtil
{
    /// <summary>
    /// Creates a zip containing the given files. The name is removed from the files.
    /// </summary>
    public static async System.Threading.Tasks.Task CompressAsync(string directoryPath, string fileDestination,
        UnityAction onSuccess)
    {
        if (File.Exists(fileDestination))
            File.Delete(fileDestination);

        await System.Threading.Tasks.Task.Run(() => ZipFile.CreateFromDirectory(directoryPath, fileDestination));

        onSuccess.Invoke();
    }

    /// <summary>
    /// Removes the zip file.
    /// </summary>
    public static void RemoveZipFile(string file)
    {
        if (File.Exists(file))
            File.Delete(file);
    }


    /// <summary>
    /// Extracts the zip file and adds the file name as a prefix to the extracted files.
    /// </summary>
    public static async System.Threading.Tasks.Task ExtractAsync(FileInfo file, UnityAction onSuccess)
    {
        string targetDir = file.FullName.Replace(".zip", "");

        if (Directory.Exists(targetDir))
            Directory.Delete(targetDir, true);
        Directory.CreateDirectory(targetDir);

        await System.Threading.Tasks.Task.Run(() => ZipFile.ExtractToDirectory(file.FullName, targetDir));

        await System.Threading.Tasks.Task.Run(() => File.Delete(file.FullName));

        onSuccess.Invoke();
    }
}
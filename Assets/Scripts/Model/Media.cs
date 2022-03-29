using System.IO;
using UnityEngine;

public class Media
{
    public long id;
    public Permission permission;
    public string name;
    public string description;
    public MediaType type;
    public string data;

    public Media()
    {
    }

    public Media(long id, string name, string description, MediaType type, string data)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.type = type;
        this.data = data;
    }

    public enum MediaType
    {
        Video,
        Image,
        Audio
    }

    /// <summary>
    /// Returns the path to the file.
    /// </summary>
    public string GetPath()
    {
        return Path.Combine(Application.streamingAssetsPath, data);
    }

    /// <summary>
    /// Determines the directory of the file in dependence of the media type.
    /// </summary>
    public static string MediaTypeToDirectory(MediaType mediaType)
    {
        switch (mediaType)
        {
            case MediaType.Image:
                return "Images";
            case MediaType.Audio:
                return "Audio";
        }
        return "Videos";
    }
}
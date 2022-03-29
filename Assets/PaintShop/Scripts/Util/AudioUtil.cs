using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public static class AudioUtil
{
    public static IEnumerator LoadAudioFile(string path, Action<AudioClip> onLoadingCompleted)
    {
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN))
        {
            uwr.useHttpContinue = false;
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError ||
                uwr.result == UnityWebRequest.Result.ProtocolError)
                Debug.Log(uwr.error);
            else
                onLoadingCompleted.Invoke(DownloadHandlerAudioClip.GetContent(uwr));
        }
    }
}

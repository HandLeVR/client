using System;
using System.IO;
using JetBrains.Annotations;
using Proyecto26;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// WIP
/// 
/// Allows to create audio files from text.
/// </summary>
public class TTSController : MonoBehaviour
{
    private string APIKey;

    // This languageCode will be used to get voices that correspond to the language code
    public string languageCode;

    // A list of voices to choose from
    public Voice[] voices;

    // text input to synthesize
    public string synthesisInput;

    // To hand over details about the chosen voice to the synthesize request
    public VoiceSelectionParams voiceSelectionParams;

    // Does not display every possibility in the Editor, offers multiple ways to customize the synthesize request
    public AudioConfig audioConfig;

    // provides the result of the TTS request for other uses
    public String result;


    void Start()
    {
        APIKey = File.ReadAllText(Application.streamingAssetsPath + "/APIKey.txt");
    }

    /// <summary>
    /// Requests and loads all voices that correspond to a given language code
    /// </summary>
    public void GetVoices()
    {
        RestClient.Request(new RequestHelper
        {
            Uri = ("https://texttospeech.googleapis.com/v1/voices?key=" + APIKey + "&languageCode=" + languageCode),
            Method = "GET"
        }).Then(response => { SetVoices(response.Text); }).Catch(err =>
        {
            if (err != null)
            {
                Debug.Log("Error: " + err);
            }
            else
            {
                Debug.Log("Unknown Error");
            }
        });
    }

    /// <summary>
    /// Saves all voices from a json string to the local variable
    /// </summary>
    /// <param name="response">Json string containing voices</param>
    void SetVoices(string response)
    {
        JObject obj = JObject.Parse(response);
        voices = JsonConvert.DeserializeObject<Voice[]>(obj.GetValue("voices").ToString());
    }

    /// <summary>
    /// Makes a synthesis request to the google text to speech API
    /// </summary>
    /// <param name="synthesisInput">Text to be synthesized into speech</param>
    /// <param name="voiceSelectionParams">Chosen voice</param>
    /// <param name="audioConfig">Audio configuration for the synthesis</param>
    public void Synthesize(string synthesisInput, VoiceSelectionParams voiceSelectionParams, AudioConfig audioConfig)
    {
        SynthesisInput input = new SynthesisInput();
        input.text = synthesisInput;

        RequestBody requestBody = new RequestBody();
        requestBody.input = input;
        requestBody.voice = voiceSelectionParams;
        requestBody.audioConfig = audioConfig;

        String json = JsonConvert.SerializeObject(requestBody);

        RestClient.Request(new RequestHelper
        {
            Uri = ("https://texttospeech.googleapis.com/v1/text:synthesize?key=" + APIKey),
            Method = "POST",
            BodyString = json,
        }).Then(response => { SaveToMp3(response.Text); }).Catch(err =>
        {
            if (err != null)
            {
                Debug.Log("Error: " + err);
            }
            else
            {
                Debug.Log("Unknown Error");
            }
        });
    }


    /// <summary>
    /// Takes a base64 encoded string and saves it as a mp3 file in StreamingAssets
    /// </summary>
    /// <param name="response">base64 encoded string</param>
    void SaveToMp3(string response)
    {
        result = response;
        JObject responseObject = JObject.Parse(response);
        byte[] data = Convert.FromBase64String(responseObject.GetValue("audioContent").ToString());
        File.WriteAllBytes(Application.streamingAssetsPath + "/Output.mp3", data);
    }
}


public enum SsmlVoiceGender
{
    SSML_VOICE_GENDER_UNSPECIFIED,
    MALE,
    FEMALE,
    NEUTRAL
}

public enum AudioEncoding
{
    AUDIO_ENCODING_UNSPECIFIED,
    LINEAR16,
    MP3,
    OGG_OPUS
}

[Serializable]
public class Voice
{
    public string[] languageCodes;
    public string name;
    public SsmlVoiceGender ssmlGender;
    public int naturalSampleRateHertz;

    public Voice(string[] languageCodes, string name, SsmlVoiceGender ssmlGender, int naturalSampleRateHertz)
    {
        this.languageCodes = languageCodes;
        this.name = name;
        this.ssmlGender = ssmlGender;
        this.naturalSampleRateHertz = naturalSampleRateHertz;
    }

    public string[] GetLanguageCodes()
    {
        return this.languageCodes;
    }

    public void SetLanguageCodes(string[] newLanguageCodes)
    {
        this.languageCodes = newLanguageCodes;
    }

    public string GetName()
    {
        return this.name;
    }

    public void SetName(string newName)
    {
        this.name = newName;
    }

    public SsmlVoiceGender GetSsmlGender()
    {
        return this.ssmlGender;
    }

    public void SetSsmlGender(SsmlVoiceGender newSsmlGender)
    {
        this.ssmlGender = newSsmlGender;
    }

    public int GetNaturalSampleHertzRate()
    {
        return this.naturalSampleRateHertz;
    }

    public void SetNaturalSampleHertzRate(int newNaturalSampleHertzRate)
    {
        this.naturalSampleRateHertz = newNaturalSampleHertzRate;
    }
}

[Serializable]
public class VoiceSelectionParams
{
    public string languageCode;
    public string name;
    public SsmlVoiceGender ssmlGender;

    public VoiceSelectionParams(string languageCode, string name, SsmlVoiceGender ssmlGender)
    {
        this.languageCode = languageCode;
        this.name = name;
        this.ssmlGender = ssmlGender;
    }

    public string GetLanguageCode()
    {
        return this.languageCode;
    }

    public void SetLanguageCode(string newLanguageCode)
    {
        this.languageCode = newLanguageCode;
    }

    public string GetName()
    {
        return this.name;
    }

    public void SetName(string newName)
    {
        this.name = newName;
    }

    public SsmlVoiceGender GetSsmlGender()
    {
        return this.ssmlGender;
    }

    public void SetSsmlGender(SsmlVoiceGender newSsmlGender)
    {
        this.ssmlGender = newSsmlGender;
    }
}

[Serializable]
public class AudioConfig
{
    public AudioEncoding audioEncoding;
    public float? speakingRate;
    public float? pitch;
    public float? volumeGainDb;
    public int? sampleRateHertz;
    [CanBeNull] public string[] effectsProfileId;

    public AudioConfig(AudioEncoding audioEncoding, float? speakingRate, float? pitch, float? volumeGainDb,
        int? sampleRateHertz, [CanBeNull] string[] effectsProfileId)
    {
        this.audioEncoding = audioEncoding;
        this.speakingRate = speakingRate;
        this.pitch = pitch;
        this.volumeGainDb = volumeGainDb;
        this.sampleRateHertz = sampleRateHertz;
        this.effectsProfileId = effectsProfileId;
    }

    public AudioEncoding GetAudioEncoding()
    {
        return this.audioEncoding;
    }

    public void SetAudioEncoding(AudioEncoding audioEncoding)
    {
        this.audioEncoding = audioEncoding;
    }

    public float? GetSpeakingRate()
    {
        return this.speakingRate;
    }

    public void SetSpeakingRate(float? speakingRate)
    {
        this.speakingRate = speakingRate;
    }

    public float? GetPitch()
    {
        return this.pitch;
    }

    public void SetPitch(float? pitch)
    {
        this.pitch = pitch;
    }

    public float? GetVolumeGainDb()
    {
        return this.volumeGainDb;
    }

    public void SetVolumeGainDb(float? volumeGainDb)
    {
        this.volumeGainDb = volumeGainDb;
    }

    public int? GetSampleRateHertz()
    {
        return this.sampleRateHertz;
    }

    public void SetSampleRateHertz(int? sampleRateHertz)
    {
        this.sampleRateHertz = sampleRateHertz;
    }

    [CanBeNull]
    public string[] GetEffectsProfileId()
    {
        return this.effectsProfileId;
    }

    public void SetEffectsProfileId([CanBeNull] string[] effectsProfileId)
    {
        this.effectsProfileId = effectsProfileId;
    }
}

[Serializable]
public class SynthesisInput
{
    public string text;
    [CanBeNull] public string ssml;
}

[Serializable]
public class RequestBody
{
    public SynthesisInput input;
    public VoiceSelectionParams voice;
    public AudioConfig audioConfig;
}
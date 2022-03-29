using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Takes care of the conversion of the color class from and to json.
/// </summary>
public class ColorJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue("#" + ColorUtility.ToHtmlStringRGB((Color)value));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        ColorUtility.TryParseHtmlString(JToken.Load(reader).ToString(), out Color color);
        return color;
    }

    public override bool CanRead => true;

    public override bool CanConvert(Type objectType)
    {
        // CanConvert is not called when [JsonConverter] attribute is used
        return false;
    }
}
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Converts a SubTask object to json and a json to a SubClass object. This is need because in the Task class we get
/// the sub task list as a string containing a list of json objects instead of a json list.
/// </summary>
public class SubTaskJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(JsonConvert.SerializeObject(value));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return JsonConvert.DeserializeObject<List<SubTask>>(JToken.Load(reader).ToString());
    }

    public override bool CanRead => true;

    public override bool CanConvert(Type objectType)
    {
        // CanConvert is not called when [JsonConverter] attribute is used
        return false;
    }
}
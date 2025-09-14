using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//public class ItemDataConverter : JsonConverter<ItemData2>
//{
//    public override void WriteJson(JsonWriter writer, ItemData2 value, JsonSerializer serializer)
//    {
//        //writer.WriteStartObject();
//        //writer.WritePropertyName("id");
//        writer.WriteValue(value.Id);
//        //writer.WriteEndObject();
//    }
//    public override ItemData ReadJson(JsonReader reader, System.Type objectType, ItemData2 existingValue, bool hasExistingValue, JsonSerializer serializer)
//    {
//        var id = reader.Value as string;
//        return DataTableManger2.ItemTable.Get(id);
//    }
//}

public class ColorConverter : JsonConverter<Color>
{
    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("r");
        writer.WriteValue(value.r);
        writer.WritePropertyName("g");
        writer.WriteValue(value.g);
        writer.WritePropertyName("b");
        writer.WriteValue(value.b);
        writer.WritePropertyName("a");
        writer.WriteValue(value.a);
        writer.WriteEndObject();
    }
    public override Color ReadJson(JsonReader reader, System.Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        Color color = Color.white;
        JObject obj = JObject.Load(reader);
        color.r = (float)(obj["r"]?.ToObject<double>() ?? 1.0);
        color.g = (float)(obj["g"]?.ToObject<double>() ?? 1.0);
        color.b = (float)(obj["b"]?.ToObject<double>() ?? 1.0);
        color.a = (float)(obj["a"]?.ToObject<double>() ?? 1.0);
        return color;
    }
}

public class Vector3Converter : JsonConverter<Vector3>
{
    public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(value.x);
        writer.WritePropertyName("y");
        writer.WriteValue(value.y);
        writer.WritePropertyName("z");
        writer.WriteValue(value.z);
        writer.WriteEndObject();
    }
    public override Vector3 ReadJson(JsonReader reader, System.Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        Vector3 vector3 = Vector3.zero;
        JObject obj = JObject.Load(reader);
        vector3.x = (float)(obj["x"]?.ToObject<double>() ?? 0.0);
        vector3.y = (float)(obj["y"]?.ToObject<double>() ?? 0.0);
        vector3.z = (float)(obj["z"]?.ToObject<double>() ?? 0.0);
        return vector3;
    }
}

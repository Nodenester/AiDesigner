using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodeBaseApi.Version2;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AiDesigner.Shared.Data
{
    public class TupleConverter : JsonConverter
    {
        public override bool CanConvert(System.Type objectType)
        {
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Tuple<,>);
        }

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                var jArray = JArray.Load(reader);

                var item1 = jArray[0].ToObject(objectType.GetGenericArguments()[0], serializer);
                var item2 = jArray[1].ToObject(objectType.GetGenericArguments()[1], serializer);

                return Activator.CreateInstance(objectType, item1, item2);
            }

            throw new JsonSerializationException("Invalid JSON for Tuple.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var tuple = (dynamic)value;

            writer.WriteStartArray();
            writer.WriteValue(tuple.Item1);
            writer.WriteValue(tuple.Item2);
            writer.WriteEndArray();
        }
    }
    public class BlockJsonConverter : JsonConverter
    {
        public override bool CanConvert(System.Type objectType)
        {
            return typeof(Block).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var blockName = jsonObject["Name"].Value<string>().Replace(" ", "");

            var blockType = Assembly.GetExecutingAssembly().GetTypes()
                .FirstOrDefault(t => t.IsSubclassOf(typeof(Block)) && t.Name == blockName);

            if (blockType == null)
                throw new InvalidDataException($"BlockType not supported: {blockName}");

            var block = (Block)Activator.CreateInstance(blockType);
            serializer.Populate(jsonObject.CreateReader(), block);

            return block;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            var block = (Block)value;
            var blockType = block.GetType();

            var jo = new JObject
    {
        // Add the name of the derived class, you're relying on this in ReadJson
        // Prepend "Block" to the "Name" property to ensure it's unique
        { "BlockName", blockType.Name }
    };

            foreach (var prop in blockType.GetProperties().Where(p => p.CanRead))
            {
                var propVal = prop.GetValue(block, null);
                if (propVal != null)
                {
                    jo.Add(prop.Name, JToken.FromObject(propVal, serializer));
                }
            }
            jo.WriteTo(writer);
        }
    }
}

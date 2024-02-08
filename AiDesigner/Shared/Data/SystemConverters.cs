using NodeBaseApi.Version2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AiDesigner.Shared.Data
{
    public class TupleJsonConverterSystem : System.Text.Json.Serialization.JsonConverter<Tuple<int, int>>
    {
        public override Tuple<int, int> Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var elements = doc.RootElement.EnumerateArray().ToArray();
                int item1 = elements[0].GetInt32();
                int item2 = elements[1].GetInt32();
                return new Tuple<int, int>(item1, item2);
            }
        }

        public override void Write(Utf8JsonWriter writer, Tuple<int, int> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Item1);
            writer.WriteNumberValue(value.Item2);
            writer.WriteEndArray();
        }
    }

    public class BlockJsonConverterSystem : JsonConverter<Block>
    {
        public override Block Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var jsonObject = doc.RootElement;
                var blockName = jsonObject.GetProperty("BlockName").GetString().Replace(" ", "");
                var blockType = Assembly.GetExecutingAssembly().GetTypes()
                                        .FirstOrDefault(t => t.IsSubclassOf(typeof(Block)) && t.Name.Equals(blockName, StringComparison.OrdinalIgnoreCase));

                if (blockType == null)
                {
                    throw new JsonException($"BlockType not supported: {blockName}");
                }

                var block = (Block)JsonSerializer.Deserialize(jsonObject.GetRawText(), blockType, options);
                return block ?? throw new JsonException("Failed to deserialize block.");
            }
        }

        public override void Write(Utf8JsonWriter writer, Block value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("BlockName", value.GetType().Name);

            foreach (var prop in value.GetType().GetProperties().Where(p => p.CanRead))
            {
                var propValue = prop.GetValue(value);
                writer.WritePropertyName(prop.Name);
                JsonSerializer.Serialize(writer, propValue, prop.PropertyType, options);
            }

            writer.WriteEndObject();
        }
    }

    //public class ProgramStructureConverter : JsonConverter<ProgramStructure>
    //{
    //    public override ProgramStructure Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        // Implement deserialization logic here
    //    }

    //    public override void Write(Utf8JsonWriter writer, ProgramStructure value, JsonSerializerOptions options)
    //    {
    //        // Implement serialization logic here
    //    }
    //}

}

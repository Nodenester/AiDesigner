using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodeBaseApi.Version2;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace AiDesigner.Shared.Data
{
    public class TupleConverter : JsonConverter
    {
        public override bool CanConvert(System.Type objectType)
        {
            var Canconvert = objectType.IsGenericType &&
                   objectType.GetGenericTypeDefinition().FullName.StartsWith("System.Tuple`");
            return Canconvert;
        }

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartArray)
            {
                throw new JsonSerializationException("Expected StartArray token");
            }

            JArray jArray = JArray.Load(reader);
            System.Type[] tupleTypes = objectType.GetGenericArguments();
            if (jArray.Count != tupleTypes.Length)
            {
                throw new JsonSerializationException($"Expected {tupleTypes.Length} elements in tuple.");
            }

            object[] tupleValues = new object[tupleTypes.Length];
            for (int i = 0; i < tupleTypes.Length; i++)
            {
                tupleValues[i] = jArray[i].ToObject(tupleTypes[i], serializer);
            }

            // Construct the tuple using reflection
            object tuple = Activator.CreateInstance(objectType, args: tupleValues);
            return tuple;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            System.Type tupleType = value.GetType();
            System.Type[] tupleElementTypes = tupleType.GetGenericArguments();

            writer.WriteStartArray();

            for (int i = 1; i <= tupleElementTypes.Length; i++)
            {
                object item = tupleType.GetProperty($"Item{i}").GetValue(value, null);
                serializer.Serialize(writer, item);
            }

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

    public class IgnoreTuplesContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (IsTuple(property.PropertyType))
            {
                property.ShouldSerialize = instance => false; // Ignore tuple properties.
            }

            return property;
        }

        private bool IsTuple(System.Type type)
        {
            // Check if the type is a Tuple by checking its definition.
            // This method can be expanded to include more checks or refined to target specific tuple types.
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Tuple<,>)
                || type.GetGenericTypeDefinition() == typeof(Tuple<,,>)); // Add more as needed
        }
    }
}

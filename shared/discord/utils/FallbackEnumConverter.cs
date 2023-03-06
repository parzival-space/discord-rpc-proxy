using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RPCProxy.Shared.Discord.Utils
{

  public class FallbackEnumConverter<T> : StringEnumConverter where T : struct, Enum
  {
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
      if (reader.TokenType == JsonToken.String)
      {
        string? enumString = reader.Value!.ToString();
        if (!String.IsNullOrEmpty(enumString) && Enum.TryParse(enumString, true, out T result))
        {
          return result;
        }
      }
      else if (reader.TokenType == JsonToken.Integer && !this.AllowIntegerValues)
      {
        long enumValue = (long)reader.Value!;
        if (Enum.IsDefined(typeof(T), enumValue))
        {
          return Enum.ToObject(typeof(T), enumValue);
        }
      }

      return Enum.GetValues(typeof(T)).Cast<T>().First();
    }
  }

}